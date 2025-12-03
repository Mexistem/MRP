using MRP.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Server.Storage.InMemory
{
    public class InMemoryMediaRepository
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

            _entries.Add(entry);
        }
    }
}
