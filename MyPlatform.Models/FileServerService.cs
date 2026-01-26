using MyPlatformModels.Exceptions;
using MyPlatformModels.HttpClients;
using MyPlatformModels.Models;
using MyPlatformModels.Services;
using Microsoft.Extensions.Logging;

namespace MyPlatformInfrastructure.Services
{
    public class FileServerService : IFileServerService
    {
        private readonly ILogger<FileServerService> _logger;
        private readonly IFileServerHttpClient _httpClient;

        public FileServerService(
            ILogger<FileServerService> logger,
            IFileServerHttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<FileMetadata> UploadFileAsync(
            string fileName,
            string contentType,
            Stream fileContent,
            Dictionary<string, string>? metadata = null)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new UnprocessableRequestException("FileName cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(contentType))
            {
                throw new UnprocessableRequestException("ContentType cannot be null or empty");
            }

            if (fileContent == null || !fileContent.CanRead)
            {
                throw new UnprocessableRequestException("FileContent must be a readable stream");
            }

            // Extract file extension
            var fileExtension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                throw new UnprocessableRequestException("FileName must have a valid extension");
            }

            // Get file size
            long fileSize = 0;
            if (fileContent.CanSeek)
            {
                fileSize = fileContent.Length;
            }

            var uploadedFileInfo = await _httpClient.UploadFileAsync(fileName, contentType, fileContent, metadata);

            _logger.LogInformation("File uploaded successfully with Id: {id}, FileName: {fileName}, Size: {size} bytes",
                uploadedFileInfo.Id, fileName, fileSize);

            return uploadedFileInfo;
        }
    }
}
