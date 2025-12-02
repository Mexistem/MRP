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

        public async Task RegisterAsync(RequestContext context)
        {
            var registerRequest = await JsonSerializer.DeserializeAsync<RegisterRequest>(context.Request.InputStream);

            if (registerRequest!.IsAdmin)
                _userManager.RegisterAdmin(registerRequest.Username, registerRequest.Password);
            else
                _userManager.Register(registerRequest.Username, registerRequest.Password);

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { success = true });
        }

        public async Task ProfileAsync(RequestContext context)
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
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "Unauthorized" });
                return;
            }

            var user = _userManager.GetUser(usernameFromToken);

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await JsonSerializer.SerializeAsync(context.Response.OutputStream, new
            {
                username = user!.Username,
                role = user.Role,
                createdAt = user.CreatedAt
            });
        }
    }
}
