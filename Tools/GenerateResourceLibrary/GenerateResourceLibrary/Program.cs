using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GenerateResourceLibrary
{
    class Program
    {
        public static string ResourceLibraryFile = "ResourceLibrary.xaml";
        public static string ConverterResourcesFile = "ConverterResources.xaml";
        public static string MiscFile = "Misc.xaml";
        public static string CommonFile = "Common.xaml";

        public static string BaseLine = "\t\t<ResourceDictionary Source=\"/ResourceDictionaries/{0}\" />";

        static void Main(string[] args)
        {
            var rootDir = ValidateArgs(args);

            var sb = new StringBuilder();
            AppendLine(sb, "<ResourceDictionary xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" >");
            AppendLine(sb, "\t<ResourceDictionary.MergedDictionaries>");

            foreach (var xamlFile in Directory.GetFiles(rootDir, "*.xaml", SearchOption.AllDirectories))
            {
                var parent = Directory.GetParent(xamlFile).ToString();
                var filename = Path.GetFileName(xamlFile);

                if (string.IsNullOrEmpty(filename) || filename.Equals(ResourceLibraryFile) || filename.Equals(ConverterResourcesFile) || filename.Equals(MiscFile)) continue;

                var relativePath = Regex.Replace(parent, @".*\\ResourceDictionaries\\?", "").Replace("\\", "/");

                AppendLine(sb, string.IsNullOrEmpty(relativePath)
                    ? string.Format(BaseLine, $"{relativePath}{filename}")
                    : string.Format(BaseLine, $"{relativePath}/{filename}"));
            }

            AppendLine(sb, "\t</ResourceDictionary.MergedDictionaries>");
            AppendLine(sb, "</ResourceDictionary>");
            var contents = sb.ToString();

            File.WriteAllText(Path.Combine(rootDir, ResourceLibraryFile), contents, Encoding.UTF8);
        }

        private static void AppendLine(StringBuilder sb, string appendant)
        {
            sb.Append(appendant);
            sb.Append(Environment.NewLine);
        }

        private static string ValidateArgs(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                ConsoleWriteError("Resource Folder Path required:: Example: C:\\SourceCode\\UserInterface\\ScoutUI\\ResourceDictionaries");
            }

            var rootDir = args[0];
            if (!Directory.Exists(rootDir))
            {
                ConsoleWriteError($"Folder path given does not exist: '{rootDir}'");
            }

            var resourceFile = Path.Combine(rootDir, ResourceLibraryFile);
            if (!File.Exists(resourceFile))
            {
                ConsoleWriteError($"File does not exist: '{resourceFile}'");
            }

            return rootDir;
        }

        private static void ConsoleWriteError(string error, int exitCode = 1)
        {
            Console.WriteLine();
            Console.WriteLine(error);
            Console.WriteLine();
            Environment.Exit(exitCode);
        }
    }
}
