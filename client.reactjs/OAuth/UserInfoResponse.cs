using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace client.reactjs.OAuth
{
    /// <summary>
    /// User info response to the client incuding trust builder fields
    /// </summary>
    public class UserInfoResponse : BaseUserInfo
    {
        public bool IsAuthenticated { get; set; } = true;
    }
}
