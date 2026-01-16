using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MRP.Server.Services;
using MRP.Server.Services.Interfaces;

namespace MRP.Server.Http.Handlers
{
    public sealed class LeaderboardHandler
    {
        private readonly ILeaderboardManager _leaderboardManager;
        private readonly IAuthManager _authManager;

        public LeaderboardHandler(ILeaderboardManager leaderboardManager, IAuthManager authManager)
        {
            _leaderboardManager = leaderboardManager ?? throw new ArgumentNullException(nameof(leaderboardManager));
            _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        }

        private static string? ExtractBearerToken(string? headerValue)
        {
            if (string.IsNullOrWhiteSpace(headerValue))
            {
                return null;
            }

            const string bearer = "Bearer ";
            if (headerValue.StartsWith(bearer, StringComparison.OrdinalIgnoreCase))
            {
                return headerValue.Substring(bearer.Length).Trim();
            }

            return headerValue.Trim();
        }

        private async Task<string?> RequireUsernameFromTokenAsync(RequestContext context)
        {
            string? header = context.Request.Headers["Authorization"];
            string? token = ExtractBearerToken(header);

            if (string.IsNullOrWhiteSpace(token))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "missing bearer token" });
                return null;
            }

            string? username = _authManager.GetUsernameByToken(token);
            if (username is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid or expired token" });
                return null;
            }

            return username;
        }

        public async Task Get(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            var usernameFromToken = await RequireUsernameFromTokenAsync(context);
            if (usernameFromToken is null)
            {
                return;
            }

            var list = _leaderboardManager.GetLeaderboard();

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await JsonSerializer.SerializeAsync(context.Response.OutputStream, list);
        }
    }
}
