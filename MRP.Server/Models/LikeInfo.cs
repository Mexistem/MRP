namespace MRP.Server.Models
{
    public sealed class LikeInfo
    {
        public string MediaTitle { get; }
        public string RatingUsername { get; }
        public int LikeCount { get; }

        public LikeInfo(string mediaTitle, string ratingUsername, int likeCount)
        {
            MediaTitle = mediaTitle;
            RatingUsername = ratingUsername;
            LikeCount = likeCount;
        }
    }
}
