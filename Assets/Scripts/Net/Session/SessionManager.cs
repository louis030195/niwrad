using System;
using System.Linq;
using System.Threading.Tasks;
using Nakama;
using UnityEngine;
using Utils;

namespace Net.Session
{

    /// <summary>
    /// Manages Nakama server interaction and user session throughout the game.
    /// </summary>
    /// <remarks>
    /// Whenever a user tries to communicate with game server it ensures that their session hasn't expired. If the
    /// session is expired the user will have to reauthenticate the session and obtain a new session.
    /// </remarks>
    public class SessionManager : Singleton<SessionManager>
    {
        #region Variables

        /// <summary>
        /// IP Address of the server.
        /// For demonstration purposes, the value is set through Inspector.
        /// </summary>
        [SerializeField] private string ipAddress = "localhost";

        /// <summary>
        /// Port behind which Nakama server can be found.
        /// The default value is 7350
        /// For demonstration purposes, the value is set through Inspector.
        /// </summary>
        [SerializeField] private int port = 7350;

        /// <summary>
        /// Cached value of <see cref="SystemInfo.deviceUniqueIdentifier"/>.
        /// Used to authenticate this device on Nakama server.
        /// </summary>
        private string m_DeviceId;

        /// <summary>
        /// Used to establish connection between the client and the server.
        /// Contains a list of usefull methods required to communicate with Nakama server.
        /// Do not use this directly, use <see cref="client"/> instead.
        /// </summary>
        private Client m_Client;

        /// <summary>
        /// Socket responsible for maintaining connection with Nakama server and exchanger realtime messages.
        /// Do not use this directly, use <see cref="socket"/> instead.
        /// </summary>
        private ISocket m_Socket;

        #region Debug

        [Header("Debug")]
        [Tooltip("If true, stored session authentication token and device id will be erased on start"), SerializeField]
        private bool erasePlayerPrefsOnStart;

        /// <summary>
        /// Suffix added to <see cref="m_DeviceId"/> to generate new device id.
        /// </summary>
        [Tooltip("Suffix added to device id to generate new device id"), SerializeField]
        private string suffix = string.Empty;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Used to communicate with Nakama server.
        /// For the user to send and receive messages from the server, <see cref="session"/> must not be expired.
        /// Default expiration time is 60s, but for this demo we set it to 3 weeks (1 814 400 seconds).
        /// To initialize the session, call <see cref="AuthenticateDeviceIdAsync"/>.
        /// To reinitialize expired session, call <see cref="Reauthenticate"/> method.
        /// </summary>
        public ISession session { get; private set; }

        /// <summary>
        /// Contains all the identifying data of a <see cref="client"/>, like User Id, linked Device IDs,
        /// username, etc.
        /// </summary>
        public IApiAccount account { get; private set; }

        /// <summary>
        /// Used to establish connection between the client and the server.
        /// Contains a list of useful methods required to communicate with Nakama server.
        /// </summary>
        public Client client
        {
            get
            {
                if (m_Client == null)
                {
                    // "defaultkey" should be changed when releasing the app
                    // see https://heroiclabs.com/docs/install-configuration/#socket
                    // for logger see https://heroiclabs.com/docs/unity-client-guide/#logs-and-errors
                    m_Client = new Client("http",ipAddress, port, "defaultkey",  UnityWebRequestAdapter.Instance)
                    {
#if UNITY_EDITOR
	                    Logger = new UnityLogger()
#endif
                    };
                }
                return m_Client;
            }
        }

        /// <summary>
        /// Socket responsible for maintaining connection with Nakama server and exchange realtime messages.
        /// </summary>
        public ISocket socket
        {
            get
            {
                if (m_Socket == null)
                {
                    // Initializing socket
                    m_Socket = m_Client.NewSocket();
                }
                return m_Socket;
            }
        }

        /// <summary>
        /// Returns true if <see cref="session"/> between this device and Nakama server exists.
        /// </summary>
        public bool isConnected
        {
            get
            {
                if (session == null || session.HasExpired(DateTime.UtcNow) == true)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoked whenever client first authorizes using DeviceId.
        /// </summary>
        public event Action ConnectionSucceed = delegate { Debug.Log(">> Connection Success"); };

        /// <summary>
        /// Invoked whenever client first authorizes using DeviceId.
        /// </summary>
        public event Action NewAccountCreated = delegate { Debug.Log(">> New Account Created"); };

        /// <summary>
        /// Invoked upon DeviceId authorisation failure.
        /// </summary>
        public event Action ConnectionFailed = delegate { Debug.Log(">> Connection Error"); };

        /// <summary>
        /// Invoked after <see cref="Disconnect"/> is called.
        /// </summary>
        public event Action Disconnected = delegate { Debug.Log(">> Disconnected"); };

        #endregion

        #region Mono

        /// <summary>
        /// Creates new <see cref="Nakama.Client"/> object used to communicate with Nakama server.
        /// Authenticates this device using its <see cref="SystemInfo.deviceUniqueIdentifier"/>.
        /// </summary>
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Closes Nakama session.
        /// </summary>
        protected override void OnDestroy()
        {
            Disconnect();
        }

        #endregion

        #region Authentication

        public void SetIp(string ip)
        {
            if (isConnected == false)
            {
                ipAddress = ip;
            }
        }

        /// <summary>
        /// Restores session or tries to establish a new one.
        /// Invokes <see cref="ConnectionSucceed"/> or <see cref="ConnectionFailed"/>.
        /// </summary>
        /// <returns></returns>
        public async Task<(bool success, string message)> ConnectAsync(string email, string password, bool create = false)
        {
	        try
	        {
		        session = await client.AuthenticateEmailAsync(email, password, create: create);
	        }
	        catch (ApiResponseException ex)
	        {
		        ConnectionFailed?.Invoke();
		        return (false, ex.Message);
	        }

	        Debug.LogFormat("New user: {0}, {1}", session.Created, session);
            ConnectionSucceed?.Invoke();
            if (session.Created)
            {
	            NewAccountCreated?.Invoke();
            }

            return (true, session.Created ? "Account created, connecting ..." : "Connecting");
        }


        /// <summary>
        /// Connects <see cref="socket"/> to Nakama server to enable real-time communication.
        /// </summary>
        /// <returns>Returns true if socket has connected successfully.</returns>
        private async Task<bool> ConnectSocketAsync()
        {
            try
            {
                if (m_Socket != null)
                {
                    await m_Socket.CloseAsync();
                }
            }
            catch (Exception e)
            {
	            Debug.Log($"Couldn't disconnect the socket: {e}");
            }

            try
            {
                await socket.ConnectAsync(session);
                return true;
            }
            catch (Exception e)
            {
	            Debug.Log("An error has occured while connecting socket: {e}");
                return false;
            }
        }

        /// <summary>
        /// Removes session and account from cache and invokes <see cref="Disconnected"/>.
        /// </summary>
        public void Disconnect()
        {
            if (session != null)
            {
	            session = null;
                account = null;

                Debug.Log("Disconnected from Nakama");
                Disconnected.Invoke();
            }
        }

        #endregion

        #region UserInfo

        /// <summary>
        /// Receives currently logged in user's <see cref="IApiAccount"/> from server.
        /// </summary>
        public async Task<IApiAccount> GetAccountAsync()
        {
            try
            {
                var results = await client.GetAccountAsync(session);
                return results;
            }
            catch (Exception e)
            {
	            Debug.Log($"An error has occured while retrieving account: {e}");
                return null;
            }
        }

        /// <summary>
        /// Receives <see cref="IApiUser"/> info from server using user id or username.
        /// Either <paramref name="userId"/> or <paramref name="username"/> must not be null.
        /// </summary>
        public async Task<IApiUser> GetUserInfoAsync(string userId, string username)
        {
            try
            {
                var results = await client.GetUsersAsync(session, new string[] { userId }, new string[] { username });
                if (results.Users.Count() != 0)
                {
                    return results.Users.ElementAt(0);
                }
                else
                {
	                Debug.Log($"Couldn't find user with id: {userId}");
                    return null;
                }
            }
            catch (System.Exception e)
            {
	            Debug.Log($"An error has occured while retrieving user info: {e}");
                return null;
            }
        }

        #endregion
    }

}
