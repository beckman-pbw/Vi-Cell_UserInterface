using Ninject.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using ScoutUtilities.Common;
using ScoutUtilities.UIConfiguration;

namespace ScoutUtilities.Services
{
    public class FileSystemService : IFileSystemService
    {
        public FileSystemService(ILogger logger)
        {
            _logger = logger;
        }

        private readonly ILogger _logger;

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public bool CreateDirectory(string path)
        {
            try
            {
                if (DirectoryExists(path))
                    return true;

                var info = Directory.CreateDirectory(path);
                return info.Exists;
            }
            catch (Exception e)
            {
                _logger?.Error(e, $"Unable to create directory '{path}'.");
                return false;
            }
        }

        public bool DeleteFileSafe(string fullFilePath)
        {
            try
            {
                if (File.Exists(fullFilePath)) File.Delete(fullFilePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteAllFilePath(string path)
        {
            try
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    DeleteFileSafe(file);
                }

                var myDirInfo = new DirectoryInfo(path);
                foreach (var dir in myDirInfo.GetDirectories())
                {
                    dir.Delete(true);
                }

                return true;
            }
            catch (Exception e)
            {
                _logger?.Error(e, $"Error while deleting '{path}' and it's contents");
                return false;
            }
        }

        public bool FileNameIsValid(string filename)
        {
            return FileSystem.IsFileNameValid(filename);
        }

        public bool FolderIsValidForScheduledExport(string path)
        {
            return FileSystem.IsFolderValidForExport(path);
        }

    }
}