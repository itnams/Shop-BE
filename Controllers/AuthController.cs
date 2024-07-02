using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop_BE.Data;
using Shop_BE.Entities;
using Shop_BE.Request;

namespace Shop_BE.Controllers
{
    [EnableCors("MyPolicy")]
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;

        public AuthController(DataContext context)
        {
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
            }
            else
            {
                response.Success = true;
                response.Data = customer;
            }
            return Ok(response);
        }
        [Route("register")]
        [HttpPost]
        public async Task<ActionResult<BaseResponse<Customer>>> Register([FromForm] LoginRequest request)
        {
            var response = new BaseResponse<Customer>();
            // Check if username is already taken
            var existingCustomer = await _context.Customer.FirstOrDefaultAsync(c => c.UserName == request.UserName);
            if (existingCustomer != null)
            {
                response.Success = false;
                response.Message = "Username is already taken";
                return BadRequest(response);
            }
            // Create new customer
            var newCustomer = new Customer
            {
                UserName = request.UserName,
                Password = request.Password,
                Role = "Customer"
                // Assign other properties as needed
            };
            // Save to database
            _context.Customer.Add(newCustomer);
            await _context.SaveChangesAsync();
            response.Success = true;
            response.Data = newCustomer;
            return Ok(response);
        }
    }
}
