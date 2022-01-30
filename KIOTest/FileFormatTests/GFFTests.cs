using System;
using System.IO;
using System.Linq;
using KotOR_IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileFormatTests
{
    [TestClass]
    public class GFFTests
    {
        /// <summary>
        /// Verify that GFF.ToRawData() returns an array of data with equivalent length to the
        /// data used to construct the GFF object.
        /// </summary>
        [TestMethod]
        public void ToRawData_SimpleRead_SizeUnchanged()
        {
            // Arrange
            var fileToRead = @"C:\Program Files (x86)\Steam\steamapps\common\swkotor\data\templates.bif";
            BIF templates = new BIF(fileToRead);
            var vre = templates.VariableResourceTable.First(vre => vre.ResourceType == ResourceType.UTC);
            int oldSize = vre.EntryData.Length;
            GFF utc = new GFF(vre.EntryData);

            // Act
            var rawData = utc.ToRawData();

            // Assert
            int newSize = rawData.Length;
            Assert.AreEqual(oldSize, newSize, "Size of raw data has changed.");
        }

        /// <summary>
        /// Verify that GFF.WriteToFile() creates a data file of equivalent size to the file
        /// used to construct the GFF object.
        /// </summary>
        [TestMethod]
        public void WriteToFile_ValidPath_SizeUnchanged()
        {
            // Arrange
            string pathToRead = @"C:\Dev\KIO Test\test1.git";
            string pathToWrite = @"C:\Dev\KIO Test\test1_copy.git";
            FileInfo oldFileInfo = new FileInfo(pathToRead);
            GFF gff = new GFF(pathToRead);

            // Act
            gff.WriteToFile(pathToWrite);
            FileInfo newFileInfo = new FileInfo(pathToWrite);

            // Assert
            var oldFileSize = oldFileInfo.Length;
            var newFileSize = newFileInfo.Length;
            Assert.AreEqual(oldFileSize, newFileSize, "File size has changed.");

            // Cleanup
            File.Delete(pathToWrite);
        }
    }
}
