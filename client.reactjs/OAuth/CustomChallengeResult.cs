//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;

//namespace Microsoft.AspNetCore.Mvc
//{
//    /// <summary>
//    /// An <see cref="ActionResult"/> that on execution invokes <see cref="M:AuthenticationManager.ChallengeAsync"/>.
//    /// </summary>
//    public class CustomChallengeResult : ActionResult
//    {
//        /// <summary>
//        /// Initializes a new instance of <see cref="CustomChallengeResult"/>.
//        /// </summary>
//        public CustomChallengeResult()
//            : this(Array.Empty<string>())
//        {
//        }

//        /// <summary>
//        /// Initializes a new instance of <see cref="CustomChallengeResult"/> with the
//        /// specified authentication scheme.
//        /// </summary>
//        /// <param name="authenticationScheme">The authentication scheme to challenge.</param>
//        public CustomChallengeResult(string authenticationScheme)
//            : this(new[] { authenticationScheme })
//        {
//        }

//        /// <summary>
//        /// Initializes a new instance of <see cref="CustomChallengeResult"/> with the
//        /// specified authentication schemes.
//        /// </summary>
//        /// <param name="authenticationSchemes">The authentication schemes to challenge.</param>
//        public CustomChallengeResult(IList<string> authenticationSchemes)
//            : this(authenticationSchemes, properties: null)
//        {
//        }

//        /// <summary>
//        /// Initializes a new instance of <see cref="CustomChallengeResult"/> with the
//        /// specified <paramref name="properties"/>.
//        /// </summary>
//        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the authentication
//        /// challenge.</param>
//        public CustomChallengeResult(AuthenticationProperties properties)
//            : this(Array.Empty<string>(), properties)
//        {
//        }

//        /// <summary>
//        /// Initializes a new instance of <see cref="CustomChallengeResult"/> with the
//        /// specified authentication scheme and <paramref name="properties"/>.
//        /// </summary>
//        /// <param name="authenticationScheme">The authentication schemes to challenge.</param>
//        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the authentication
//        /// challenge.</param>
//        public CustomChallengeResult(string authenticationScheme, AuthenticationProperties properties)
//            : this(new[] { authenticationScheme }, properties)
//        {
//        }

//        /// <summary>
//        /// Initializes a new instance of <see cref="CustomChallengeResult"/> with the
//        /// specified authentication schemes and <paramref name="properties"/>.
//        /// </summary>
//        /// <param name="authenticationSchemes">The authentication scheme to challenge.</param>
//        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the authentication
//        /// challenge.</param>
//        public CustomChallengeResult(IList<string> authenticationSchemes, AuthenticationProperties properties)
//        {
//            AuthenticationSchemes = authenticationSchemes;
//            Properties = properties;
//        }

//        /// <summary>
//        /// Gets or sets the authentication schemes that are challenged.
//        /// </summary>
//        public IList<string> AuthenticationSchemes { get; set; }

//        /// <summary>
//        /// Gets or sets the <see cref="AuthenticationProperties"/> used to perform the authentication challenge.
//        /// </summary>
//        public AuthenticationProperties Properties { get; set; }

//        /// <inheritdoc />
//        public override async Task ExecuteResultAsync(ActionContext context)
//        {
//            if (context == null)
//            {
//                throw new ArgumentNullException(nameof(context));
//            }

//            var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
//            //var logger = loggerFactory.CreateLogger<CustomChallengeResult>();

//            // logger.ChallengeResultExecuting(AuthenticationSchemes);

//            if (AuthenticationSchemes != null && AuthenticationSchemes.Count > 0)
//            {
//                foreach (var scheme in AuthenticationSchemes)
//                {
//                    await context.HttpContext.ChallengeAsync(scheme, Properties);
//                }
//            }
//            else
//            {
//                await context.HttpContext.ChallengeAsync(Properties);
//            }
//        }
//    }
//}


////internal class CustomChallengeResult : ActionResult
////{
////    public CustomChallengeResult(string provider, string redirectUri)
////    {
////        LoginProvider = provider;
////        RedirectUri = redirectUri;
////    }

////    public string LoginProvider { get; set; }
////    public string RedirectUri { get; set; }
////    public override void ExecuteResult(ActionContext context)
////    {
////        var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
////        context.HttpContext.ChallengeAsync(LoginProvider, properties);

////        var cookies = context.HttpContext.Response.Cookies;
////        //context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
////        base.ExecuteResult(context);

////    }
////}