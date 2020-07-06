using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HPlusSport.API.Classes;
using HPlusSport.API.Dtabase;
using HPlusSport.API.Models;

namespace HPlusSport.API.Controllers
{
    [ApiVersion("2.0")]
    // [Route("v{v:apiVersion}/products")]
    [Route("products")]
    [ApiController]
    public class ProductsV2_0Controller : ControllerBase
    {
        private readonly ShopContext _context;

        public ProductsV2_0Controller(ShopContext context)
        {
            _context = context;

            _context.Database.EnsureCreated();
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ProductQueryParameters queryParameters)
        {
            IQueryable<Product> products = _context.Products.Where(p => p.IsAvailable == true);
            
            if (queryParameters.MinPrice != null && 
                queryParameters.MaxPrice != null) 
            {
                products = products.Where(p => 
                    p.Price >= queryParameters.MinPrice && p.Price <= queryParameters.MaxPrice
                );
            }
            if(!string.IsNullOrEmpty(queryParameters.Sku))
            {
                products = products.Where(p => p.Sku == queryParameters.Sku);
            }
            if(!string.IsNullOrEmpty(queryParameters.Name))
            {
                products = products.Where(
                    p => p.Name.ToLower().Contains(queryParameters.Name.ToLower())
                );
            }

            if(!string.IsNullOrEmpty(queryParameters.SortBy))
            {
                if(typeof(Product).GetProperty(queryParameters.SortBy) != null)
                {
                    products = products.OrderByCustom(queryParameters.SortBy,queryParameters.SortOrder);
                } 
            }

            products =  products
                .Skip(queryParameters.Size * (queryParameters.Page - 1))
                .Take(queryParameters.Size);

            return Ok(await products.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var product = await _context.Products.FindAsync(id);    
            if(product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> Post([FromBody]Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                "Get",
                new { id = product.Id },
                product
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            } 

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_context.Products.Find(id) == null)
                {
                    return NotFound();
                }
            }

            return NoContent();
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }
    }
}