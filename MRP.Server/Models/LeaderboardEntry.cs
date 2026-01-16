using System.Text.Json.Serialization;

namespace MRP.Server.Models
{
    public sealed class LeaderboardEntry
    {
        [JsonPropertyName("username")]
        public string Username { get; init; } = string.Empty;

        [JsonPropertyName("totalRatings")]
        public int TotalRatings { get; init; }

        [JsonPropertyName("totalLikesReceived")]
        public int TotalLikesReceived { get; init; }

        [JsonPropertyName("averageScore")]
        public double AverageScore { get; init; }
    }
}
