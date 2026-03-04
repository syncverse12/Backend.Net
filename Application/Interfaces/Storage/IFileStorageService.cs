namespace SyncVerse.Application.Interfaces.Storage
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder);
        Task<Stream> DownloadFileAsync(string filePath);
        Task<bool> DeleteFileAsync(string filePath);
        Task<string> GetFileUrlAsync(string filePath);
        Task<bool> FileExistsAsync(string filePath);
    }
}
