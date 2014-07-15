using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using BuddySDK.BuddyServiceClient;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;


namespace BuddySDK
{
    public class AuthenticatedUser : User
    {
        /// <summary>
        /// Gets the unique user token that is the secret used to log-in this user. Each user has a unique ID, a secret user token and a user/pass combination.
        /// </summary>
        public string AccessToken
        {
            get
            {
                return GetValueOrDefault<string>("AccessToken");
            }
            protected set
            {
                SetValue<string>("AccessToken", value);
            }
        }

        internal AuthenticatedUser(string id, string accessToken, BuddyClient client)
            : base(id, client)
        {
            this.AccessToken = accessToken;
        }

        public override string ToString()
        {
            return base.ToString() + ", Email: " + this.Email;
        }


        public Task<BuddyResult<bool>> AddIdentityAsync(string identityProviderName, string identityID)
        {
            return AddRemoveIdentityCoreAsync("POST", "/users/me/identities/" + Uri.EscapeDataString(identityProviderName), new
            {
                IdentityID = identityID
            });
        }

        public Task<BuddyResult<bool>> RemoveIdentityAsync(string identityProviderName, string identityID)
        {
            return AddRemoveIdentityCoreAsync("DELETE", "/users/me/identities/" + Uri.EscapeDataString(identityProviderName), new { IdentityID = identityID });
        }

        private Task<BuddyResult<bool>> AddRemoveIdentityCoreAsync(string verb, string path, object parameters)
        {
            var t = Client.CallServiceMethod<string>(verb, path, parameters);
            return t.WrapResult<string, bool>((r1) => r1.IsSuccess);
           

        }

        public Task<BuddyResult<IEnumerable<string>>> GetIdentitiesAsync(string identityProviderName = null)
        {
            return Task.Run<BuddyResult<IEnumerable<string>>>(() =>
            {
                var encodedIdentityProviderName = string.IsNullOrEmpty(identityProviderName) ? "" : Uri.EscapeDataString(identityProviderName);

                var r = Client.CallServiceMethod<IEnumerable<Newtonsoft.Json.Linq.JObject>>("GET", "/users/me/identities/" + encodedIdentityProviderName);

                return r.Result.Convert<IEnumerable<string>>(jObjects => jObjects.Select(jObject => jObject.Value<string>("identityProviderID")));
            });
        }
    }
}
