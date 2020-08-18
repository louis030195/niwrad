using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Api.Rpc;
using Nakama;
using NUnit.Framework;
using Google.Protobuf;
using Nakama.TinyJson;

namespace ApiTest
{
    public class ApiTest
    {
        private Client _client;
        private ISession _session;
        private ISocket _socket;
        private string _matchId;

        [SetUp]
        public async Task Setup()
        {
            var env = Environment.GetEnvironmentVariables();
            const string scheme = "http";
            var host = env["NAKAMA_HOST"] as string;
            var port = env["NAKAMA_PORT"] as string;
            var defaultUsername = env["USERNAME"] as string;
            var defaultPassword = env["PASSWORD"] as string;
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
        
        [TearDown]
        public async Task TearDown()
        {
            if (_matchId == null) return;
            var req = new StopMatchRequest{ MatchId = _matchId }.ToByteString().ToStringUtf8();
            var res = await _socket.RpcAsync("stop_match", req);
            Assert.NotNull(res.Payload);
            var parsedRes = CreateMatchResponse.Parser.ParseFrom(Encoding.UTF8.GetBytes(res.Payload));
            Assert.NotNull(parsedRes);
            Assert.AreEqual(StopMatchCompletionResult.StopServerCompletionResultSucceeded, parsedRes.Result);
        }

        [Test]
        public async Task CreateMatch()
        {
            var req = new CreateMatchRequest().ToByteString().ToStringUtf8();
            var res = await _socket.RpcAsync("create_match", req);
            Assert.NotNull(res.Payload);
            var parsedRes = CreateMatchResponse.Parser.ParseFrom(Encoding.UTF8.GetBytes(res.Payload));
            Assert.NotNull(parsedRes);
            Assert.AreEqual( CreateMatchCompletionResult.Succeeded, parsedRes.Result);
            Assert.NotNull(parsedRes.MatchId);
            _matchId = parsedRes.MatchId;
        }

        [Test]
        public async Task ListMatches()
        {
            var res = await _socket.RpcAsync("list_matches");
            Assert.NotNull(res.Payload);
            System.Console.WriteLine($"matches: {res.Payload}");
            var parsedRes = res.Payload.FromJson<Dictionary<string, string[]>>();
            if (parsedRes.ContainsKey("matches") && parsedRes["matches"].Length > 0) _matchId = parsedRes["matches"][0];
        }
    }
}
