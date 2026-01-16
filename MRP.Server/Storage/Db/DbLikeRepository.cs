using System;
using MRP.Server.Storage.Interfaces;
using Npgsql;

namespace MRP.Server.Storage.Db
{
    public sealed class DbLikeRepository : ILikeRepository
    {
        private readonly string _connectionString;

        public DbLikeRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        private NpgsqlConnection OpenConnection()
        {
            var c = new NpgsqlConnection(_connectionString);
            c.Open();
            return c;
        }

        public bool Exists(string mediaTitle, string ratingUsername, string likedBy)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT 1
                  FROM rating_likes
                  WHERE LOWER(media_title) = LOWER(@mediaTitle)
                    AND LOWER(rating_username) = LOWER(@ratingUsername)
                    AND LOWER(liked_by) = LOWER(@likedBy)
                  LIMIT 1",
                conn);

            cmd.Parameters.AddWithValue("mediaTitle", mediaTitle);
            cmd.Parameters.AddWithValue("ratingUsername", ratingUsername);
            cmd.Parameters.AddWithValue("likedBy", likedBy);

            return cmd.ExecuteScalar() is not null;
        }

        public void Add(string mediaTitle, string ratingUsername, string likedBy)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"INSERT INTO rating_likes (media_title, rating_username, liked_by)
                  VALUES (@mediaTitle, @ratingUsername, @likedBy)",
                conn);

            cmd.Parameters.AddWithValue("mediaTitle", mediaTitle);
            cmd.Parameters.AddWithValue("ratingUsername", ratingUsername);
            cmd.Parameters.AddWithValue("likedBy", likedBy);

            cmd.ExecuteNonQuery();
        }

        public void Remove(string mediaTitle, string ratingUsername, string likedBy)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"DELETE FROM rating_likes
                  WHERE LOWER(media_title) = LOWER(@mediaTitle)
                    AND LOWER(rating_username) = LOWER(@ratingUsername)
                    AND LOWER(liked_by) = LOWER(@likedBy)",
                conn);

            cmd.Parameters.AddWithValue("mediaTitle", mediaTitle);
            cmd.Parameters.AddWithValue("ratingUsername", ratingUsername);
            cmd.Parameters.AddWithValue("likedBy", likedBy);

            cmd.ExecuteNonQuery();
        }

        public int CountForRating(string mediaTitle, string ratingUsername)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT COUNT(*)
                  FROM rating_likes
                  WHERE LOWER(media_title) = LOWER(@mediaTitle)
                    AND LOWER(rating_username) = LOWER(@ratingUsername)",
                conn);

            cmd.Parameters.AddWithValue("mediaTitle", mediaTitle);
            cmd.Parameters.AddWithValue("ratingUsername", ratingUsername);

            var countObj = cmd.ExecuteScalar();
            return Convert.ToInt32(countObj);
        }

        public void DeleteByMediaTitle(string mediaTitle)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"DELETE FROM rating_likes
          WHERE LOWER(media_title) = LOWER(@mediaTitle)",
                conn);

            cmd.Parameters.AddWithValue("mediaTitle", mediaTitle);
            cmd.ExecuteNonQuery();
        }

        public void DeleteByUser(string username)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"DELETE FROM rating_likes
          WHERE LOWER(liked_by) = LOWER(@u)
             OR LOWER(rating_username) = LOWER(@u)",
                conn);

            cmd.Parameters.AddWithValue("u", username);
            cmd.ExecuteNonQuery();
        }

    }
}
