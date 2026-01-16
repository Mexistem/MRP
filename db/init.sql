CREATE EXTENSION IF NOT EXISTS citext;

CREATE TABLE IF NOT EXISTS users (
  username   CITEXT PRIMARY KEY,
  password   TEXT   NOT NULL,
  role       TEXT   NOT NULL DEFAULT 'User',
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS media_entries (
  title            CITEXT PRIMARY KEY,
  description      TEXT NOT NULL,
  release_year     INT  NOT NULL,
  age_restriction  INT  NOT NULL,
  type             TEXT NOT NULL,
  created_by       CITEXT NOT NULL REFERENCES users(username) ON DELETE CASCADE,
  created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  last_modified_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS media_genres (
  media_title CITEXT NOT NULL REFERENCES media_entries(title) ON DELETE CASCADE,
  genre       TEXT   NOT NULL,
  PRIMARY KEY (media_title, genre)
);

CREATE TABLE IF NOT EXISTS ratings (
  media_title      CITEXT NOT NULL REFERENCES media_entries(title) ON DELETE CASCADE,
  username         CITEXT NOT NULL REFERENCES users(username) ON DELETE CASCADE,
  value            INT    NOT NULL CHECK (value BETWEEN 1 AND 5),
  comment          TEXT,
  created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  last_modified_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  PRIMARY KEY (media_title, username)
);

CREATE TABLE IF NOT EXISTS rating_likes (
  media_title      CITEXT NOT NULL,
  rating_username  CITEXT NOT NULL,
  liked_by         CITEXT NOT NULL REFERENCES users(username) ON DELETE CASCADE,
  created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  PRIMARY KEY (media_title, rating_username, liked_by),
  FOREIGN KEY (media_title, rating_username)
    REFERENCES ratings(media_title, username)
    ON DELETE CASCADE
    ON UPDATE CASCADE
);

CREATE TABLE IF NOT EXISTS favorites (
  username    CITEXT NOT NULL REFERENCES users(username) ON DELETE CASCADE,
  media_title CITEXT NOT NULL REFERENCES media_entries(title) ON DELETE CASCADE ON UPDATE CASCADE,
  created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  PRIMARY KEY (username, media_title)
);

CREATE OR REPLACE FUNCTION set_last_modified_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.last_modified_at = NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_media_entries_last_modified ON media_entries;
CREATE TRIGGER trg_media_entries_last_modified
BEFORE UPDATE ON media_entries
FOR EACH ROW
EXECUTE FUNCTION set_last_modified_at();

DROP TRIGGER IF EXISTS trg_ratings_last_modified ON ratings;
CREATE TRIGGER trg_ratings_last_modified
BEFORE UPDATE ON ratings
FOR EACH ROW
EXECUTE FUNCTION set_last_modified_at();

CREATE INDEX IF NOT EXISTS idx_ratings_media_title ON ratings(media_title);
CREATE INDEX IF NOT EXISTS idx_ratings_username   ON ratings(username);

CREATE INDEX IF NOT EXISTS idx_rating_likes_rating
  ON rating_likes(media_title, rating_username);

CREATE INDEX IF NOT EXISTS idx_rating_likes_liked_by
  ON rating_likes(liked_by);

CREATE INDEX IF NOT EXISTS idx_favorites_username
  ON favorites(username);

CREATE INDEX IF NOT EXISTS idx_favorites_media_title
  ON favorites(media_title);
