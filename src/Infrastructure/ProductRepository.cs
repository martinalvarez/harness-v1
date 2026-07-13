using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

internal sealed class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dbContext;

    public ProductRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync() =>
        await _dbContext.Products.AsNoTracking().ToListAsync();

    public async Task<Product?> GetByIdAsync(Guid id) =>
        await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<bool> ExistsWithNameAsync(string name, Guid? excludeId) =>
        await _dbContext.Products.AnyAsync(p =>
            p.Name.ToLower() == name.ToLower() && (excludeId == null || p.Id != excludeId));

    public async Task AddAsync(Product product) =>
        await _dbContext.Products.AddAsync(product);

    public Task DeleteAsync(Product product)
    {
        _dbContext.Products.Remove(product);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => _dbContext.SaveChangesAsync();
}
