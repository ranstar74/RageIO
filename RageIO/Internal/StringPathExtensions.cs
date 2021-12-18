using System.IO;

namespace RageIO.Internal
{
    internal static class StringPathExtensions
    {
        public static string NormalizePath(this string path)
        {
            path = path.Replace(
                oldChar: Path.AltDirectorySeparatorChar, 
                newChar: Path.DirectorySeparatorChar);

            return path;
        }
    }
}
