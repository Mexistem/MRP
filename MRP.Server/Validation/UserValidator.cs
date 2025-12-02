using System.Text.RegularExpressions;

namespace MRP.Server.Validation
{
    public static class UserValidator
    {
        private const string UsernamePattern = @"^[a-zA-Z0-9_]+$";

        public static void ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is not allowed to be empty or contain only whitespace");

            if (!Regex.IsMatch(username, UsernamePattern))
                throw new ArgumentException("Username cannot contain special characters");

            if (username.Length < 3 || username.Length > 30)
                throw new ArgumentException("Username must be between 3 and 30 characters long");
        }
    }
}