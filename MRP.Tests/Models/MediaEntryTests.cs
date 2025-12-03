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
    }
}