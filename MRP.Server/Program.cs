using System.Text.Json;
using MRP.Server;
using MRP.Server.Http;
using MRP.Server.Services;
using MRP.Server.Storage.Db;
using MRP.Server.Storage.InMemory;

var settingsJson = File.ReadAllText("settings.json");

var settings = JsonSerializer.Deserialize<ServerSettings>(settingsJson)
               ?? throw new InvalidOperationException("Failed to load settings.json");

var userRepository = new DbUserRepository(settings.ConnectionString);
var tokenRepository = new InMemoryTokenRepository();
var mediaRepository = new DbMediaRepository(settings.ConnectionString);
var ratingRepository = new DbRatingRepository(settings.ConnectionString);
var likeRepository = new DbLikeRepository(settings.ConnectionString);
var favoriteRepository = new DbFavoriteRepository(settings.ConnectionString);

var userManager = new UserManager(userRepository,tokenRepository);
var ratingManager = new RatingManager(ratingRepository);
var mediaManager = new MediaManager(mediaRepository, userManager);
var authManager = new AuthManager(userManager, tokenRepository);
var likeManager = new LikeManager(likeRepository);
var favoriteManager = new FavoriteManager(favoriteRepository);
var leaderboardManager = new LeaderboardManager(userManager, ratingManager, likeManager, mediaManager);

var server = new HttpServer(userManager, authManager, 
                            mediaManager, ratingManager,
                            likeManager, favoriteManager, leaderboardManager);

await server.StartAsync();