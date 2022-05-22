using System;
using System.Net;

namespace Poly.Rest
{
    public class BasicAuth
    {
        public string Username { get; }
        public string Password { get; }

        public BasicAuth(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
    public class RestServer
    {
        private HttpListener listener;
        private int port;
        private BasicAuth basicAuth;
        private RestRoutingManager routingManager;
        //private ILogger logger;

        public RestServer(int port, BasicAuth basicAuth, RestRoutingManager routingManager)
        {
            //logger = Debug.unityLogger;
            this.basicAuth = basicAuth;
            this.port = port;
            this.routingManager = routingManager;
        }
        public void Start()
        {
            listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{port}/");
            if (basicAuth != null)
                listener.AuthenticationSchemes = AuthenticationSchemes.Basic;
            listener.Start();
            Listen();
        }
        public void Stop()
        {
            if (listener == null)
                return;
            listener.Stop();
            listener = null;
        }
        private async void Listen()
        {
            while (listener != null && listener.IsListening)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    //logger.Log(LogType.Log, TAG, $"{context}");
                    Process(context);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString());
                }
            }
        }
        private async void Process(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            var calledURL = context.Request.Url.AbsolutePath;

            // Verbose information
            Console.WriteLine($"Process: Request Type: {request.HttpMethod}, Requested URL: {calledURL}");

            bool hasAccess = false;
            if (basicAuth == null)
            {
                hasAccess = true;
            }
            else
            {
                var identity = (HttpListenerBasicIdentity)context.User.Identity;
                if (basicAuth.Username.Equals(identity.Name) && basicAuth.Password.Equals(identity.Password))
                {
                    hasAccess = true;
                    Console.WriteLine($"Username: {identity.Name}, Password: {identity.Password}");
                }
            }

            if (!hasAccess)
            {
                var msg = $"No access!";
                Console.Error.WriteLine($"Process: {msg}");
                // 401 Page - Unauthorized
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await response.SendResponseAsync(msg);
                return;
            }
            if (!Enum.TryParse<ERestMethod>(request.HttpMethod, out var restMethod))
            {
                var msg = $"Not support {request.HttpMethod}!";
                Console.Error.WriteLine($"Process: {msg}");
                // 404 Page - Not found
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await response.SendResponseAsync(msg);
                return;
            }
            var route = routingManager.GetRestRoute(calledURL, restMethod);
            if (route == null)
            {
                var msg = $"Not support {calledURL}, {restMethod}!";
                Console.Error.WriteLine($"Process: {msg}");
                // 404 Page - Not found
                response.StatusCode = (int)HttpStatusCode.NotFound;
                await response.SendResponseAsync(msg);
                return;
            }
            await route.InvokeRestAPI(context);
        }
    }
}