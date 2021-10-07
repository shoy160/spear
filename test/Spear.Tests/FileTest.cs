using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Spear.Tests
{
    [TestClass]
    public class FileTest
    {
        [TestMethod]
        public void Test()
        {
            var file = new FileInfo("C:\\Users\\shay\\Desktop\\Untitled-2.txt");
            file.CreationTime = DateTime.Now.AddDays(-15);
            file.LastAccessTime = DateTime.Now;
            file.LastWriteTime = DateTime.Now.AddDays(-10);
        }
    }
}
