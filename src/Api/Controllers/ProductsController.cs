using Api.Contracts;
using Application;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAllAsync()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products.Select(ProductResponse.FromDomain));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> GetByIdAsync(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        return product is null ? NotFound() : Ok(ProductResponse.FromDomain(product));
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> CreateAsync(ProductRequest request)
    {
        var product = await _productService.CreateAsync(request.Name, request.Price, request.Stock);
        var response = ProductResponse.FromDomain(product);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> UpdateAsync(Guid id, ProductRequest request)
    {
        var product = await _productService.UpdateAsync(id, request.Name, request.Price, request.Stock);
        return product is null ? NotFound() : Ok(ProductResponse.FromDomain(product));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var deleted = await _productService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
