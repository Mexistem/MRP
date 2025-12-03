using System.Net;

namespace MRP.Server.Http
{
    public sealed class RequestContext
    {
        public HttpListenerRequest Request { get; }
        public HttpListenerResponse Response { get; }
        public Dictionary<string, string> Parameters { get; } = new();

        public RequestContext(HttpListenerContext context)
        {
            Request = context.Request;
            Response = context.Response;
        }
    }
}
