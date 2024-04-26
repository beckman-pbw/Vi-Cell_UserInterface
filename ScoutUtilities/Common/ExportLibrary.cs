using ScoutLanguageResources;
using ScoutUtilities.Enums;
using ScoutUtilities.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScoutUtilities.Common
{
    public class ExportLibrary
    {
        public static void Export<T>(IEnumerable<T> list, int listType, string fileName, string filePath, string key,
            Action<Exception, string> exceptionAction)
        {
            string outputPath;
            var extension = Path.GetExtension(fileName);
            var fileNameNoExt = Path.GetFileNameWithoutExtension(fileName);
            var dir = Path.GetDirectoryName(filePath) ?? string.Empty;
            var inputPath = Path.Combine(dir, fileNameNoExt + ".xml");

            if (!string.IsNullOrEmpty(extension) && extension.Equals(".dat"))
            {
                outputPath = filePath;
            }
            else
            {
                outputPath = Path.Combine(dir, fileNameNoExt + ".dat");
            }

            var enumerable = list.ToList();
            try
            {
                if (XmlSerializeUtil.SerializeToXml(inputPath, enumerable))
                    FileSystem.EncryptFile(inputPath, outputPath, key);
            }
            catch (InvalidOperationException ex)
            {
                exceptionAction.Invoke(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_EXPORT_DATA"));
                return;
            }

            XmlHelper.CreateFileXml(FileType.Csv, outputPath, listType);
        }
    }
}