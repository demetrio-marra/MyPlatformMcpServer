using MyPlatformModels.Exceptions;
using MyPlatformModels.Helpers;
using MyPlatformModels.Models;
using MyPlatformModels.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MyPlatformMcpServer.Tools;

[McpServerToolType]
public sealed class CompanyInfoTools
{
    private const string Desc_Company = "Company name";
    private const string Desc_Family = "Family name";
    private const string Desc_Product = "Product name";

    private readonly ICompanyInfoService _companyInfoService;
    private readonly ILogger<CompanyInfoTools> _logger;

    public CompanyInfoTools(ICompanyInfoService companyInfoService, ILogger<CompanyInfoTools> logger)
    {
        _companyInfoService = companyInfoService;
        _logger = logger;
    }

    /// <summary>
    /// Asynchronously retrieves a flattened map of company, family, and product hierarchies.
    /// </summary>
    /// <remarks>This method retrieves a hierarchical map of products based on the specified company and
    /// family filters.  If no filters are provided, the complete hierarchy is returned. The method ensures that only
    /// valid  company and family identifiers are processed, throwing an exception for invalid inputs.</remarks>
    /// <param name="company">The company identifier to filter the hierarchy. If null, the filter is omitted.</param>
    /// <param name="family">The family identifier to filter the hierarchy. If null, the filter is omitted.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of  <see
    /// cref="CompanyInfoProductHierarchyItem"/> representing the filtered product hierarchy.</returns>
    /// <exception cref="CustomToolException">Thrown if there is a validation error or if the hierarchy retrieval fails due to an internal error.</exception>
    [McpServerTool(Name = "MyPlatform_CompanyInfo_GetProductsHierarchy", 
        Idempotent = true, OpenWorld = false, ReadOnly = true, Destructive = false, UseStructuredContent = true),
        Description("CompanyInfo - Retrieves the Company/Family/Product flattened map. Omit filters to get the complete map.")]
    public async Task<IEnumerable<CompanyInfoProductHierarchyItem>> GetProductsHierarchyAsync(
        [Description(Desc_Company)] string? company = null,
        [Description(Desc_Family)] string? family = null)
    {
        Companies? companyEnum = null;
        Families? familyEnum = null;

        try
        {
            // Parse and validate company parameter
            if (!string.IsNullOrWhiteSpace(company))
            {
                companyEnum = MyPlatformEnumHelper.LabelToEnum<Companies>(company);
                if (companyEnum == null)
                {
                    throw new ArgumentException($"Invalid company value: '{company}'. Valid values are: {string.Join(", ", Enum.GetNames<Companies>())}");
                }
            }

            // Parse and validate family parameter
            if (!string.IsNullOrWhiteSpace(family))
            {
                familyEnum = MyPlatformEnumHelper.LabelToEnum<Families>(family);
                if (familyEnum == null)
                {
                    throw new ArgumentException($"Invalid family value: '{family}'. Valid values are: {string.Join(", ", Enum.GetNames<Families>())}");
                }
            }

            // Validate arguments using the service method
            await _companyInfoService.ValidateProductHierarchy(companyEnum, familyEnum);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error in GetProductsHierarchyAsync: {ErrorMessage}", ex.Message);
            throw new CustomToolException("Validation error in GetProductsHierarchy: " + ex.Message, ex);
        }

        try
        {
            var ret = await _companyInfoService.GetCompanyInfoProductsHierarchyItemsAsync(companyEnum, familyEnum);

            // ci sono prodotti sbagliati su db: fixiamo qui
            var cleaned = ret.Where(item =>
                (item.Company != MyPlatformEnumHelper.UNKNOWN_VALUE && item.Family != MyPlatformEnumHelper.UNKNOWN_VALUE && item.Product != MyPlatformEnumHelper.UNKNOWN_VALUE)
            ).ToList();

            _logger.LogInformation("Successfully retrieved products hierarchy with {Count} items", cleaned.Count);

            return cleaned;
        }
        catch (AgentIdNotFoundException ex)
        {
            _logger.LogWarning("Failed to retrieve products hierarchy in GetProductsHierarchyAsync: {ErrorMessage}", ex.Message);
            throw new CustomToolException(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetProductsHierarchyAsync for Company {Company}, Family {Family}: {ExceptionMessage}", company, family, ex.Message);
            throw new CustomToolException("Error in GetProductsHierarchy.", ex);
        }
    }

    /// <summary>
    /// Asynchronously retrieves the company and family hierarchy information for a specified product.
    /// </summary>
    /// <remarks>This method logs warnings and errors related to validation and retrieval issues. Ensure that
    /// the product name is valid and exists in the database to avoid exceptions.</remarks>
    /// <param name="product">The name of the product for which to find the hierarchy. This parameter cannot be null or empty.</param>
    /// <returns>A <see cref="CompanyInfoProductHierarchyItem"/> representing the hierarchy information of the specified product,
    /// or <see langword="null"/> if the product hierarchy is not found.</returns>
    /// <exception cref="CustomToolException">Thrown if there is a validation error, the product hierarchy is not found, or an error occurs during the
    /// retrieval process.</exception>
    /// <usedBy>Statistics tools</usedBy>
    [McpServerTool(Name = "MyPlatform_CompanyInfo_FindProductHierarchy",
        Idempotent = true, OpenWorld = false, ReadOnly = true, Destructive = false, UseStructuredContent = true),
        Description("CompanyInfo - Finds which Company and Family a specific Product belongs to.")]
    public async Task<CompanyInfoProductHierarchyItem?> GetProductHierarchyAsync(
        [Description(Desc_Product)] string product)
    {
        Products? productEnum = null;

        try
        {
            // Parse and validate product parameter
            if (string.IsNullOrWhiteSpace(product))
            {
                throw new ArgumentException("Product parameter is required and cannot be null or empty.");
            }

            productEnum = MyPlatformEnumHelper.LabelToEnum<Products>(product.Trim());
            if (productEnum == null)
            {
                throw new ArgumentException($"Invalid product value: '{product}'. Valid values are: {string.Join(", ", Enum.GetNames<Products>())}");
            }
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error in GetProductHierarchyAsync: {ErrorMessage}", ex.Message);
            throw new CustomToolException("Validation error in GetProductHierarchy: " + ex.Message, ex);
        }

        try
        {
            var result = await _companyInfoService.GetProductHierarchyAsync(productEnum.Value);

            if (result == null)
            {
                _logger.LogInformation("Product hierarchy not found for Product: {Product}", product);
                throw new CustomToolException($"Product hierarchy not found for product: '{product}'. The product may not exist in the database or may be disabled.");
            }

            _logger.LogInformation("Successfully retrieved product hierarchy for product {Product}", product);

            return result;
        }
        catch (AgentIdNotFoundException ex)
        {
            _logger.LogWarning("Failed to retrieve product hierarchy in GetProductHierarchyAsync: {ErrorMessage}", ex.Message);
            throw new CustomToolException(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetProductHierarchyAsync for Product {Product}: {ExceptionMessage}", product, ex.Message);
            throw new CustomToolException("Error in GetProductHierarchy.", ex);
        }
    }

    /// <summary>
    /// Asynchronously retrieves a collection of all available product names.
    /// </summary>
    /// <remarks>This method fetches product names from the company's product hierarchy, ensuring that only
    /// unique and valid product names are returned. The product names are sorted in ascending order. If an error occurs
    /// during retrieval, a <see cref="CustomToolException"/> is thrown.</remarks>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string"/> containing the names of all available products. The
    /// collection will be empty if no products are found.</returns>
    /// <exception cref="CustomToolException">Thrown if there is an error retrieving the product names, such as when the agent ID is not found.</exception>
    /// <usedBy>Troubleshouter</usedBy>
    [McpServerTool(Name = "MyCompany_CompanyInfo_GetAllProductNames",
        Idempotent = true, Destructive = false, OpenWorld = false, ReadOnly = true, UseStructuredContent = true),
        Description("CompanyInfo - Retrieves all available product names.")]
    public async Task<IEnumerable<string>> GetAllProductsAsync()
    {
        try
        {
            var hierarchyItems = await _companyInfoService.GetCompanyInfoProductsHierarchyItemsAsync(null, null);

            // Extract unique products, excluding unknown values
            var products = hierarchyItems
                .Where(item => item.Product != null && item.Product != MyPlatformEnumHelper.UNKNOWN_VALUE)
                .Select(item => item.Product!)
                .Distinct()
                .OrderBy(product => product)
                .ToList();

            _logger.LogInformation("Successfully retrieved {Count} products", products.Count);

            return products;
        }
        catch (AgentIdNotFoundException ex)
        {
            _logger.LogWarning("Failed to retrieve all products in GetAllProductsAsync: {ErrorMessage}", ex.Message);
            throw new CustomToolException(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllProductsAsync: {ExceptionMessage}", ex.Message);
            throw new CustomToolException("Error in GetAllProducts.", ex);
        }
    }
}