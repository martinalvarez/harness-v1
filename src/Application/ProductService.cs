using Domain;

namespace Application;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync() =>
        await _repository.GetAllAsync();

    public async Task<Product?> GetByIdAsync(Guid id) =>
        await _repository.GetByIdAsync(id);

    public async Task<Product> CreateAsync(string name, decimal price, int stock)
    {
        await EnsureNameNotDuplicateAsync(name, excludeId: null);

        var product = Product.Create(name, price, stock);
        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateAsync(Guid id, string name, decimal price, int stock)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
        {
            return null;
        }

        await EnsureNameNotDuplicateAsync(name, excludeId: id);

        product.Update(name, price, stock);
        await _repository.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
        {
            return false;
        }

        await _repository.DeleteAsync(product);
        await _repository.SaveChangesAsync();
        return true;
    }

    private async Task EnsureNameNotDuplicateAsync(string name, Guid? excludeId)
    {
        if (await _repository.ExistsWithNameAsync(name, excludeId))
        {
            throw new DomainValidationException(new Dictionary<string, string[]>
            {
                ["Name"] = new[] { "A product with this name already exists." }
            });
        }
    }
}
