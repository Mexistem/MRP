using System.Net;
using System.Text.Json;
using MRP.Server.Services;
using MRP.Server.Storage;

namespace MRP.Server.Http.Handlers
{
    public sealed class RatingHandler
    {
        private readonly IRatingManager _ratingManager;
        private readonly IMediaRepository _mediaRepository;

        public RatingHandler(IRatingManager ratingManager, IMediaRepository mediaRepository)
        {
            _ratingManager = ratingManager ?? throw new ArgumentNullException(nameof(ratingManager));
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
        }

        public async Task Create(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            if (!string.Equals(context.Request.ContentType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            RatingCreateRequest? request;

            try
            {
                request = await JsonSerializer.DeserializeAsync<RatingCreateRequest>(context.Request.InputStream);
            }
            catch
            {
                context.Response.StatusCode = 400;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            if (request is null ||
                string.IsNullOrWhiteSpace(request.MediaTitle) ||
                string.IsNullOrWhiteSpace(request.Username))
            {
                context.Response.StatusCode = 400;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            var trimmedTitle = request.MediaTitle.Trim();
            bool exists = _mediaRepository.GetAll().Any(m => m.Title.Equals(trimmedTitle, StringComparison.OrdinalIgnoreCase));

            if (!exists)
            {
                context.Response.StatusCode = 404;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "media not found" });
                return;
            }

            try
            {
                var rating = _ratingManager.CreateRating(
                    trimmedTitle,
                    request.Username.Trim(),
                    request.Value,
                    request.Comment);

                context.Response.StatusCode = 201;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, rating);
            }
            catch (ArgumentException ex)
            {
                context.Response.StatusCode = 400;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
        }

        public async Task GetAllForMedia(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            if (!context.Parameters.TryGetValue("title", out var title))
            {
                context.Response.StatusCode = 400;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid title" });
                return;
            }

            var trimmedTitle = title.Trim();

            var ratings = _ratingManager
                .GetRatingsForMedia(trimmedTitle)
                .ToList();

            context.Response.StatusCode = 200;
            await JsonSerializer.SerializeAsync(context.Response.OutputStream, ratings);
        }
    }
}