namespace MyPlatformMcpServer.Configuration
{
    public class JwtConfiguration
    {
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public bool Enabled { get; set; }
    }
}