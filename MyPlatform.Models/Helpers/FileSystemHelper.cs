namespace MyPlatformModels.Helpers
{
    public static class FileSystemHelper
    {
        /// <summary>
        /// Sanitizes a string to be used as a safe filename by removing invalid characters
        /// and applying proper filename conventions. Preserves the file extension if present.
        /// </summary>
        /// <param name="fileName">The filename to sanitize (with or without extension)</param>
        /// <returns>A sanitized filename with extension preserved</returns>
        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("Filename cannot be null or whitespace.", nameof(fileName));
            }

            // Separate the extension from the filename
            var extension = Path.GetExtension(fileName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            // Remove invalid filename characters from the name part
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", nameWithoutExtension.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            // Replace multiple spaces with a single underscore
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"\s+", "_");

            // Remove leading/trailing underscores or dots
            sanitized = sanitized.Trim('_', '.', ' ');

            // Limit the length to avoid filesystem issues (max 200 chars for the name part, excluding extension)
            const int maxLength = 200;
            if (sanitized.Length > maxLength)
            {
                sanitized = sanitized.Substring(0, maxLength).TrimEnd('_', '.', ' ');
            }

            // If after sanitization the string is empty, throw exception
            if (string.IsNullOrWhiteSpace(sanitized))
            {
                throw new ArgumentException("Filename cannot be empty after sanitization.", nameof(fileName));
            }

            // Return with original extension preserved
            return sanitized + extension;
        }
    }
}
