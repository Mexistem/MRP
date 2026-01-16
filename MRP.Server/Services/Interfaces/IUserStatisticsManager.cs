using MRP.Server.Models;

namespace MRP.Server.Services.Interfaces
{
    public interface IUserStatisticsManager
    {
        UserStatistics ComputePublic(string username);
        UserStatistics ComputePrivate(string username);
    }
}
