namespace Domain;

public sealed class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }

    private Product()
    {
    }

    public static Product Create(string name, decimal price, int stock)
    {
        Validate(name, price, stock);
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Price = price,
            Stock = stock
        };
    }

    public void Update(string name, decimal price, int stock)
    {
        Validate(name, price, stock);
        Name = name.Trim();
        Price = price;
        Stock = stock;
    }

    private static void Validate(string name, decimal price, int stock)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors["Name"] = new[] { "Name is required." };
        }

        if (price <= 0)
        {
            errors["Price"] = new[] { "Price must be greater than 0." };
        }

        if (stock < 0)
        {
            errors["Stock"] = new[] { "Stock must be greater than or equal to 0." };
        }

        if (errors.Count > 0)
        {
            throw new DomainValidationException(errors);
        }
    }
}
