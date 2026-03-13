using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SyncVerse.Application.Interfaces.Storage;

namespace SyncVerse.Infrastructure.Storage
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _uploadsFolder;

        public LocalFileStorageService(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
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
            var normalizedPath = filePath.Replace("\\", "/");
            var relativePath = $"/uploads/{normalizedPath}";

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                return Task.FromResult(relativePath);

            var absoluteUrl = $"{request.Scheme}://{request.Host}{relativePath}";
            return Task.FromResult(absoluteUrl);
        }

        public Task<bool> FileExistsAsync(string filePath)
        {
            var fullPath = Path.Combine(_uploadsFolder, filePath);
            return Task.FromResult(File.Exists(fullPath));
        }
    }
}
