
using MRP.Server.Validation;
namespace MRP.Server.Models
{
    public sealed class RatingEntry
    {
        public string MediaTitle { get; }
        public string Username { get; }
        public int Value { get; }
        public string? Comment { get; }
        public DateTime CreatedAt { get; }

        public RatingEntry(
            string mediaTitle,
            string username,
            int value,
            string? comment)
        {
            RatingValidator.ValidateForCreate(
                mediaTitle,
                username,
                value,
                comment);

            MediaTitle = mediaTitle;
            Username = username;
            Value = value;
            Comment = comment?.Trim();
            CreatedAt = DateTime.UtcNow;
        }
    }
}