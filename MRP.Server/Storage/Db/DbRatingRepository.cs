using MRP.Server.Models;
using MRP.Server.Storage.Interfaces;
using Npgsql;

namespace MRP.Server.Storage.Db
{
    public sealed class DbRatingRepository : IRatingRepository
    {
        private readonly string _connectionString;

        public DbRatingRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        private NpgsqlConnection OpenConnection()
        {
            var c = new NpgsqlConnection(_connectionString);
            c.Open();
            return c;
        }

        public IEnumerable<RatingEntry> GetAll()
        {
            var result = new List<RatingEntry>();

            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT media_title, username, value, comment, created_at
                  FROM ratings",
                conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(RatingEntry.FromDatabase(
                    mediaTitle: reader.GetString(0),
                    username: reader.GetString(1),
                    value: reader.GetInt32(2),
                    comment: reader.IsDBNull(3) ? null : reader.GetString(3),
                    createdAt: reader.GetDateTime(4)
                ));
            }

            return result;
        }

        public IEnumerable<RatingEntry> GetByMediaTitle(string mediaTitle)
        {
            var result = new List<RatingEntry>();

            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT media_title, username, value, comment, created_at
                  FROM ratings
                  WHERE LOWER(media_title) = LOWER(@title)",
                conn);

            cmd.Parameters.AddWithValue("title", mediaTitle);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(RatingEntry.FromDatabase(
                    mediaTitle: reader.GetString(0),
                    username: reader.GetString(1),
                    value: reader.GetInt32(2),
                    comment: reader.IsDBNull(3) ? null : reader.GetString(3),
                    createdAt: reader.GetDateTime(4)
                ));
            }

            return result;
        }

        public RatingEntry? GetByMediaTitleAndUsername(string mediaTitle, string username)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT media_title, username, value, comment, created_at
                  FROM ratings
                  WHERE LOWER(media_title) = LOWER(@title)
                  AND LOWER(username) = LOWER(@username)",
                conn);

            cmd.Parameters.AddWithValue("title", mediaTitle);
            cmd.Parameters.AddWithValue("username", username);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            return RatingEntry.FromDatabase(
                mediaTitle: reader.GetString(0),
                username: reader.GetString(1),
                value: reader.GetInt32(2),
                comment: reader.IsDBNull(3) ? null : reader.GetString(3),
                createdAt: reader.GetDateTime(4)
            );
        }

        public void Add(RatingEntry rating)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"INSERT INTO ratings (media_title, username, value, comment, created_at)
                  VALUES (@title, @username, @value, @comment, @createdAt)",
                conn);

            cmd.Parameters.AddWithValue("title", rating.MediaTitle);
            cmd.Parameters.AddWithValue("username", rating.Username);
            cmd.Parameters.AddWithValue("value", rating.Value);
            cmd.Parameters.AddWithValue("comment", (object?)rating.Comment ?? DBNull.Value);
            cmd.Parameters.AddWithValue("createdAt", rating.CreatedAt);

            cmd.ExecuteNonQuery();
        }

        public void Update(RatingEntry rating)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"UPDATE ratings
                  SET value=@value, comment=@comment
                  WHERE LOWER(media_title)=LOWER(@title)
                  AND LOWER(username)=LOWER(@username)",
                conn);

            cmd.Parameters.AddWithValue("title", rating.MediaTitle);
            cmd.Parameters.AddWithValue("username", rating.Username);
            cmd.Parameters.AddWithValue("value", rating.Value);
            cmd.Parameters.AddWithValue("comment", (object?)rating.Comment ?? DBNull.Value);

            var affected = cmd.ExecuteNonQuery();
            if (affected == 0)
                throw new KeyNotFoundException("Rating does not exist and cannot be updated");
        }

        public void DeleteByMediaTitle(string mediaTitle)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"DELETE FROM ratings WHERE LOWER(media_title)=LOWER(@title)",
                conn);

            cmd.Parameters.AddWithValue("title", mediaTitle);
            cmd.ExecuteNonQuery();
        }

        public bool DeleteRating(string mediaTitle, string username)
        {
            if (string.IsNullOrWhiteSpace(mediaTitle))
            {
                throw new ArgumentException("Media title is required.", nameof(mediaTitle));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username is required.", nameof(username));
            }

            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"DELETE FROM ratings
          WHERE LOWER(media_title) = LOWER(@title)
          AND LOWER(username) = LOWER(@username)",
                conn);

            cmd.Parameters.AddWithValue("title", mediaTitle);
            cmd.Parameters.AddWithValue("username", username);

            var affected = cmd.ExecuteNonQuery();

            if (affected == 0)
            {
                return false;
            }

            return true;
        }

        public void RenameMediaTitle(string oldTitle, string newTitle)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"UPDATE ratings
          SET media_title = @newTitle
          WHERE LOWER(media_title) = LOWER(@oldTitle)",
                conn);

            cmd.Parameters.AddWithValue("oldTitle", oldTitle);
            cmd.Parameters.AddWithValue("newTitle", newTitle);
            cmd.ExecuteNonQuery();
        }

        public void DeleteByUsername(string username)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"DELETE FROM ratings
          WHERE LOWER(username) = LOWER(@username)",
                conn);

            cmd.Parameters.AddWithValue("username", username);
            cmd.ExecuteNonQuery();
        }

    }
}
