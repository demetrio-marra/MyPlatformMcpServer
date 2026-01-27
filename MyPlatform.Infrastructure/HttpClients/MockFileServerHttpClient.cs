using AutoMapper;
using MyPlatformModels.HttpClients;
using MyPlatformModels.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MyPlatformInfrastructure.HttpClients
{
    public class MockFileServerHttpClient : IFileServerHttpClient
    {
        private readonly IMapper _mapper;
        private readonly ILogger<MockFileServerHttpClient> _logger;

        public MockFileServerHttpClient(
            HttpClient httpClient, 
            IMapper mapper, 
            ILogger<MockFileServerHttpClient> logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<FileMetadata> UploadFileAsync(
            string fileName,
            string contentType,
            Stream fileContent,
            Dictionary<string, string>? metadata = null)
        {
            _logger.LogDebug("Starting file upload: FileName={FileName}, ContentType={ContentType}, HasMetadata={HasMetadata}", 
                fileName, contentType, metadata != null && metadata.Count > 0);

            using var content = new MultipartFormDataContent();

            // Add file content
            var streamContent = new StreamContent(fileContent);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(streamContent, "file", fileName);

            _logger.LogDebug("File content added to multipart form data");

            // Add metadata as JSON if provided
            if (metadata != null && metadata.Count > 0)
            {
                var metadataJson = JsonSerializer.Serialize(metadata);
                content.Add(new StringContent(metadataJson), "metadataJson");
                _logger.LogDebug("Metadata added to request: {MetadataJson}", metadataJson);
            }

            // Send the request
            _logger.LogDebug("Sending POST request to: {Endpoint}", "api/v1/FileServer/upload");
            await Task.Delay(100); // Simulate network delay
            
            _logger.LogDebug("Received response with status code: {StatusCode}", System.Net.HttpStatusCode.OK);

            var ret = new FileMetadata { ContentType = contentType, FileName = fileName, Id = Guid.NewGuid().ToString(), 
                FileExtension = Path.GetExtension(fileName), FileSizeBytes = fileContent.Length, 
                UploadedAt = DateTime.UtcNow, Metadata = metadata ?? new Dictionary<string, string>() };

            _logger.LogDebug("File upload completed successfully. FileId={FileId}, FileName={FileName}", 
                ret.Id, ret.FileName);

            return ret;
        }
    }
}
