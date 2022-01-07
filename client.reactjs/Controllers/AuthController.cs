using client.reactjs.OAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace client.reactjs.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IExternalProvider _externalProvider;
        public AuthController(IExternalProvider externalProvider)
        {
            _externalProvider = externalProvider;
        }

        #region final end point for authentication

        [HttpGet]
        [Route("external/login")]
        public async Task<ActionResult> Login(string returnUrl = "/")
        {
            return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = returnUrl,
                IsPersistent = true,
                AllowRefresh = true
            });
        }

        [HttpGet("external/authenticate")]
        public async Task<BaseUserInfo> AuthenticateUser(string code )
        {
            var request = new AuthenticateUserRequest() { Code = code }; // TODO :: Remove this line after GET to POST and request will be (AuthenticateUserRequest request)

            request.RequestedHost = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}";

            var userInfo = await _externalProvider.AuthenticateUser(request);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                _externalProvider.ClaimsPrincipal,
                _externalProvider.AuthenticationProperties
                );
            return userInfo;
        }

        [Authorize]
        [Route("external/getuser")]
        public async Task<UserInfoResponse> GetUserInfo()
        {
            if (!User.Identity.IsAuthenticated)
                return null;

            var accessToken = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "access_token");
            return await _externalProvider.GetUserInfo(accessToken);

        }

        [Authorize]
        [Route("external/logout")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return new SignOutResult(new[] { OpenIdConnectDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme }, new AuthenticationProperties()
            {
                RedirectUri = $"/"
            });
        }

        #endregion

        #region Other Code Commented

        //[HttpGet("external/getauthenticate")]
        //public async Task<TokenInfo> AuthenticateUser_Get(string authCode)
        //{
        //    string token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjIzRTMxMUQ5NTgzRkVDMzM3NzMwMDk2MkNCQTVCMzI1IiwidHlwIjoiYXQrand0In0.eyJpc3MiOiJodHRwczovL2RlbW8uZHVlbmRlc29mdHdhcmUuY29tIiwibmJmIjoxNjQxMjk5NjkxLCJpYXQiOjE2NDEyOTk2OTEsImV4cCI6MTY0MTMwMzI5MSwiYXVkIjoiYXBpIiwic2NvcGUiOlsib3BlbmlkIiwicHJvZmlsZSIsImFwaSIsIm9mZmxpbmVfYWNjZXNzIl0sImFtciI6WyJwd2QiXSwiY2xpZW50X2lkIjoiaW50ZXJhY3RpdmUuY29uZmlkZW50aWFsIiwic3ViIjoiMiIsImF1dGhfdGltZSI6MTY0MTI5OTY4MiwiaWRwIjoibG9jYWwiLCJuYW1lIjoiQm9iIFNtaXRoIiwiZW1haWwiOiJCb2JTbWl0aEBlbWFpbC5jb20iLCJzaWQiOiIxNDJEMkYwOTAyRjZENkMyMTAxQzBFRDk1QzI0OUY0QiIsImp0aSI6Ijc4NzUyMTI1MTZBNTU1N0M5N0JFOTExMjZDRTcxMEEwIn0.ahreO3GvXKsbss7Pt1iQtDez9O2qPx6_FtVcKaHTx5Klv86q83hMHCFyib01JEzNj8SQeiK166N2j90cY8VgXmhfLtRrpnL4sWfGasNULIfp5EJlRMUZl0CCxSQdO2ypZbWSaVWnRpTCMQJUzPLkA5OxUmkeBz70PEgwiinHE83KyRTSUFFfQW1ozgiZOUuyFcOZZ8O1gwiLa5bWsjZHLYwUfZI9u02rxaUD7pKBa9y7YDUcKYBERpg1ZWMRmuBYgKQzHRdafBYA9zBBw4OsavZosUPfEl7fi7Ewp1s5Lz0088Zx7HyTGt99E8RxBj5aBrMVONpaJNxvYkXSvw3tyQ";

        //    // Send the post request on /connect/token endpoint

        //    var endPoint = "https://demo.duendesoftware.com/connect/token";
        //    Dictionary<string, string> request = new Dictionary<string, string>()
        //    {
        //        { "client_id","interactive.confidential"},
        //        { "client_secret","secret"},
        //        { "code",authCode},
        //        { "grant_type","authorization_code"},
        //        { "redirect_uri","https://127.0.0.1:4200/client/external/login"},
        //        { "code_verifier","v6QtcIXo2gUk1jxphC26fUjQxChi3vOBzmuyYCQsrZc"},
        //        { "scope","openid profile api offline_access"}
        //    };
        //    var header = new Dictionary<string, string>()
        //    {

        //    };

        //    var tokenInfo = await ClientHttpHandler.PostAsync<TokenInfo>(endPoint, request, header);
        //    var claims = new[]
        //    {
        //        new Claim("name", "bob"),
        //        new Claim("given_name", "bob"),
        //        new Claim("family_name", "bob"),
        //        new Claim("website", "bob"),
        //        new Claim("sub", "2")
        //    };
        //    var claimsIdentity = new ClaimsIdentity(claims, OpenIdConnectDefaults.AuthenticationScheme);

        //    claimsIdentity.BootstrapContext = tokenInfo.AccessToken;


        //    var principal = new ClaimsPrincipal(claimsIdentity);



        //    var properties = new AuthenticationProperties()
        //    {
        //        AllowRefresh = true,
        //        IsPersistent = true,


        //    };



        //    //do something the the claimsPrincipal, possibly create a new one with additional information
        //    //create a local user, etc
        //    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
        //    var authenticate = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        //    return tokenInfo;
        //}

        //[HttpGet("oidc/callback")]
        //public async Task<RedirectResult> CallBack()
        //{

        //    var authenticate = await HttpContext.AuthenticateAsync("Bearer");
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var accessToken = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "access_token");
        //        var ticketInfo = authenticate.Ticket.Properties.Items;
        //        var claims = ((ClaimsIdentity)this.User.Identity).Claims.Select(c =>
        //                        new { type = c.Type, value = c.Value })
        //                        .ToArray();
        //        HttpContext.Response.Cookies.Append("__Authorization", "Bearer " + accessToken, new CookieOptions { IsEssential = true });

        //        HttpContext.Response.Headers.Add("__Authorization1", accessToken);

        //        //foreach (var item in ticketInfo)
        //        //{
        //        //    HttpContext.Response.Cookies.Append(item.Key, item.Value, new CookieOptions { IsEssential = true });
        //        //}
        //    }

        //    //var authenticate = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //    if (authenticate.Succeeded)
        //        return Redirect("https://127.0.0.1:4200/callback?code=123");

        //    return Redirect("https://127.0.0.1:4200/login");
        //}

        //[HttpPost]
        //[Route("oidc/getcode")]
        //public async Task<string> CallBack(string authorizationCode, string scope, string state, string session_state, string iss)
        //{
        //    string clientID = "interactive.confidential";
        //    string clientSecret = "secret";
        //    return JsonConvert.SerializeObject(new { Code = authorizationCode });
        //}

        //[Authorize]
        //[Route("GetUserInfo_old")]
        //public async Task<UserInfoResponse> GetUserInfo_old()
        //{
        //    if (!User.Identity.IsAuthenticated)
        //        return null;

        //    var accessToken = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "access_token");
        //    return await _externalProvider.GetUserInfo(accessToken);


        //    //if (User.Identity.IsAuthenticated)
        //    //{
        //    //    var accessToken = await HttpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "access_token");

        //    //    var authenticate = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //    //    var ticketInfo = authenticate.Ticket.Properties.Items;
        //    //    var claims = ((ClaimsIdentity)this.User.Identity).Claims.Select(c =>
        //    //                    new { type = c.Type, value = c.Value })
        //    //                    .ToArray();
        //    //    return JsonConvert.SerializeObject(
        //    //        new
        //    //        {
        //    //            isAuthenticated = true,
        //    //            Token = accessToken,
        //    //            claims = claims,
        //    //            IssuedUtc = authenticate.Ticket.Properties.IssuedUtc,
        //    //            ExpiresUtc = authenticate.Ticket.Properties.ExpiresUtc,
        //    //            IsPersistent = authenticate.Ticket.Properties.IsPersistent,
        //    //            AllowRefresh = authenticate.Ticket.Properties.AllowRefresh
        //    //        });
        //    //}
        //    //return JsonConvert.SerializeObject(new { isAuthenticated = false });
        //}

        //[HttpGet("oidc/authenticate")]
        //public async Task<bool> AuthenticateUser(string authCode)
        //{
        //    string token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjIzRTMxMUQ5NTgzRkVDMzM3NzMwMDk2MkNCQTVCMzI1IiwidHlwIjoiYXQrand0In0.eyJpc3MiOiJodHRwczovL2RlbW8uZHVlbmRlc29mdHdhcmUuY29tIiwibmJmIjoxNjQxMjk5NjkxLCJpYXQiOjE2NDEyOTk2OTEsImV4cCI6MTY0MTMwMzI5MSwiYXVkIjoiYXBpIiwic2NvcGUiOlsib3BlbmlkIiwicHJvZmlsZSIsImFwaSIsIm9mZmxpbmVfYWNjZXNzIl0sImFtciI6WyJwd2QiXSwiY2xpZW50X2lkIjoiaW50ZXJhY3RpdmUuY29uZmlkZW50aWFsIiwic3ViIjoiMiIsImF1dGhfdGltZSI6MTY0MTI5OTY4MiwiaWRwIjoibG9jYWwiLCJuYW1lIjoiQm9iIFNtaXRoIiwiZW1haWwiOiJCb2JTbWl0aEBlbWFpbC5jb20iLCJzaWQiOiIxNDJEMkYwOTAyRjZENkMyMTAxQzBFRDk1QzI0OUY0QiIsImp0aSI6Ijc4NzUyMTI1MTZBNTU1N0M5N0JFOTExMjZDRTcxMEEwIn0.ahreO3GvXKsbss7Pt1iQtDez9O2qPx6_FtVcKaHTx5Klv86q83hMHCFyib01JEzNj8SQeiK166N2j90cY8VgXmhfLtRrpnL4sWfGasNULIfp5EJlRMUZl0CCxSQdO2ypZbWSaVWnRpTCMQJUzPLkA5OxUmkeBz70PEgwiinHE83KyRTSUFFfQW1ozgiZOUuyFcOZZ8O1gwiLa5bWsjZHLYwUfZI9u02rxaUD7pKBa9y7YDUcKYBERpg1ZWMRmuBYgKQzHRdafBYA9zBBw4OsavZosUPfEl7fi7Ewp1s5Lz0088Zx7HyTGt99E8RxBj5aBrMVONpaJNxvYkXSvw3tyQ";

        //    var claims = new[]
        //    {
        //        new Claim("name", "bob"),
        //        new Claim("given_name", "bob"),
        //        new Claim("family_name", "bob"),
        //        new Claim("website", "bob"),
        //        new Claim("sub", "2")

        //    };
        //    var claimsIdentity = new ClaimsIdentity(claims, OpenIdConnectDefaults.AuthenticationScheme);
        //    var principal = new ClaimsPrincipal(claimsIdentity);
        //    //var properties = new AuthenticationProperties()
        //    //{
        //    //    AllowRefresh =true,
        //    //};


        //    //do something the the claimsPrincipal, possibly create a new one with additional information
        //    //create a local user, etc

        //    await HttpContext.SignInAsync(OpenIdConnectDefaults.AuthenticationScheme, principal);
        //    var authenticate = await HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);
        //    return authenticate.Succeeded;
        //}

        #endregion
    }
}
