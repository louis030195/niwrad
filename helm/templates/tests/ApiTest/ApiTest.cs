using System;
using System.Collections.Generic;
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
            host ??= "172.17.0.3";
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
        
        [OneTimeTearDown]
        public async Task TearDown()
        {
            if (_matchId == null) return;
            // await Task.Delay(10000); // TODO: somehow wait that all unity executors are up
            // TODO: or just stfu the executors ?
            var req = new StopMatchRequest{ MatchId = _matchId }.ToByteString().ToStringUtf8();
            var res = await _socket.RpcAsync("stop_match", req);
            Assert.NotNull(res.Payload);
            var parsedRes = CreateMatchResponse.Parser.ParseFrom(Encoding.UTF8.GetBytes(res.Payload));
            Assert.NotNull(parsedRes);
            Assert.AreEqual(StopMatchCompletionResult.StopServerCompletionResultSucceeded, parsedRes.Result);
            res = await _socket.RpcAsync("delete_all_accounts");
            Assert.NotNull(res.Payload);
        }

        [Test]
        public async Task CreateMatchJoinAndSpawn()
        {
            await CreateMatch();
            await ListMatches();
            await JoinMatch();
            await ReqSpawnAnimal();
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
        private async Task ListMatches()
        {
            var tries = 0;
            ListMatchesResponse matches;
            while (true)
            {
                var res = await _socket.RpcAsync("list_matches");
                Assert.NotNull(res.Payload);
                matches = ListMatchesResponse.Parser.ParseFrom(Encoding.UTF8.GetBytes(res.Payload));
                if (matches.MatchesId.Count > 0) break;
                tries++;
                if (tries > 10) Assert.Fail("Can't find a match after 10 tries");
                await Task.Delay(500);
            }
            _matchId = matches.MatchesId[0];
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
                IsServer = false,
                Impact = null,
                RequestSpawn = new Spawn
                {
                    Animal = new Animal
                    {
                        Transform = new Transform
                        {
                            Id = default, // Server handle the atomic objects ID
                            Position = new Vector3 {X = 10, Y = 10, Z = 10},
                            Rotation = new Quaternion {X = 0, Y = 0, Z = 0, W = 0}
                        }
                    }
                }
            }.ToByteArray();
            var states = new List<IMatchState>();
            _socket.ReceivedMatchState += states.Add;
            
            // Ask to spawn an animal
            await _socket.SendMatchStateAsync(_matchId, 0, req);
            await Task.WhenAny(Task.Run(() => states.Count > 0));
            
            // Expect a positive answer
            Assert.IsTrue(states.Exists(state =>
            {
                var p = Packet.Parser.ParseFrom(state.State);
                return p?.Spawn?.Animal?.Transform != null &&
                       Math.Abs(p.Spawn.Animal.Transform.Position.X - 10) < 0.0001;
            }));
        }
    }
}
