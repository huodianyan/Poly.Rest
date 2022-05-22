using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Poly.Rest
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RestResourceAttribute : Attribute
    {
        public string BasePath;
    }
    public class RestResource
    {
        internal readonly string name;
        internal readonly object instance;
        private Dictionary<string, RestRoute> routeDict;// = new Dictionary<string, RestRoute>();

        internal RestResource(string name, object instance)
        {
            this.name = name;
            this.instance = instance;
            routeDict = new Dictionary<string, RestRoute>();
            var type = instance.GetType();
            var methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < methodInfos.Length; i++)
            {
                var methodInfo = methodInfos[i];
                var methodName = methodInfo.Name;
                var routeAttributes = methodInfo.GetCustomAttributes<RestRouteAttribute>();
                foreach (var routeAttribute in routeAttributes)
                {
                    var route = new RestRoute(this, routeAttribute.RouteTemplate, routeAttribute.RestMethod, methodInfo);
                    routeDict.Add(route.name, route);
                }
            }
        }
        public override string ToString()
        {
            return $"{name}:{string.Join(",", routeDict.Select((pair) => $"{pair.Key}:{pair.Value}"))}";
        }

        public RestRoute GetRestRoute(string routeName, ERestMethod restMethod)
        {
            if (!routeDict.TryGetValue(routeName, out var route))
                return null;
            if (route.restMethod != restMethod)
                return null;
            return route;
        }
    }

}