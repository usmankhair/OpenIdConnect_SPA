using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace client.reactjs.OAuth
{
    public class ExternalProvider : IExternalProvider
    {
        private readonly string _authority;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _allowedScope;
        private readonly string _codeVerifier;
        private readonly string _redirectUrl;
        public ClaimsPrincipal ClaimsPrincipal { get; set; }
        public AuthenticationProperties AuthenticationProperties { get; set; }

        public ExternalProvider(IConfiguration configuration)
        {
            _authority = configuration["Oidc:Authority"];
            _clientId = configuration["Oidc:ClientId"];
            _clientSecret = configuration["Oidc:ClientSecret"];
            _allowedScope = configuration["Oidc:AllowedScope"];
            _codeVerifier = configuration["Oidc:CodeVerifier"];
            _redirectUrl = configuration["Oidc:RedirectUrl"];
        }

        public async Task<UserInfoResponse> AuthenticateUser(AuthenticateUserRequest request)
        {

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //Authorize user from trust builder
            var tokenInfo = await GetAuthenticationToken(request);

            //Get user info from trust builder
            var userInfo = await GetUserInfo(tokenInfo.AccessToken);

            // setup claims for signin current user on the server
            ClaimsPrincipal = await GetClaimsPrinciple(userInfo, tokenInfo.AccessToken);
            AuthenticationProperties = await GetAuthenticationProperties(tokenInfo);

            return userInfo;
        }

        public async Task<UserInfoResponse> GetUserInfo(string accessToken)
        {
            // Get the user info to populate the claims
            var getHeader = new Dictionary<string, string>()
            {
                { "Authorization",$"Bearer {accessToken}"}
            };

            return await ClientHttpHandler.GetAsync<UserInfoResponse>(string.Format(ExternalProviderEndPoint.GetUserInfo, _authority), getHeader);
        }

        private async Task<TokenInfo> GetAuthenticationToken(AuthenticateUserRequest request)
        {
            var clientRequest = new Dictionary<string, string>()
            {
                { "client_id", _clientId},
                { "client_secret",_clientSecret},
                { "code",request.Code},
                { "grant_type","authorization_code"},
                { "redirect_uri",$"{request.RequestedHost}/{_redirectUrl}"},
                { "code_verifier",_codeVerifier},
                { "scope",string.Join(' ',_allowedScope.Split(','))}
            };
            var postHeader = new Dictionary<string, string>()
            {

            };

            return await ClientHttpHandler.PostAsync<TokenInfo>(string.Format(ExternalProviderEndPoint.GetToken, _authority), clientRequest, postHeader);
        }

        private async Task<AuthenticationProperties> GetAuthenticationProperties(TokenInfo tokenInfo)
        {
            var data = new Dictionary<string, string>()
            {
                { ".Token.access_token",tokenInfo.AccessToken},
                { ".Token.refresh_token",tokenInfo.RefreshToken}
            };
            return new AuthenticationProperties(data);
        }

        private async Task<ClaimsPrincipal> GetClaimsPrinciple(BaseUserInfo userInfo, string accessToken)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(nameof(userInfo.Name), userInfo.Name));
            claims.Add(new Claim(nameof(userInfo.FamilyName), userInfo.FamilyName));
            claims.Add(new Claim(nameof(userInfo.GivenName), userInfo.GivenName));
            var claimsIdentity = new ClaimsIdentity(claims, OpenIdConnectDefaults.AuthenticationScheme);
            claimsIdentity.BootstrapContext = accessToken;
            return new ClaimsPrincipal(claimsIdentity);
        }
    }
}
