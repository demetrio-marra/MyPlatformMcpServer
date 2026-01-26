using MyPlatformModels.Models;

namespace MyPlatformModels.HttpClients
{
    public interface IFileServerHttpClient
    {
        Task<FileMetadata> UploadFileAsync(
            string fileName,
            string contentType,
            Stream fileContent,
            Dictionary<string, string>? metadata = null);
    }
}
