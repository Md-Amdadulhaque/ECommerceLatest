using E_commerce.Models;
using Microsoft.Extensions.Options;
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


    public async Task<Product?> GetWithIdAsync(string id)
    {
        var product = await _databaseService.FindAsync(id);
        return product;
    }

    public async Task CreateAsync(Product product)
    {
        await _databaseService.AddAsync(product);
    }



    public async Task UpdateAsync(string id,Product updatedProduct)
    {
        await _databaseService.UpdateAsync(id, updatedProduct);
    }

    public async Task RemoveAsync(string id)
    {
        await _databaseService.DeleteAsync(id);
    }






}
