using Aikido.Entities;

namespace Aikido.Services.FileStorageServices
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _basePath;

        public FileStorageService(IWebHostEnvironment env)
        {
            _basePath = Path.Combine(env.WebRootPath, "uploads");
        }

        public async Task SaveFileAsync(IFormFile file, string relativePath)
        {
            var fullPath = Path.Combine(_basePath, relativePath);
            var directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        public async Task<Stream> GetFileAsync(string relativePath)
        {
            var fullPath = Path.Combine(_basePath, relativePath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Файл не найден", relativePath);

            var memory = new MemoryStream();
            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }

            memory.Position = 0;
            return memory;
        }

        public void DeleteFile(string relativePath)
        {
            var fullPath = Path.Combine(_basePath, relativePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        public bool FileExists(string relativePath)
        {
            var fullPath = Path.Combine(_basePath, relativePath);
            return File.Exists(fullPath);
        }
    }
}
