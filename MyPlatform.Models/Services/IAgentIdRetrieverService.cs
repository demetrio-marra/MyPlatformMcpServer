namespace MyPlatformModels.Services
{
    /// <summary>
    /// Defines a service for retrieving agent identifiers.
    /// </summary>
    /// <remarks>This interface is intended to provide a contract for services that retrieve unique
    /// identifiers associated with agents. Implementations may define the specific mechanism for retrieving these
    /// identifiers.</remarks>
    public interface IAgentIdRetrieverService
    {
        Task<string?> GetAgentIdAsync();
    }
}
