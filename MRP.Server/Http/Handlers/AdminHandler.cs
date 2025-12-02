using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MRP.Server.Services;

namespace MRP.Server.Http.Handlers
{
    public sealed class AdminHandler
    {
        private readonly IUserManager _userManager;
        private readonly IAuthManager _authManager;

        public AdminHandler(IUserManager userManager, IAuthManager authManager)
        {
            _userManager = userManager;
            _authManager = authManager;
        }

        private bool IsRequestFromAdmin(RequestContext context)
        {
            string? tokenHeader = context.Request.Headers["Authorization"];
            if (tokenHeader is null) return false;

            string? usernameFromToken = _authManager.GetUsernameByToken(tokenHeader);
            if (usernameFromToken is null) return false;

            return _authManager.IsAdmin(usernameFromToken);
        }

        public async Task RegisterAdminAsync(RequestContext context)
        {
            if (!IsRequestFromAdmin(context))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "Forbidden" });
                return;
            }

            var registerRequest = await JsonSerializer.DeserializeAsync<RegisterRequest>(context.Request.InputStream);

            _userManager.RegisterAdmin(registerRequest!.Username, registerRequest.Password);

            context.Response.StatusCode = (int)HttpStatusCode.Created;
            await JsonSerializer.SerializeAsync(context.Response.OutputStream,
                new { message = "Admin created successfully" });
        }

        public async Task ListUsersAsync(RequestContext context)
        {
            if (!IsRequestFromAdmin(context))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "Forbidden" });
                return;
            }

            var allUsers = _userManager.GetAllUsers()
                .Select(user => new
                {
                    username = user.Username,
                    role = user.Role,
                    createdAt = user.CreatedAt
                });

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await JsonSerializer.SerializeAsync(context.Response.OutputStream, allUsers);
        }

        public async Task DeleteUserAsync(RequestContext context)
        {
            if (!IsRequestFromAdmin(context))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "Forbidden" });
                return;
            }

            var queryString = context.Request.Url!.Query; 
            var parsedQuery = System.Web.HttpUtility.ParseQueryString(queryString);
            string? usernameToDelete = parsedQuery.Get("username");

            if (string.IsNullOrWhiteSpace(usernameToDelete))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "Missing username parameter" });
                return;
            }

            _userManager.DeleteUser(usernameToDelete);

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await JsonSerializer.SerializeAsync(context.Response.OutputStream,
                new { message = $"User '{usernameToDelete}' deleted (if existed)." });
        }
    }
}
