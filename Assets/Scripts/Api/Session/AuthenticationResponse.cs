namespace Api.Session
{

    /// <summary>
    /// Response received after calling <see cref="Sm.AuthenticateDeviceIdAsync"/>.
    /// </summary>
    public enum AuthenticationResponse
    {
        /// <summary>
        /// Successfully authenticated using an existing account.
        /// </summary>
        Authenticated,

        /// <summary>
        /// Couldn't authenticate given device id.
        /// </summary>
        ErrorInternal,
        
        /// <summary>
        /// Username already.
        /// </summary>
        ErrorUsernameAlreadyExists,

        /// <summary>
        /// Given device id not found on the server. New account created.
        /// </summary>
        NewAccountCreated,
        UserInfoUpdated
    }
}
