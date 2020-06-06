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
                    m_Client = new Client("http",ipAddress, port, "defaultkey",  UnityWebRequestAdapter.Instance);
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
        public event Action OnConnectionSuccess = delegate { Debug.Log(">> Connection Success"); };

        /// <summary>
        /// Invoked whenever client first authorizes using DeviceId.
        /// </summary>
        public event Action OnNewAccountCreated = delegate { Debug.Log(">> New Account Created"); };

        /// <summary>
        /// Invoked upon DeviceId authorisation failure.
        /// </summary>
        public event Action OnConnectionFailure = delegate { Debug.Log(">> Connection Error"); };

        /// <summary>
        /// Invoked after <see cref="Disconnect"/> is called.
        /// </summary>
        public event Action OnDisconnected = delegate { Debug.Log(">> Disconnected"); };

        #endregion

        #region Mono

        /// <summary>
        /// Creates new <see cref="Nakama.Client"/> object used to communicate with Nakama server.
        /// Authenticates this device using its <see cref="SystemInfo.deviceUniqueIdentifier"/>.
        /// </summary>
        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            if (erasePlayerPrefsOnStart == true)
            {
                PlayerPrefs.SetString("nakama.authToken", "");
                PlayerPrefs.SetString("nakama.deviceId", "");
            }

            GetDeviceId();
            //await ConnectAsync();
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
        /// Invokes <see cref="OnConnectionSuccess"/> or <see cref="OnConnectionFailure"/>.
        /// </summary>
        /// <returns></returns>
        public async Task<AuthenticationResponse> ConnectAsync()
        {
            AuthenticationResponse response = await RestoreTokenAsync();
            switch (response)
            {
                case AuthenticationResponse.Authenticated:
                    OnConnectionSuccess?.Invoke();
                    break;
                case AuthenticationResponse.NewAccountCreated:
                    OnNewAccountCreated?.Invoke();
                    OnConnectionSuccess?.Invoke();
                    break;
                case AuthenticationResponse.Error:
                    OnConnectionFailure?.Invoke();
                    break;
                default:
	                Debug.Log($"Unhandled response received: {response}");
                    break;
            }
            return response;
        }

        /// <summary>
        /// Restores saved Session Authentication Token if user has already authenticated with the server in the past.
        /// If it's the first time authenticating using this device id, a new account will be created.
        /// </summary>
        private async Task<AuthenticationResponse> RestoreTokenAsync()
        {
            // Restoring authentication token from player prefs
            string authToken = PlayerPrefs.GetString("nakama.authToken", null);
            if (string.IsNullOrWhiteSpace(authToken) == true)
            {
                // Token not found
                // Authenticating new session
                return await AuthenticateAsync();
            }
            else
            {
                // Restoring previous session
                session = Nakama.Session.Restore(authToken);
                if (session.HasExpired(DateTime.UtcNow))
                {
                    // Restored session has expired
                    // Authenticating new session
                    return await AuthenticateAsync();
                }
                else
                {
                    // Session restored
                    // Getting Account info
                    account = await GetAccountAsync();
                    if (account == null)
                    {
                        // Account not found
                        // Creating new account
                        return await AuthenticateAsync();
                    }

                    // Creating real-time communication socket
                    bool socketConnected = await ConnectSocketAsync();
                    if (socketConnected == false)
                    {
                        return AuthenticationResponse.Error;
                    }

                    Debug.Log("Session restored with token:" + session.AuthToken);
                    return AuthenticationResponse.Authenticated;
                }
            }
        }

        /// <summary>
        /// This method authenticates this device using local <see cref="m_DeviceId"/> and initializes new session
        /// with Nakama server. If it's the first time user logs in using this device, a new account will be created
        /// (calling <see cref="OnDeviceIdAccountCreated"/>). Upon successfull authentication, Account data is retrieved
        /// and real-time communication socket is connected.
        /// </summary>
        /// <returns>Returns true if every server call was successful.</returns>
        private async Task<AuthenticationResponse> AuthenticateAsync()
        {
            AuthenticationResponse response = await AuthenticateDeviceIdAsync();
            if (response == AuthenticationResponse.Error)
            {
                return AuthenticationResponse.Error;
            }

            account = await GetAccountAsync();
            if (account == null)
            {
                return AuthenticationResponse.Error;
            }

            bool socketConnected = await ConnectSocketAsync();
            if (socketConnected == false)
            {
                return AuthenticationResponse.Error;
            }

            StoreSessionToken();
            return response;
        }

        /// <summary>
        /// Authenticates a new session using DeviceId. If it's the first time authenticating using
        /// this device, new account is created.
        /// </summary>
        /// <returns>Returns true if every server call was successful.</returns>
        private async Task<AuthenticationResponse> AuthenticateDeviceIdAsync()
        {
            try
            {
                session = await client.AuthenticateDeviceAsync(m_DeviceId, null, false);
                Debug.Log("Device authenticated with token:" + session.AuthToken);
                return AuthenticationResponse.Authenticated;
            }
            catch (ApiResponseException e)
            {
                if (e.StatusCode == (long)System.Net.HttpStatusCode.NotFound)
                {
	                Debug.Log($"Couldn't find DeviceId in database, creating new user; message: {e}");
                    return await CreateAccountAsync();
                }
                else
                {
	                Debug.Log($"An error has occured reaching Nakama server; message: {e}");
                    return AuthenticationResponse.Error;
                }
            }
            catch (Exception e)
            {
	            Debug.Log($"Couldn't connect to Nakama server; message: {e}");
                return AuthenticationResponse.Error;
            }
        }

        /// <summary>
        /// Creates new account on Nakama server using local <see cref="m_DeviceId"/>.
        /// </summary>
        /// <returns>Returns true if account was successfully created.</returns>
        private async Task<AuthenticationResponse> CreateAccountAsync()
        {
            try
            {
                session = await client.AuthenticateDeviceAsync(m_DeviceId, null, true);
                return AuthenticationResponse.NewAccountCreated;
            }
            catch (Exception e)
            {
	            Debug.Log($"Couldn't create account using DeviceId; message: {e}");
                return AuthenticationResponse.Error;
            }
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
        /// Removes session and account from cache and invokes <see cref="OnDisconnected"/>.
        /// </summary>
        public void Disconnect()
        {
            if (session != null)
            {
	            session = null;
                account = null;

                Debug.Log("Disconnected from Nakama");
                OnDisconnected.Invoke();
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
                IApiAccount results = await client.GetAccountAsync(session);
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
                IApiUsers results = await client.GetUsersAsync(session, new string[] { userId }, new string[] { username });
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

        /// <summary>
        /// Async method used to update user's username and avatar url.
        /// </summary>
        public async Task<AuthenticationResponse> UpdateUserInfoAsync(string username, string avatarUrl)
        {
            try
            {
                await client.UpdateAccountAsync(session, username, null, avatarUrl);
                return AuthenticationResponse.UserInfoUpdated;
            }
            catch (ApiResponseException e)
            {
	            Debug.Log($"Couldn't update user info with code {e.StatusCode}: {e}");
                return AuthenticationResponse.Error;
            }
            catch (Exception e)
            {
	            Debug.Log($"Couldn't update user info: {e}");
                return AuthenticationResponse.Error;
            }
        }

        /// <summary>
        /// Retrieves device id from player prefs. If it's the first time running this app
        /// on this device, <see cref="m_DeviceId"/> is filled with <see cref="SystemInfo.deviceUniqueIdentifier"/>.
        /// </summary>
        private void GetDeviceId()
        {
            if (string.IsNullOrEmpty(m_DeviceId) == true)
            {
                m_DeviceId = PlayerPrefs.GetString("nakama.deviceId");
                if (string.IsNullOrWhiteSpace(m_DeviceId) == true)
                {
                    // SystemInfo.deviceUniqueIdentifier is not supported in WebGL,
                    // we generate a random one instead via System.Guid
#if UNITY_WEBGL && !UNITY_EDITOR
                    _deviceId = System.Guid.NewGuid().ToString();
#else
                    m_DeviceId = SystemInfo.deviceUniqueIdentifier;
#endif
                    PlayerPrefs.SetString("nakama.deviceId", m_DeviceId);
                }
                m_DeviceId += System.Guid.NewGuid(); // _sufix;
            }
        }

        /// <summary>
        /// Stores Nakama session authentication token in player prefs
        /// </summary>
        private void StoreSessionToken()
        {
            if (session == null)
            {
	            Debug.Log("Session is null; cannot store in player prefs");
            }
            else
            {
                PlayerPrefs.SetString("nakama.authToken", session.AuthToken);
            }
        }

        #endregion
    }

}
