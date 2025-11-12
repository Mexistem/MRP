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
        private readonly UserManager _users;
        private readonly AuthManager _auth;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public HttpServer(UserManager users, AuthManager auth)
        {
            _users = users;
            _auth = auth;
        }

        public void AddPrefix(string prefix) => _listener.Prefixes.Add(prefix);

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _listener.Prefixes.Add("http://localhost:8080/");
            _listener.Start();
            Console.WriteLine("Http Server started on http://localhost:8080");

            try
            {
                while(!cancellationToken.IsCancellationRequested)
                {
                    var context = await Task.Factory.FromAsync(_listener.BeginGetContext, _listener.EndGetContext, null);

                    _ = HandleRequestAsync(context);
                }
            }

            finally
            {
                _listener.Stop();
                Console.WriteLine("HTTP server stopped");
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            string path = request.Url!.AbsolutePath;
            string method = request.HttpMethod;

            try
            {
                if(method == "POST" && path == "/api/users/login")
                {
                    await HandleLoginAsync(context);
                }

                else if(method == "POST" && path == "/api/users/register")
                {
                    await HandleRegisterAsync(context);
                }

                else if (method == "POST" && path == "/api/users/logout")
                {
                    await HandleLogoutAsync(context);
                }

                else if (method == "GET" && path.StartsWith("/api/users/") && path.EndsWith("/profile"))
                {
                    string username = path.Split('/', StringSplitOptions.RemoveEmptyEntries)[2];
                    await HandleProfileAsync(context, username);
                }

                else if (path.StartsWith("/api/admin/", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleAdminRoutesAsync(context);
                }

                else
                {
                    await WriteJsonAsync(response, HttpStatusCode.NotFound, new { error = "Not Found" });
                }
            }

            catch (Exception exception)
            {
                await WriteJsonAsync(response, HttpStatusCode.InternalServerError, new { error = "Internal Server Error", Details = exception.Message });
            }

            finally
            {
                if (response.OutputStream.CanWrite)
                {
                    response.Close();
                }
            }
        }

        private async Task HandleLoginAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            if(request.ContentType?.Contains("application/json") != true)
            {
                await WriteJsonAsync(response, HttpStatusCode.BadRequest, new { error = "Invalid Content-Type", Details = "Use application/json" });
                return;
            }

            var login = await ReadJsonAsync<LoginRequest>(request);
            if(login is null || string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
            {
                await WriteJsonAsync(response, HttpStatusCode.BadRequest, new { error = "Invalid body", Details = "username and password required" });
                return;
            }

            try
            {
                string token = _auth.Login(login.Username, login.Password);
                await WriteBytesAsync(response, HttpStatusCode.OK, Encoding.UTF8.GetBytes($"\"{token}\""), "application/json");
            }

            catch (InvalidOperationException)
            {
                await WriteJsonAsync(response, HttpStatusCode.Unauthorized, new { error = "Unknown user" });
            }
            catch (UnauthorizedAccessException)
            {
                await WriteJsonAsync(response, HttpStatusCode.Unauthorized, new { error = "Invalid credentials" });
            }
        }

        private async Task HandleRegisterAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            if (request.ContentType?.Contains("application/json") != true)
            {
                await WriteJsonAsync(response, HttpStatusCode.BadRequest, new { error = "Invalid Content-Type", Details = "Use application/json" });
                return;
            }

            var register = await ReadJsonAsync<RegisterRequest>(request);
            if(register is null || string.IsNullOrWhiteSpace(register.Username) || string.IsNullOrWhiteSpace(register.Password))
            {
                await WriteJsonAsync(response, HttpStatusCode.BadRequest, new { error = "Invalid body", Details = "username and password required" });
                return;
            }

            try
            {
                if (register.IsAdmin)
                {
                    _users.RegisterAdmin(register.Username, register.Password);
                }
                else
                {
                    _users.Register(register.Username, register.Password);
                }
                await WriteJsonAsync(response, HttpStatusCode.Created, new { message = register.IsAdmin ? "Admin created successfully" : "User created successfully" });
            }

            catch (InvalidOperationException exception)
            {
                await WriteJsonAsync(response, HttpStatusCode.Conflict, new { error = exception.Message });
            }

        }

        private async Task HandleLogoutAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            string? authHeader = request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ") || string.IsNullOrWhiteSpace(authHeader.Substring("Bearer ".Length).Trim()))
            {
                await WriteJsonAsync(response, HttpStatusCode.Unauthorized, new { error = "missing bearer token" });
                return;
            }

            string token = authHeader.Substring("Bearer ".Length).Trim();

            var matchingUser = _auth.GetUsernameByToken(token);
            if (matchingUser == null)
            {
                await WriteJsonAsync(response, HttpStatusCode.Unauthorized, new { error = "Invalid or expired token" });
                return;
            }

            _auth.Logout(matchingUser);

            await WriteJsonAsync(response, HttpStatusCode.OK, new { message = "Logout successful" });
        }

        private async Task HandleProfileAsync(HttpListenerContext context, string usernameInPath)
        {
            var request = context.Request;
            var response = context.Response;

            string? authHeader = request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ") || string.IsNullOrWhiteSpace(authHeader.Substring("Bearer ".Length)))
            {
                await WriteJsonAsync(response, HttpStatusCode.Unauthorized, new { error = "Missing bearer token" });
                return;
            }

            string token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                _auth.ValidateToken(usernameInPath, token);
            }
            catch (UnauthorizedAccessException exception)
            {
                await WriteJsonAsync(response, HttpStatusCode.Unauthorized, new { error = exception.Message });
                return;
            }

            var user = _users.GetUser(usernameInPath);
            if (user is null)
            {
                await WriteJsonAsync(response, HttpStatusCode.NotFound, new { error = "User not found" });
                return;
            }

            var profile = new { username = user.Username, createdAt = user.CreatedAt };

            await WriteJsonAsync(response, HttpStatusCode.OK, profile);
        }

        private static async Task<T?> ReadJsonAsync<T>(HttpListenerRequest request)
        {
            if (request.InputStream == Stream.Null || request.ContentEncoding == null)
            {
                return default;
            }

            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            string body = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(body, JsonOptions);
        }

        private static async Task WriteJsonAsync(HttpListenerResponse response, HttpStatusCode status, object payload)
        {
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions);
            await WriteBytesAsync(response, status, bytes, "application/json");
        }

        private static async Task WriteBytesAsync(HttpListenerResponse response, HttpStatusCode status, byte[] bytes, string contentType)
        {
            response.StatusCode = (int)status;
            response.ContentType = contentType;
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}