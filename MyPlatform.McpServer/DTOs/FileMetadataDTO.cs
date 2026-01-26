namespace MyPlatformMcpServer.DTOs
{
    /// <summary>
    /// File metadata response DTO
    /// </summary>
    public class FileMetadataDTO
    {
        /// <summary>
        /// Unique identifier for the file (MongoDB ObjectId)
        /// </summary>
        /// <example>507f1f77bcf86cd799439011</example>
        public string? Id { get; set; }

        /// <summary>
        /// Original file name
        /// </summary>
        /// <example>profile-picture.jpg</example>
        public required string FileName { get; set; }

        /// <summary>
        /// File extension
        /// </summary>
        /// <example>.jpg</example>
        public required string FileExtension { get; set; }

        /// <summary>
        /// MIME type of the file
        /// </summary>
        /// <example>image/jpeg</example>
        public required string ContentType { get; set; }

        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        /// <example>1048576</example>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// When the file was uploaded (UTC)
        /// </summary>
        /// <example>2024-01-15T10:30:00Z</example>
        public DateTime UploadedAt { get; set; }

        /// <summary>
        /// Generic metadata dictionary
        /// </summary>
        /// <example>
        /// {
        ///   "description": "User profile picture",
        ///   "category": "images"
        /// }
        /// </example>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
