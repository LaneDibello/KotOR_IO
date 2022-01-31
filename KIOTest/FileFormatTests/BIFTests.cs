using System;
using System.IO;
using System.Linq;
using KotOR_IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFormatTests
{
    [TestClass]
    public class BIFTests
    {
        /// <summary>
        /// Verify that BIF.ToRawData() returns an array of data with equivalent length to the
        /// data used to construct the BIF object.
        /// </summary>
        [TestMethod]
        public void ToRawData_SimpleRead_SizeUnchanged()
        {
            // Arrange
            var fileToRead = @"C:\Program Files (x86)\Steam\steamapps\common\swkotor\data\templates.bif";
            var oldFileInfo = new FileInfo(fileToRead);
            long oldSize = oldFileInfo.Length;
            BIF bif = new BIF(fileToRead);

            // Act
            var rawData = bif.ToRawData();

            // Assert
            long newSize = rawData.Length;
            Assert.AreEqual(oldSize, newSize, "Size of raw data has changed.");
        }

        /// <summary>
        /// Verify that BIF.WriteToFile() creates a data file of equivalent size to the file
        /// used to construct the BIF object.
        /// </summary>
        [TestMethod]
        public void WriteToFile_ValidPath_SizeUnchanged()
        {
            // Arrange
            var pathToRead = @"C:\Program Files (x86)\Steam\steamapps\common\swkotor\data\templates.bif";
            var pathToWrite = Environment.CurrentDirectory + "templates_copy.bif";
            var oldFileInfo = new FileInfo(pathToRead);
            long oldSize = oldFileInfo.Length;
            BIF bif = new BIF(pathToRead);

            // Act
            bif.WriteToFile(pathToWrite);
            var newFileInfo = new FileInfo(pathToWrite);

            // Assert
            long newSize = newFileInfo.Length;
            Assert.AreEqual(oldSize, newSize, "Size of raw data has changed.");

            // Cleanup
            File.Delete(pathToWrite);
        }
    }
}
