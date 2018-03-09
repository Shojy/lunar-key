using IdentityServer4;
using IdentityServer4.Models;
//using Lunar.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lunar.Auth
{
    public class Config
    {
        public static IEnumerable<ApiResource> GetResources() => new List<ApiResource> {
            new ApiResource("api1", "API 1")
        };

        public static IEnumerable<Client> GetClients() => new List<Client> {
            new Client
            {
                ClientId = "mvc",
                ClientName = "MVC Client",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                RequireConsent = false,

                ClientSecrets = { new Secret("secret".Sha256()) },
                

                AllowedScopes = {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "api1"
                },
                AllowOfflineAccess = true
            }
        };

        public static IEnumerable<IdentityResource> GetIdentityResources() => new List<IdentityResource>();
    }
}
