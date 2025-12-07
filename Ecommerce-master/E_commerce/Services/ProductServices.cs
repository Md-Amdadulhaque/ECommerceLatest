using E_commerce.Models;
using Microsoft.Extensions.Options;
using System.Linq;
using MongoDB.Driver;
using E_commerce.Interface;
namespace E_commerce.Services;

public class ProductServices:IProductService
{
    private  IDatabaseService<Product> _databaseService;

    public ProductServices(IDatabaseService<Product> databaseService)
    {
        _databaseService = databaseService;
        _databaseService.SetCollection(nameof(Product));
    }

    public async Task<List<Product>> GetAsync()
    {
        var products = await _databaseService.GetAllAsync();
        return products;
    }

    public async Task<List<Product>> GetAsync(int pageNumber,int pageSize)
    {
        var products = await _databaseService.GetAllAsync();
        int total = products.Count;
        var productPerPage = products.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return productPerPage;
    }
    
    public async Task<List<Product>> GetAllProductByCategoryAsync(string category)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Category, category);
        var products = await _databaseService.GetByFilterAsync(filter);
        return products;
    }
    public async Task<Product?> GetWithIdAsync(string id)
    {
        var product = await _databaseService.FindAsync(id);
        return product;
    }

    public async Task CreateAsync(Product product)
    {
        await _databaseService.AddAsync(product);
    }
    
    public async Task<List<Product>> GetTopCheapestAsync(int count = 1)
    {
        if (count <= 0) return new List<Product>();

        var sort = Builders<Product>.Sort.Ascending(p => p.Price);
        var filter = Builders<Product>.Filter.Ne(p => p.Price,0);

        var cheapest = await _databaseService.GetBySortThenFilterAsync(sort, filter, count);
        return cheapest;
    }

    public async Task<List<Product>> GetByCategoryAsync(string category)
    {
        if (string.IsNullOrWhiteSpace(category)) return new List<Product>();

        var filter = Builders<Product>.Filter.Eq(p => p.Category, category);
        var products = await _databaseService.GetByFilterAsync(filter);
        return products;
    }

    public async Task UpdateAsync(string id,Product updatedProduct)
    {
        await _databaseService.UpdateAsync(id, updatedProduct);
    }

    public async Task RemoveAsync(string id)
    {
        await _databaseService.DeleteAsync(id);
    }

    public async Task<List<Product>> GetWithFilter(ProductFilterRequest productFilterRequest)
    {
        var filterBuilder = Builders<Product>.Filter;
        var filters = new List<FilterDefinition<Product>>();

        if (!string.IsNullOrEmpty(productFilterRequest.Name))
            filters.Add(filterBuilder.Eq(p => p.Name, productFilterRequest.Name));

        if (!string.IsNullOrEmpty(productFilterRequest.Category))
            filters.Add(filterBuilder.Eq(p => p.Category, productFilterRequest.Category));

        if (!string.IsNullOrEmpty(productFilterRequest.Color))
            filters.Add(filterBuilder.Eq(p => p.Color, productFilterRequest.Color));

        // Handle ranges (these need special logic)
        if (productFilterRequest.MinPrice.HasValue)
            filters.Add(filterBuilder.Gte(p => p.Price, productFilterRequest.MinPrice.Value));

        if (productFilterRequest.MaxPrice.HasValue)
            filters.Add(filterBuilder.Lte(p => p.Price, productFilterRequest.MaxPrice.Value));

        if (productFilterRequest.NumberOfItemAvaiable.HasValue)
            filters.Add(filterBuilder.Gte(p => p.NumberOfItemAvaiable, productFilterRequest.NumberOfItemAvaiable.Value));

        var combinedFilter = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;
        var products = await _databaseService.GetByFilterAsync(combinedFilter);
        return products;
    }






}
