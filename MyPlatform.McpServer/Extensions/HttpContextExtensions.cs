namespace MyPlatformMcpServer.Extensions;

/// <summary>
/// Extensions for HttpContext
/// </summary>
public static class HttpContextExtensions
{
    private const string AgentIdHeaderName = "x-agent-id";


    /// <summary>
    /// Gets the agent ID from the AgentIdHeaderName header
    /// </summary>
    /// <param name="httpContext">The HTTP context</param>
    /// <returns>The agent ID from the header</returns>
    /// <exception cref="InvalidOperationException">Thrown if multiple agent IDs are found</exception>
    public static string? GetAgentId(this HttpContext httpContext)
    {
        // Get agent id from AgentIdHeaderName header
        if (!httpContext.Request.Headers.TryGetValue(AgentIdHeaderName, out var agentId))
        {
            return null;
        }

        return agentId.SingleOrDefault();
    }
}