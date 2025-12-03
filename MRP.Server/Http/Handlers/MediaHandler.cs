using System.Net;
using System.Text.Json;
using MRP.Server.Services;
using MRP.Server.Storage;

namespace MRP.Server.Http.Handlers
{
    public sealed class MediaHandler
    {
        private readonly IMediaRepository _repository;
        private readonly IMediaManager _mediaManager;

        public MediaHandler(IMediaRepository repository, IMediaManager mediaManager)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mediaManager = mediaManager ?? throw new ArgumentNullException(nameof(mediaManager));
        }

        public async Task GetAll(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            var items = _repository
                .GetAll()
                .ToList();

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await JsonSerializer.SerializeAsync(context.Response.OutputStream, items);
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

            MediaCreateRequest? request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<MediaCreateRequest>(context.Request.InputStream);
            }
            catch
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            if (request is null ||
                string.IsNullOrWhiteSpace(request.Title) ||
                string.IsNullOrWhiteSpace(request.Description) ||
                request.Genres is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            try
            {
                var entry = _mediaManager.CreateMedia(
                    request.Title,
                    request.Description,
                    request.ReleaseYear,
                    request.Genres,
                    request.AgeRestriction,
                    request.Type,
                    request.CreatedBy);

                context.Response.StatusCode = (int)HttpStatusCode.Created;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, entry);
            }
            catch (ArgumentException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
        }
    }
}
