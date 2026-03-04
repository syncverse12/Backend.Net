using Microsoft.AspNetCore.Hosting;
using SyncVerse.Application.Interfaces.Storage;

namespace SyncVerse.Infrastructure.Storage
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadsFolder;

        public LocalFileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads");
            
            if (!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder);
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder)
        {
            var folderPath = Path.Combine(_uploadsFolder, folder);
            
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(folderPath, uniqueFileName);

            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            return Path.Combine(folder, uniqueFileName).Replace("\\", "/");
        }

        public async Task<Stream> DownloadFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_uploadsFolder, filePath);
            
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            var memoryStream = new MemoryStream();
            using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(memoryStream);
            }
            
            memoryStream.Position = 0;
            return memoryStream;
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_uploadsFolder, filePath);
                
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return Task.FromResult(true);
                }
                
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<string> GetFileUrlAsync(string filePath)
        {
            return Task.FromResult($"/uploads/{filePath}");
        }

        public Task<bool> FileExistsAsync(string filePath)
        {
            var fullPath = Path.Combine(_uploadsFolder, filePath);
            return Task.FromResult(File.Exists(fullPath));
        }
    }
}
