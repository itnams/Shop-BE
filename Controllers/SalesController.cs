using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop_BE.Data;
using Shop_BE.Response;

namespace Shop_BE.Controllers
{
    [EnableCors("MyPolicy")]
    [Route("sale")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        public SalesController(IConfiguration configuration, DataContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpGet("monthly-sales")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse<List<MonthlySalesResponse>>>> GetMonthlySales()
        {
            var response = new BaseResponse<List<MonthlySalesResponse>>();
            var currentYear = DateTime.Now.Year;
            var orders = await _context.Orders
                .Where(o => !string.IsNullOrEmpty(o.OrderDate))
                .ToListAsync();

            var salesData = orders
                .Where(o => DateTime.Parse(o.OrderDate).Year == currentYear)
                .GroupBy(o => DateTime.Parse(o.OrderDate).Month)
                .Select(g => new MonthlySalesResponse
                {
                    Month = g.Key,
                    TotalSales = g.Sum(o => o.TotalAmount)
                })
                .ToList();
            response.Data = salesData;
            return Ok(response);
        }
        [HttpGet("monthly-sales-count")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse<List<MonthlySalesCountResponse>>>> GetMonthlySalesCount()
        {
            var response = new BaseResponse<List<MonthlySalesCountResponse>>();
            var currentYear = DateTime.Now.Year;
            var orders = await _context.Orders
                .Where(o => !string.IsNullOrEmpty(o.OrderDate))
                .ToListAsync();

            var salesCountData = orders
                .Where(o => DateTime.Parse(o.OrderDate).Year == currentYear)
                .GroupBy(o => DateTime.Parse(o.OrderDate).Month)
                .Select(g => new MonthlySalesCountResponse
                {
                    Month = g.Key,
                    OrderCount = g.Count()
                })
                .ToList();
            response.Data = salesCountData;
            return Ok(response);
        }

        [HttpGet("monthly-product-sales")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponse<List<MonthlyProductSalesResponse>>>> GetMonthlyProductSales()
        {
            var response = new BaseResponse<List<MonthlyProductSalesResponse>>();
            var currentYear = DateTime.Now.Year;
            var orderDetails = await _context.OrderDetails
                .ToListAsync();
            var orders = await _context.Orders
                .Where(o => !string.IsNullOrEmpty(o.OrderDate))
                .ToListAsync();

            var productSalesData = orderDetails
                .Where(od => orders.Any(o => o.OrderId == od.OrderId && DateTime.Parse(o.OrderDate).Year == currentYear))
                .GroupBy(od => DateTime.Parse(orders.First(o => o.OrderId == od.OrderId).OrderDate).Month)
                .Select(g => new MonthlyProductSalesResponse
                {
                    Month = g.Key,
                    ProductCount = g.Sum(od => od.Quantity)
                })
                .ToList();
            response.Data = productSalesData;
            return Ok(response);
        }
    }
}
