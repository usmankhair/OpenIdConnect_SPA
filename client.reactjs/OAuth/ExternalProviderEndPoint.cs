using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace client.reactjs.OAuth
{
    public static class ExternalProviderEndPoint
    {
        public static string GetToken { get; } = $"{{0}}/connect/token";

        public static string GetUserInfo { get; } = $"{{0}}/connect/userinfo";

    }
}
