using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using JetBrains.Annotations;
using Nakama;
using Nakama.TinyJson;
using Net.Realtime;
using Net.Session;
using UnityEngine;
using Utils;
using static Net.Realtime.Packet.Types.SpawnPacket.Types;
using Random = UnityEngine.Random;

namespace Net.Match
{

	public enum Recipient
	{
		All,
		Others,
		Self
	}

    /// <summary>
    /// Role of this manager is sending match information to other players
    /// and receiving match messages from them through Nakama Server.
    /// Should be kept as single responsibility without mixing with logic / gameplay.
    /// </summary>
    public class MatchCommunicationManager : Singleton<MatchCommunicationManager>
    {
	    //This region contains events for all type of match messages that could be send in the game.
        //Events are fired after getting message sent by other players from Nakama server
        #region PUBLIC EVENTS

        // Match states
        public event Action<string> Initialized; // Client's game play handlers initialized, calling for state sync
        // TODO: maybe will require other states ?

        // General objects
        public event Action<Realtime.Transform> TransformUpdated;

        // Evolution
        public event Action<Realtime.Transform> AnimalSpawned;
        public event Action<Realtime.Transform> TreeSpawned;
        public event Action<Packet.Types.UpdateMeme> MemeUpdated;
        //
        // //SPELLS
        // public event Action<MatchMessageSpellActivated> OnSpellActivated;
        //
        // //CARDS
        // public event Action<MatchMessageCardPlayRequest> OnCardRequested;
        // public event Action<MatchMessageCardPlayed> OnCardPlayed;
        // public event Action<MatchMessageCardCanceled> OnCardCancelled;
        // public event Action<MatchMessageStartingHand> OnStartingHandReceived;


        #endregion

        #region PROPERTIES

        /// <summary>
        /// List of IUserPresence of all players
        /// </summary>
        public List<IUserPresence> players { get; private set; }

        /// <summary>
        /// Id of current match
        /// </summary>
        public string matchId
        {
            get;
            private set;
        }

        /// <summary>
        /// Current socket which connects client to Nakama server. Through this socket are sent match messages.
        /// </summary>
        private ISocket socket => SessionManager.instance.socket;

        /// <summary>
        /// Behaviours are dependent on randomness, we want it deterministic across server and clients
        /// so we can avoid to sync many things
        /// </summary>
        public int seed
        {
	        get;
	        private set;
        } = -1;

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
	        var payload = new Dictionary<string, string>();
	        var payloadJson = payload.ToJson();
	        // Calling an rpc method which returns our reward
	        var response = await SessionManager.instance.client
		        .RpcAsync(SessionManager.instance.session, "list_matches", payloadJson);
	        var result = response.Payload.FromJson<Dictionary<string, string[]>>();
	        return result.ContainsKey("matches") ? result["matches"] : new string[]{};
        }


        /// <summary>
        /// Joins given match or create it if the given id is null
        /// </summary>
        /// <param name="id"></param>
        public async Task JoinMatchAsync([CanBeNull] string id = null)
        {
	        //Filling list of match participants
            players = new List<IUserPresence>();

            try
            {
                // Listen to incoming match messages and user connection changes
                socket.ReceivedMatchPresence += OnMatchPresence;
                socket.ReceivedMatchState += ReceiveMatchStateMessage;
                socket.ReceivedStreamState += OnReceivedStreamState;
                IMatch match;
                if (id == null)
                {
	                match = await socket.CreateMatchAsync();
	                // var res = await socket.RpcAsync( "create_match", "");

	                Debug.Log($"Created match with id: {match.Id}");
	                seed = 1995; // Best generation of hosts
                }
                else
                {
	                // Join the match
	                match = await socket.JoinMatchAsync(id);
	                Debug.Log($"Joined match with id: {match.Id}; presences count: {match.Presences.Count()}");
                }

                // Set current match id
                // It will be used to leave the match later
                matchId = match.Id;
            }
            catch (Exception e)
            {
                Debug.Log($"Couldn't join match: {e.Message}");
            }
        }

        /// <summary>
        /// This method sends match state message to other players through Nakama server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="recipient">Who should be notified</param>
        /// <param name="recipients">Who should be notified</param>
        public void Rpc<T>(T message, Recipient recipient = Recipient.Others, IUserPresence[] recipients = null)
            where T : IMessage<T> // TODO: should it be possible to have a single client recipient?
        {
	        if (recipient == Recipient.Self && recipients != null)
	        {
		        Debug.LogError($"It's pointless to give a list of recipients while sending only to self");
	        }

	        // Handle self message
	        if (recipient == Recipient.All)
	        {
		        ReceiveMatchStateHandle(message as Packet);
	        }
	        else if (recipient == Recipient.Self)
	        {
		        ReceiveMatchStateHandle(message as Packet);
		        return;
	        }
            try
            {
	            var m = message.ToByteArray();
                //Then server sends it to other players
                socket.SendMatchStateAsync(matchId, 0, m, recipients);
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

	        // MainThreadDispatcher.instance.Enqueue(() =>
	        // {
		        switch (p.TypeCase)
	            {
	                case Packet.TypeOneofCase.UpdateTransform:
	                    TransformUpdated?.Invoke(p.UpdateTransform.ObjectTransform);
	                    break;
	                case Packet.TypeOneofCase.Destroy:
		                // ?.Invoke(p.UpdateRotation);
		                break;
	                case Packet.TypeOneofCase.Spawn:
		                switch (p.Spawn.TypeCase)
		                {
			                case Packet.Types.SpawnPacket.TypeOneofCase.Any:
				                break;
			                case Packet.Types.SpawnPacket.TypeOneofCase.Tree:
				                TreeSpawned?.Invoke(p.Spawn.Tree.ObjectTransform);
				                break;
			                case Packet.Types.SpawnPacket.TypeOneofCase.Animal:
				                AnimalSpawned?.Invoke(p.Spawn.Animal.ObjectTransform);
				                break;
			                case Packet.Types.SpawnPacket.TypeOneofCase.None:
				                break;
			                default:
				                throw new ArgumentOutOfRangeException();
		                }

		                break;
	                case Packet.TypeOneofCase.Meme:
		                MemeUpdated?.Invoke(p.Meme);
		                break;
	                case Packet.TypeOneofCase.Initialized:
		                Initialized?.Invoke(p.SenderId);
		                break;
	                case Packet.TypeOneofCase.MatchInformation:
		                seed = p.MatchInformation.Seed;
		                break;
	                case Packet.TypeOneofCase.None:
		                break;
	                default:
		                throw new ArgumentOutOfRangeException();
	            }
	        // });
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
	            // If user is not already in the list
	            if (players.FindIndex(x => x.UserId == user.UserId) == -1)
                {
                    players.Add(user);
                    Debug.Log($"User {user.UserId} joined match {e.MatchId}");

                    // Notify the new player of the current seed for deterministic behaviours
                    if (SessionManager.instance.isServer)
                    {
	                    Rpc(new Packet
	                    {
		                    MatchInformation = new Packet.Types.MatchInformationPacket{Seed = seed}
	                    }, recipients: new []{user});
                    }
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

        /// <summary>
        /// Receives and dispatches match state message to be handled in ReceiveMatchStateMesage in main thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="matchState"></param>
        private void ReceiveMatchStateMessage(object sender, IMatchState matchState)
        {
            Debug.Log($"Receiving {matchState.ToJson()}");
            ReceiveMatchStateMessage(matchState);
        }

        /// <summary>
        /// Decodes match state message json from byte form of matchState.State and then sends it to ReceiveMatchStateHandle
        /// for further reading and handling
        /// </summary>
        /// <param name="matchState"></param>
        private void ReceiveMatchStateMessage(IMatchState matchState)
        {
            var message= Packet.Parser.ParseFrom(matchState.State);

            if (message == null)
            {
                return;
            }

            ReceiveMatchStateHandle(message);
        }

        private void OnReceivedStreamState(IStreamState state)
        {
	        try
	        {
		        var res = state.State.FromJson<Dictionary<string, int>>();
	        }
	        catch (Exception ex)
	        {
		        Debug.LogError($"Server sent incorrect json through stream");
	        }
        }

        #endregion
    }

}
