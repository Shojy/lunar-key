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
                ClientId = "api",
                ClientName = "API Client",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                RequireConsent = false,

                ClientSecrets = { new Secret("secret".Sha256()) },


                AllowedScopes = {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "api1"
                },
                AllowOfflineAccess = true
            },
            new Client
            {
                ClientId = "ro.client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                ClientSecrets ={new Secret("secret".Sha256())},
                AllowedScopes = { "api1" }
            },

            new Client
            {
                ClientId = "mvc",
                ClientName = "MVC Client",
                AllowedGrantTypes = GrantTypes.Implicit,

                // where to redirect to after login
                RedirectUris = { "http://localhost:5002/signin-oidc" },

                // where to redirect to after logout
                PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile
                }
            }
        };

        public static IEnumerable<IdentityResource> GetIdentityResources() => new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };
    }
}
