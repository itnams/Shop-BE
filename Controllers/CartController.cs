using Azure.Core;
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
        public async Task<ActionResult<BaseResponse<bool>>> AddItem(AddCartItemRequest request)
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

                var products = await _context.Products
                    .GroupJoin(
                        _context.ProductImages,
                        product => product.ProductId,
                        pi => pi.ProductId,
                        (product, pis) => new ProductResponse(product, pis.Select(item => new ProductImageResponse(item)).ToList())
                    )
                    .ToListAsync();
                var productsEnumerable = products.AsEnumerable();
                var cartItems = _context.CartItems
                    .Where(item => item.CartId == customerID)
                    .AsEnumerable()
                    .Join(
                        productsEnumerable,
                        item => item.ProductId,
                        product => product.ProductId,
                        (item, product) => new CartItemsResponse(item, product)
                    )
                    .ToList();
                var result = await _context.Cart
                    .Where(cart => cart.CartId == customerID)
                    .Select(cart => new CartResponse(cart, cartItems))
                    .FirstOrDefaultAsync();

                response.Data = result;
                response.Success = true;
            }
            else
            {
                return Forbid("You do not have permission to access this resource");
            }
            return Ok(response);
        }
        [Route("orders")]
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<BaseResponse<bool>>> Orders(AddOrders request)
        {
            var response = new BaseResponse<bool>();
            IEnumerable<Claim> claims = User.Claims;
            Claim customerIDClaim = claims.FirstOrDefault(c => c.Type == "customerID");
            if (customerIDClaim != null && int.TryParse(customerIDClaim.Value, out int customerID))
            {
                var currentDateTime = DateTime.Now;
                var dateTimeString = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                var order = new Orders
                {
                    UserId = customerID,
                    OrderDate = dateTimeString,
                    TotalAmount = request.TotalAmount,
                    Status = request.Status,
                    Address = request.Address,
                    PaymentMethods = request.PaymentMethods
                };

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                foreach (var cartItem in request.CartItems)
                {
                    var orderDetail = new OrderDetails
                    {
                        OrderId = order.OrderId,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        Price = cartItem.Price,
                    };
                    await _context.OrderDetails.AddAsync(orderDetail);
                    await _context.SaveChangesAsync();

                    var itemRemove = await _context.CartItems.Where(item => item.CartItemId == cartItem.CartItemId).FirstOrDefaultAsync();
                    if (itemRemove != null)
                    {
                        _context.CartItems.Remove(itemRemove);
                        await _context.SaveChangesAsync();
                    }
                }

                response.Data = true;
            }
            else
            {
                return Forbid("You do not have permission to access this resource");
            }
            return Ok(response);
        }
    }
}
