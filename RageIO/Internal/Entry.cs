using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    internal abstract class DirEntry : Entry
    {
        public abstract List<DirEntry> Entries { get; }

        protected DirEntry(string path, Entry parent = null) : base(path, parent)
        {

        }
    }
    internal abstract class FileEntry : Entry
    {
        public FileEntry(string path, Entry parent = null) : base(path, parent)
        {

        }

        public abstract Stream Open(bool overwrite = false);
    }

    internal static class EntryFactory
    {
        public static Entry Get(string path)
        {
            path = path.NormalizePath();

            StringBuilder currPath = new StringBuilder();
            Entry prevEntry = null;
            string rpfPath = string.Empty;
            
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
                    // *.RPF Directory / File
                    if (isInRpf)
                    {
                        if (Path.HasExtension(segment))
                        {
                            entry = new RageFile(currPathStr, prevEntry);
                        }
                        else
                        {
                            entry = new RageDirectory(currPathStr, rpfPath, prevEntry);
                        }
                    }
                    else // Win Directory / File
                    {
                        if (Path.HasExtension(segment))
                        {
                            entry = new WinFile(currPathStr, prevEntry);
                        }
                        else
                        {
                            entry = new WinDirectory(currPathStr, prevEntry);
                        }
                    }
                }
                prevEntry = entry;
            }
            return prevEntry;
        }
    }

    // Windows directories
    internal class WinDirectory : DirEntry
    {
        public override string Name
        {
            get => _dirInfo.Name;
            set
            {
                string newPath = System.IO.Path.Combine(Parent.Path, value);
                _dirInfo.MoveTo(newPath);

                Path = newPath;
            }
        }

        public override bool Exists => _dirInfo.Exists;

        public override List<DirEntry> Entries
        {
            get
            {
                if (!Exists)
                    return new List<DirEntry>();

                List<string> dirs = new List<string>();

                // Regular directories
                dirs.AddRange(Directory
                    .GetDirectories(_dirInfo.FullName));

                // Archives
                dirs.AddRange(Directory
                    .GetFiles(_dirInfo.FullName)
                    .Where(file => file
                    .ToLower()
                    .Contains(".rpf")));

                return dirs
                    .Select(d => (DirEntry)EntryFactory.Get(d))
                    .ToList();
            }
        }

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
    internal class RageArchive : DirEntry
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

        public override bool Exists
        {
            get
            {
                // Try to get it again, in case if it was 
                // created
                if(_rpfFile == null)
                    _rpfFile = CwHelpers.GetArchive(Path);

                return _rpfFile != null;
            }
        }

        public bool IsInArchive { get; }

        public override List<DirEntry> Entries => GetEntries();

        private RpfFile _rpfFile;

        public RageArchive(string path, bool isInArchive, Entry parent) : base(path, parent)
        {
            _rpfFile = CwHelpers.GetArchive(Path);
            IsInArchive = isInArchive;
        }

        public override void Create()
        {
            if (Exists)
            {
                return;
            }

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
                RpfDirectoryEntry parentDir = null;

                if(Parent is RageArchive rageArchive)
                {
                    parentDir = CwHelpers.GetArchiveDirectory(rageArchive, rageArchive.Path);
                }
                else if(Parent is RageDirectory rageDirectory)
                {
                    parentDir = CwHelpers.GetDirectory(rageDirectory.ArchivePath, rageDirectory.Path);
                }

                createdArchive = RpfFile.CreateNew(parentDir, Name);
            }

            // Add it to the list of scanned rpf
            CwCore.RpfManager.AllRpfs.Add(createdArchive);

            _rpfFile = createdArchive;
        }

        public override void Delete()
        {
            throw new NotImplementedException();
        }

        private List<DirEntry> GetEntries()
        {
            if (!Exists)
                return new List<DirEntry>();

            List<string> dirs = new List<string>();

            dirs.AddRange(_rpfFile.AllEntries
                .Where(e =>
                {
                    // Root directory doesn't have name
                    if (e.Name == "")
                        return false;

                    Type type = e.GetType();

                    return type == typeof(RpfDirectoryEntry) || type == typeof(RpfFile);
                })
                .Select(e => e.Name));

            return dirs
                .Select(d => (DirEntry)EntryFactory.Get(d))
                .ToList();
        }
    }

    // Directories inside .RPF archive
    internal class RageDirectory : DirEntry
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

        public override bool Exists
        {
            get
            {
                // Try to get it again, in case if it was 
                // created
                if (_entry == null)
                    _entry = CwHelpers.GetArchiveDirectory(Parent, Path);

                return _entry != null;
            }
        }

        public string ArchivePath { get; }

        public override List<DirEntry> Entries => GetEntries();

        private RpfDirectoryEntry _entry;

        public RageDirectory(string path, string archivePath, Entry parent) : base(path, parent)
        {
            _entry = CwHelpers.GetArchiveDirectory(Parent, Path);

            ArchivePath = archivePath;
        }

        public override void Create()
        {
            if(Exists)
            {
                return;
            }    

            if (!Parent.Exists)
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

        private List<DirEntry> GetEntries()
        {
            if (!Exists)
                return new List<DirEntry>();

            List<string> dirs = new List<string>();

            RpfDirectoryEntry entry = _entry.Directories[0];

            dirs.AddRange(entry.Directories.Select(e => e.Name));
            dirs.AddRange(entry.Files
                .Where(f => f.Name
                .ToLower()
                .Contains(".rpf"))
                .Select(f => f.Name));

            return dirs
                .Select(d => (DirEntry)EntryFactory.Get(d))
                .ToList();
        }
    }

    // Windows files
    internal class WinFile : FileEntry
    {
        public override string Name
        {
            get => _fileInfo.Name;
            set
            {
                string newPath = System.IO.Path.Combine(Parent.Path, value);
                _fileInfo.MoveTo(newPath);

                Path = newPath;
            }
        }
        public override bool Exists => _fileInfo.Exists;

        private readonly FileInfo _fileInfo;

        public WinFile(string path, Entry parent) : base(path, parent)
        {
            _fileInfo = new FileInfo(path);
        }

        public override void Create()
        {
            if (!Parent.Exists)
            {
                Parent.Create();
            }

            _fileInfo.Create();
        }

        public override void Delete()
        {
            _fileInfo.Delete();
        }

        public override Stream Open(bool overwrite = false)
        {
            var mode = overwrite ? FileMode.Create : FileMode.OpenOrCreate;

            return _fileInfo.Open(mode, FileAccess.ReadWrite);
        }
    }

    // Files inside .RPF archive
    internal class RageFile : FileEntry
    {
        public override string Name
        {
            get => System.IO.Path.GetFileName(Path);
            set
            {
                if (!Exists)
                    return;

                RpfFile.RenameEntry(RpfFileEntry, value);

                string newPath = System.IO.Path.Combine(Parent.Path, value);
                Path = newPath;
            }
        }

        public override bool Exists => RpfFileEntry != null;

        internal RpfDirectoryEntry RpfRootDirectory;
        internal RpfFileEntry RpfFileEntry;

        public RageFile(string path, Entry parent) : base(path, parent)
        {
            RpfRootDirectory = GetRootDir();
            RpfFileEntry = GetThisFile();
        }

        public override void Create()
        {
            if (!Parent.Exists)
            {
                Parent.Create();

                RpfRootDirectory = GetRootDir();
            }

            RpfFileEntry = RpfFile.CreateFile(RpfRootDirectory, Name, new byte[0], false);
        }

        public override void Delete()
        {
            if (!Exists)
                return;

            RpfFile.DeleteEntry(RpfFileEntry);
        }

        private RpfDirectoryEntry GetRootDir()
        {
            return CwHelpers.GetArchiveDirectory(Parent, Parent.Path);
        }

        private RpfFileEntry GetThisFile()
        {
            if (RpfRootDirectory == null)
                return null;

            var files = RpfRootDirectory.Files.Where(f => f.Name == Name);
            if (files.Count() == 0)
                return null;

            return files.First();
        }

        public override Stream Open(bool overwrite = false)
        {
            if (!Exists)
            {
                Create();
            }

            return new RageStream(this, overwrite);
        }

        internal void RefreshFile()
        {
            RpfFileEntry = GetThisFile();
        }
    }
}
