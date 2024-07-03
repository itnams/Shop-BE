using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shop_BE.Data;
using Shop_BE.Entities;
using Shop_BE.Request;
using Shop_BE.Response;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        
    }
}
