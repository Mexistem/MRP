using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MRP.Server.Services;

namespace MRP.Server.Http.Handlers
{
    public sealed class UserHandler
    {
        private readonly IUserManager _userManager;
        private readonly IAuthManager _authManager;

        public UserHandler(IUserManager userManager, IAuthManager authManager)
        {
            _userManager = userManager;
            _authManager = authManager;
        }

        public async Task Register(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            if (string.IsNullOrWhiteSpace(context.Request.ContentType) || !context.Request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 400;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            RegisterRequest? request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<RegisterRequest>(context.Request.InputStream);
            }
            catch
            {
                context.Response.StatusCode = 400;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                context.Response.StatusCode = 400;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            try
            {
                if (request.IsAdmin)
                {
                    _userManager.RegisterAdmin(request.Username, request.Password);
                    context.Response.StatusCode = 201;
                    await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { message = "Admin created successfully" });
                }
                else
                {
                    _userManager.Register(request.Username, request.Password);
                    context.Response.StatusCode = 201;
                    await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { message = "User created successfully" });
                }
            }
            catch (InvalidOperationException)
            {
                context.Response.StatusCode = 409;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "User already exists" });
            }
            catch (ArgumentException ex)
            {
                context.Response.StatusCode = 400;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
        }

        public async Task Login(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            LoginRequest? request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<LoginRequest>(context.Request.InputStream);
            }
            catch
            {
                context.Response.StatusCode = 400;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            try
            {
                string token = _authManager.Login(request!.Username, request.Password);
                context.Response.StatusCode = 200;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { token });
            }
            catch (UnauthorizedAccessException)
            {
                context.Response.StatusCode = 401;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid credentials" });
            }
            catch (InvalidOperationException)
            {
                context.Response.StatusCode = 401;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "unknown user" });
            }
        }

        public async Task Profile(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            var path = context.Request.Url!.AbsolutePath;
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length != 4 || segments[0] != "api" || segments[1] != "users" || segments[3] != "profile")
            {
                context.Response.StatusCode = 404;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "not found" });
                return;
            }

            string usernameFromPath = segments[2];

            string? header = context.Request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(header))
            {
                context.Response.StatusCode = 401;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "missing bearer token" });
                return;
            }

            string token = header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header.Substring("Bearer ".Length).Trim() : header.Trim();

            try
            {
                _authManager.ValidateToken(usernameFromPath, token);
            }
            catch (UnauthorizedAccessException)
            {
                context.Response.StatusCode = 401;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid or expired token" });
                return;
            }

            var user = _userManager.GetUser(usernameFromPath);
            if (user is null)
            {
                context.Response.StatusCode = 404;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "not found" });
                return;
            }

            context.Response.StatusCode = 200;
            await JsonSerializer.SerializeAsync(context.Response.OutputStream, new
            {
                username = user.Username,
                role = user.Role,
                createdAt = user.CreatedAt
            });
        }

        public async Task Logout(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            string? tokenHeader = context.Request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(tokenHeader))
            {
                context.Response.StatusCode = 401;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "missing token" });
                return;
            }

            string token = tokenHeader.StartsWith("Bearer ") ? tokenHeader.Substring(7).Trim() : tokenHeader.Trim();
            string? username = _authManager.GetUsernameByToken(token);

            if (username == null)
            {
                context.Response.StatusCode = 401;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid or expired token" });
                return;
            }

            _authManager.Logout(username);

            context.Response.StatusCode = 200;
            await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { message = "logout successful" });
        }
    }
}
