using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using log4net;
using ScoutLanguageResources;
using ScoutUtilities.Services;
using ScoutUtilities.UIConfiguration;

namespace ScoutUtilities.Common
{
    public static class FileSystem
    {
        #region Constructor

        static FileSystem()
        {
            DefaultImageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ApplicationConstants.TargetFolderName);

            _fileSystemService = new FileSystemService(null);
        }

        #endregion

        #region Properties & Fields
        
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static readonly Encoding DefaultEncoding = Encoding.UTF8;
        
        public static string DefaultImageDirectory { get; }
        private static readonly IFileSystemService _fileSystemService;

        #endregion

        #region Public Methods

        public static bool ConvertGenericListToCsvXlsFile<T>(List<T> list, string csvCompletePath)
        {
            try
            {
                if (list == null || list.Count == 0) return false;

                var t = list[0].GetType();
                var directoryName = Path.GetDirectoryName(csvCompletePath);

                if (!Directory.Exists(directoryName) && !string.IsNullOrEmpty(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                if (File.Exists(csvCompletePath))
                {
                    File.Delete(csvCompletePath);
                    using (var file = File.Create(csvCompletePath))
                    {
                        file.Close();
                    }
                }

                using (var sw = new StreamWriter(csvCompletePath))
                {
                    var o = Activator.CreateInstance(t);
                    var props = o.GetType().GetProperties();
                    sw.Write(string.Join(",", props.Select(d => d.Name).ToArray()) + Environment.NewLine);

                    //Write all clr properties of CarouselDomain with its property info
                    foreach (var item in list)
                    {
                        var itemType = item.GetType();
                        var values = from prop in props
                                     let itemProp = itemType.GetProperty(prop.Name)
                                     where itemProp != null
                                     select itemProp.PropertyType == typeof(DateTime)
                                         ? Misc.ConvertToCustomLongDateTimeFormat((DateTime)itemProp.GetValue(item, null))
                                         : itemProp.GetValue(item, null);

                        var row = string.Join(",", values);
                        sw.Write(row + Environment.NewLine);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to write CSV file '{csvCompletePath}'", e);
                return false;
            }
        }

        public static void EncryptFile(string inputFile, string outputFile, string skey)
        {
            using (var aes = new RijndaelManaged())
            {
                var key = Encoding.UTF8.GetBytes(skey);
                var iv = Encoding.UTF8.GetBytes(skey);
                using (var fsCrypt = new FileStream(outputFile, FileMode.Create))
                {
                    using (var encryptor = aes.CreateEncryptor(key, iv))
                    {
                        using (var cs = new CryptoStream(fsCrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (var fsIn = new FileStream(inputFile, FileMode.Open))
                            {
                                int data;
                                while ((data = fsIn.ReadByte()) != -1)
                                {
                                    cs.WriteByte((byte)data);
                                }
                            }
                        }
                    }
                }
            }

            File.Delete(inputFile);
        }

        public static void DecryptFile(string inputFile, string outputFile, string skey)
        {
            var path = Path.GetDirectoryName(outputFile);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            using (var aes = new RijndaelManaged())
            {
                var key = Encoding.UTF8.GetBytes(skey);
                var iv = Encoding.UTF8.GetBytes(skey);
                using (var fsCrypt = new FileStream(inputFile, FileMode.Open))
                {
                    using (var fsOut = new FileStream(outputFile, FileMode.Create))
                    {
                        using (var decryptor = aes.CreateDecryptor(key, iv))
                        {
                            using (var cs = new CryptoStream(fsCrypt, decryptor, CryptoStreamMode.Read))
                            {
                                int data;
                                while ((data = cs.ReadByte()) != -1)
                                {
                                    fsOut.WriteByte((byte)data);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static bool ExportConcentrationSlopeToCsv(DataSet ds, string fullFilePath)
        {
            try
            {
                using (var writer = new StreamWriter(fullFilePath))
                {
                    foreach (DataTable sourceTable in ds.Tables)
                    {
                        var includeHeaders = !sourceTable.TableName.Equals("Second");
                        writer.WriteLine();

                        if (includeHeaders)
                        {
                            var headerValues = sourceTable.Columns.OfType<DataColumn>().Select(column => QuoteValue(column.ColumnName));
                            writer.WriteLine(string.Join(",", headerValues));
                        }

                        foreach (DataRow row in sourceTable.Rows)
                        {
                            var items = row.ItemArray.Select(o => QuoteValue(o?.ToString() ?? string.Empty));
                            writer.WriteLine(string.Join(",", items));
                        }

                        writer.Flush();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed the export concentration slope to CSV '{fullFilePath}'", ex);
                return false;
            }
        }

        public static bool DeleteAllFilePath(string filePathLocation)
        {
            return _fileSystemService.DeleteAllFilePath(filePathLocation);
        }

        public static bool DeleteFileSafe(string fullFilePath)
        {
            return _fileSystemService.DeleteFileSafe(fullFilePath);
        }

        public static void ExportDataTableWithKeysToFile(DataTable dataTable, string filePath, bool isAppend = false, bool includeHeader = true)
        {
            IEnumerable<string> headerValues = null;
            if (includeHeader)
            {
                headerValues = dataTable.Columns.OfType<DataColumn>().Select(column => QuoteValue(LanguageResourceHelper.Get(column.ColumnName)));
            }

            ExportDataTableToFileInternal(filePath, isAppend, dataTable.Rows, headerValues);
        }

		public static void ExportDataTableWithKeysToFile_NoXlate(DataTable dataTable, string filePath, bool isAppend = false, bool includeHeader = true)
		{
			IEnumerable<string> headerValues = null;
			if (includeHeader)
			{
				headerValues = dataTable.Columns.OfType<DataColumn>().Select(column => QuoteValue(column.ColumnName));
			}

			ExportDataTableToFileInternal(filePath, isAppend, dataTable.Rows, headerValues);
		}

        public static string GetImageSavePath(string workItemName, bool createDirIfNotExists)
        {
            var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ApplicationConstants.TargetFolderName, workItemName);

            if (createDirIfNotExists && !Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            return basePath;
        }

        public static void OpenFolderLocationInExplorer(string pathToOpen)
        {
            try
            {
                var folderPath = Path.GetDirectoryName(pathToOpen);
                Process.Start("explorer.exe", folderPath);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to open explorer.exe to path '{pathToOpen}'", e);
            }
        }

        public static string QuoteValue(string value)
        {
            return string.Concat("\"", value.Replace("\"", "\"\""), "\"");
        }

        public static bool IsFileNameValid(string filename)
        {
            try
            {
                if (string.IsNullOrEmpty(filename))
                    return false;

                var outFile = Path.GetFileName(filename);
                if (string.IsNullOrEmpty(outFile))
                    return false;

                string dir = Path.GetDirectoryName(filename);
                if (!string.IsNullOrEmpty(dir) && !Path.IsPathRooted(dir))
                    return false;

                var badChars = Path.GetInvalidFileNameChars();
                int badIdx = outFile.IndexOfAny(badChars);
                if (badIdx >= 0)
                    return false;

                return true;
            }
            catch (Exception e)
            {
                Log.Error($"IsFileNameValid error '{filename}'", e);
            }
            return false;
        }

        public static bool IsPathValid(string strPath)
        {
            try
            {
                if (string.IsNullOrEmpty(strPath))
                    return false;
                var badChars = Path.GetInvalidPathChars();
                int badIdx = strPath.IndexOfAny(badChars);
                if (badIdx >= 0)
                    return false;
                string outPath = GetDirectoryName(strPath);
                if (string.IsNullOrEmpty(outPath))
                    return false;
                badIdx = outPath.IndexOfAny(badChars);
                if (badIdx >= 0)
                    return false;

                if (Path.IsPathRooted(outPath))
                    return true;

            }
            catch (Exception e)
            {
                Log.Error($"Error - IsPathValid", e);
            }
            return false;
        }

        public static bool IsFolderValidForExport(string path)
        {
            // Keep in mind that some of these folder checks require having a trailing '\' to be 
            // considered a path. Lots of little things but the unit tests should be checking
            // all of these things. See unit tests here:
            try
            {
                if (string.IsNullOrEmpty(path))
                    return false;

                if (!IsPathValid(path))
                    return false;

                path = GetDirectoryName(path);
                if (!FileSystem.IsPathValid(path))
                    return false;

                if (!UISettings.IsHardwareOrSimulated())
                    return true;

                DirectoryInfo dirInfo = new DirectoryInfo(path);
                string pathRoot = dirInfo.Root.FullName;
                string instrRoot = UISettings.DriveName; // "C:" at the time of writing
                if (!instrRoot.EndsWith(@"\")) instrRoot += @"\";
                if (instrRoot.Equals(pathRoot, StringComparison.InvariantCultureIgnoreCase))
                {
                    // This directory is on the C: drive 
                    // AND 
                    // We are on an instrument or simulating one
                    var pathUri = new Uri(path);
                    var allowedLocalExportPath = $"{UISettings.DriveName}{ApplicationConstants.BasePathToExportedZip}";
                    if (!allowedLocalExportPath.EndsWith("\\")) allowedLocalExportPath += "\\";
                    var allowedPathUri = new Uri(allowedLocalExportPath);
                    var allowedDirInfo = new DirectoryInfo(allowedLocalExportPath);

                    // check if pathUri is C:\Instrument\Export
                    string allowedDirName = allowedDirInfo.FullName.TrimEnd('\\');

                    if (allowedDirName.Equals(dirInfo.FullName.TrimEnd('\\')))
                        return true;

                    // check that it is under C:\Instrument\Export
                    var retval = allowedPathUri.IsBaseOf(pathUri);
                    return retval;
                }

                // Not on C: - any path is allowed
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"Error - IsFolderValidForExport", e);
            }
            return false;
        }

        public static string GetDefaultExportPath(string username)
        {
            try
            {
                var path = $"{UISettings.DriveName}{ApplicationConstants.BasePathToExportedZip}";
                if (!path.EndsWith("\\")) path += "\\";
                if (username?.Length > 0)
                {
                    path = Path.Combine(path, username);
                    if (!path.EndsWith("\\")) path += "\\";
                }
                return path;
            }
            catch (Exception e)
            {
                Log.Error("Failed to get default export path", e);
            }
            return string.Empty;
        }

        // ******************************************************************
        public static bool EnsureDirectoryExists(string dirname)
        {
            try
            {
                if (string.IsNullOrEmpty(dirname))
                    return false;

                dirname = GetDirectoryName(dirname);

                if (!IsPathValid(dirname))
                {
                    Log.Error("EnsureDirectoryExists invalid path " + dirname);
                    return false;
                }

                if (Directory.Exists(dirname))
                    return true;

                DirectoryInfo di = Directory.CreateDirectory(dirname);
                if ((di != null) && di.Exists)
                {
                    Log.Debug("EnsureDirectoryExists Created " + dirname);
                    return true;
                }
                Log.Error("EnsureDirectoryExists " + dirname);
            }
            catch (Exception e)
            {
                Log.Error("EnsureDirectoryExists " + dirname, e);
            }
            return false;
        }

        public static string GetDirectoryName(string fullFilePath)
        {
            string outpath = "";
            try
            {
                if (fullFilePath.EndsWith("\\"))
                {
                    outpath = Path.GetFullPath(fullFilePath);
                    if (string.IsNullOrEmpty(outpath) && Path.IsPathRooted(fullFilePath))
                        outpath = Path.GetPathRoot(fullFilePath + "\\");
                }
                else
                {
                    try
                    {
                        if (!Path.HasExtension(fullFilePath))
                        {
                            outpath = Path.GetDirectoryName(fullFilePath + "\\");
                            if (string.IsNullOrEmpty(outpath) && Path.IsPathRooted(fullFilePath + "\\"))
                                outpath = Path.GetPathRoot(fullFilePath + "\\");
                        }
                        else
                        {
                            outpath = Path.GetDirectoryName(fullFilePath);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"GetDirectoryName failed: ", e);
                    }
                }

                if (!outpath.EndsWith("\\"))
                    outpath += "\\";
            }
            catch (Exception e)
            {
                Log.Error($"GetDirectoryName failed: ", e);
            }
            return outpath;
        }

        #endregion

        #region Private Methods

        private static void ExportDataTableToFileInternal(string filePath, bool isAppend, DataRowCollection rowCollection, IEnumerable<string> headerValues = null)
        {
            try
            {
                var directoryName = Path.GetDirectoryName(filePath);
                if(!EnsureDirectoryExists(directoryName))
                {
                    Log.Error($"ExportDataTableToFileInternal failed to create directory: " + directoryName);
                    return;
                }
                using (var sw = new StreamWriter(filePath, isAppend, DefaultEncoding))
                {
                    if (headerValues != null && headerValues.Any())
                    {
                        sw.WriteLine(string.Join(",", headerValues));
                    }

                    foreach (DataRow row in rowCollection)
                    {
                        if (row == null) continue;
                        var items = row.ItemArray.Select(o => QuoteValue(o?.ToString() ?? string.Empty));
                        sw.WriteLine(string.Join(",", items));
                    }
                    sw.Flush();
                }
            }
            catch (Exception e)
            {
                Log.Error($"ExportDataTableToFileInternal failed: ", e);
            }
        }

        #endregion
    }
}