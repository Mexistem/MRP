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

            if (method == "GET" && path.StartsWith("/api/users/") && path.EndsWith("/profile"))
            {
                if (_routes.TryGetValue((method, "/api/users/profile"), out handler))
                {
                    await handler(context);
                    return true;
                }
            }

            return false;
        }
    }
}
