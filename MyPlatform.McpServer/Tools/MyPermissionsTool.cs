using AutoMapper;
using MyPlatformMcpServer.DTOs;
using MyPlatformModels.Exceptions;
using MyPlatformModels.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MyPlatformMcpServer.Tools
{
    [McpServerToolType]
    public sealed class MyPermissionsTool
    {
        private readonly IACLService _aclService;
        private readonly ILogger<MyPermissionsTool> _logger;
        private readonly IMapper _mapper;

        public MyPermissionsTool(
            IACLService aclService,
            ILogger<MyPermissionsTool> logger,
            IMapper mapper)
        {
            _aclService = aclService;
            _logger = logger;
            _mapper = mapper;
        }

        [McpServerTool(Name = "MyPlatform_MyPermissions_Get",
            Idempotent = true, Destructive = false, OpenWorld = false, ReadOnly = true, UseStructuredContent = true),
            Description("Get the current user's permissions (ACLs). Returns a list of access control permissions including Company, Family, and Product access rights.")]
        public async Task<IEnumerable<MyPermissionsDTO>> GetMyPermissionsAsync()
        {
            try
            {
                var result = await _aclService.GetUserAclAsync();

                _logger.LogInformation("Successfully retrieved user permissions with {Count} ACL details", result.AclDetails?.Count() ?? 0);

                return _mapper.Map<IEnumerable<MyPermissionsDTO>>(result.AclDetails);
            }
            catch (AgentIdNotFoundException ex)
            {
                _logger.LogWarning("Failed to retrieve user permissions in GetMyPermissionsAsync: {ErrorMessage}", ex.Message);
                throw new CustomToolException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMyPermissionsAsync: {ErrorMessage}", ex.Message);
                throw new CustomToolException("An error occurred while retrieving user permissions.", ex);
            }
        }
    }
}
