using MRP.Server.Models;

namespace MRP.Server.Storage.Interfaces
{
    public interface IMediaRepository
    {
        IEnumerable<MediaEntry> GetAll();
        void Add(MediaEntry entry);
        MediaEntry? GetByTitle(string title);
        void Update(MediaEntry entry);
        void Delete(string title);
        bool ExistsByTitle(string title);
        void Rename(string oldTitle, MediaEntry renamedEntry);
    }
}