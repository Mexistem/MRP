using MRP.Server.Models;

namespace MRP.Server.Services.Interfaces
{
    public interface IUserStatisticsHandler
    {
        UserStatistics ComputePublic(string username);
        UserStatistics ComputePrivate(string username);
    }
}
