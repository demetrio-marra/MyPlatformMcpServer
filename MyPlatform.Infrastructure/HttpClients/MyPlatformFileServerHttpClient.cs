using AutoMapper;
using MyPlatformInfrastructure.DTOs;
using MyPlatformModels.HttpClients;
using MyPlatformModels.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MyPlatformInfrastructure.HttpClients
{
    public class MyPlatformFileServerHttpClient : IFileServerHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        private readonly ILogger<MyPlatformFileServerHttpClient> _logger;

        public MyPlatformFileServerHttpClient(
            HttpClient httpClient, 
            IMapper mapper, 
            ILogger<MyPlatformFileServerHttpClient> logger)
        {
            _httpClient = httpClient;
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
            var response = await _httpClient.PostAsync("api/v1/FileServer/upload", content);
            
            _logger.LogDebug("Received response with status code: {StatusCode}", response.StatusCode);
            response.EnsureSuccessStatusCode();

            // Read and deserialize the response
            var responseJson = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Response body received, length: {Length} characters", responseJson?.Length ?? 0);
            
            var fileMetadataDto = JsonSerializer.Deserialize<FileMetadataDTO>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (fileMetadataDto == null)
            {
                _logger.LogDebug("Failed to deserialize response to FileMetadataDTO");
                throw new InvalidOperationException("Failed to deserialize file metadata response");
            }

            _logger.LogDebug("Successfully deserialized response. FileId={FileId}", fileMetadataDto.Id);

            var ret = _mapper.Map<FileMetadata>(fileMetadataDto);

            _logger.LogDebug("File upload completed successfully. FileId={FileId}, FileName={FileName}", 
                ret.Id, ret.FileName);

            return ret;
        }
    }
}
