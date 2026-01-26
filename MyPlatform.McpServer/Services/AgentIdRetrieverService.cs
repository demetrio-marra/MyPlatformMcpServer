using MyPlatformMcpServer.Extensions;
using MyPlatformModels.Services;

namespace MyPlatformMcpServer.Services
{
    public class AgentIdRetrieverService : IAgentIdRetrieverService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AgentIdRetrieverService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string?> GetAgentIdAsync()
        {
            var agentId = _httpContextAccessor.HttpContext!.GetAgentId();
            return await Task.FromResult(agentId);
        }
    }
}
