using log4net;
using ScoutUtilities.UIConfiguration;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ScoutModels.Common
{

    public class DiskSpaceModel
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public double PercentDiskSpaceFree { get; set; }
        public double PercentDiskSpaceData { get; set; }
        public double PercentDiskSpaceOther { get; set; }
        public double PercentDiskSpaceExport { get; set; }
        public double TotalFreeSpace { get; set; }
        public double SizeOfData { get; set; }
        public double SizeOfOther { get; set; }
        public double SizeOfExport { get; set; }
        public double TotalDiskSpace { get; set; }

        public void CalculateDiskSpace()
        {
            var drivePath = AppDomain.CurrentDomain.BaseDirectory;
            var instrumentPath = UISettings.InstrumentPath;
            var exportPath = UISettings.ExportPath;

            ulong totalSize = 0;
            ulong totalFreeSpace = 0;
            SizeOfExport = 0;
            SizeOfOther = 0;
            SizeOfData = 0;
            TotalFreeSpace = 0;

            GetDiskSpace(drivePath, out totalSize, out totalFreeSpace);

            var instrumentSizeInBytes = GetDirectorySize(instrumentPath);
            var exportSizeInBytes = GetDirectorySize(exportPath);
            var dataSizeInBytes = instrumentSizeInBytes - exportSizeInBytes;
            var otherSizeInBytes = totalSize - instrumentSizeInBytes - totalFreeSpace;

            TotalDiskSpace = totalSize;
            TotalFreeSpace = totalFreeSpace;

            SizeOfData = dataSizeInBytes;
            SizeOfExport = exportSizeInBytes;
            SizeOfOther = otherSizeInBytes;

            if (totalSize > 0)
            {
                PercentDiskSpaceOther = (otherSizeInBytes / (double)totalSize) * 100.0;
                PercentDiskSpaceData = (dataSizeInBytes / (double)totalSize) * 100.0;
                PercentDiskSpaceExport = (exportSizeInBytes / (double)totalSize) * 100.0;
                PercentDiskSpaceFree = (totalFreeSpace / (double)totalSize) *100.0;
            }
            else
            {
                PercentDiskSpaceOther = 0;
                PercentDiskSpaceData = 0;
                PercentDiskSpaceExport = 0;
                PercentDiskSpaceFree = 0;
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetDiskFreeSpaceEx(string directoryName, out ulong freeBytesAvailable, out ulong totalNumberOfBytes, out ulong totalNumberOfFreeBytes);

        public static void GetDiskSpace(string folderName, out ulong totalSpace, out ulong totalFreeSpace)
        {
            totalSpace = 0;
            totalFreeSpace = 0;

            try
            {
                if (GetDiskFreeSpaceEx(folderName, out _, out var totalNumberOfBytes, out var totalNumberOfFreeBytes))
                {
                    totalSpace = totalNumberOfBytes;
                    totalFreeSpace = totalNumberOfFreeBytes;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get disk space for '{folderName}'", ex);
            }
        }

        private ulong GetDirectorySize(string sourceDir)
        {
            ulong size = 0;

            if (Directory.Exists(sourceDir))
            {
                try
                {
                    var currentDirectory = new DirectoryInfo(sourceDir);
                    //Add size of files in the Current Directory to main size.
                    currentDirectory.GetFiles().ToList().ForEach(f => size += (ulong) f.Length);

                    //Loop on Sub Directories in the Current Directory and Calculate their files size.
                    currentDirectory.GetDirectories().ToList()
                        .ForEach(d => size += GetDirectorySize(d.FullName));
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to get directory size for '{sourceDir}'", e);
                }
            }

            return size;
        }
    }
}
