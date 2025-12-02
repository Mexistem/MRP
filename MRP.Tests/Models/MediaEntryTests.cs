namespace MRP.Tests
{
    [TestClass]
    public class MediaEntryTests
    {
        [TestMethod]
        public void CreatingMediaEntry_ShouldStoreBasicInformation()
        {
            // Arrange
            string title = "Inception";
            string description = "A mind-bending thriller";
            int releaseYear = 2010;
            var genres = new List<string> { "Sci-Fi" };
            int ageRestriction = 12;
            MediaType type = MediaType.Film;
            string creator = "melanie";

            // Act
            var entry = new MediaEntry(
                title,
                description,
                releaseYear,
                genres,
                ageRestriction,
                type,
                creator);

            // Assert
            Assert.AreEqual(title, entry.Title);
            Assert.AreEqual(description, entry.Description);
            Assert.AreEqual(releaseYear, entry.ReleaseYear);
            CollectionAssert.AreEqual(genres, entry.Genres);
            Assert.AreEqual(ageRestriction, entry.AgeRestriction);
            Assert.AreEqual(type, entry.Type);
            Assert.AreEqual(creator, entry.CreatedBy);
        }
    }
}