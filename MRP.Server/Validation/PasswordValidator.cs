using System.Text.RegularExpressions;

namespace MRP.Server.Validation
{
    public static class PasswordValidator
    {
        private const string PasswordSpecialPattern = @"[!@#$%&*(),.)""':{}|<>]";

        public static void ValidatePassword(string password, string username)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is not allowed to be empty or contain only whitespace");

            if (password.Length < 8 ||
                !Regex.IsMatch(password, @"[0-9]") ||
                !Regex.IsMatch(password, PasswordSpecialPattern))
            {
                throw new ArgumentException("Password must be at least 8 characters long and include a number and special character");
            }

            if (password.ToLower().Contains(username.ToLower()))
                throw new ArgumentException("Password is not allowed to contain the username");
        }
    }
}