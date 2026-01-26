using MyPlatformModels.Models;

namespace MyPlatformModels.Services
{
    public interface IFileServerService
    {
        /// <summary>
        /// Upload a file with its metadata
        /// </summary>
        /// <param name="fileName">Original file name</param>
        /// <param name="contentType">MIME type of the file</param>
        /// <param name="fileContent">File content as stream</param>
        /// <param name="metadata">Generic metadata dictionary</param>
        /// <returns>File metadata with generated ID</returns>
        Task<FileMetadata> UploadFileAsync(string fileName, string contentType, Stream fileContent, Dictionary<string, string>? metadata = null);
    }
}
