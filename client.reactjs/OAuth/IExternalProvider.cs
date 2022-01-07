using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace client.reactjs.OAuth
{
    public interface IExternalProvider
    {
        public ClaimsPrincipal ClaimsPrincipal { get; set; }
        public AuthenticationProperties AuthenticationProperties { get; set; }
        Task<UserInfoResponse> AuthenticateUser(AuthenticateUserRequest request);
        Task<UserInfoResponse> GetUserInfo(string accessToken);

    }
}
