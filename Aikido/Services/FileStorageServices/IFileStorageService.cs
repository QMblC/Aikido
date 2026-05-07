using Aikido.Entities;

namespace Aikido.Services.FileStorageServices
{
    public interface IFileStorageService
    {
        Task SaveFileAsync(Stream stream, string relativePath);
        Task<Stream> GetFileAsync(string relativePath);
        void DeleteFile(string relativePath);
        bool FileExists(string relativePath);
    }

}
