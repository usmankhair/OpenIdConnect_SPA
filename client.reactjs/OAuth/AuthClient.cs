using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace client.reactjs.OAuth
{
    public class AuthClient
    {

        #region Important Code

        public static string Base64UrlEncodeNoPadding(byte[] buffer)
        {
            string base64 = Convert.ToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }

        #endregion

        #region Other code
        public static void Execute()
        {
            string clientID = "interactive.confidential";
            string clientSecret = "secret";

            // Generates code verifier value.
            string codeVerifier = RandomDataBase64Url(32);

            //// Creates a redirect URI using an available port on the loopback address.
            string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, GetRandomUnusedPort());
            Console.WriteLine("redirect URI: " + redirectURI);

            // Extracts the authorization code.
            var authorizationCode = GetAuthorizationCode(clientID, codeVerifier, redirectURI);

            //Obtains the access token from the authorization code.
            string accessToken = GetAccessToken(authorizationCode, clientID, clientSecret, codeVerifier, redirectURI);
        }

        public static void Execute(string code, string scope, string state, string session_state, string iss)
        {
            string clientID = "interactive.confidential";
            string clientSecret = "secret";

            // Generates code verifier value.
            string codeVerifier = RandomDataBase64Url(32);

            // Creates a redirect URI using an available port on the loopback address.
            string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, GetRandomUnusedPort());
            Console.WriteLine("redirect URI: " + redirectURI);

            // Extracts the authorization code.
            var authorizationCode = GetAuthorizationCode(clientID, codeVerifier, redirectURI);

            //Obtains the access token from the authorization code.
            string accessToken = GetAccessToken(authorizationCode, clientID, clientSecret, codeVerifier, redirectURI);
        }

        public static string GenerateNonce()
        {
            const string chars = "oidc";
            var random = new Random();
            var nonce = new char[128];
            for (int i = 0; i < nonce.Length; i++)
            {
                nonce[i] = chars[random.Next(chars.Length)];
            }

            return new string(nonce);
        }

        private static string GenerateCodeChallenge(string codeVerifier)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            var b64Hash = Convert.ToBase64String(hash);
            var code = Regex.Replace(b64Hash, "\\+", "-");
            code = Regex.Replace(code, "\\/", "_");
            code = Regex.Replace(code, "=+$", "");
            return code;
        }

        public static string GetAuthorizationCode(string clientID, string codeVerifier, string redirectURI)
        {
            //var codeChallengeKey = Microsoft.AspNetCore.Authentication.OAuth.OAuthConstants.CodeChallengeKey;
            //var codeChallengeMethodKey = Microsoft.AspNetCore.Authentication.OAuth.OAuthConstants.CodeChallengeMethodKey;
            //var codeChallengeMethodS256 = Microsoft.AspNetCore.Authentication.OAuth.OAuthConstants.CodeChallengeMethodS256;
            //var codeVerifierKey = Microsoft.AspNetCore.Authentication.OAuth.OAuthConstants.CodeVerifierKey;

            // Generates state and PKCE values.
            string state = RandomDataBase64Url(32);
            string codeChallenge = Base64UrlEncodeNoPadding(Sha256(codeVerifier));
            const string codeChallengeMethod = "S256";

            // Creates an HttpListener to listen for requests on that redirect URI.
            var http = new HttpListener();
            http.Prefixes.Add(redirectURI);
            Console.WriteLine("Listening..");
            http.Start();

            // Creates the OAuth 2.0 authorization request.
            string authorizationRequestURI = "https://demo.duendesoftware.com/connect/authorize";
            string scope = "openid profile api offline_access";

            string authorizationRequest = string.Format("{0}?response_type=code&scope={6}&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
                authorizationRequestURI,
                Uri.EscapeDataString(redirectURI),
                clientID,
                state,
                codeChallenge,
                codeChallengeMethod,
                Uri.EscapeDataString(scope)
                );


            //return authorizationRequest;   // return full path

            // Opens request in the browser.
            try
            {
                OpenUrl(authorizationRequest);
            }
            catch (Exception ex)
            {

            }

            // Waits for the OAuth authorization response.
            var context = http.GetContext();

            // Sends an HTTP response to the browser.
            var response = context.Response;
            string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;></head><body>Please return to the app.</body></html>");
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            using (var responseOutput = response.OutputStream)
                responseOutput.Write(buffer, 0, buffer.Length);
            http.Stop();
            Console.WriteLine("HTTP server stopped.");

            // Checks for errors.
            if (context.Request.QueryString.Get("error") != null)
            {
                Console.WriteLine(String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
                return null;
            }

            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null)
            {
                Console.WriteLine("Malformed authorization response. " + context.Request.QueryString);
                return null;
            }

            // extracts the code
            var code = context.Request.QueryString.Get("code");
            var incomingState = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (incomingState != state)
            {
                Console.WriteLine(String.Format("Received request with invalid state ({0})", incomingState));
                return null;
            }

            Console.WriteLine("Authorization code: " + code);
            return code;
        }

        public static string GetAuthorizationCode(string redirectURI)
        {
            //var codeChallengeKey = Microsoft.AspNetCore.Authentication.OAuth.OAuthConstants.CodeChallengeKey;
            //var codeChallengeMethodKey = Microsoft.AspNetCore.Authentication.OAuth.OAuthConstants.CodeChallengeMethodKey;
            //var codeChallengeMethodS256 = Microsoft.AspNetCore.Authentication.OAuth.OAuthConstants.CodeChallengeMethodS256;
            //var codeVerifierKey = Microsoft.AspNetCore.Authentication.OAuth.OAuthConstants.CodeVerifierKey;
            string codeVerifier = RandomDataBase64Url(32);
            string clientID = "interactive.confidential";
            string clientSecret = "secret";

            // Generates state and PKCE values.
            string state = RandomDataBase64Url(32);
            string codeChallenge = Base64UrlEncodeNoPadding(Sha256(codeVerifier));
            const string codeChallengeMethod = "S256";

            // Creates the OAuth 2.0 authorization request.
            string authorizationRequestURI = "https://demo.duendesoftware.com/connect/authorize";
            string scope = "openid profile api offline_access";

            string authorizationRequest = string.Format("{0}?response_type=code&scope={6}&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
                authorizationRequestURI,
                Uri.EscapeDataString(redirectURI),
                clientID,
                state,
                codeChallenge,
                codeChallengeMethod,
                Uri.EscapeDataString(scope)
                );


            return authorizationRequest;   // return full path

        }

        public static string GetAccessToken(string code, string clientID, string clientSecret, string codeVerifier, string redirectURI)
        {
            Console.WriteLine("Exchanging code for tokens...");

            // builds the  request
            string tokenRequestURI = "https://demo.duendesoftware.com/connect/token";
            string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&grant_type=authorization_code",
                code,
                Uri.EscapeDataString(redirectURI),
                clientID,
                codeVerifier,
                clientSecret
                );

            // sends the request
            HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenRequestURI);
            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            byte[] tokenRequestBytes = Encoding.ASCII.GetBytes(tokenRequestBody);
            tokenRequest.ContentLength = tokenRequestBytes.Length;
            Stream stream = tokenRequest.GetRequestStream();
            stream.Write(tokenRequestBytes, 0, tokenRequestBytes.Length);
            stream.Close();

            try
            {
                // gets the response
                WebResponse tokenResponse = tokenRequest.GetResponse();
                using StreamReader reader = new StreamReader(tokenResponse.GetResponseStream());
                // reads response body
                string responseText = reader.ReadToEnd();
                Console.WriteLine(responseText);

                // converts to dictionary
                Dictionary<string, string> tokenEndpointDecoded = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                string accessToken = tokenEndpointDecoded["access_token"];
                return accessToken;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    if (ex.Response is HttpWebResponse response)
                    {
                        Console.WriteLine("HTTP: " + response.StatusCode);
                        using StreamReader reader = new StreamReader(response.GetResponseStream());
                        // reads response body
                        string responseText = reader.ReadToEnd();
                        Console.WriteLine(responseText);
                    }
                }
                return null;
            }
        }

        public static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public static string RandomDataBase64Url(uint length)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return Base64UrlEncodeNoPadding(bytes);
        }

        public static byte[] Sha256(string inputStirng)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(inputStirng);
            SHA256Managed sha256 = new SHA256Managed();
            return sha256.ComputeHash(bytes);
        }

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        #endregion
    }
}
