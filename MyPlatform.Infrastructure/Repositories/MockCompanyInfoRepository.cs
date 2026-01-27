using MyPlatformModels.Models;
using MyPlatformModels.Repositories;

namespace MyPlatformInfrastructure.Repositories
{
    public class MockCompanyInfoRepository : ICompanyInfoRepository
    {
        private static readonly List<CompanyInfoProductHierarchyItem> _mockHierarchy = new()
        {
            new CompanyInfoProductHierarchyItem { Company = "NetWave", Family = "Connectivity", Product = "FiberInternet1Gbps" },
            new CompanyInfoProductHierarchyItem { Company = "NetWave", Family = "Connectivity", Product = "BusinessEthernet" },
            new CompanyInfoProductHierarchyItem { Company = "NetWave", Family = "CloudCompute", Product = "VirtualPrivateServer" },
            new CompanyInfoProductHierarchyItem { Company = "NetWave", Family = "CloudCompute", Product = "DedicatedBareMetalServer" },
            new CompanyInfoProductHierarchyItem { Company = "NetWave", Family = "CloudStorage", Product = "ObjectStorage" },
            new CompanyInfoProductHierarchyItem { Company = "SkyLink", Family = "Connectivity", Product = "FiberInternet1Gbps" },
            new CompanyInfoProductHierarchyItem { Company = "SkyLink", Family = "Connectivity", Product = "BusinessEthernet" },
            new CompanyInfoProductHierarchyItem { Company = "SkyLink", Family = "CloudCompute", Product = "VirtualPrivateServer" },
            new CompanyInfoProductHierarchyItem { Company = "SkyLink", Family = "CloudStorage", Product = "ObjectStorage" },
            new CompanyInfoProductHierarchyItem { Company = "SkyLink", Family = "CloudStorage", Product = "BackupAsAService" },
            new CompanyInfoProductHierarchyItem { Company = "SkyLink", Family = "NetworkServices", Product = "ManagedFirewall" },
            new CompanyInfoProductHierarchyItem { Company = "NetWave", Family = "NetworkServices", Product = "ManagedFirewall" },
            new CompanyInfoProductHierarchyItem { Company = "NetWave", Family = "NetworkServices", Product = "LoadBalancerAsAService" },
        };

        public Task<IEnumerable<CompanyInfoProductHierarchyItem>> GetCompanyInfoProductsHierarchyItemsAsync(
            Companies? company, 
            Families? family, 
            AclModel? userAcl = null)
        {
            var query = _mockHierarchy.AsEnumerable();

            if (company.HasValue)
            {
                var companyName = company.Value.ToString();
                query = query.Where(x => x.Company == companyName);
            }

            if (family.HasValue)
            {
                var familyName = family.Value.ToString();
                query = query.Where(x => x.Family == familyName);
            }

            if (userAcl != null && userAcl.AclDetails?.Any() == true)
            {
                var allowedCompanies = userAcl.AclDetails
                    .Where(a => a.Company.HasValue)
                    .Select(a => ((Companies)a.Company!.Value).ToString())
                    .ToHashSet();
                
                if (allowedCompanies.Any())
                {
                    query = query.Where(x => x.Company != null && allowedCompanies.Contains(x.Company));
                }
            }

            return Task.FromResult(query);
        }

        public Task<IEnumerable<CompanyInfoProductHierarchyItem>> GetProductHierarchyListAsync(
            Products product, 
            AclModel? userAcl = null)
        {      
            var productName = product.ToString();
            var query = _mockHierarchy.Where(x => x.Product == productName);

            if (userAcl != null && userAcl.AclDetails?.Any() == true)
            {
                var allowedCompanies = userAcl.AclDetails
                    .Where(a => a.Company.HasValue)
                    .Select(a => ((Companies)a.Company!.Value).ToString())
                    .ToHashSet();
                
                if (allowedCompanies.Any())
                {
                    query = query.Where(x => x.Company != null && allowedCompanies.Contains(x.Company));
                }
            }

            return Task.FromResult(query);
        }
    }
}
