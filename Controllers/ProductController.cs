using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shop_BE.Data;
using Shop_BE.Entities;
using Shop_BE.Request;
using Shop_BE.Response;
using System.Globalization;

namespace Shop_BE.Controllers
{
    [EnableCors("MyPolicy")]
    [Route("products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        public ProductController(IConfiguration configuration, DataContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        [Route("add")]
        [HttpPost]
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult<BaseResponse<ProductResponse>>> AddProduct([FromForm] AddProduct request)
        {
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
        [Route("search")]
        [HttpPost]
        public async Task<ActionResult<BaseResponse<List<ProductResponse>>>> SearchProducts([FromForm] SearchProductRequest request, int pageSize = 10, int pageIndex = 1, string sortOrder = "asc_price")
        {
            try
            {
                var totalItems = await _context.Products.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                IQueryable<Products> query = _context.Products;

                switch (sortOrder.ToLower())
                {
                    case "asc_price":
                        query = query.OrderBy(p => p.Price);
                        break;
                    case "desc_price":
                        query = query.OrderByDescending(p => p.Price);
                        break;
                    case "asc_id":
                        query = query.OrderBy(p => p.ProductId);
                        break;
                    case "desc_id":
                        query = query.OrderByDescending(p => p.ProductId);
                        break;
                    default:
                        query = query.OrderBy(p => p.ProductId);
                        break;
                }
                if (!string.IsNullOrEmpty(request.ProductName))
                {
                    query = query.Where(p => p.ProductName.Contains(request.ProductName));
                }

                if (request.MinPrice != null)
                {
                    query = query.Where(p => p.Price >= request.MinPrice);
                }

                if (request.MaxPrice != null)
                {
                    query = query.Where(p => p.Price <= request.MaxPrice);
                }

                if (request.CategoryId != null)
                {
                    query = query.Where(p => p.CategoryId == request.CategoryId);
                }
                var result = await query
                .GroupJoin(_context.ProductImages,
                product => product.ProductId,
                pi => pi.ProductId,
                (product, pis) => new ProductResponse(product,
                pis.Select(item => new ProductImageResponse(item)).ToList()))
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                var response = new BaseResponse<List<ProductResponse>>();
                response.Data = result;
                response.Success = true;
                if (pageIndex < totalPages)
                {
                    response.NextLink = "/products/search?pageSize=" + pageSize + "&pageIndex=" + (pageIndex + 1);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
    }
}
