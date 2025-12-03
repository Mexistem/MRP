using MRP.Server.Models;

namespace MRP.Server.Storage
{
    public interface IMediaRepository
    {
        IEnumerable<MediaEntry> GetAll();
        void Add(MediaEntry entry);
    }
}