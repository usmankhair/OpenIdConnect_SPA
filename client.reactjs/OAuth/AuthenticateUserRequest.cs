namespace client.reactjs.OAuth
{
    using Newtonsoft.Json;

    /// <summary>
    /// Authenticate user from the external provider
    /// </summary>
    public class AuthenticateUserRequest
    {
        /// <summary>
        /// Authentication Code
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// Scope of the api, setup in the startup "openid profile api offline_access"
        /// </summary>
        [JsonProperty("scope")]
        public string Scope { get; set; }

        /// <summary>
        /// state of the request
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }

        /// <summary>
        /// session state of the request
        /// </summary>
        [JsonProperty("session_state")]
        public string SessionState { get; set; }

        /// <summary>
        /// requested authority 
        /// </summary>
        [JsonProperty("iss")]
        public string Iss { get; set; }

        /// <summary>
        /// Requested host 
        /// </summary>
        public string RequestedHost { get; set; }

    }
}
