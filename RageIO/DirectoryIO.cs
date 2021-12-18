using RageIO.Internal;
using System;

namespace RageIO
{
    public enum DirectoryIOType
    {
        SystemDirectory,
        Archive,
        ArchiveDirectory
    }

    public sealed class DirectoryIO : RageIO
    {
        public DirectoryIO Parent { get; }

        public DirectoryIOType DirectoryType { get; }

        internal DirectoryIO(Entry entry) : base(entry)
        {
            DirectoryType = GetDirType();
            Parent = GetParent();
        }

        public DirectoryIO(string path) : base(path)
        {
            DirectoryType = GetDirType();
            Parent = GetParent();
        }

        private DirectoryIOType GetDirType()
        {
            switch(_entry)
            {
                case WinDirectory _:
                    return DirectoryIOType.SystemDirectory;
                case RageArchive _:
                    return DirectoryIOType.Archive;
                case RageDirectory _:
                    return DirectoryIOType.ArchiveDirectory;
                default:
                    throw new NotSupportedException($"Type: {_entry.GetType()} is not supported.");
            }
        }

        private DirectoryIO GetParent()
        {
            if (_entry.Parent == null)
            {
                return null;
            }

            return new DirectoryIO(_entry.Parent);
        }
    }
}
