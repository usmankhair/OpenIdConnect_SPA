using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace client.reactjs.OAuth
{
    /// <summary>
    /// Uesr info coming from Trust Builder 
    /// </summary>
    public class BaseUserInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("given_name")]
        public string GivenName { get; set; }

        [JsonProperty("family_name")]
        public string FamilyName { get; set; }

    }
}
