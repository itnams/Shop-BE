using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop_BE.Data;
using Shop_BE.Entities;
using Shop_BE.Request;
using Shop_BE.Response;
using System.Security.Claims;

namespace Shop_BE.Controllers
{
    [EnableCors("MyPolicy")]
    [Route("cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        public CartController(IConfiguration configuration, DataContext context)
        {
            _configuration = configuration;
            _context = context;
        }
        [Route("add-item")]
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<BaseResponse<bool>>> AddItem([FromForm] AddCartItemRequest request)
        {
            var response = new BaseResponse<bool>();
            IEnumerable<Claim> claims = User.Claims;
            Claim customerIDClaim = claims.FirstOrDefault(c => c.Type == "customerID");
            if (customerIDClaim != null && int.TryParse(customerIDClaim.Value, out int customerID))
            {
                var newCartItem = new CartItems
                {
                    CartId = customerID,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                };
                await _context.AddAsync(newCartItem);
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
        [Route("items")]
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<BaseResponse<CartResponse>>> GetItem()
        {
            var response = new BaseResponse<CartResponse>();
            IEnumerable<Claim> claims = User.Claims;
            Claim customerIDClaim = claims.FirstOrDefault(c => c.Type == "customerID");
            if (customerIDClaim != null && int.TryParse(customerIDClaim.Value, out int customerID))
            {

                // Truy vấn sản phẩm và ảnh liên quan trước
                var products = await _context.Products
                    .GroupJoin(
                        _context.ProductImages,
                        product => product.ProductId,
                        pi => pi.ProductId,
                        (product, pis) => new ProductResponse(product, pis.Select(item => new ProductImageResponse(item)).ToList())
                    )
                    .ToListAsync();

                // Chuyển đổi danh sách sản phẩm sang enumerable để sử dụng trong join
                var productsEnumerable = products.AsEnumerable();

                // Truy vấn các mục giỏ hàng và tham gia với sản phẩm
                var cartItems =  _context.CartItems
                    .Where(item => item.CartId == customerID)
                    .AsEnumerable()
                    .Join(
                        productsEnumerable,
                        item => item.ProductId,
                        product => product.ProductId,
                        (item, product) => new CartItemsResponse(item, product)
                    )
                    .ToList();

                // Truy vấn giỏ hàng và bao gồm các mục
                var result = await _context.Cart
                    .Where(cart => cart.CartId == customerID)
                    .Select(cart => new CartResponse(cart, cartItems))
                    .FirstOrDefaultAsync();

                response.Data = result;
            }
            else
            {
                return Forbid("You do not have permission to access this resource");
            }
            return Ok(response);
        }
    }
}
