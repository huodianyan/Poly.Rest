using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Poly.Rest
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RestRouteAttribute : Attribute
    {
        public ERestMethod RestMethod { get; private set; }
        public string RouteTemplate { get; private set; }

        public RestRouteAttribute(ERestMethod httpMethod, string routeTemplate)
        {
            RestMethod = httpMethod;
            RouteTemplate = routeTemplate;
        }
    }
    public enum ERestMethod
    {
        GET,
        POST,
        DELETE,
        PUT
    }
    public class RestRoute
    {
        private readonly RestResource resource;
        internal readonly string name;
        internal readonly ERestMethod restMethod;
        private readonly MethodInfo methodInfo;

        private object[] params1 = new object[1];

        internal RestRoute(RestResource resource, string name, ERestMethod restMethod, MethodInfo methodInfo)
        {
            this.resource = resource;
            this.name = name;
            this.restMethod = restMethod;
            this.methodInfo = methodInfo;
        }
        public override string ToString()
        {
            return $"{name}:{restMethod},{methodInfo.Name}";
        }
        public async Task InvokeRestAPI(HttpListenerContext context)
        {
            params1[0] = context;
            var returnObj = methodInfo.Invoke(resource.instance, params1);
            if (returnObj is Task)
            {
                var task = returnObj as Task;
                await task;
            }
        }
    }
}