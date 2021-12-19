using System;
using System.IO;

namespace RageIO.Internal
{
    internal static class StringExtensions
    {
        public static string NormalizePath(this string path)
        {
            path = path.Replace(
                oldChar: Path.AltDirectorySeparatorChar, 
                newChar: Path.DirectorySeparatorChar);

            return path;
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
}
