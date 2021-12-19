using RageIO.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public List<DirectoryIO> Directories
        {
            get
            {
                if(_directories == null)
                {
                    DirEntry entry = _entry as DirEntry;
                    _directories = entry.Entries
                        .Select(e => new DirectoryIO(e))
                        .ToList();
                }

                return _directories;
            }
        }

        public DirectoryIOType DirectoryType { get; }

        private List<DirectoryIO> _directories = null;
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
