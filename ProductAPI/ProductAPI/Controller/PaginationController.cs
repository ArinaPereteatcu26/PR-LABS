using Microsoft.AspNetCore.Mvc;
using ProductAPI.Data;
using ProductAPI.Dtos;
using Microsoft.EntityFrameworkCore;


namespace ProductAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductPaginationController : ControllerBase
    {
        private readonly ProductContext _context;

        public ProductPaginationController(ProductContext context)
        {
            _context = context;
        }

        // Read pagination
        [HttpGet]
        public async Task<ActionResult<PaginatedList<Product>>> GetProducts(int offset = 0, int limit = 5, string? name = null)
        {
            var totalCount = await _context.Products.CountAsync();
            var productsQuery = string.IsNullOrEmpty(name)
                ? _context.Products
                : _context.Products.Where(p => p.Name != null && p.Name.Contains(name)
);

            var products = await productsQuery
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            var paginatedList = new PaginatedList<Product>(products, totalCount, (offset / limit) + 1, limit);
            return Ok(paginatedList);
        }
    }
}
