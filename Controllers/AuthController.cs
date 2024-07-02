using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shop_BE.Data;
using Shop_BE.Entities;
using Shop_BE.Request;
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
        public async Task<ActionResult<BaseResponse<Customer>>> Login([FromForm] LoginRequest request)
        {
            var response = new BaseResponse<Customer>();

            var customer = await _context.Customer
                .FirstOrDefaultAsync(item => item.UserName == request.UserName && item.Password == request.Password);

            if (customer == null)
            {
                response.Success = false;
                response.Message = "Invalid username or password";
                return BadRequest(response);
            }

            // Tạo và trả về JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]); 
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.Name, customer.UserName),
                }),
                Expires = DateTime.UtcNow.AddHours(1), 
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            response.Success = true;
            response.Data = customer;
            response.Token = tokenString; 

            return Ok(response);
        }

        [Route("register")]
        [HttpPost]
        public async Task<ActionResult<BaseResponse<Customer>>> Register([FromForm] LoginRequest request)
        {
            var response = new BaseResponse<Customer>();
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
            _context.Customer.Add(newCustomer);
            await _context.SaveChangesAsync();
            response.Success = true;
            response.Data = newCustomer;
            return Ok(response);
        }
        [Authorize]
        [Route("users")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetUsers()
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

            return await _context.Customer.ToListAsync();
        }
    }
}
