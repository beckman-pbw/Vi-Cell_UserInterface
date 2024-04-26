using System;
using System.Windows.Markup;

namespace ScoutUtilities.Enums
{  
    public enum FileType
    {
        Fcs,
        Csv,
        Pdf,
        Txt,
        Xls
    }

    public static class FileTypeHelper
    {
        public static string GetString(FileType fileType, bool includeDot = true)
        {
            switch (fileType)
            {
                case FileType.Fcs: return includeDot ? ".fcs" : "fcs";
                case FileType.Csv: return includeDot ? ".csv" : "csv";
                case FileType.Pdf: return includeDot ? ".pdf" : "pdf";
                case FileType.Txt: return includeDot ? ".txt" : "txt";
                case FileType.Xls: return includeDot ? ".xls" : "xls";
            }

            return string.Empty;
        }

        public static FileType GetFileType(string fileExt)
        {
            var ext = fileExt.Replace(".", "").ToLower();
            switch (ext)
            {
                case "fcs": return FileType.Fcs;
                case "csv": return FileType.Csv;
                case "pdf": return FileType.Pdf;
                case "txt": return FileType.Txt;
                case "xls": return FileType.Xls;
            }

            throw new ArgumentException($"File Extension ({fileExt}) is not a valid FileType enum.");
        }
    }
}