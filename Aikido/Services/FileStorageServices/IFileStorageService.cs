using Aikido.Entities;

namespace Aikido.Services.FileStorageServices
{
    public interface IFileStorageService
    {
        Task SaveFileAsync(IFormFile file, string path);
        Task<Stream> GetFileAsync(string path);
        void DeleteFile(string path);
        bool FileExists(string path);
    }

}
