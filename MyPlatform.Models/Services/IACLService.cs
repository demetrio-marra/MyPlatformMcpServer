using MyPlatformModels.Models;

namespace MyPlatformModels.Services
{
    public interface IACLService
    {
        Task<AclModel> GetUserAclAsync();
    }
}
