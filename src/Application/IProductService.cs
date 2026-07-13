using Domain;

namespace Application;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetAllAsync();

    Task<Product?> GetByIdAsync(Guid id);

    Task<Product> CreateAsync(string name, decimal price, int stock);

    Task<Product?> UpdateAsync(Guid id, string name, decimal price, int stock);

    Task<bool> DeleteAsync(Guid id);
}
