using CodeWalker.GameFiles;
using System;
using System.IO;
using System.Text;

namespace RageIO.Internal
{
    internal abstract class Entry
    {
        public abstract string Name { get; set; }
        public abstract bool Exists { get; }
        public string Path { get; set; }
        public Entry Parent { get; }

        public Entry(string path, Entry parent = null)
        {
            Path = path;
            Parent = parent;
        }

        public abstract void Create();
        public abstract void Delete();
    }

    internal static class EntryFactory
    {
        public static Entry Get(string path)
        {
            path = path.NormalizePath();

            StringBuilder currPath = new StringBuilder();
            Entry prevEntry = null;
            string rpfPath = string.Empty;
;
            bool isInRpf = false;
            foreach (string segment in path.Split(Path.DirectorySeparatorChar))
            {
                // As alternative solution there could be IEnumerable<string>
                // with currPathStr = string.Join("\\", segments);
                currPath.Append(Path.DirectorySeparatorChar + segment);
                string currPathStr = currPath
                    .ToString()
                    .TrimStart(Path.DirectorySeparatorChar);

                Entry entry;
                // *.RPF
                if (segment.ToLower().Contains(".rpf"))
                {
                    entry = new RageArchive(currPathStr, isInRpf, prevEntry);

                    rpfPath = currPathStr;
                    isInRpf = true;
                }
                else
                {
                    // *.RPF Directory
                    if (isInRpf)
                    {
                        entry = new RageDirectory(currPathStr, rpfPath, prevEntry);
                    }
                    else // Win Directory
                    {
                        entry = new WinDirectory(currPathStr, prevEntry);
                    }
                }
                prevEntry = entry;
            }
            return prevEntry;
        }
    }

    // Windows directories
    internal class WinDirectory : Entry
    {
        public override string Name
        {
            get => _dirInfo.Name;
            set
            {
                string newPath = System.IO.Path.Combine(Parent.Path, value);
                _dirInfo.MoveTo(newPath);
            }
        }

        public override bool Exists => _dirInfo.Exists;

        private readonly DirectoryInfo _dirInfo;

        public WinDirectory(string path, Entry parent) : base(path, parent)
        {
            _dirInfo = new DirectoryInfo(path);
        }

        public override void Create()
        {
            _dirInfo.Create();
        }

        public override void Delete()
        {
            _dirInfo.Delete();
        }
    }

    // *.RPF archives
    internal class RageArchive : Entry
    {
        public override string Name
        {
            get => System.IO.Path.GetFileName(Path);
            set
            {
                if (!Exists)
                    return;

                if (System.IO.Path.GetExtension(value) != ".rpf")
                {
                    value = System.IO.Path.ChangeExtension(value, ".rpf");
                }

                RpfFile.RenameArchive(_rpfFile, value);

                string newPath = System.IO.Path.Combine(Parent.Path, value);
                if (!IsInArchive)
                {
                    new FileInfo(Path).MoveTo(newPath);
                }
                Path = newPath;
            }
        }

        public override bool Exists => _rpfFile != null;
        public bool IsInArchive { get; }

        private RpfFile _rpfFile;

        public RageArchive(string path, bool isInArchive, Entry parent) : base(path, parent)
        {
            _rpfFile = CwHelpers.GetArchive(Path);
            IsInArchive = isInArchive;
        }

        public override void Create()
        {
            if (Exists)
                return;

            if (!Parent.Exists)
            {
                Parent.Create();
            }

            RpfFile createdArchive;
            if (Parent is WinDirectory)
            {
                createdArchive = RpfFile.CreateNew(CwCore.GtaDirectory, Path);
            }
            else
            {
                RpfDirectoryEntry dir = CwHelpers.GetDirectory(Parent.Path, Path);

                createdArchive = RpfFile.CreateNew(dir, Name);
            }

            // Add it to the list of scanned rpf
            CwCore.RpfManager.AllRpfs.Add(createdArchive);

            _rpfFile = createdArchive;
        }

        public override void Delete()
        {
            throw new NotImplementedException();
        }
    }

    // Directories inside .RPF archive
    internal class RageDirectory : Entry
    {
        public override string Name
        {
            get => System.IO.Path.GetFileName(Path);
            set
            {
                if (!Exists)
                    return;

                RpfFile.RenameEntry(_entry, value);

                string newPath = System.IO.Path.Combine(Parent.Path, value);
                Path = newPath;
            }
        }

        public override bool Exists => _entry != null;

        public string ArchivePath { get; }

        private RpfDirectoryEntry _entry;

        public RageDirectory(string path, string archivePath, Entry parent) : base(path, parent)
        {
            _entry = CwHelpers.GetArchiveDirectory(Parent, Path);

            ArchivePath = archivePath;
        }

        public override void Create()
        {
            if(!Parent.Exists)
            {
                Parent.Create();
            }

            // Get parent directory and create this directory in it
            RpfDirectoryEntry parentDirectory = CwHelpers.GetArchiveDirectory(Parent, Parent.Path);

            _entry = RpfFile.CreateDirectory(parentDirectory, Name);
        }

        public override void Delete()
        {
            throw new System.NotImplementedException();
        }
    }

    // Windows files
    internal class WinFile : Entry
    {
        public override string Name { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public override bool Exists => throw new System.NotImplementedException();

        public WinFile(string path, Entry parent) : base(path, parent)
        {

        }

        public override void Create()
        {
            throw new System.NotImplementedException();
        }

        public override void Delete()
        {
            throw new System.NotImplementedException();
        }
    }

    // Files inside .RPF archive
    internal class RageFile : Entry
    {
        public override string Name { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public override bool Exists => throw new System.NotImplementedException();

        public RageFile(string path, Entry parent) : base(path, parent)
        {

        }

        public override void Create()
        {
            throw new System.NotImplementedException();
        }

        public override void Delete()
        {
            throw new System.NotImplementedException();
        }
    }
}
