using MRP.Server.Models;
using Npgsql;
using MRP.Server.Storage.Interfaces;

namespace MRP.Server.Storage.Db
{
    public sealed class DbUserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public DbUserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private NpgsqlConnection OpenConnection()
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        public bool Exists(string username)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand("SELECT 1 FROM users WHERE username = @u LIMIT 1", conn);
            cmd.Parameters.AddWithValue("u", username);

            var result = cmd.ExecuteScalar();
            return result != null;
        }

        public User? Get(string username)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT username, password, role, created_at FROM users WHERE username = @u",
                conn
            );
            cmd.Parameters.AddWithValue("u", username);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            var uname = reader.GetString(0);
            var pwHash = reader.GetString(1);
            var role = reader.GetString(2);
            var createdAt = reader.GetDateTime(3);

            return CreateUserFromDb(uname, pwHash, role, createdAt);
        }

        public void Add(User user)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                "INSERT INTO users (username, password, role, created_at) VALUES (@u, @p, @r, @c)",
                conn
            );

            cmd.Parameters.AddWithValue("u", user.Username);
            cmd.Parameters.AddWithValue("p", user.Password);      
            cmd.Parameters.AddWithValue("r", user.Role);
            cmd.Parameters.AddWithValue("c", user.CreatedAt);

            cmd.ExecuteNonQuery();
        }

        public IEnumerable<User> GetAll()
        {
            var users = new List<User>();

            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT username, password, role, created_at FROM users ORDER BY created_at ASC",
                conn
            );

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var uname = reader.GetString(0);
                var pwHash = reader.GetString(1);
                var role = reader.GetString(2);
                var createdAt = reader.GetDateTime(3);

                users.Add(CreateUserFromDb(uname, pwHash, role, createdAt));
            }

            return users;
        }

        public void Delete(string username)
        {
            using var conn = OpenConnection();
            using var cmd = new NpgsqlCommand("DELETE FROM users WHERE username = @u", conn);
            cmd.Parameters.AddWithValue("u", username);
            cmd.ExecuteNonQuery();
        }

        private static User CreateUserFromDb(string username, string passwordHash, string role, DateTime createdAt)
        {
            
            return new User(username, passwordHash, role, createdAt);
        }
    }
}
