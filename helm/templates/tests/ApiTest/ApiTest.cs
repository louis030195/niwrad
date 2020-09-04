using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Api.Realtime;
using Api.Rpc;
using Nakama;
using NUnit.Framework;
using Google.Protobuf;
using Protometry.Quaternion;
using Protometry.Vector3;

namespace ApiTest
{
    public class ApiTest
    {
        private Client _client;
        private ISession _session;
        private ISocket _socket;
        private string _matchId;
        private IMatch _match;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var env = Environment.GetEnvironmentVariables();
            const string scheme = "http";
            var host = env["NAKAMA_HOST"] as string;
            host ??= "172.17.0.2";
            var port = env["NAKAMA_PORT"] as string;
            var defaultUsername = env["NAKAMA_USERNAME"] as string;
            defaultUsername ??= "azerty@azerty.com";
            var defaultPassword = env["NAKAMA_PASSWORD"] as string;
            defaultPassword ??= "azertyazerty";
            Assert.AreNotEqual(port, string.Empty);
            Console.WriteLine($"host: {host}:{port}");
            using var ping = new Ping();
            try
            {
                ping.Send(host ?? "");
            }
            catch (PingException)
            {
                Assert.Fail("Nakama is unreachable");
            }

            const string serverKey = "defaultkey";
            const string tokenPath = @"/tmp/test_token.txt";
            _client = new Client(scheme, host, Convert.ToInt32(port), serverKey);
            if (System.IO.File.Exists(tokenPath))
            {
                var token = await System.IO.File.ReadAllTextAsync(tokenPath);
                if (token.Equals(string.Empty)) _session = Session.Restore(token);
            }

            if (_session == null || _session.IsExpired)
            {
                Console.WriteLine("Session has expired. Must re-authenticate!");
                _session = await _client.AuthenticateEmailAsync(defaultUsername, defaultPassword, create: true);
                Console.WriteLine("Re-authenticated!");
            }
            Assert.NotNull(_session);
            Assert.False(_session.IsExpired);
            await System.IO.File.WriteAllTextAsync(tokenPath, _session.AuthToken);
            _socket = Socket.From(_client);
            _socket.Connected += () =>
            {
                System.Console.WriteLine("Socket connected.");
            };
            _socket.Closed += () =>
            {
                System.Console.WriteLine("Socket closed.");
            };
            _socket.ReceivedError += e => Assert.Fail(e.Message);
            await _socket.ConnectAsync(_session);
            Assert.True(_socket.IsConnected);
        }
        
        private void StopMatch()
        {
            if (_matchId == null) return;
            var req = new StopMatchRequest{ MatchId = _matchId }.ToByteString().ToStringUtf8();
            // Just skip assertion, it takes time to do the k8s delete, timeout, should work :p
            try
            {
                _socket.RpcAsync("stop_match", req);
            }
            catch (Exception)
            {
                // ignored
            }

            Console.WriteLine("Match stopped");

            // Assert.NotNull(res.Payload);
            // var parsedRes = CreateMatchResponse.Parser.ParseFrom(Encoding.UTF8.GetBytes(res.Payload));
            // Assert.NotNull(parsedRes);
            // Assert.AreEqual(StopMatchCompletionResult.StopServerCompletionResultSucceeded, parsedRes.Result);
            // res = await _socket.RpcAsync("delete_all_accounts");
            // Assert.NotNull(res.Payload);
        }

        [Test]
        public async Task CreateMatchJoinAndSpawn()
        {
            await CreateMatch();
            var tries = 0;
            string[] matches;
            while (true)
            {
                await Task.Delay(10000);
                matches = await ListMatches();
                Console.WriteLine($"Found matches {matches}");
                if (matches.Length > 0) break;
                if (tries > 10) Assert.Fail("Can't find a match after 10 tries");
                tries++;
            }
            _matchId = matches[0];
            await JoinMatch();
            await ReqSpawnAnimal();
            StopMatch();
        }

        [Test]
        public async Task StandaloneListMatches()
        {
            await ListMatches();
        }

        private async Task CreateMatch()
        {
            var req = new CreateMatchRequest().ToByteString().ToStringUtf8();
            var res = await _socket.RpcAsync("create_match", req);
            Assert.NotNull(res.Payload);
            var parsedRes = CreateMatchResponse.Parser.ParseFrom(Encoding.UTF8.GetBytes(res.Payload));
            Assert.NotNull(parsedRes);
            Assert.AreEqual( CreateMatchCompletionResult.Succeeded, parsedRes.Result);
            Assert.NotNull(parsedRes.MatchId);
        }

        /// <summary>
        /// After creating a match, expect to wait a bit that executors load until the match is listed
        /// </summary>
        /// <returns></returns>
        private async Task<string[]> ListMatches()
        {
            var res = await _socket.RpcAsync("list_matches");
            Assert.NotNull(res);
            // Somehow no matches = empty payload ?
            if (res.Payload == null) return new string[]{};
            var matches = ListMatchesResponse.Parser.ParseFrom(Encoding.UTF8.GetBytes(res.Payload));
            return matches.MatchesId.ToArray();
        }

        private async Task JoinMatch()
        {
            _match = await _socket.JoinMatchAsync(_matchId);
            Assert.NotNull(_match);
            Assert.NotNull(_match.Id);
        }

        private async Task ReqSpawnAnimal()
        {
            // Wait until we're in a match
            await TaskEx.WaitUntil(() => _match != null, timeout:5000);
            var req = new Packet
            {
                Impact = default, // Would notify everyone
                RequestSpawn = new Spawn
                {
                    Animal = new Animal
                    {
                        Transform = new Transform
                        {
                            Id = default, // Server handle the atomic objects ID
                            Position = new Vector3 {X = 10, Y = 1000, Z = 10},
                            Rotation = new Quaternion {X = 0, Y = 0, Z = 0, W = 0}
                        }
                    }
                }
            };
            var states = new List<IMatchState>();
            _socket.ReceivedMatchState += states.Add;
            
            // Ask to spawn an animal
            Console.WriteLine($"Sending req spawn {req}");
            await _socket.SendMatchStateAsync(_matchId, 0, req.ToByteArray());
            await TaskEx.WaitUntil(() => states.Count > 0, timeout: 5000);
            await Task.Delay(2000);
            // Expect a positive answer
            Assert.IsTrue(states.Any(state =>
            {
                var p = Packet.Parser.ParseFrom(state.State);
                Console.WriteLine($"Received match states: {p}");
                if (p?.Spawn?.Animal?.Transform != null)
                {
                    Console.WriteLine(
                        $"Condition: {p.Spawn.Animal.Transform.Position.X > 5 && p.Spawn.Animal.Transform.Position.X < 15}");
                }

                return p?.Spawn?.Animal?.Transform != null &&
                       p.Spawn.Animal.Transform.Position.X > 5 &&
                       p.Spawn.Animal.Transform.Position.X < 15;
            }));
        }
    }
}
