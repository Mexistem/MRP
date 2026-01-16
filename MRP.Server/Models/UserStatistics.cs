using System;
using System.Text.Json.Serialization;

namespace MRP.Server.Models
{
    public sealed class UserStatistics
    {
        [JsonPropertyName("totalRatings")]
        public int TotalRatings { get; init; }

        [JsonPropertyName("totalLikesReceived")]
        public int TotalLikesReceived { get; init; }

        [JsonPropertyName("averageScore")]
        public double AverageScore { get; init; }

        [JsonPropertyName("favoriteGenre")]
        public string? FavoriteGenre { get; init; }

        [JsonPropertyName("ratedMediaCount")]
        public int RatedMediaCount { get; init; }

        [JsonPropertyName("highestScore")]
        public int? HighestScore { get; init; }

        [JsonPropertyName("lowestScore")]
        public int? LowestScore { get; init; }

        [JsonPropertyName("totalFavorites")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int TotalFavorites { get; init; } 

        [JsonPropertyName("lastRatedAt")]

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? LastRatedAt { get; init; } 
    }
}
