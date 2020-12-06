using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nakama;
using Nakama.TinyJson;
using UnityEngine;
using Utils;

namespace Api.Session
{

    /// <summary>
    /// Manages Nakama server interaction and user session throughout the game.
    /// </summary>
    /// <remarks>
    /// Whenever a user tries to communicate with game server it ensures that their session hasn't expired. If the
    /// session is expired the user will have to reauthenticate the session and obtain a new session.
    /// </remarks>
    public class Sm : Singleton<Sm>
    {
        #region Variables

        public bool debug;

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
        private string _deviceId;

        /// <summary>
        /// Used to establish connection between the client and the server.
        /// Contains a list of useful methods required to communicate with Nakama server.
        /// Do not use this directly, use <see cref="Client"/> instead.
        /// </summary>
        private Client _client;

        /// <summary>
        /// Socket responsible for maintaining connection with Nakama server and exchanger realtime messages.
        /// Do not use this directly, use <see cref="Socket"/> instead.
        /// </summary>
        private ISocket _socket;

        #endregion

        #region Properties

        /// <summary>
        /// Used to communicate with Nakama server.
        /// For the user to send and receive messages from the server, <see cref="Session"/> must not be expired.
        /// Default expiration time is 60s, but for this demo we set it to 3 weeks (1 814 400 seconds).
        /// </summary>
        public ISession Session { get; private set; }

        /// <summary>
        /// Contains all the identifying data of a <see cref="Client"/>, like User Id, linked Device IDs,
        /// username, etc.
        /// </summary>
        public IApiAccount Account { get; private set; }

        /// <summary>
        /// Used to establish connection between the client and the server.
        /// Contains a list of useful methods required to communicate with Nakama server.
        /// </summary>
        public Client Client
        {
            get
            {
                if (_client == null || _client.Host != ipAddress || _client.Port != port) // Not created or host/port changed
                {
                    // "defaultkey" should be changed when releasing the app
                    // see https://heroiclabs.com/docs/install-configuration/#socket
                    // for logger see https://heroiclabs.com/docs/unity-client-guide/#logs-and-errors
                    _client = new Client("http",ipAddress, port, "defaultkey",  UnityWebRequestAdapter.Instance)
                    {
#if UNITY_EDITOR
	                    Logger = new UnityLogger()
#endif
                    };
                }
                return _client;
            }
        }

        /// <summary>
        /// Socket responsible for maintaining connection with Nakama server and exchange realtime messages.
        /// </summary>
        public ISocket Socket => _socket ?? (_socket = _client.NewSocket());

        /// <summary>
        /// Returns true if <see cref="Session"/> between this device and Nakama server exists.
        /// </summary>
        public bool IsConnected => Session != null && !Session.HasExpired(DateTime.UtcNow);

        /// <summary>
        /// Is it server ?
        /// </summary>
        public bool isServer;

        #endregion
        
        #region Debug

        [Header("Debug")]
        // If true, stored session authentication token and device id will be erased on start
        [SerializeField] private bool erasePlayerPrefsOnStart = false;

        /// <summary>
        /// Sufix added to <see cref="_deviceId"/> to generate new device id.
        /// </summary>
        [SerializeField] private string suffix = string.Empty;

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
        private async void Start()
        {
            
            if (erasePlayerPrefsOnStart)
            {
                PlayerPrefs.SetString("nakama.authToken", "");
                PlayerPrefs.SetString("nakama.deviceId", "");
            }

            GetDeviceId();
            // await ConnectAsync();
        }

        /// <summary>
        /// Closes Nakama session.
        /// </summary>
        protected override void OnDestroy()
        {
            Disconnect();
        }

        private void OnApplicationQuit()
        {
            // TODO: get last log and push somewhere
        }

        #endregion

        #region Authentication

        /// <summary>
        /// Restores session or tries to establish a new one.
        /// Invokes <see cref="ConnectionSucceed"/> or <see cref="ConnectionFailed"/>.
        /// </summary>
        /// <returns></returns>
        public async Task<AuthenticationResponse> ConnectAsync()
        {
            var response = await RestoreTokenAsync();
            switch (response)
            {
                case AuthenticationResponse.Authenticated:
                    ConnectionSucceed?.Invoke();
                    break;
                case AuthenticationResponse.NewAccountCreated:
                    NewAccountCreated?.Invoke();
                    ConnectionSucceed?.Invoke();
                    break;
                case AuthenticationResponse.Error:
                    ConnectionFailed?.Invoke();
                    break;
                case AuthenticationResponse.UserInfoUpdated:
                    break;
                default:
                    Debug.LogError("Unhandled response received: " + response);
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
            var authToken = PlayerPrefs.GetString("nakama.authToken", null);
            if (string.IsNullOrWhiteSpace(authToken))
            {
                // Token not found
                // Authenticating new session
                return await AuthenticateAsync();
            }

            // Restoring previous session
            Session = Nakama.Session.Restore(authToken);
            if (Session.HasExpired(DateTime.UtcNow))
            {
                // Restored session has expired
                // Authenticating new session
                return await AuthenticateAsync();
            }

            // Session restored
            // Getting Account info
            Account = await GetAccountAsync();
            if (Account == null)
            {
                // Account not found
                // Creating new account
                return await AuthenticateAsync();
            }

            // Creating real-time communication socket
            var socketConnected = await ConnectSocketAsync();
            if (socketConnected == false)
            {
                return AuthenticationResponse.Error;
            }

            Debug.Log("Session restored with token:" + Session.AuthToken);
            return AuthenticationResponse.Authenticated;
        }

        /// <summary>
        /// This method authenticates this device using local <see cref="_deviceId"/> and initializes new session
        /// with Nakama server. If it's the first time user logs in using this device, a new account will be created
        /// Upon successful authentication, Account data is retrieved
        /// and real-time communication socket is connected.
        /// </summary>
        /// <returns>Returns true if every server call was successful.</returns>
        private async Task<AuthenticationResponse> AuthenticateAsync()
        {
            var response = await AuthenticateDeviceIdAsync();
            if (response == AuthenticationResponse.Error)
            {
                return AuthenticationResponse.Error;
            }

            Account = await GetAccountAsync();
            if (Account == null)
            {
                return AuthenticationResponse.Error;
            }

            var socketConnected = await ConnectSocketAsync();
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
                Session = await Client.AuthenticateDeviceAsync(_deviceId, null, false);
                Debug.Log("Device authenticated with token:" + Session.AuthToken);
                return AuthenticationResponse.Authenticated;
            }
            catch (ApiResponseException e)
            {
                if (e.StatusCode == (long)System.Net.HttpStatusCode.NotFound)
                {
                    Debug.Log("Couldn't find DeviceId in database, creating new user; message: " + e);
                    return await CreateAccountAsync();
                }

                Debug.LogWarning("An error has occured reaching Nakama server; message: " + e);
                return AuthenticationResponse.Error;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Couldn't connect to Nakama server; message: " + e);
                return AuthenticationResponse.Error;
            }
        }

        /// <summary>
        /// Creates new account on Nakama server using local <see cref="_deviceId"/>.
        /// </summary>
        /// <returns>Returns true if account was successfully created.</returns>
        private async Task<AuthenticationResponse> CreateAccountAsync()
        {
            try
            {
                Session = await Client.AuthenticateDeviceAsync(_deviceId);
                return AuthenticationResponse.NewAccountCreated;
            }
            catch (Exception e)
            {
                Debug.LogError("Couldn't create account using DeviceId; message: " + e);
                return AuthenticationResponse.Error;
            }
        }



        /// <summary>
        /// Connects <see cref="Socket"/> to Nakama server to enable real-time communication.
        /// </summary>
        /// <returns>Returns true if socket has connected successfully.</returns>
        public async Task<bool> ConnectSocketAsync()
        {
            try
            {
                if (_socket != null)
                {
                    await _socket.CloseAsync();
                }
            }
            catch (Exception e)
            {
	            Debug.Log($"Couldn't disconnect the socket: {e}");
            }

            try
            {
                await Socket.ConnectAsync(Session);
                return true;
            }
            catch (Exception e)
            {
	            Debug.Log($"An error has occured while connecting socket: {e}");
                return false;
            }
        }

        /// <summary>
        /// Removes session and account from cache and invokes <see cref="Disconnected"/>.
        /// </summary>
        public void Disconnect()
        {
            if (Session != null)
            {
	            Session = null;
                Account = null;

                Debug.Log("Disconnected from Nakama");
                Disconnected.Invoke();
            }
        }
        


        #endregion

        #region UserInfo
        
        /// <summary>
        /// Retrieves device id from player prefs. If it's the first time running this app
        /// on this device, <see cref="_deviceId"/> is filled with <see cref="SystemInfo.deviceUniqueIdentifier"/>.
        /// </summary>
        private void GetDeviceId()
        {
            if (string.IsNullOrEmpty(_deviceId) == true)
            {
                _deviceId = PlayerPrefs.GetString("nakama.deviceId");
                if (string.IsNullOrWhiteSpace(_deviceId) == true)
                {
                    // SystemInfo.deviceUniqueIdentifier is not supported in WebGL,
                    // we generate a random one instead via System.Guid
#if UNITY_WEBGL && !UNITY_EDITOR
                    _deviceId = System.Guid.NewGuid().ToString();
#else
                    _deviceId = SystemInfo.deviceUniqueIdentifier;
#endif                    
                    PlayerPrefs.SetString("nakama.deviceId", _deviceId);
                }
                _deviceId += suffix;
            }
        }

        /// <summary>
        /// Stores Nakama session authentication token in player prefs
        /// </summary>
        private void StoreSessionToken()
        {
            if (Session == null)
            {
                Debug.LogWarning("Session is null; cannot store in player prefs");
            }
            else
            {
                PlayerPrefs.SetString("nakama.authToken", Session.AuthToken);
            }
        }

        /// <summary>
        /// Receives currently logged in user's <see cref="IApiAccount"/> from server.
        /// </summary>
        public async Task<IApiAccount> GetAccountAsync()
        {
            try
            {
                var results = await Client.GetAccountAsync(Session);
                return results;
            }
            catch (Exception e)
            {
	            Debug.Log($"An error has occured while retrieving account: {e}");
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
                await Client.UpdateAccountAsync(Session, username, null, avatarUrl);
                return AuthenticationResponse.UserInfoUpdated;
            }
            catch (ApiResponseException e)
            {
                Debug.LogError("Couldn't update user info with code " + e.StatusCode + ": " + e);
                return AuthenticationResponse.Error;
            }
            catch (Exception e)
            {
                Debug.LogError("Couldn't update user info: " + e);
                return AuthenticationResponse.Error;
            }
        }

        #endregion
    }

}
