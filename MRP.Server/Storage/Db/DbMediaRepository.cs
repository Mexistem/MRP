using MRP.Server.Models;
using MRP.Server.Storage.Interfaces;
using Npgsql;

namespace MRP.Server.Storage.Db
{
    public sealed class DbMediaRepository : IMediaRepository
    {
        private readonly string _connectionString;

        public DbMediaRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        private NpgsqlConnection OpenConnection()
        {
            var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public bool ExistsByTitle(string title)
        {
            using var connection = OpenConnection();
            using var command = new NpgsqlCommand(
                "SELECT 1 FROM media_entries WHERE LOWER(title) = LOWER(@title)",
                connection);

            command.Parameters.AddWithValue("title", title);
            return command.ExecuteScalar() != null;
        }

        public MediaEntry? GetByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return null;

            using var connection = OpenConnection();
            using var command = new NpgsqlCommand(
                @"SELECT m.title,
                         m.description,
                         m.release_year,
                         m.age_restriction,
                         m.type,
                         m.created_by,
                         m.created_at,
                         m.last_modified_at,
                         g.genre
                  FROM media_entries m
                  LEFT JOIN media_genres g ON g.media_title = m.title
                  WHERE LOWER(m.title) = LOWER(@title)",
                connection);

            command.Parameters.AddWithValue("title", title);

            using var reader = command.ExecuteReader();

            MediaEntry? media = null;
            var genres = new List<string>();

            while (reader.Read())
            {
                if (media is null)
                {
                    media = MediaEntry.FromDatabase(
                        title: reader.GetString(0),
                        description: reader.GetString(1),
                        releaseYear: reader.GetInt32(2),
                        genres: genres,
                        ageRestriction: reader.GetInt32(3),
                        type: Enum.Parse<MediaType>(reader.GetString(4), true),
                        createdBy: reader.GetString(5),
                        createdAt: reader.GetDateTime(6),
                        lastModifiedAt: reader.GetDateTime(7)
                    );
                }

                if (!reader.IsDBNull(8))
                    genres.Add(reader.GetString(8));
            }

            return media;
        }

        public void Add(MediaEntry media)
        {
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();

            using (var command = new NpgsqlCommand(
                @"INSERT INTO media_entries
                  (title, description, release_year, age_restriction, type, created_by, created_at)
                  VALUES (@title, @description, @releaseYear, @ageRestriction, @type, @createdBy, @createdAt)",
                connection, transaction))
            {
                command.Parameters.AddWithValue("title", media.Title);
                command.Parameters.AddWithValue("description", media.Description);
                command.Parameters.AddWithValue("releaseYear", media.ReleaseYear);
                command.Parameters.AddWithValue("ageRestriction", media.AgeRestriction);
                command.Parameters.AddWithValue("type", media.Type.ToString());
                command.Parameters.AddWithValue("createdBy", media.CreatedBy);
                command.Parameters.AddWithValue("createdAt", media.CreatedAt);

                command.ExecuteNonQuery();
            }

            foreach (var genre in media.Genres)
            {
                using var command = new NpgsqlCommand(
                    @"INSERT INTO media_genres (media_title, genre)
                      VALUES (@title, @genre)",
                    connection, transaction);

                command.Parameters.AddWithValue("title", media.Title);
                command.Parameters.AddWithValue("genre", genre);

                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public void Update(MediaEntry media)
        {
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();

            using (var command = new NpgsqlCommand(
                @"UPDATE media_entries
                  SET description = @description,
                      release_year = @releaseYear,
                      age_restriction = @ageRestriction,
                      type = @type
                  WHERE LOWER(title) = LOWER(@title)",
                connection, transaction))
            {
                command.Parameters.AddWithValue("title", media.Title);
                command.Parameters.AddWithValue("description", media.Description);
                command.Parameters.AddWithValue("releaseYear", media.ReleaseYear);
                command.Parameters.AddWithValue("ageRestriction", media.AgeRestriction);
                command.Parameters.AddWithValue("type", media.Type.ToString());

                var affected = command.ExecuteNonQuery();
                if (affected == 0)
                    throw new KeyNotFoundException("Media entry not found.");
            }

            using (var del = new NpgsqlCommand(
                @"DELETE FROM media_genres WHERE LOWER(media_title) = LOWER(@title)",
                connection, transaction))
            {
                del.Parameters.AddWithValue("title", media.Title);
                del.ExecuteNonQuery();
            }

            foreach (var genre in media.Genres)
            {
                using var ins = new NpgsqlCommand(
                    @"INSERT INTO media_genres (media_title, genre)
                      VALUES (@title, @genre)",
                    connection, transaction);

                ins.Parameters.AddWithValue("title", media.Title);
                ins.Parameters.AddWithValue("genre", genre);
                ins.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public void Delete(string title)
        {
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();

            using (var delGenres = new NpgsqlCommand(
                @"DELETE FROM media_genres WHERE LOWER(media_title) = LOWER(@title)",
                connection, transaction))
            {
                delGenres.Parameters.AddWithValue("title", title);
                delGenres.ExecuteNonQuery();
            }

            using (var delMedia = new NpgsqlCommand(
                @"DELETE FROM media_entries WHERE LOWER(title) = LOWER(@title)",
                connection, transaction))
            {
                delMedia.Parameters.AddWithValue("title", title);
                var affected = delMedia.ExecuteNonQuery();
                if (affected == 0)
                    throw new KeyNotFoundException("Media entry not found.");
            }

            transaction.Commit();
        }

        public IEnumerable<MediaEntry> GetAll()
        {
            var result = new List<MediaEntry>();

            using var connection = OpenConnection();
            using var command = new NpgsqlCommand(
                @"SELECT m.title,
                         m.description,
                         m.release_year,
                         m.age_restriction,
                         m.type,
                         m.created_by,
                         m.created_at,
                         m.last_modified_at,
                         g.genre
                  FROM media_entries m
                  LEFT JOIN media_genres g ON g.media_title = m.title
                  ORDER BY m.created_at",
                connection);

            using var reader = command.ExecuteReader();

            var byTitle = new Dictionary<string, MediaEntry>(StringComparer.OrdinalIgnoreCase);

            while (reader.Read())
            {
                var title = reader.GetString(0);

                if (!byTitle.TryGetValue(title, out var media))
                {
                    media = MediaEntry.FromDatabase(
                        title: title,
                        description: reader.GetString(1),
                        releaseYear: reader.GetInt32(2),
                        genres: new List<string>(),
                        ageRestriction: reader.GetInt32(3),
                        type: Enum.Parse<MediaType>(reader.GetString(4), true),
                        createdBy: reader.GetString(5),
                        createdAt: reader.GetDateTime(6),
                        lastModifiedAt: reader.GetDateTime(7)
                    );

                    byTitle[title] = media;
                    result.Add(media);
                }

                if (!reader.IsDBNull(8))
                {
                    media.Genres.Add(reader.GetString(8));
                }
            }

            return result;
        }

        public void Rename(string oldTitle, MediaEntry renamedEntry)
        {
            if (renamedEntry is null)
            {
                throw new ArgumentNullException(nameof(renamedEntry));
            }

            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();

            using (var dup = new NpgsqlCommand(
                @"SELECT 1
                  FROM media_entries
                  WHERE LOWER(title) = LOWER(@newTitle)
                  LIMIT 1",
                connection, transaction))
            {
                dup.Parameters.AddWithValue("newTitle", renamedEntry.Title);

                var exists = dup.ExecuteScalar() != null;
                if (exists && !string.Equals(oldTitle, renamedEntry.Title, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Title already exists");
                }
            }

            using (var cmd = new NpgsqlCommand(
                @"UPDATE media_entries
                  SET title = @newTitle,
                      description = @description,
                      release_year = @releaseYear,
                      age_restriction = @ageRestriction,
                      type = @type,
                      last_modified_at = @lastModifiedAt
                  WHERE LOWER(title) = LOWER(@oldTitle)",
                connection, transaction))
            {
                cmd.Parameters.AddWithValue("oldTitle", oldTitle);
                cmd.Parameters.AddWithValue("newTitle", renamedEntry.Title);
                cmd.Parameters.AddWithValue("description", renamedEntry.Description);
                cmd.Parameters.AddWithValue("releaseYear", renamedEntry.ReleaseYear);
                cmd.Parameters.AddWithValue("ageRestriction", renamedEntry.AgeRestriction);
                cmd.Parameters.AddWithValue("type", renamedEntry.Type.ToString());
                cmd.Parameters.AddWithValue("lastModifiedAt", renamedEntry.LastModifiedAt);

                var affected = cmd.ExecuteNonQuery();
                if (affected == 0)
                {
                    throw new KeyNotFoundException("Media entry not found.");
                }
            }

            using (var updateGenresFk = new NpgsqlCommand(
                @"UPDATE media_genres
                  SET media_title = @newTitle
                  WHERE LOWER(media_title) = LOWER(@oldTitle)",
                connection, transaction))
            {
                updateGenresFk.Parameters.AddWithValue("oldTitle", oldTitle);
                updateGenresFk.Parameters.AddWithValue("newTitle", renamedEntry.Title);
                updateGenresFk.ExecuteNonQuery();
            }

            using (var deleteGenres = new NpgsqlCommand(
                @"DELETE FROM media_genres
                  WHERE LOWER(media_title) = LOWER(@newTitle)",
                connection, transaction))
            {
                deleteGenres.Parameters.AddWithValue("newTitle", renamedEntry.Title);
                deleteGenres.ExecuteNonQuery();
            }

            foreach (var genre in renamedEntry.Genres)
            {
                using var ins = new NpgsqlCommand(
                    @"INSERT INTO media_genres (media_title, genre)
                      VALUES (@title, @genre)",
                    connection, transaction);

                ins.Parameters.AddWithValue("title", renamedEntry.Title);
                ins.Parameters.AddWithValue("genre", genre);
                ins.ExecuteNonQuery();
            }

            using (var moveRatings = new NpgsqlCommand(
                @"UPDATE ratings
                  SET media_title = @newTitle
                  WHERE LOWER(media_title) = LOWER(@oldTitle)",
                connection, transaction))
            {
                moveRatings.Parameters.AddWithValue("oldTitle", oldTitle);
                moveRatings.Parameters.AddWithValue("newTitle", renamedEntry.Title);
                moveRatings.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }
}
