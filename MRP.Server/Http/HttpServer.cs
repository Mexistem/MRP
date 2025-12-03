using MRP.Server.Http.Handlers;
using MRP.Server.Services;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace MRP.Server.Http
{
    public sealed class HttpServer
    {
        private readonly HttpListener _listener = new();
        private readonly Router _router = new();

        public HttpServer(IUserManager userManager, IAuthManager authManager)
        {
            var userHandler = new UserHandler(userManager, authManager);

            _router.Map("POST", "/api/users/register", userHandler.Register);
            _router.Map("POST", "/api/users/login", userHandler.Login);
            _router.Map("GET", "/api/users/profile", userHandler.Profile);
            _router.Map("POST", "/api/users/logout", userHandler.Logout);
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _listener.Prefixes.Add("http://localhost:8080/");
            _listener.Start();

            while (true)
            {
                var context = new RequestContext(await _listener.GetContextAsync());

                if (!await _router.TryHandleAsync(context))
                {
                    context.Response.StatusCode = 404;
                    await JsonSerializer.SerializeAsync(context.Response.OutputStream,
                        new { error = "Not Found" });
                }

                context.Response.OutputStream.Close();
            }
        }
    }
}
