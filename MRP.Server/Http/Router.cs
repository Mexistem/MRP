using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MRP.Server.Http
{
    public sealed class Router
    {
        private readonly Dictionary<(string method, string route), Func<RequestContext, Task>> _routes = new();

        public void Map(string method, string route, Func<RequestContext, Task> handler)
        {
            _routes[(method.ToUpper(), route.ToLower())] = handler;
        }

        public async Task<bool> TryHandleAsync(RequestContext context)
        {
            string method = context.Request.HttpMethod.ToUpper();
            string path = context.Request.Url!.AbsolutePath.ToLower();

            if (_routes.TryGetValue((method, path), out var handler))
            {
                await handler(context);
                return true;
            }

            return false;
        }
    }
}