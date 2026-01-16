using MRP.Server.Models;
using MRP.Server.Services;
using MRP.Server.Storage.InMemory;
using MRP.Tests.Helpers;

[TestClass]
public class RatingAverageCalculationTests
{
    private InMemoryUserRepository _userRepo = null!;
    private UserManager _userManager = null!;

    private InMemoryMediaRepository _mediaRepo = null!;
    private InMemoryRatingRepository _ratingRepo = null!;

    private IMediaManager _mediaManager = null!;
    private IRatingManager _ratingManager = null!;
    private InMemoryTokenRepository _tokenRepo = null!;
    private InMemoryLikeRepository _likeRepo = null!;
    private InMemoryFavoriteRepository _favoriteRepo = null!;

    [TestInitialize]
    public void Setup()
    {
        var t = TestSetup.Create();

        _userRepo = t.UserRepo;
        _tokenRepo = t.TokenRepo;
        _mediaRepo = t.MediaRepo;
        _ratingRepo = t.RatingRepo;
        _likeRepo = t.LikeRepo;
        _favoriteRepo = t.FavoriteRepo;

        _userManager = new UserManager(_userRepo, _tokenRepo);

        _userManager.Register("testuser1", "!123Password");
        _userManager.Register("testuser2", "!123Password");

        _mediaManager = new MediaManager(_mediaRepo, _userManager);
        _ratingManager = new RatingManager(_ratingRepo);

        _mediaManager.CreateMedia(
            "inception",
            "Valid description",
            2010,
            new List<string> { "Sci-Fi" },
            12,
            MediaType.Movie,
            "testuser1"
        );
    }

    [TestMethod]
    public void Average_IsCalculatedCorrectly()
    {
        _ratingManager.CreateRating("inception", "testuser1", 5, null);
        _ratingManager.CreateRating("inception", "testuser2", 3, null);

        var avg = _ratingManager.GetAverageRatingForMedia("inception");

        Assert.AreEqual(4.0, avg);
    }

    [TestMethod]
    public void Average_Changes_WhenNewRatingIsAdded()
    {
        _ratingManager.CreateRating("inception", "testuser1", 5, null);

        Assert.AreEqual(5.0, _ratingManager.GetAverageRatingForMedia("inception"));

        _ratingManager.CreateRating("inception", "testuser2", 3, null);

        Assert.AreEqual(4.0, _ratingManager.GetAverageRatingForMedia("inception"));
    }

    [TestMethod]
    public void Delete_RemovesRatingFromAverage()
    {
        _ratingManager.CreateRating("inception", "testuser1", 5, null);
        _ratingManager.CreateRating("inception", "testuser2", 1, null);

        Assert.AreEqual(3.0, _ratingManager.GetAverageRatingForMedia("inception"));

        _ratingRepo.DeleteRating("inception", "testuser2");

        Assert.AreEqual(5.0, _ratingManager.GetAverageRatingForMedia("inception"));
    }

    [TestMethod]
    public void InvalidRatings_ShouldThrow_AndNotAffectAverage()
    {
        _ratingManager.CreateRating("inception", "testuser1", 5, null);

        Assert.ThrowsException<ArgumentException>(() =>
            _ratingManager.CreateRating("inception", "testuser2", 99, null));

        Assert.AreEqual(5.0, _ratingManager.GetAverageRatingForMedia("inception"));
    }
}
