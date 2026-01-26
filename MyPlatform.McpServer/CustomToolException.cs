using ModelContextProtocol;

namespace MyPlatformMcpServer
{
    public class CustomToolException : McpException
    {
        public CustomToolException(string message) : base(message)
        {
            
        }

        public CustomToolException(string message, Exception innerException) : base(message, innerException)
        {
            
        }
    }
}
