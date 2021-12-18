using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace RageIO.Tests
{
    [TestClass()]
    public class DirectoryIOTests
    {
        // Do not change name of this folder
        // It's automatically being removed
        // on pre-build event.
        private const string _testDir = "Tests";

        public DirectoryIOTests()
        {
            CwCore.Init(Directory.GetCurrentDirectory());
        }

        #region WIN
        [TestMethod()]
        public void DirectoryIO_Creates_SystemDirectory()
        {
            string path = Path.Combine(_testDir, "Foo");
            DirectoryIO dir = new DirectoryIO(path);

            Assert.IsTrue(dir.DirectoryType == DirectoryIOType.SystemDirectory);
        }
        
        [TestMethod()]
        public void WinEntry_Create_CreatesNewDirectory()
        {
            string path = Path.Combine(_testDir, "Foo");
            DirectoryIO dir = new DirectoryIO(path);

            dir.Create();

            Assert.IsTrue(new DirectoryInfo(path).Exists);
        }

        [TestMethod()]
        public void WinEntry_Create_CreatesAllMissingDirectoriesInPath()
        {
            string path = Path.Combine(_testDir, "Foo\\Bar");
            DirectoryIO dir = new DirectoryIO(path);

            dir.Create();

            Assert.IsTrue(new DirectoryInfo(path).Exists);
        }

        [TestMethod]
        public void WinEntry_Rename()
        {
            string path = Path.Combine(_testDir, "Foo\\Bar");
            string pathTo = Path.Combine(_testDir, "Foo\\NewBar");
            DirectoryIO dir = new DirectoryIO(path);

            dir.Create();
            dir.Name = "NewBar";
            DirectoryIO dirTo = new DirectoryIO(pathTo);

            Assert.IsTrue(dirTo.Exists);
        }
        #endregion

        #region RPF
        [TestMethod()]
        public void DirectoryIO_Creates_Archive()
        {
            string path = Path.Combine(_testDir, "Foo.rpf");
            DirectoryIO dir = new DirectoryIO(path);

            Assert.IsTrue(dir.DirectoryType == DirectoryIOType.Archive);
        }

        [TestMethod()]
        public void RageArchive_Create_CreatesNewArchive()
        {
            string path = Path.Combine(_testDir, "Foo.rpf");
            DirectoryIO dir = new DirectoryIO(path);

            dir.Create();

            Assert.IsTrue(dir.Exists);
        }

        [TestMethod()]
        public void RageArchive_Create_CreatesAllMissingDirectoriesInPath()
        {
            string path = Path.Combine(_testDir, "Foo\\Bar.rpf");
            DirectoryIO dir = new DirectoryIO(path);

            dir.Create();

            Assert.IsTrue(dir.Exists);
        }

        [TestMethod]
        public void RageArchive_Rename()
        {
            string path = Path.Combine(_testDir, "Bar.rpf");
            string pathTo = Path.Combine(_testDir, "NewBar.rpf");
            DirectoryIO dir = new DirectoryIO(path);

            dir.Create();
            dir.Name = "NewBar.rpf";

            DirectoryIO newDir = new DirectoryIO(pathTo);

            Assert.IsTrue(newDir.Exists);
        }
        #endregion

        #region RPFDIR
        [TestMethod()]
        public void DirectoryIO_Creates_ArchiveDirectory()
        {
            string path = Path.Combine(_testDir, "FooDir.rpf\\Sample");
            DirectoryIO dir = new DirectoryIO(path);

            Assert.IsTrue(dir.DirectoryType == DirectoryIOType.ArchiveDirectory);
        }

        [TestMethod()]
        public void RageArchiveDirectory_Create_CreatesNewArchiveDirectory()
        {
            string path = Path.Combine(_testDir, "Foo.rpf\\Bur");
            DirectoryIO dir = new DirectoryIO(path);

            dir.Create();

            Assert.IsTrue(dir.Exists);
        }

        [TestMethod()]
        public void RageArchiveDirectory_Create_CreatesAllMissingDirectoriesInPath()
        {
            string path = Path.Combine(_testDir, "Foo\\Bar.rpf\\To\\Sample");
            DirectoryIO dir = new DirectoryIO(path);

            dir.Create();

            Assert.IsTrue(dir.Exists);
        }

        [TestMethod]
        public void RageArchiveDirectory_Rename()
        {
            string path = Path.Combine(_testDir, "Bar.rpf\\Sample");
            string pathTo = Path.Combine(_testDir, "Bar.rpf\\Sample123");
            DirectoryIO dir = new DirectoryIO(path);

            dir.Create();
            dir.Name = "Sample123.rpf";

            DirectoryIO newDir = new DirectoryIO(pathTo);

            Assert.IsTrue(newDir.Exists);
        }
        #endregion
    }
}
