using MRP.Server.Http;
using MRP.Server.Services;
using MRP.Server.Storage.InMemory;

var userRepository = new InMemoryUserRepository();
var userManager = new UserManager(userRepository);

var tokenRepository = new InMemoryTokenRepository();
var authManager = new AuthManager(userManager, tokenRepository);

var server = new HttpServer(userManager, authManager);

await server.StartAsync();