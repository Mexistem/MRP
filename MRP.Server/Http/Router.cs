using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRP.Server.Http
{
    public sealed class Router
    {
        private readonly Dictionary<(string method, string route), Func<RequestContext, Task>> _routes = new();

        public void Map(string method, string route, Func<RequestContext, Task> handler)
        {
            _routes[(method.ToUpperInvariant(), route.ToLowerInvariant())] = handler;
        }

        public async Task<bool> TryHandleAsync(RequestContext context)
        {
            string method = context.Request.HttpMethod.ToUpperInvariant();
            string path = context.Request.Url!.AbsolutePath.ToLowerInvariant();

            if (_routes.TryGetValue((method, path), out var handler))
            {
                await handler(context);
                return true;
            }

            foreach (var kvp in _routes)
            {
                var key = kvp.Key;
                if (!string.Equals(key.Item1, method, StringComparison.Ordinal))
                    continue;

                string template = key.Item2;
                if (!template.Contains('{'))
                    continue;

                if (IsMatch(template, path, out var parameters))
                {
                    foreach (var p in parameters)
                    {
                        context.Parameters[p.Key] = p.Value;
                    }

                    await kvp.Value(context);
                    return true;
                } 
            }

            return false;
        }

        private static bool IsMatch(string template, string path, out Dictionary<string, string> parameters)
        {
            parameters = new Dictionary<string, string>();

            var templateParts = template.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (templateParts.Length != pathParts.Length)
                return false;

            for (int i = 0; i < templateParts.Length; i++)
            {
                var t = templateParts[i];
                var p = pathParts[i];

                if (t.StartsWith("{") && t.EndsWith("}"))
                {
                    string key = t.Trim('{', '}');
                    parameters[key] = p;
                    continue;
                }

                if (!string.Equals(t, p, StringComparison.Ordinal))
                    return false;
            }

            return true;
        }
    }
}
