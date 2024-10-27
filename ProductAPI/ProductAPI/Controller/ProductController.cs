using Microsoft.AspNetCore.Mvc;
using ProductAPI.Data;
using ProductAPI.Dtos; 
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProductAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductContext _context;

        public ProductsController(ProductContext context)
        {
            _context = context; 
        }

        // Create
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        // Read 
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // Read pagination
        [HttpGet]
        public async Task<ActionResult<PaginatedList<Product>>> GetProducts(int offset = 0, int limit = 5, string name = null)
        {
           
            var totalCount = await _context.Products.CountAsync();
            var productsQuery = string.IsNullOrEmpty(name)
                ? _context.Products
                : _context.Products.Where(p => p.Name.Contains(name));

            var products = await productsQuery
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

           
            var totalPages = (int)Math.Ceiling(totalCount / (double)limit);
            var paginatedList = new PaginatedList<Product>(products, (offset / limit) + 1, totalPages);          
            return Ok(paginatedList);
        }


        // Update 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest("Product ID mismatch.");
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!_context.Products.Any(e => e.Id == id))
            {
                return NotFound();
            }

            return NoContent();
        }

        // Delete 
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
