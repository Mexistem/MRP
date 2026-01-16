using MRP.Server.Storage.InMemory;

namespace MRP.Tests.Helpers
{
    public sealed class TestSetup
    {
        public InMemoryUserRepository UserRepo { get; }
        public InMemoryTokenRepository TokenRepo { get; }
        public InMemoryMediaRepository MediaRepo { get; }
        public InMemoryRatingRepository RatingRepo { get; }
        public InMemoryLikeRepository LikeRepo { get; }
        public InMemoryFavoriteRepository FavoriteRepo { get; }

        private TestSetup(
            InMemoryUserRepository userRepo,
            InMemoryTokenRepository tokenRepo,
            InMemoryMediaRepository mediaRepo,
            InMemoryRatingRepository ratingRepo,
            InMemoryLikeRepository likeRepo,
            InMemoryFavoriteRepository favoriteRepo)
        {
            UserRepo = userRepo;
            TokenRepo = tokenRepo;
            MediaRepo = mediaRepo;
            RatingRepo = ratingRepo;
            LikeRepo = likeRepo;
            FavoriteRepo = favoriteRepo;
        }

        public static TestSetup Create()
        {
            var userRepo = new InMemoryUserRepository();
            var tokenRepo = new InMemoryTokenRepository();
            var mediaRepo = new InMemoryMediaRepository();
            var ratingRepo = new InMemoryRatingRepository();
            var likeRepo = new InMemoryLikeRepository();
            var favoriteRepo = new InMemoryFavoriteRepository();

            likeRepo.SetDependencies(ratingRepo, userRepo);
            favoriteRepo.SetDependencies(mediaRepo, userRepo);
            ratingRepo.SetDependencies(likeRepo, mediaRepo, userRepo);
            mediaRepo.SetDependencies(ratingRepo, favoriteRepo, likeRepo);
            userRepo.SetDependencies(ratingRepo, favoriteRepo, likeRepo, mediaRepo);
            tokenRepo.SetDependencies(userRepo);

            return new TestSetup(
                userRepo,
                tokenRepo,
                mediaRepo,
                ratingRepo,
                likeRepo,
                favoriteRepo
            );
        }
    }
}
