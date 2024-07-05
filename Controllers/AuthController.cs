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
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        public AuthController(IConfiguration configuration, DataContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [Route("login")]
        [HttpPost]
        public async Task<ActionResult<BaseResponse<CustomerResponse>>> Login(LoginRequest request)
        {
            var response = new BaseResponse<CustomerResponse>();

            var customer = await _context.Customer
                .FirstOrDefaultAsync(item => item.UserName == request.UserName && item.Password == request.Password);

            if (customer == null)
            {
                response.Success = false;
                response.Message = "Invalid username or password";
                return BadRequest(response);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]); 
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("customerID", customer.Id.ToString(), ClaimValueTypes.Integer),
                    new Claim(ClaimTypes.Name, customer.UserName),
                    new Claim(ClaimTypes.Role, customer.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1), 
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            response.Success = true;
            response.Data = new CustomerResponse(customer);
            response.Token = tokenString; 

            return Ok(response);
        }

        [Route("register")]
        [HttpPost]
        public async Task<ActionResult<BaseResponse<CustomerResponse>>> Register(LoginRequest request)
        {
            var response = new BaseResponse<CustomerResponse>();
            var existingCustomer = await _context.Customer.FirstOrDefaultAsync(c => c.UserName == request.UserName);
            if (existingCustomer != null)
            {
                response.Success = false;
                response.Message = "Username is already taken";
                return BadRequest(response);
            }
            var newCustomer = new Customer
            {
                UserName = request.UserName,
                Password = request.Password,
                Role = "Customer"
            };
            await _context.Customer.AddAsync(newCustomer);
            await _context.SaveChangesAsync();
            var newCart = new Cart {
                UserId = newCustomer.Id
            };
            await _context.Cart.AddAsync(newCart);
            await _context.SaveChangesAsync();
            response.Success = true;
            response.Data = new CustomerResponse(newCustomer);
            return Ok(response);
        }
        [Authorize(Roles = "Admin")]
        [Route("users")]
        [HttpGet]
        public async Task<ActionResult<BaseResponse<IEnumerable<CustomerResponse>>>> GetUsers()
        {
            var response = new BaseResponse<IEnumerable<CustomerResponse>>();
            response.Success = true;
            response.Data = await _context.Customer.Select(customer => new CustomerResponse(customer)).ToListAsync();
            return Ok(response);
        }
    }
}
