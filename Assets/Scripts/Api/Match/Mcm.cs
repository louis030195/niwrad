using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Nakama;
using Nakama.TinyJson;
using Api.Realtime;
using Api.Rpc;
using Api.Session;
using Gameplay;
using Protometry.Volume;
using UnityEngine;
using Utils;

namespace Api.Match
{
	/// <summary>
    /// Role of this manager is sending match information to other players
    /// and receiving match messages from them through Nakama Server.
    /// Should be kept as single responsibility without mixing with logic / gameplay.
    /// </summary>
    public class Mcm : Singleton<Mcm>
    {
	    //This region contains events for all type of match messages that could be send in the game.
        //Events are fired after getting message sent by other players from Nakama server
        #region PUBLIC EVENTS

        // Match states
        public event Action<string> Initialized; // Client's game play handlers initialized, calling for state sync
        // TODO: maybe will require other states ?

        // General objects
        public event Action<Realtime.Transform> TransformUpdated;
        public event Action<NavMeshUpdate> NavMeshUpdated;

        // Evolution
        public event Action<Realtime.Transform> AnimalSpawned;
        public event Action<Realtime.Transform> PlantSpawned;
        public event Action<Realtime.Transform> AnimalDestroyed;
        public event Action<Realtime.Transform> PlantDestroyed;
        public event Action<Realtime.Transform> AnimalSpawnRequested;
        public event Action<Realtime.Transform> PlantSpawnRequested;
        public event Action<Realtime.Transform> AnimalDestroyRequested;
        public event Action<Realtime.Transform> PlantDestroyRequested;
        public event Action<Meme> MemeUpdated;


        #endregion

        #region PROPERTIES

        /// <summary>
        /// List of IUserPresence of all players
        /// </summary>
        public List<IUserPresence> players { get; private set; }

        /// <summary>
        /// Id of current match
        /// </summary>
        public string matchId { get; private set; }

        /// <summary>
        /// Current socket which connects client to Nakama server. Through this socket are sent match messages.
        /// </summary>
        private ISocket socket => Sm.instance.socket;
        
        public IUserPresence self { get; private set; }
        public Box region { get; private set; }

        #endregion

        #region PRIVATE FIELDS


        #endregion

        #region MONO

        #endregion

        #region PUBLIC METHODS


        /// <summary>
        /// Get match lists
        /// </summary>
        /// <returns></returns>
        public async Task<string[]> GetMatchListAsync()
        {
	        // The message containing last match id we send to server in order to receive required match info
	        var response = await Sm.instance.socket.RpcAsync( "list_matches");
            if (response.Payload == null) return new string[]{};
            var matches = ListMatchesResponse.Parser.ParseFrom(Encoding.UTF8.GetBytes(response.Payload));
            return matches.MatchesId.ToArray();
        }


        /// <summary>
        /// Joins given match
        /// </summary>
        /// <param name="id"></param>
        public async Task JoinMatchAsync(string id)
        {
	        //Filling list of match participants
            players = new List<IUserPresence>();

            try
            {
                // Listen to incoming match messages and user connection changes
                socket.ReceivedMatchPresence += OnMatchPresence;
                socket.ReceivedMatchState += ReceiveMatchStateMessage;
                socket.ReceivedStreamState += OnReceivedStreamState;
                socket.Closed += Application.Quit; // Stop when server close
                // Join the match
                var match = await socket.JoinMatchAsync(id);
                matchId = match.Id;
                players.AddRange(match.Presences);
                Debug.Log($"Joined match with id: {match.Id}; presences count: {match.Presences.Count()}");
            }
            catch (Exception e)
            {
                Debug.Log($"Couldn't join match: {e.Message}");
                Application.Quit();
            }
        }

        /// <summary>
        /// This method sends match state message to other players through Nakama server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public void RpcAsync<T>(T message) where T : IMessage<T>
        {
	        try
	        {
		        var p = message as Packet;
		        if (p == null) throw new Exception("Tried to send something else than Packet");

		        // No recipients has been set, send to others by default
                // Done on Nakama side for now
		        // if (p.Recipients.Count == 0)
		        // {
			       //  for (var i = 0; i < players.Count; i++)
			       //  {
				      //   if (players[i].UserId != self.UserId) p.Recipients.Add(players[i].UserId);
			       //  }
		        // }

		        //Then server sends it to other players
		        socket.SendMatchStateAsync(matchId, 0, p.ToByteArray());
	        }
	        catch (Exception e)
            {
                Debug.Log($"Error while sending match state: {e.Message}");
            }
        }

        /// <param name="p"></param>
        private async void ReceiveMatchStateHandle(Packet p)
        {
	        // Outgoing messages can be async but absolutely everything in have to be ran on the main thread
	        // (few exceptions: pure computing functions ...)
	        await UniTask.SwitchToMainThread();
	        switch (p.TypeCase)
            {
                case Packet.TypeOneofCase.MatchJoin:
                    Debug.Log($"Received match information {p.MatchJoin}");
                    Gm.instance.seed = p.MatchJoin.Seed;
                    region = p.MatchJoin.Region;
                    break;
                case Packet.TypeOneofCase.UpdateTransform:
                    TransformUpdated?.Invoke(p.UpdateTransform.Transform);
                    break;
                case Packet.TypeOneofCase.NavMeshUpdate:
	                NavMeshUpdated?.Invoke(p.NavMeshUpdate);
	                break;
                case Packet.TypeOneofCase.RequestSpawn:
	                switch (p.RequestSpawn.TypeCase)
	                {
		                case Spawn.TypeOneofCase.Any:
			                break;
		                case Spawn.TypeOneofCase.Plant:
			                PlantSpawnRequested?.Invoke(p.RequestSpawn.Plant.Transform);
			                break;
		                case Spawn.TypeOneofCase.Animal:
			                AnimalSpawnRequested?.Invoke(p.RequestSpawn.Animal.Transform);
			                break;
		                case Spawn.TypeOneofCase.None:
			                break;
		                default:
			                throw new ArgumentOutOfRangeException();
	                }
	                break;
                case Packet.TypeOneofCase.RequestDestroy:
	                switch (p.RequestDestroy.TypeCase)
	                {
		                case Realtime.Destroy.TypeOneofCase.Any:
			                break;
		                case Realtime.Destroy.TypeOneofCase.Plant:
			                PlantDestroyRequested?.Invoke(p.RequestDestroy.Plant.Transform);
			                break;
		                case Realtime.Destroy.TypeOneofCase.Animal:
			                AnimalDestroyRequested?.Invoke(p.RequestDestroy.Animal.Transform);
			                break;
		                case Realtime.Destroy.TypeOneofCase.None:
			                break;
		                default:
			                throw new ArgumentOutOfRangeException();
	                }
	                break;
                case Packet.TypeOneofCase.Spawn:
	                switch (p.Spawn.TypeCase)
	                {
		                case Spawn.TypeOneofCase.Any:
			                break;
		                case Spawn.TypeOneofCase.Plant:
			                PlantSpawned?.Invoke(p.Spawn.Plant.Transform);
			                break;
		                case Spawn.TypeOneofCase.Animal:
			                AnimalSpawned?.Invoke(p.Spawn.Animal.Transform);
			                break;
		                case Spawn.TypeOneofCase.None:
			                break;
		                default:
			                throw new ArgumentOutOfRangeException();
	                }
	                break;
                case Packet.TypeOneofCase.Destroy:
	                switch (p.Destroy.TypeCase)
	                {
		                case Realtime.Destroy.TypeOneofCase.Any:
			                break;
		                case Realtime.Destroy.TypeOneofCase.Animal:
			                AnimalDestroyed?.Invoke(p.Destroy.Animal.Transform);
			                break;
		                case Realtime.Destroy.TypeOneofCase.Plant:
			                PlantDestroyed?.Invoke(p.Destroy.Plant.Transform);
			                break;
		                case Realtime.Destroy.TypeOneofCase.None:
			                break;
		                default:
			                throw new ArgumentOutOfRangeException();
	                }
	                break;
                case Packet.TypeOneofCase.Meme:
	                MemeUpdated?.Invoke(p.Meme);
	                break;
                case Packet.TypeOneofCase.Initialized:
	                Initialized?.Invoke("");
	                break;
                case Packet.TypeOneofCase.None:
	                break;
                default:
	                throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Method fired when any user leaves or joins the match
        /// </summary>
        /// <param name="e"></param>
        private void OnMatchPresence(IMatchPresenceEvent e)
        {
	        foreach (var user in e.Joins)
            {
	            if (Sm.instance.session.UserId == user.UserId) self = user; // Set myself
	            // If user is not already in the list
	            if (players.FindIndex(x => x.UserId == user.UserId) == -1)
                {
                    players.Add(user);
                    Debug.Log($"User {user.UserId} joined match {e.MatchId}");
                }
	            else
	            {
		            // User is already present in the game
		            // Two devices use the same account, this is not allowed
		            Debug.LogError("Two devices uses the same account, this is not allowed");
		            // TODO: kick him ?
	            }
            }

            foreach (var user in e.Leaves)
            {
	            if (players.FindIndex(x => x.UserId == user.UserId) != -1)
	            {
		            Debug.Log($"User {user.UserId} left match");
		            players.Remove(user);
	            }
	            else
	            {
		            // User is already present in the game
		            // Two devices use the same account, this is not allowed
		            Debug.LogError($"New user {user.UserId} tried to leave the game ? WTF ?");
	            }
            }
        }

        private void ReceiveMatchStateMessage(IMatchState matchState) => ReceiveMatchStateMessage(matchState.State);

        /// <summary>
        /// Decodes match state message json from byte form of matchState.State and then sends it to ReceiveMatchStateHandle
        /// for further reading and handling
        /// </summary>
        /// <param name="matchState"></param>
        private void ReceiveMatchStateMessage(byte[] matchState)
        {
            var message= Packet.Parser.ParseFrom(matchState);

            if (message == null)
            {
                return;
            }

            ReceiveMatchStateHandle(message);
        }

        /// <summary>
        /// See <a href="https://heroiclabs.com/docs/advanced-streams/#built-in-streams">Nakama docs</a>
        /// </summary>
        /// <param name="state"></param>
        private void OnReceivedStreamState(IStreamState state)
        {
	        try
	        {
		        // switch (state.Sender.UserId) // TODO: case server, case player ...
		        // {
			       //  case "":
				      //   SessionManager.instance.sessio
		        // }
		        // state.Stream.
		        switch (state.Stream.Mode)
		        {
			        case 0: // Notifications
			        case 1: // Status
			        case 2: // Chat Channel
			        case 3: // Group Chat
			        case 4: // Direct Message
			        case 5: // Relayed Match
				        throw new NotImplementedException();
			        case 6: // Authoritative Match
				        ReceiveMatchStateMessage(Encoding.UTF8.GetBytes(state.State));
				        break;
		        }
	        }
	        catch (Exception ex)
	        {
		        Debug.LogError($"Server sent incorrect message through stream {ex}");
	        }
        }

        #endregion
    }

}
