using System;
using System.Collections.Generic;
using MRP.Server.Storage.Interfaces;
using Npgsql;

namespace MRP.Server.Storage.Db
{
    public sealed class DbFavoriteRepository : IFavoriteRepository
    {
        private readonly string _connectionString;

        public DbFavoriteRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        private NpgsqlConnection OpenConnection()
        {
            var c = new NpgsqlConnection(_connectionString);
            c.Open();
            return c;
        }

        public bool Exists(string username, string mediaTitle)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT 1
                  FROM favorites
                  WHERE LOWER(username) = LOWER(@username)
                    AND LOWER(media_title) = LOWER(@mediaTitle)
                  LIMIT 1",
                conn);

            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("mediaTitle", mediaTitle);

            return cmd.ExecuteScalar() is not null;
        }

        public void Add(string username, string mediaTitle)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"INSERT INTO favorites (username, media_title)
                  VALUES (@username, @mediaTitle)",
                conn);

            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("mediaTitle", mediaTitle);

            cmd.ExecuteNonQuery();
        }

        public void Remove(string username, string mediaTitle)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"DELETE FROM favorites
                  WHERE LOWER(username) = LOWER(@username)
                    AND LOWER(media_title) = LOWER(@mediaTitle)",
                conn);

            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("mediaTitle", mediaTitle);

            cmd.ExecuteNonQuery();
        }

        public IEnumerable<string> GetFavoriteMediaTitles(string username)
        {
            var result = new List<string>();

            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT media_title
                  FROM favorites
                  WHERE LOWER(username) = LOWER(@username)
                  ORDER BY created_at",
                conn);

            cmd.Parameters.AddWithValue("username", username);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(reader.GetString(0));
            }

            return result;
        }

        public void DeleteByUsername(string username)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"DELETE FROM favorites
          WHERE LOWER(username) = LOWER(@username)",
                conn);

            cmd.Parameters.AddWithValue("username", username);
            cmd.ExecuteNonQuery();
        }

        public void DeleteByMediaTitle(string mediaTitle)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                @"DELETE FROM favorites
          WHERE LOWER(media_title) = LOWER(@mediaTitle)",
                conn);

            cmd.Parameters.AddWithValue("mediaTitle", mediaTitle);
            cmd.ExecuteNonQuery();
        }

    }
}
