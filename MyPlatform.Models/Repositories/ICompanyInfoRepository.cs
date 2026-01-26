using MyPlatformModels.Models;

namespace MyPlatformModels.Repositories
{
    public interface ICompanyInfoRepository
    {
        Task<IEnumerable<CompanyInfoProductHierarchyItem>> GetCompanyInfoProductsHierarchyItemsAsync(Companies? company, Families? family, AclModel? userAcl = null);
        Task<CompanyInfoProductHierarchyItem?> GetProductHierarchyAsync(Products product, AclModel? userAcl = null);
        Task<IEnumerable<CompanyInfoProductHierarchyItem>> GetProductHierarchyListAsync(Products product, AclModel? userAcl = null);
    }
}
