using CodeWalker.GameFiles;
using RageIO.Internal;
using System;
using System.Linq;

namespace RageIO
{
    internal static class CwHelpers
    {
        public static RpfDirectoryEntry GetDirectory(string archivePath, string directoryPath)
        {
            RpfFile archive = GetArchive(archivePath);

            return archive.AllEntries
                .OfType<RpfDirectoryEntry>()
                .FirstOrDefault(dir => dir.Path
                .Equals(directoryPath, StringComparison.InvariantCultureIgnoreCase));
        }

        public static RpfFile GetArchive(string archivePath)
        {
            // Don't use FindRpfFile because it's case sensetive

            return CwCore.RpfManager.AllRpfs
                .Where(x => x.Path
                .Equals(archivePath, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();
        }

        public static RpfDirectoryEntry GetArchiveDirectory(Entry entry, string dirPath)
        {
            if (!entry.Exists)
                return null;

            if (entry is RageArchive)
            {
                RpfFile archive = GetArchive(entry.Path);

                // root dir - seems to be always at 0 index
                return archive.AllEntries[0] as RpfDirectoryEntry;
            }
            
            if(entry is RageDirectory rageDir)
            {
                return GetDirectory(rageDir.ArchivePath, dirPath);
            }

            // Shoudln't happen...
            return GetDirectory(entry.Path, dirPath);
        }
    }
}
