using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Poly.Rest
{
    public class RestRoutingManager : IDisposable
    {
        private Dictionary<string, RestResource> resourceDict;

        public RestRoutingManager()
        {
            resourceDict = new Dictionary<string, RestResource>();
        }
        public void Dispose()
        {
            resourceDict.Clear();
            resourceDict = null;
        }
        public RestRoute GetRestRoute(string url, ERestMethod restMethod)
        {
            int startIndex = 0;
            if (url.StartsWith("/"))
                startIndex = 1;
            // url = url.Substring(1);
            var index = url.IndexOf('/', startIndex);
            var resourceName = url.Substring(0, index);
            var routeName = url.Substring(index);

            if (!resourceDict.TryGetValue(resourceName, out var resource))
                return null;
            return resource.GetRestRoute(routeName, restMethod);
        }
        public void RegisterResource(string @namespace)
        {
            var typeList = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsInterface || type.IsAbstract || type.IsNotPublic)
                        continue;
                    //if (type.Namespace != null && type.Namespace.StartsWith(@namespace))
                    if (type.Namespace != null && type.Namespace.StartsWith(@namespace))
                        typeList.Add(type);
                }
            }
            foreach (var type in typeList)
            {
                var resourceAttributes = type.GetCustomAttributes<RestResourceAttribute>();
                foreach (var resourceAttribute in resourceAttributes)
                    RegisterResource(resourceAttribute.BasePath, type);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterResource(string name, Type resourceType)
        {
            var instance = Activator.CreateInstance(resourceType);
            RegisterResource(name, instance);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterResource(string name, object instance)
        {
            var resource = new RestResource(name, instance);
            resourceDict[name] = resource;
            Console.WriteLine($"RegisterResource: {name},{resource}");
        }
        public void UnregisterResource(string name)
        {
            resourceDict.Remove(name);
        }
    }
}