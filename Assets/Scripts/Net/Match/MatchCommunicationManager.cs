using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using Nakama;
using Nakama.TinyJson;
using Net.Realtime;
using Net.Session;
using UnityEngine;
using Utils;

namespace Net.Match
{

    /// <summary>
    /// Role of this manager is sending match information to other players and receiving match messages from them through Nakama Server.
    /// </summary>
    public class MatchCommunicationManager : Singleton<MatchCommunicationManager>
    {
	    //This region contains events for all type of match messages that could be send in the game.
        //Events are fired after getting message sent by other players from Nakama server
        #region PUBLIC EVENTS

        //GAME
        public event Action OnGameStarted;

        public event Action<Packet.Types.UpdatePositionPacket> PositionUpdated;
        public event Action<Packet.Types.UpdateRotationPacket> RotationUpdated;
        public event Action<Packet.Types.DestroyObjectPacket> Destroyed;

        #endregion

        #region PROPORTIES

        /// <summary>
        /// Id of current game host
        /// </summary>
        public string hostId { private set; get; }

        /// <summary>
        /// Returns true if local player is host
        /// </summary>
        public bool isHost => hostId == SessionManager.instance.session.UserId;

        /// <summary>
        /// Id of opponent of local player in current game
        /// </summary>
        public string opponentId { get; private set; }

        /// <summary>
        /// List of IUserPresence of all players
        /// </summary>
        public List<IUserPresence> players { get; private set; }

        /// <summary>
        /// Returns true if game is already started
        /// </summary>
        public bool gameStarted { get; private set; }

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

        #endregion

        #region PRIVATE FIELDS

        /// <summary>
        /// Indicates if player already joined match
        /// </summary>
        private bool m_MatchJoined;

        /// <summary>
        /// Indicates if player is already leaving match
        /// </summary>
        private bool m_IsLeaving;


        #endregion

        #region MONO

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Joins given match found by matchmaker
        /// </summary>
        /// <param name="matched"></param>
        public async void JoinMatchAsync(IMatchmakerMatched matched)
        {
            //Choosing host in deterministic way, with no need to exchange data between players
            ChooseHost(matched);

            //Filling list of match participants
            players = new List<IUserPresence>();

            try
            {
                // Listen to incoming match messages and user connection changes
                socket.ReceivedMatchPresence += OnMatchPresence;
                socket.ReceivedMatchState += ReceiveMatchStateMessage;

                // Join the match
                IMatch match = await socket.JoinMatchAsync(matched);
                // Set current match id
                // It will be used to leave the match later
                matchId = match.Id;
                Debug.Log($"Joined match with id: {match.Id}; presences count: {match.Presences.Count()}");

                // Add all players already connected to the match
                // If both players uses the same account, exit the game
                var noDuplicateUsers = AddConnectedPlayers(match);
                if (noDuplicateUsers)
                {
                    // Match joined successfully
                    // Setting gameplay
                    m_MatchJoined = true;
                    StartGame();
                }
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
        public void SendMatchStateMessage<T>(T message)
            where T : IMessage<T>
        {
            try
            {
	            var m = message.ToByteArray();
                //Then server sends it to other players
                socket.SendMatchStateAsync(matchId, message.Descriptor.Index, m);
            }
            catch (Exception e)
            {
                Debug.Log($"Error while sending match state: {e.Message}");
            }
        }


        /// <summary>
        /// This method is used by host to invoke locally event connected with match message which is sent to other players.
        /// Should be always ran on host client after sending any message, otherwise some of the game logic would not be runned on host game instance.
        /// Don't use this method when client is not a host!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public void SendMatchStateMessageSelf<T>(T message)
            where T : IMessage<T>
        {
	        // var p = Packet.Parser.ParseFrom(message);
	        // switch (p.TypeCase)
	        // {
		       //  case Packet.TypeOneofCase.UpdatePosition:
			      //   PositionUpdated?.Invoke(p.UpdatePosition);
			      //   break;
		       //  case Packet.TypeOneofCase.UpdateRotation:
			      //   RotationUpdated?.Invoke(p.UpdateRotation);
			      //   break;
		       //  case Packet.TypeOneofCase.DestroyObject:
			      //   Destroyed?.Invoke(p.DestroyObject);
			      //   break;
		       //  case Packet.TypeOneofCase.None:
			      //   break;
		       //  default:
			      //   throw new ArgumentOutOfRangeException();
	        // }
        }

        /// <param name="p"></param>
        public void ReceiveMatchStateHandle(Packet p)
        {
            switch (p.TypeCase)
            {
                case Packet.TypeOneofCase.UpdatePosition:
                    PositionUpdated?.Invoke(p.UpdatePosition);
                    break;
                case Packet.TypeOneofCase.UpdateRotation:
	                RotationUpdated?.Invoke(p.UpdateRotation);
	                break;
                case Packet.TypeOneofCase.DestroyObject:
	                Destroyed?.Invoke(p.DestroyObject);
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
                if (players.FindIndex(x => x.UserId == user.UserId) == -1)
                {
                    Debug.Log($"User {user.Username} joined match");
                    players.Add(user);
                    if (user.UserId != SessionManager.instance.session.UserId)
                    {
                        opponentId = user.UserId;
                    }
                    StartGame();
                }
            }
        }

        /// <summary>
        /// Adds all users from given match to <see cref="players"/> list.
        /// If any user is already on the list, this means there are two devices
        /// playing on the same account, which is not allowed.
        /// </summary>
        /// <returns>True if there are no duplicate user id.</returns>
        private bool AddConnectedPlayers(IMatch match)
        {
            foreach (var user in match.Presences)
            {
                // Check if user is already in the game
                if (players.FindIndex(x => x.UserId == user.UserId) == -1)
                {
                    Debug.Log($"User {user.Username} joined match");

                    // Add to player list
                    players.Add(user);

                    // Set opponent id for better access
                    if (user.UserId != SessionManager.instance.session.UserId)
                    {
                        opponentId = user.UserId;
                    }
                }
                else
                {
                    // User is already present in the game
                    // Two devices use the same account, this is not allowed
                    Debug.Log("Two devices uses the same account, this is not allowed");
                    return false;
                }
            }
            return true;
        }

        private void StartGame()
        {

        }


        /// <summary>
        /// Chooses host in deterministic way
        /// </summary>
        private void ChooseHost(IMatchmakerMatched matched)
        {
            // Add the session id of all users connected to the match
            List<string> userSessionIds = new List<string>();
            foreach (var user in matched.Users)
            {
                userSessionIds.Add(user.Presence.SessionId);
            }

            // Perform a lexicographical sort on list of user session ids
            userSessionIds.Sort();

            // First user from the sorted list will be the host of current match
            string hostSessionId = userSessionIds.First();

            // Get the user id from session id
            var hostUser = matched.Users.First(x => x.Presence.SessionId == hostSessionId);
            hostId = hostUser.Presence.UserId;
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

        #endregion
    }

}
