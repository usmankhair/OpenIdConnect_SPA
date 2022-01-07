using client.reactjs.OAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace client.reactjs
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureOpenIdConnect(services);

            services.AddHttpClient();

            services.AddControllersWithViews();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            services.AddTransient<IExternalProvider, ExternalProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                IdentityModelEventSource.ShowPII = true;
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }

        /// <summary>
        /// Just to use this method to enable SSO
        /// </summary>
        /// <param name="services"></param>
        private void ConfigureOpenIdConnect(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
               .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
               {
                   options.Cookie.Name = "__ATMS-bff-server";
                   options.Cookie.SameSite = SameSiteMode.Strict;
                   options.Cookie.IsEssential = true;
                   if (Configuration["Oidc:CookieExpireIn"] != null) 
                       options.ExpireTimeSpan = TimeSpan.FromMinutes(Convert.ToInt32(Configuration["Oidc:CookieExpireIn"]));
               })
               .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
               {
                   options.Authority = Configuration["Oidc:Authority"];
                   options.ClientId = Configuration["Oidc:ClientId"];
                   options.ClientSecret = Configuration["Oidc:ClientSecret"];
                   options.ResponseType = "code";
                   options.ResponseMode = "query";
                   options.CallbackPath = new PathString("/signin-oidc");
                   options.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
                   options.RemoteSignOutPath = new PathString("/signout-oidc");
                   options.GetClaimsFromUserInfoEndpoint = true;
                   options.SaveTokens = true;
                   options.Scope.Clear();

                   foreach (var scope in Configuration["Oidc:AllowedScope"].Split(','))
                       options.Scope.Add(scope);

                   //options.Scope.Add("openid");
                   //options.Scope.Add("profile");
                   //options.Scope.Add("api");
                   //options.Scope.Add("offline_access");

                   // TODO : if we need to enable role based permissions in future
                   //options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                   //{
                   //    NameClaimType = "name",
                   //    RoleClaimType = "role"
                   //};

                   //options.Events.OnAuthorizationCodeReceived += OnAuthorizationCodeReceived;
                   //options.Events.OnTokenValidated += OnTokenValidated;
                   //options.Events.OnTicketReceived += OnOpenIdConnectTicketReceived;

                   options.Events = new OpenIdConnectEvents
                   {
                       //When the client will receive the authorization code from Trust Builder
                       OnRedirectToIdentityProvider = context =>
                       {
                           var overrideUrl = context.ProtocolMessage.RedirectUri.Replace("signin-oidc", Configuration["Oidc:RedirectUrl"]);

                           var builder = new UriBuilder(overrideUrl);
                           context.ProtocolMessage.RedirectUri = builder.ToString();
                           // : TODO :: Combination to show in the message: Ticks.HASH STRING 
                           string codeVerifier = Configuration["Oidc:CodeVerifier"];
                           string codeChallenge = AuthClient.Base64UrlEncodeNoPadding(AuthClient.Sha256(codeVerifier));
                           context.ProtocolMessage.Parameters["nonce"] = codeVerifier;
                           context.ProtocolMessage.Parameters["code_challenge"] = codeChallenge;
                           return Task.FromResult(0);
                       },
                       //OnSignedOutCallbackRedirect = context =>
                       //{
                       //    context.Response.Redirect(context.Options.SignedOutRedirectUri);
                       //    context.HandleResponse();

                       //    return Task.CompletedTask;
                       //},
                       // handle the logout redirection
                       OnRedirectToIdentityProviderForSignOut = (context) =>
                       {
                           var dd = context.ProtocolMessage.RedirectUri;
                           //var logoutUri = $"https://{Configuration["Oidc:Authority"]}/Account/logout?client_id={Configuration["Oidc:ClientId"]}";
                           var overrideUrl = context.ProtocolMessage.PostLogoutRedirectUri.Replace("signout-callback-oidc", Configuration["Oidc:SignoutRedirectUrl"]);
                           var builder = new UriBuilder(overrideUrl);
                           context.ProtocolMessage.PostLogoutRedirectUri = builder.ToString();
                           return Task.FromResult(0);

                           //    var ddd = context.Options.SignedOutRedirectUri;
                           //    // Working without endsession on the Trust Builder
                           //    var logoutUri = $"{Configuration["Oidc:Authority"]}/Account/logout?client_id={Configuration["Oidc:ClientId"]}";
                           //    var postLogoutUri = context.Properties.RedirectUri;      // $"{Configuration["Oidc:SignoutRedirectUrl"]}";
                           //    if (!string.IsNullOrEmpty(postLogoutUri))
                           //    {
                           //        if (postLogoutUri.StartsWith("/"))
                           //        {
                           //            // transform to absolute
                           //            var request = context.Request;
                           //            postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                           //        }
                           //        logoutUri += $"&returnTo={ Uri.EscapeDataString(postLogoutUri)}";
                           //    }
                           //    context.Response.Redirect(logoutUri);
                           //    context.HandleResponse();
                           //    return Task.CompletedTask;
                       }
                   };
               });
        }

        //public static Task OnOpenIdConnectTicketReceived(TicketReceivedContext context)
        //{
        //    //if (context.Principal.Identity is ClaimsIdentity identity)
        //    //{
        //    //    identity.AddClaim(new Claim("foo", "bar"));
        //    //}

        //    return Task.CompletedTask;
        //}

        //public static Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
        //{
        //    //if (context.Principal.Identity is ClaimsIdentity identity)
        //    //{
        //    //    identity.AddClaim(new Claim("foo", "bar"));
        //    //}

        //    return Task.CompletedTask;
        //}

        //public static Task OnTokenValidated(TokenValidatedContext context)
        //{
        //    //if (context.Principal.Identity is ClaimsIdentity identity)
        //    //{
        //    //    identity.AddClaim(new Claim("foo", "bar"));
        //    //}

        //    return Task.CompletedTask;
        //}

    }
}
