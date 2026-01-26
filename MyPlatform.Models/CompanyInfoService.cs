using MyPlatformModels.Helpers;
using MyPlatformModels.Models;
using MyPlatformModels.Repositories;
using MyPlatformModels.Services;

namespace MyPlatformModels
{
    public class CompanyInfoService : ICompanyInfoService
    {
        private readonly ICompanyInfoRepository _companyInfoRepository;
        private readonly IACLService _aclService;

        public CompanyInfoService(ICompanyInfoRepository companyInfoRepository, IACLService aclService)
        {
            _companyInfoRepository = companyInfoRepository;
            _aclService = aclService;
        }

        public async Task<IEnumerable<CompanyInfoProductHierarchyItem>> GetCompanyInfoProductsHierarchyItemsAsync(Companies? company, Families? family)
        {
            var userAcl = await _aclService.GetUserAclAsync();
            return await _companyInfoRepository.GetCompanyInfoProductsHierarchyItemsAsync(company, family, userAcl);
        }

        public async Task<CompanyInfoProductHierarchyItem?> GetProductHierarchyAsync(Products product)
        {
            var userAcl = await _aclService.GetUserAclAsync();
            return await _companyInfoRepository.GetProductHierarchyAsync(product, userAcl);
        }

        public async Task<IEnumerable<CompanyInfoProductHierarchyItem>> GetProductHierarchyListAsync(Products product)
        {
            var userAcl = await _aclService.GetUserAclAsync();
            return await _companyInfoRepository.GetProductHierarchyListAsync(product, userAcl);
        }

        public async Task ValidateProductHierarchy(Companies? company, Families? family, Products? product = null)
        {
            // Further validations can be added as per business rules
            // e.g., if family is provided, company must also be provided
            if (family != null && company == null)
            {
                throw new ArgumentException("If 'family' is provided, 'company' must also be provided.");
            }

            // If product is provided, both company and family must also be provided
            if (product != null && (company == null || family == null))
            {
                throw new ArgumentException("If 'product' is provided, both 'company' and 'family' must also be provided.");
            }

            // Get the complete hierarchy to validate the relationships
            var hierarchyItems = await GetCompanyInfoProductsHierarchyItemsNoAclAsync(null, null);

            // Check if the family belongs to the specified company
            if (family != null && company != null)
            {
                var companyLabel = MyPlatformEnumHelper.EnumToLabel<Companies>((int)company);
                var familyLabel = MyPlatformEnumHelper.EnumToLabel<Families>((int)family);

                var familyExistsInCompany = hierarchyItems
                    .Where(item => item.Company != null && item.Company != MyPlatformEnumHelper.UNKNOWN_VALUE)
                    .Where(item => item.Family != null && item.Family != MyPlatformEnumHelper.UNKNOWN_VALUE)
                    .Any(item => item.Company == companyLabel && item.Family == familyLabel);

                if (!familyExistsInCompany)
                {
                    throw new ArgumentException($"Family '{familyLabel}' does not belong to company '{companyLabel}'.");
                }
            }

            // Check if the product belongs to the specified company and family
            if (product != null && company != null && family != null)
            {
                var companyLabel = MyPlatformEnumHelper.EnumToLabel<Companies>((int)company);
                var familyLabel = MyPlatformEnumHelper.EnumToLabel<Families>((int)family);
                var productLabel = MyPlatformEnumHelper.EnumToLabel<Products>((int)product);

                var productExistsInCompanyFamily = hierarchyItems
                    .Where(item => item.Company != null && item.Company != MyPlatformEnumHelper.UNKNOWN_VALUE)
                    .Where(item => item.Family != null && item.Family != MyPlatformEnumHelper.UNKNOWN_VALUE)
                    .Where(item => item.Product != null && item.Product != MyPlatformEnumHelper.UNKNOWN_VALUE)
                    .Any(item => item.Company == companyLabel && item.Family == familyLabel && item.Product == productLabel);

                if (!productExistsInCompanyFamily)
                {
                    throw new ArgumentException($"Product '{productLabel}' does not belong to company '{companyLabel}' and family '{familyLabel}'.");
                }
            }
        }


        private async Task<IEnumerable<CompanyInfoProductHierarchyItem>> GetCompanyInfoProductsHierarchyItemsNoAclAsync(Companies? company, Families? family)
        {
            return await _companyInfoRepository.GetCompanyInfoProductsHierarchyItemsAsync(company, family);
        }

    }
}
