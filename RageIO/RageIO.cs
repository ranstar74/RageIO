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
                if (_entry.Parent == null)
                {
                    _entry.Name = value;
                    return;
                }

                _entry.Name = value;
            }
        }

        public bool Exists => _entry.Exists;

        public string Path => _entry.Path;

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
