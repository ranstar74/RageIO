using CodeWalker.GameFiles;
using RageIO.Internal;
using System.IO;

namespace RageIO
{
    public sealed class FileIO : RageIO
    {
        public DirectoryIO Parent { get; }

        internal FileIO(Entry entry) : base(entry)
        {
            Parent = GetParent();
        }

        public FileIO(string path) : base(path)
        {
            Parent = GetParent();
        }

        public Stream Open(bool overwrite = false)
        {
            FileEntry entry = _entry as FileEntry;

            return entry.Open(overwrite);
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

    public class RageStream : MemoryStream
    {
        private readonly RpfFileEntry _fileEntry;
        private readonly RageFile _rageFile;

        internal RageStream(RageFile rageFile, bool overwrite)
        {
            _rageFile = rageFile;
            _fileEntry = rageFile.RpfFileEntry;

            if (overwrite)
                return;

            // Write current bytes of this file to stream
            byte[] data = _fileEntry.File.ExtractFile(_fileEntry);

            // For some reason instead of returning empty
            // array cw returns null... really stupid design
            // dexy pls fix
            if (data == null)
                data = new byte[0];

            Write(data, 0, data.Length);
            Seek(0, SeekOrigin.Begin);
        }

        public override void Flush()
        {
            Seek(0, SeekOrigin.Begin);
            
            byte[] data = new byte[Length];
            Read(data, 0, (int) Length);

            RpfFile.CreateFile(
                dir: _rageFile.RpfRootDirectory,
                name: _rageFile.Name,
                data: data);

            Seek(0, SeekOrigin.Begin);

            // Also append changes to the data structure
            // that is stored in memory

            // This crap doesn't work for some reason
            //_fileEntry.Read(new DataReader(this));

            _rageFile.RefreshFile();
        }
    }
}
