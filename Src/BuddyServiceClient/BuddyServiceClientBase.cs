using BuddySDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BuddySDK.BuddyServiceClient
{
    public class BuddyCallResult
    {
        public string Error { get; set; }
        public int? ErrorNumber { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public string RequestID { get; set; }

        public BuddyCallResult()
        {
        }
    }
    public class BuddyCallResult<T> : BuddyCallResult
    {
        public T Result { get; set; }
    }

    public class JsonEnvelope<T>
    {
        public int status { get; set; }
        public string error { get; set; }
        public int? errorNumber { get; set; }
        public string message { get; set; }
        public T result { get; set; }
        public string request_id { get; set; }
    }

    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; set; }

        public bool ThrowException { get; set; }
        public ExceptionEventArgs(Exception bex)
        {
            Exception = bex;
            ThrowException = true;
        }
    }

    public abstract partial class BuddyServiceClientBase : IRemoteMethodProvider
    {
        internal BuddySDK.BuddyClient Client
        {
            get;
            set;
        }

        internal static BuddyServiceClientBase CreateServiceClient(BuddySDK.BuddyClient client, string serviceRoot,string appID,string sharedSecret)
        {
            var type = typeof(BuddyServiceClientHttp);
            string typeName = null;

            try
            {
                typeName = PlatformAccess.Current.GetConfigSetting("BuddyServiceClientType");
            }
            catch (NotImplementedException)
            {
                // platform access doesn't provide config settings
            }

            if (typeName != null)
            {
                type = Type.GetType(typeName, true);
            }

            if (!typeof(BuddyServiceClientBase).IsAssignableFrom(type))
            {
                throw new ArgumentException(type.FullName + " is not a BuddyServiceClientBase implementor.");
            }

            var bsc = (BuddyServiceClientBase)Activator.CreateInstance(type, serviceRoot,appID, sharedSecret);
            bsc.Client = client;
            return bsc;
        }

        protected abstract string ClientName { get; }
        protected abstract string ClientVersion { get; }

        public event EventHandler<ExceptionEventArgs> ServiceException;

        public virtual bool IsLocal
        {
            get
            {
                return false;
            }
        }

        protected BuddyServiceClientBase()
        {
        }

        internal static IDictionary<string, object> ParametersToDictionary(object parameters)
        {
            IDictionary<string, object> d = parameters as IDictionary<string, object>;
            if (d != null)
            {
                return d;
            }
            else
            {
                d = new Dictionary<string, object>(DotNetDeltas.InvariantComparer(true));
                if (parameters != null)
                {
                    var props = parameters.GetType().GetProperties();
                    foreach (var prop in props)
                    {
                        d[prop.Name] = prop.GetValue(parameters, null);
                    }
                }
            }

            return d;
        }

        internal void CallOnUiThread(Action callback)
        {
            PlatformAccess.Current.InvokeOnUiThread(callback);
        }

        public async Task<BuddyCallResult<T>> CallMethodAsync<T>(
            string verb, 
            string path, 
            object parameters = null)
        {
            return await CallMethodAsyncCore<T>(verb, path, parameters).ConfigureAwait(false);
        }

        protected abstract Task<BuddyCallResult<T>> CallMethodAsyncCore<T>(string verb, string path, object parameters);

        private string serviceRoot;
        public string ServiceRoot
        {
            get
            {
                return serviceRoot;
            }
            set
            {
                // TODO: should we be doing this here, or should setters be changed to not pass in trailing slashes?

                serviceRoot = value == null ? null : value.TrimEnd('/');
            }
        }

        protected virtual void OnServiceException(Exception ex)
        {

            if (ServiceException != null)
            {
                var args = new ExceptionEventArgs(ex);
                ServiceException(this, args);
            }
        }

    }

    internal static class BuddyResultCreator
    {
        public static BuddyCallResult<T> Create<T>(T result, object err)
        {
            return null;
        }
    }

    public static class BuddyError
    {
        public const string None = null;
        public const string UnknownServiceError = "UnknownServiceError";
        public const string InternetConnectionError = "InternetConnectionError";

        public static string UserEmailTaken { get; set; }

        public static string UserNameAvailble { get; set; }

        public static string UserNameAlreadyInUse { get; set; }
    }

}
