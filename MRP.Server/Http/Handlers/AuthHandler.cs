using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MRP.Server.Services;

namespace MRP.Server.Http.Handlers
{
    public sealed class AuthHandler
    {
        private readonly IAuthManager _authManager;

        public AuthHandler(IAuthManager authManager)
        {
            _authManager = authManager;
        }

        public async Task LoginAsync(RequestContext context)
        {
            var loginRequest = await JsonSerializer.DeserializeAsync<LoginRequest>(context.Request.InputStream);

            string token = _authManager.Login(loginRequest!.Username, loginRequest.Password);

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { token });
        }

        public async Task LogoutAsync(RequestContext context)
        {
            string? tokenHeader = context.Request.Headers["Authorization"];

            if (tokenHeader is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "Missing token" });
                return;
            }

            string? usernameFromToken = _authManager.GetUsernameByToken(tokenHeader);
            if (usernameFromToken is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "Invalid token" });
                return;
            }

            _authManager.Logout(usernameFromToken);

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { success = true });
        }
    }
}
