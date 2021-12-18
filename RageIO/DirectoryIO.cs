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
        public DirectoryIOType DirectoryType => _dirType;

        private readonly DirectoryIOType _dirType;

        internal DirectoryIO(Entry entry) : base(entry)
        {
            _dirType = GetDirType();
        }

        public DirectoryIO(string path) : base(path)
        {
            _dirType = GetDirType();
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
    }
}
