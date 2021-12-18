using CodeWalker.GameFiles;
using System;

namespace RageIO
{
    /// <summary>
    /// A static instance that wraps basic .RPF functionality of Codewalker.Core
    /// </summary>
    public static class CwCore
    {
        /// <summary>
        /// The exception that is thrown when not initialized <see cref="CwCore"/> was accessed.
        /// </summary>
        public class CwCoreNotInitializedException : Exception
        {
            public override string Message => "CwCore needs to be initialized before accessing.";
        }

        /// <summary>
        /// Gets a value indicating whether CwCore is initialized or not.
        /// </summary>
        public static bool IsInitialized => _initialized;

        /// <summary>
        /// A RpfManager instance.
        /// </summary>
        /// <exception cref="CwCoreNotInitializedException"></exception>
        public static RpfManager RpfManager
        {
            get
            {
                EnsureInitialized();

                return _rpfManager;
            }
        }

        /// <summary>
        /// Absolute path to GTA root directory.
        /// </summary>
        public static string GtaDirectory
        {
            get
            {
                EnsureInitialized();

                return _gtaDirectory;
            }
        }

        private static RpfManager _rpfManager;
        private static string _gtaDirectory;
        private static bool _initialized = false;

        /// <summary>
        /// Initializes <see cref="CwCore"/> with given GTA path.
        /// </summary>
        /// <param name="gtaDirectory">Absolute path to GTA directory.</param>
        public static void Init(string gtaDirectory, bool loadKeys = false)
        {
            _gtaDirectory = gtaDirectory;

            if(loadKeys)
                GTA5Keys.LoadFromPath(_gtaDirectory);

            _rpfManager = new RpfManager();
            _rpfManager.Init(
                folder: gtaDirectory,
                updateStatus: _ => { },
                errorLog: _ => { });
            _rpfManager.EnableMods = true;

            _initialized = true;
        }

        private static void EnsureInitialized()
        {
            if (!_initialized)
            {
                throw new CwCoreNotInitializedException();
            }
        }
    }
}
