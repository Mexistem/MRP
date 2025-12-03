// InMemoryMediaRepository.cs – mit Duplikatsprüfung

using MRP.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MRP.Server.Storage.InMemory
{
    public class InMemoryMediaRepository : IMediaRepository
    {
        private readonly List<MediaEntry> _entries = new List<MediaEntry>();

        public IEnumerable<MediaEntry> GetAll()
        {
            return _entries;
        }

        public void Add(MediaEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            string normalizedTitle = entry.Title.Trim().ToLowerInvariant();

            bool exists = _entries.Any(m => m.Title.Trim().ToLowerInvariant() == normalizedTitle);
            if (exists)
            {
                throw new InvalidOperationException("A media entry with this title already exists.");
            }

            _entries.Add(entry);
        }
    }
}
