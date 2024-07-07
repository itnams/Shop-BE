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
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
            var response = new BaseResponse<ProductResponse>();
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
                        var productImage = new ProductImages
                        {
                            ProductId = product.ProductId,
                            ImagePath = memoryStream.ToArray()
                        };
                        productImages.Add(productImage);
                    }
                }
                await _context.ProductImages.AddRangeAsync(productImages);
                await _context.SaveChangesAsync();
                var resp = new ProductResponse(product, productImages.Select(pi => new ProductImageResponse(pi)).ToList());
                response.Data = resp;
                response.Success = true;
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
        [Route("search")]
        [HttpPost]
        public async Task<ActionResult<BaseResponse<List<ProductResponse>>>> SearchProducts(SearchProductRequest request, int pageSize = 10, int pageIndex = 1, string sortOrder = "asc_price")
        {
            try
            {
                var totalItems = await _context.Products.Where(p=> p.CategoryId == request.CategoryId).CountAsync();
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
                response.Total = totalItems;
                if (pageSize == result.Count)
                {
                    response.NextLink = "/products/search?pageSize=" + pageSize + "&pageIndex=" + (pageIndex + 1) + "&pageIndex=" + sortOrder;
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [Route("review")]
        [HttpPost]
        [Authorize()]
        public async Task<ActionResult<BaseResponse<bool>>> AddReview(AddReview request)
        {
            var response = new BaseResponse<bool>();
            IEnumerable<Claim> claims = User.Claims;
            Claim customerIDClaim = claims.FirstOrDefault(c => c.Type == "customerID");
            if (customerIDClaim != null && int.TryParse(customerIDClaim.Value, out int customerID))
            {
                var currentDateTime = DateTime.Now;
                var dateTimeString = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                var review = new Reviews
                {
                    ProductId = request.ProductId,
                    Rating = request.Rating,
                    UserId = customerID,
                    Comment = request.Comment,
                    ReviewDate = dateTimeString,
                };
                await _context.Reviews.AddRangeAsync(review);
                await _context.SaveChangesAsync();
                response.Data = true;
                response.Success = true;
            }
            else
            {
                return Forbid("You do not have permission to access this resource");
            }
            return Ok(response);
        }
        [HttpGet("review/{productId}")]
        public async Task<ActionResult<BaseResponse<List<ReviewResponse>>>> GetReview(int productId)
        {
            var response = new BaseResponse<List<ReviewResponse>>();
            var result = await _context.Reviews
                .Where(rv=> rv.ProductId == productId)
                .Join(_context.Customer, rv=> rv.UserId, cs => cs.Id,(rv,cs)=> new ReviewResponse(rv, new CustomerResponse(cs)))
                .ToListAsync();
                response.Data = result;
                response.Success = true;
            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponse<ProductResponse>>> ProductDetail(int id)
        {
            var response = new BaseResponse<ProductResponse>();
            IQueryable<Products> query = _context.Products;
            var result = await query
                .Where(p=> p.ProductId == id)
                .GroupJoin(_context.ProductImages,
                product => product.ProductId,
                pi => pi.ProductId,
                (product, pis) => new ProductResponse(product,
                pis.Select(item => new ProductImageResponse(item)).ToList()))
                .FirstOrDefaultAsync();
            if (result == null)
            {
                return NotFound();
            }
            else
            {
                response.Data = result;
                response.Success = true;
            }
            return Ok(response);
        }

    }
}
