using MyPlatformModels.Models;
using MyPlatformModels.Services;

namespace McpServerInfrastructure.Services
{
    public class MockACLService : IACLService
    {
        public MockACLService(HttpClient httpClient)
        {
            
        }

        public async Task<AclModel> GetUserAclAsync()
        {
            var ret = new AclModel();

            return await Task.FromResult(ret);
        }
    }
}
