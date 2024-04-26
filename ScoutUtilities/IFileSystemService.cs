namespace ScoutUtilities
{
    public interface IFileSystemService
    {
        bool DirectoryExists(string path);
        bool CreateDirectory(string path);
        bool DeleteFileSafe(string path);
        bool DeleteAllFilePath(string path);
        bool FileNameIsValid(string filename);
        bool FolderIsValidForScheduledExport(string path);

    }
}