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

        public async Task SaveFileAsync(Stream stream, string relativePath)
        {
            var fullPath = Path.Combine(_basePath, relativePath);

            EnsureDirectoryExists(fullPath);

            using var fileStream = new FileStream(
                fullPath,
                FileMode.Create,
                FileAccess.Write);

            await stream.CopyToAsync(fileStream);
        }

        public async Task<Stream> GetFileAsync(string relativePath)
        {
            var fullPath = Path.Combine(_basePath, relativePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException(
                    "Файл не найден",
                    relativePath);

            var memory = new MemoryStream();

            using (var fileStream = new FileStream(
                       fullPath,
                       FileMode.Open,
                       FileAccess.Read))
            {
                await fileStream.CopyToAsync(memory);
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

        private static void EnsureDirectoryExists(string fullPath)
        {
            var directory = Path.GetDirectoryName(fullPath);

            if (directory == null)
                throw new InvalidOperationException(
                    "Не удалось определить директорию");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
