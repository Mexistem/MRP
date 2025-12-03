using MRP.Server.Models;

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


    }
}