using MRP.Server.Http.Handlers;
using MRP.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MRP.Server.Http
{
    public sealed class HttpServer
    {
        private readonly HttpListener _listener = new();
        private readonly Router _router = new();

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public HttpServer(IUserManager users, IAuthManager auth)
        {
            _router.Map("POST", "/api/users/login", context => AuthHandler.Login(context, auth));
            _router.Map("POST", "/api/users/logout", context => AuthHandler.Logout(context, auth));

            _router.Map("POST", "/api/users/register", context => UserHandler.Register(context, users));
            _router.Map("GET", "/api/users/profile", context => UserHandler.Profile(context, users, auth));

            _router.Map("POST", "/api/admin/register", context => AdminHandler.RegisterAdmin(context, users, auth));
            _router.Map("GET", "/api/admin/users/list", context => AdminHandler.ListUsers(context, users, auth));
            _router.Map("DELETE", "/api/admin/users/delete", context => AdminHandler.DeleteUser(context, users, auth));
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _listener.Prefixes.Add("http://localhost:8080/");
            _listener.Start();
            Console.WriteLine("Http Server started on http://localhost:8080");

            while (true)
            {
                var context = new RequestContext(await _listener.GetContextAsync());

                if (!await _router.TryHandleAsync(context))
                {
                    context.Response.StatusCode = 404;
                    await JsonSerializer.SerializeAsync(
                        context.Response.OutputStream,
                        new { error = "Not Found" }
                    );
                }

                context.Response.OutputStream.Close();
            }
        }
    }
}