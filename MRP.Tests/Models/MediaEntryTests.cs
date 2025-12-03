using MRP.Server.Models;
using MRP.Server.Storage.InMemory;
using MRP.Server.Services;

namespace MRP.Tests
{
    [TestClass]
    public class MediaEntryTests
    {
        [TestMethod]
        public void CreatingMediaEntry_ShouldStoreBasicInformation()
        {
            
            string title = "Inception";
            string description = "A mind-bending thriller";
            int releaseYear = 2010;
            var genres = new List<string> { "Sci-Fi" };
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            
            var entry = new MediaEntry(
                title,
                description,
                releaseYear,
                genres,
                ageRestriction,
                type,
                creator);

            
            Assert.AreEqual(title, entry.Title);
            Assert.AreEqual(description, entry.Description);
            Assert.AreEqual(releaseYear, entry.ReleaseYear);
            CollectionAssert.AreEqual(genres, entry.Genres);
            Assert.AreEqual(ageRestriction, entry.AgeRestriction);
            Assert.AreEqual(type, entry.Type);
            Assert.AreEqual(creator, entry.CreatedBy);
        }

        [TestMethod]
        public void CreatingMediaEntry_WithEmptyTitle_ShouldThrow()
        {
            string title = "";
            string description = "desc";
            int releaseYear = 2020;
            var genres = new List<string> { "Action" };
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new MediaEntry(
                    title,
                    description,
                    releaseYear,
                    genres,
                    ageRestriction,
                    type,
                    creator);
            });
        }

        [TestMethod]
        public void CreatingMediaEntry_ShouldTrimTitle()
        {
            string rawTitle = "   Inception   ";
            string expectedTitle = "Inception";
            string description = "A mind-bending thriller";
            int releaseYear = 2010;
            var genres = new List<string> { "Sci-Fi" };
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            var entry = new MediaEntry(
                rawTitle,
                description,
                releaseYear,
                genres,
                ageRestriction,
                type,
                creator);

            Assert.AreEqual(expectedTitle, entry.Title);
        }

        [TestMethod]
        public void CreatingMediaEntry_WithTitleLengthOutOfRange_ShouldThrow()
        {
            string tooLongTitle = new string('a', 151);
            string description = "A mind-bending thriller";
            int releaseYear = 2010;
            var genres = new List<string> { "Sci-Fi" };
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new MediaEntry(
                    tooLongTitle,
                    description,
                    releaseYear,
                    genres,
                    ageRestriction,
                    type,
                    creator);
            });
        }

        [TestMethod]
        public void CreatingMediaEntry_WithEmptyDescription_ShouldThrow()
        {
            string title = "Inception";
            string emptyDescription = "   ";
            int releaseYear = 2010;
            var genres = new List<string> { "Sci-Fi" };
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new MediaEntry(
                    title,
                    emptyDescription,
                    releaseYear,
                    genres,
                    ageRestriction,
                    type,
                    creator);
            });
        }

        [TestMethod]
        public void CreatingMediaEntry_ShouldTrimDescription()
        {
            string title = "Inception";
            string rawDescription = "   A mind-bending thriller   ";
            string expectedDescription = "A mind-bending thriller";
            int releaseYear = 2010;
            var genres = new List<string> { "Sci-Fi" };
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            var entry = new MediaEntry(
                title,
                rawDescription,
                releaseYear,
                genres,
                ageRestriction,
                type,
                creator);

            Assert.AreEqual(expectedDescription, entry.Description);
        }

        [TestMethod]
        public void CreatingMediaEntry_WithDescriptionLengthOutOfRange_ShouldThrow()
        {
            string title = "Inception";
            string tooShort = "short";
            string tooLong = new string('a', 2001);
            int releaseYear = 2010;
            var genres = new List<string> { "Sci-Fi" };
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new MediaEntry(
                    title,
                    tooShort,
                    releaseYear,
                    genres,
                    ageRestriction,
                    type,
                    creator);
            });

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new MediaEntry(
                    title,
                    tooLong,
                    releaseYear,
                    genres,
                    ageRestriction,
                    type,
                    creator);
            });
        }

        [TestMethod]
        public void CreatingMediaEntry_WithEmptyGenreList_ShouldThrow()
        {
            string title = "Inception";
            string description = "A mind-bending thriller";
            int releaseYear = 2010;
            var emptyGenres = new List<string>();
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new MediaEntry(
                    title,
                    description,
                    releaseYear,
                    emptyGenres,
                    ageRestriction,
                    type,
                    creator);
            });
        }

        [TestMethod]
        public void CreatingMediaEntry_WithInvalidGenreItems_ShouldThrow()
        {
            string title = "Inception";
            string description = "A mind-bending thriller";
            int releaseYear = 2010;
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            var genresContainingEmpty = new List<string> { "Sci-Fi", "   " };
            var genresContainingNull = new List<string?> { "Action", null! };

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new MediaEntry(
                    title,
                    description,
                    releaseYear,
                    genresContainingEmpty!,
                    ageRestriction,
                    type,
                    creator);
            });

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new MediaEntry(
                    title,
                    description,
                    releaseYear,
                    genresContainingNull!,
                    ageRestriction,
                    type,
                    creator);
            });
        }

        [TestMethod]
        public void CreatingMediaEntry_ShouldTrimGenres()
        {
            string title = "Inception";
            string description = "A mind-bending thriller";
            int releaseYear = 2010;
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            var genresRaw = new List<string> { "  Sci-Fi  ", "  Action" };
            var entry = new MediaEntry(
                title,
                description,
                releaseYear,
                genresRaw,
                ageRestriction,
                type,
                creator);

            CollectionAssert.AreEqual(
                new List<string> { "Sci-Fi", "Action" },
                entry.Genres);
        }

        [TestMethod]
        public void CreatingMediaEntry_WithTooManyGenres_ShouldThrow()
        {
            string title = "Inception";
            string description = "A mind-bending thriller";
            int releaseYear = 2010;
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            var tooManyGenres = new List<string> { "A", "B", "C", "D", "E", "F" };

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new MediaEntry(
                    title,
                    description,
                    releaseYear,
                    tooManyGenres,
                    ageRestriction,
                    type,
                    creator);
            });
        }

        public void CreatingMediaEntry_WithGenreLengthOutOfRange_ShouldThrow()
        {
            string title = "Inception";
            string description = "A mind-bending thriller";
            int releaseYear = 2010;
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            var tooLong = new List<string> { new string('x', 41) };

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new MediaEntry(
                    title,
                    description,
                   releaseYear,
                    tooLong,
                    ageRestriction,
                    type,
                    creator);
            });
        }

        [TestMethod]
        public void CreatingMediaEntry_WithDuplicateGenres_ShouldThrow()
        {
            string title = "Inception";
            string description = "A mind-bending thriller";
            int releaseYear = 2010;
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            var duplicateGenres = new List<string> { "Sci-Fi", "  sci-fi  " };

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new MediaEntry(
                    title,
                    description,
                    releaseYear,
                    duplicateGenres,
                    ageRestriction,
                    type,
                    creator);
            });
        }

        [TestMethod]
        public void CreatingMediaEntry_WithInvalidAgeRestriction_ShouldThrow()
        {
            string title = "Inception";
            string description = "A mind-bending thriller";
            int releaseYear = 2010;
            var genres = new List<string> { "Sci-Fi" };
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            int belowZero = -1;
            int aboveMax = 22;

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new MediaEntry(
                    title,
                    description,
                    releaseYear,
                    genres,
                    belowZero,
                    type,
                    creator);
            });

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new MediaEntry(
                    title,
                    description,
                    releaseYear,
                    genres,
                    aboveMax,
                    type,
                    creator);
            });
        }

        [TestMethod]
        public void CreatingMediaEntry_WithInvalidReleaseYear_ShouldThrow()
        {
            string title = "Inception";
            string description = "A mind-bending thriller";
            var genres = new List<string> { "Sci-Fi" };
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            int tooEarly = 1899;
            int tooFarFuture = DateTime.Now.Year + 2;

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new MediaEntry(
                    title,
                    description,
                    tooEarly,
                    genres,
                    ageRestriction,
                    type,
                    creator);
            });

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new MediaEntry(
                    title,
                    description,
                    tooFarFuture,
                    genres,
                    ageRestriction,
                    type,
                    creator);
            });
        }

        [TestMethod]
        public void CreatingMediaEntry_WithInvalidMediaType_ShouldThrow()
        {
            string title = "Inception";
            string description = "A mind-bending thriller";
            int releaseYear = 2010;
            var genres = new List<string> { "Sci-Fi" };
            int ageRestriction = 12;
            string creator = "melanie";

            MediaType invalidType = (MediaType)999;

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new MediaEntry(
                    title,
                    description,
                    releaseYear,
                    genres,
                    ageRestriction,
                    invalidType,
                    creator);
            });
        }

        [TestMethod]
        public void CreatingMediaEntry_ShouldSetCreatedAt()
        {
            string title = "Inception";
            string description = "A mind-bending thriller";
            var genres = new List<string> { "Sci-Fi" };
            int releaseYear = 2010;
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            var before = DateTime.UtcNow;
            var entry = new MediaEntry(
                title,
                description,
                releaseYear,
                genres,
                ageRestriction,
                type,
                creator);
            var after = DateTime.UtcNow;

            Assert.IsTrue(entry.CreatedAt >= before && entry.CreatedAt <= after);
        }

        [TestMethod]
        public void CreatingMediaEntry_ShouldSetLastModifiedAtEqualToCreatedAt()
        {
            string title = "Inception";
            string description = "A mind-bending thriller";
            var genres = new List<string> { "Sci-Fi" };
            int releaseYear = 2010;
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            var entry = new MediaEntry(
                title,
                description,
                releaseYear,
                genres,
                ageRestriction,
                type,
                creator);

            Assert.AreEqual(entry.CreatedAt, entry.LastModifiedAt);
        }

        [TestMethod]
        public void CreatingMediaEntry_WithDuplicateTitle_ShouldThrow()
        {
            var repo = new InMemoryMediaRepository();
            var manager = new MediaManager(repo);

            string title1 = "Inception";
            string title2 = "  inception  "; 
            string description = "A mind-bending thriller";
            var genres = new List<string> { "Sci-Fi" };
            int releaseYear = 2010;
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            manager.CreateMedia(
                title1,
                description,
                releaseYear,
                genres,
                ageRestriction,
                type,
                creator);

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                manager.CreateMedia(
                    title2,
                    description,
                    releaseYear,
                    genres,
                    ageRestriction,
                    type,
                    creator);
            });
        }
    }
}