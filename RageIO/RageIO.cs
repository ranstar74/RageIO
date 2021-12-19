using RageIO.Internal;

namespace RageIO
{
    public abstract class RageIO
    {
        public string Name
        {
            get => _entry.Name;
            set
            {
                _entry.Name = value;
            }
        }

        public bool Exists => _entry.Exists;

        public string Path => _entry.Path;
        public string FullPath => _entry.FullPath;

        internal Entry _entry;

        internal RageIO(Entry entry)
        {
            _entry = entry;
        }

        internal RageIO(string path)
        {
            _entry = EntryFactory.Get(path);
        }

        public void Create()
        {
            _entry.Create();
        }

        public void Delete()
        {
            _entry.Delete();
        }
    }
}
