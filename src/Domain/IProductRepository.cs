namespace Domain;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync();

    Task<Product?> GetByIdAsync(Guid id);

    Task<bool> ExistsWithNameAsync(string name, Guid? excludeId);

    Task AddAsync(Product product);

    Task DeleteAsync(Product product);

    Task SaveChangesAsync();
}
