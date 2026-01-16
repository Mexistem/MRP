using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using MRP.Server.Models;

namespace MRP.Server.Services
{
    public interface IMediaManager
    {
        MediaEntry CreateMedia(
            string title,
            string description,
            int releaseYear,
            List<string> genres,
            int ageRestriction,
            MediaType type,
            string createdBy);
        MediaEntry UpdateMedia(
            string title,
            string? newTitle,
            string description,
            int releaseYear,
            List<string> genres,
            int ageRestriction,
            MediaType type,
            string requestedBy);
        bool Exists(string title);
        void DeleteMedia(string title, string requestedBy);
        MediaEntry? GetByTitle(string title);
        IEnumerable<MediaEntry> GetAll();
    }
}