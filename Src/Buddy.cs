using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuddySDK.Models;

namespace BuddySDK
{
    [Flags]
    public enum BuddyClientFlags
    {
        None              =  0,
        AutoCrashReport =    0x00000002,
        AllowReinitialize =  0x00000004,
        Default = AutoCrashReport,
    }

    class BuddyCreds
    {
        public string AppID { get; set; }
        public string AppKey { get; set; }
        public BuddyOptions Options { get; set; }

        public BuddyCreds(string appId, string appKey, BuddyOptions options)
        {
            AppID = appId;
            AppKey = appKey;
            Options = options;
        }
    }

    public static partial class Buddy
    {
        static IDictionary<string, BuddyClient> _clients = new Dictionary<string, BuddyClient>();
        static string _currentClientKey;

        private static string GetClientKey(BuddyCreds creds)
        {
            return string.Format("{0};{1};{2};{3}", creds.AppID, creds.AppKey, creds.Options.Flags, creds.Options.InstanceName);
        }

        internal static BuddyClient CurrentInstance
        {
            get
            {
                if (_currentClientKey == null || _clients[_currentClientKey] == null)
                {
                    throw new InvalidOperationException("Init must be called before accessing Instance.");
                }

                return _clients[_currentClientKey];
            }
            set
            {
                var clientKey = GetClientKey(new BuddyCreds(value.AppId, value.AppKey, value.Options));
                _currentClientKey = clientKey;

                _clients[clientKey] = value;
            }
        }

        public static async Task<BuddyResult<T>> GetAsync<T>(string path, object parameters = null)
        {
            // Do await even in this trivial case 
            // for this method to appear in debugger's call stack.
            return await CurrentInstance.GetAsync<T>(path, parameters).ConfigureAwait(false);
        }
        public static async Task<BuddyResult<T>> PostAsync<T>(string path, object parameters = null)
        {
            // Do await even in this trivial case 
            // for this method to appear in debugger's call stack.
            return await CurrentInstance.PostAsync<T>(path, parameters).ConfigureAwait(false);
        }
        public static async Task<BuddyResult<T>> PutAsync<T>(string path, object parameters = null)
        {
            // Do await even in this trivial case 
            // for this method to appear in debugger's call stack.
            return await CurrentInstance.PutAsync<T>(path, parameters).ConfigureAwait(false);
        }
        public static async Task<BuddyResult<T>> PatchAsync<T>(string path, object parameters = null)
        {
            // Do await even in this trivial case 
            // for this method to appear in debugger's call stack.
            return await CurrentInstance.PatchAsync<T>(path, parameters).ConfigureAwait(false);
        }
        public static async Task<BuddyResult<T>> DeleteAsync<T>(string path, object parameters = null)
        {
            // Do await even in this trivial case 
            // for this method to appear in debugger's call stack.
            return await CurrentInstance.DeleteAsync<T>(path, parameters).ConfigureAwait(false);
        }
       
        public static async Task<User> GetCurrentUserAsync (bool reload = false)
        {
            // Do await even in this trivial case 
            // for this method to appear in debugger's call stack.
            return await CurrentInstance.GetCurrentUserAsync (reload).ConfigureAwait(false);
        }

        public static void RunOnUiThread(Action a) {
            PlatformAccess.Current.InvokeOnUiThread (a);
        }

        #region Global Events
        public static event EventHandler AuthorizationLevelChanged {
            add {
                CurrentInstance.AuthorizationLevelChanged += value;
            }
            remove {
                CurrentInstance.AuthorizationLevelChanged -= value;
            }
        }

        public static event EventHandler AuthorizationNeedsUserLogin {
            add {
                CurrentInstance.AuthorizationNeedsUserLogin += value;
            }
            remove {
                CurrentInstance.AuthorizationNeedsUserLogin -= value;
            }
        }

        public static event EventHandler<CurrentUserChangedEventArgs> CurrentUserChanged {
            add {
                CurrentInstance.CurrentUserChanged += value;
            }
            remove {
                CurrentInstance.CurrentUserChanged -= value;
            }
        }

        public static event EventHandler<ConnectivityLevelChangedArgs> ConnectivityLevelChanged {
            add {
                CurrentInstance.ConnectivityLevelChanged += value;
            }
            remove {
                CurrentInstance.ConnectivityLevelChanged -= value;
            }
        }

        public static event EventHandler<ServiceExceptionEventArgs> ServiceException {
            add {
                CurrentInstance.ServiceException += value;
            }
            remove {
                CurrentInstance.ServiceException -= value;
            }
        }

        #endregion

        public static IBuddyClient Init(string appId, string appKey, BuddyOptions options = null)
        {
            if (options == null)
            {
                options = new BuddyOptions();
            }

            if (_currentClientKey != null && !options.Flags.HasFlag(BuddyClientFlags.AllowReinitialize))
            {
                throw new InvalidOperationException("Already initialized.");
            }

            CurrentInstance = new BuddyClient(appId, appKey, options);

            return CurrentInstance;
        }

        public static Task<BuddyResult<User>> CreateUserAsync(string username, string password, string firstName = null, string lastName = null, string email = null, User.UserGender? gender = null, DateTime? dateOfBirth = null, string tag = null) {
            return CurrentInstance.CreateUserAsync (username, password, firstName, lastName, email, gender, dateOfBirth, tag : tag);
        }

        public static Task<BuddyResult<User>> LoginUserAsync(string username, string password)
        {
            var t = CurrentInstance.LoginUserAsync(username, password);

            return t;
        }

        public static Task<BuddyResult<bool>> LogoutUserAsync ()
        {
            return CurrentInstance.LogoutUserAsync ();
        }

        public static Task<BuddyResult<SocialNetworkUser>> SocialLoginUserAsync(string identityProviderName, string identityID, string identityAccessToken)
        {
            var t = CurrentInstance.SocialLoginUserAsync(identityProviderName, identityID, identityAccessToken);

            return t;
        }

        // 
        // Push Notifications
        //
        public static void SetPushToken(string token)
        {
            CurrentInstance.SetPushToken(token);
        }

        public static void RecordNotificationReceived<T>(T args)
        {
            CurrentInstance.RecordNotificationReceived(args);
        }   

        // 
        // Metrics
        //
        public static Task<BuddyResult<Metric>> RecordMetricAsync(string key, IDictionary<string, object> value = null, TimeSpan? timeout = null, DateTime? timeStamp = null)
        {
            return CurrentInstance.RecordMetricAsync(key, value, timeout, timeStamp);
        }

        public static BuddyGeoLocation LastLocation
        {
            get
            {
                return CurrentInstance.LastLocation;
            }
            
            set
            {
                CurrentInstance.LastLocation = value;
            }
        }
    }
}
