using RageIO.Internal;

namespace RageIO
{
    public sealed class FileIO : RageIO
    {
        public DirectoryIO Parent { get; }

        internal FileIO(Entry entry) : base(entry)
        {

        }

        public FileIO(string path) : base(path)
        {
            Parent = new DirectoryIO(_entry.Parent);
        }
    }
}
