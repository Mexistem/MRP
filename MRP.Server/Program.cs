using MRP.Server.Http;
using MRP.Server.Services;

var userManager = new UserManager();
var authManager = new AuthManager(userManager);

var server = new HttpServer(userManager, authManager);
await server.StartAsync();