using MRP.Server.Models;
using System.Collections.Generic;

namespace MRP.Server.Services.Interfaces
{
    public interface ILeaderboardManager
    {
        IEnumerable<LeaderboardEntry> GetLeaderboard();
    }
}
