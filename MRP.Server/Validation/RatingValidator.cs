namespace MRP.Server.Validation
{
    public static class RatingValidator
    {
        public static void ValidateForCreate(
            string mediaTitle,
            string username,
            int value,
            string? comment)
        {
            if (value < 1 || value > 5)
            {
                throw new ArgumentException("Rating value must be between 1 and 5.", nameof(value));
            }
        }
    }
}