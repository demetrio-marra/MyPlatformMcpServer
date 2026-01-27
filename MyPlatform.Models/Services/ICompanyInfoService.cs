using MyPlatformModels.Models;

namespace MyPlatformModels.Services
{
    public interface ICompanyInfoService
    {
        Task<IEnumerable<CompanyInfoProductHierarchyItem>> GetCompanyInfoProductsHierarchyItemsAsync(Companies? company, Families? family);
        Task ValidateProductHierarchy(Companies? company, Families? family, Products? product = null);
        Task<IEnumerable<CompanyInfoProductHierarchyItem>> GetProductHierarchyListAsync(Products product);
    }
}
