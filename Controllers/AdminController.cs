using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop_BE.Data;
using Shop_BE.Entities;
using Shop_BE.Request;
using Shop_BE.Response;
using System.Globalization;

namespace Shop_BE.Controllers
{
    [EnableCors("MyPolicy")]
    [Route("admin")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        public AdminController(IConfiguration configuration, DataContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        [Route("product")]
        [HttpPost]
        public async Task<ActionResult<BaseResponse<Products>>> AddProduct([FromForm] AddProduct request)
        {
            var username = User.Identity.Name;

            var user = await _context.Customer.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                return NotFound("User not found");
            }

            if (user.Role != "Admin")
            {
                return Forbid("You do not have permission to access this resource");
            }
            try
            {
                var product = new Products
                {
                    ProductName = request.ProductName,
                    Description = request.Description,
                    Price = request.Price,
                    OldPrice = request.OldPrice,
                    CategoryId = request.CategoryId,
                };

                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
                var productImages = new List<ProductImages>();
                foreach (var image in request.Images)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await image.CopyToAsync(memoryStream);
                        var base64Image = Convert.ToBase64String(memoryStream.ToArray());

                        var productImage = new ProductImages
                        {
                            ProductId = product.ProductId,
                            ImagePath = base64Image
                        };
                        productImages.Add(productImage);
                    }
                }
                await _context.ProductImages.AddRangeAsync(productImages);
                await _context.SaveChangesAsync();
                var resp = new ProductResponse(product, productImages.Select(pi => new ProductImageResponse(pi)).ToList());
                return Ok(resp);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
    }
}
