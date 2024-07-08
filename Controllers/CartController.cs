using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shop_BE.Data;
using Shop_BE.Entities;
using Shop_BE.Request;
using Shop_BE.Response;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
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
        private readonly HttpClient _client;
        public CartController(IHttpClientFactory httpClientFactory, IConfiguration configuration, DataContext context)
        {
            _configuration = configuration;
            _context = context;
            _client = httpClientFactory.CreateClient();
        }
        [Route("item")]
        [HttpDelete]
        [Authorize]
        public async Task<ActionResult<BaseResponse<bool>>> AddItem(int CartItemId)
        {
            var response = new BaseResponse<bool>();
            var cartItem = await _context.CartItems.Where(item => item.CartItemId == CartItemId).FirstOrDefaultAsync();
            _context.CartItems.RemoveRange(cartItem);
            await _context.SaveChangesAsync();
            response.Data = true;
            response.Success = true;
            return Ok(response);
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
                var cartItem = await _context.CartItems.Where(i => i.CartId == customerID && i.ProductId == request.ProductId).FirstOrDefaultAsync();
                if(cartItem != null) {
                    cartItem.Quantity += request.Quantity;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var newCartItem = new CartItems
                    {
                        CartId = customerID,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                    };
                    await _context.AddAsync(newCartItem);
                    await _context.SaveChangesAsync();
                }
                response.Data = true;
                response.Success = true;
            }
            else
            {
                return Forbid("You do not have permission to access this resource");
            }
            return Ok(response);
        }
        [Route("update-item")]
        [HttpPut]
        [Authorize]
        public async Task<ActionResult<BaseResponse<bool>>> UpdateItem(UpdateCartItemRequest request)
        {
            var response = new BaseResponse<bool>();
            var cartItem = await _context.CartItems.Where(p => p.CartItemId == request.CartItemId).FirstOrDefaultAsync();
            cartItem.Quantity = request.Quantity;
            await _context.SaveChangesAsync();
            response.Data = true;
            response.Success = true;
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
        public async Task<ActionResult<BaseResponse<string>>> Orders(AddOrders request)
        {
            var response = new BaseResponse<string>();
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
                    Phone = request.Phone,
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
                var paymentLink = await CreatePaymentLink(order);
                response.Data = paymentLink;
                response.Success = true;
            }
            else
            {
                return Forbid("You do not have permission to access this resource");
            }
            return Ok(response);
        }
        [Route("orders")]
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<BaseResponse<List<OrdersResponse>>>> GetOrders()
        {
            var response = new BaseResponse<List<OrdersResponse>>();
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
                var OrderDetails = _context.OrderDetails
                    .AsEnumerable()
                    .Join(
                        productsEnumerable,
                        item => item.ProductId,
                        product => product.ProductId,
                        (item, product) => new OrdersDetailResponse(item, product)
                    )
                    .ToList();
                var orders = _context.Orders
                    .Where(od => od.UserId == customerID)
                    .AsEnumerable()
                    .GroupJoin(OrderDetails, order => order.OrderId, detail => detail.OrderId, (order, detail) => new OrdersResponse(order, detail)).ToList();
                response.Data = orders;
                response.Success = true;
            }
            else
            {
                return Forbid("You do not have permission to access this resource");
            }
            return Ok(response);
        }


        private async Task<string> CreatePaymentLink(Orders request)
        {
            string partnerCode = "MOMOBKUN20180529";
            string accessKey = "klm05TvNBzhg7h7j";
            string secretKey = "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa";
            string orderId = GenerateRandomString(10);
            string orderInfo = "Thanh toán qua MoMo cho đơn hàng " + request.OrderId;
            string amount = request.TotalAmount + "";
            string ipnUrl = "http://localhost:4200/ipn-payment-success/";
            string redirectUrl = "http://localhost:4200/payment-success/" + request.OrderId;
            string extraData = "";

            string requestId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            string requestType = "captureWallet";

            // Generate raw hash
            string rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";

            // Compute signature
            string signature = ComputeHMACSHA256Hash(rawHash, secretKey);

            // Prepare data for VNPay request
            var requestData = new
            {
                partnerCode,
                partnerName = "Test",
                storeId = "MomoTestStore",
                requestId,
                amount,
                orderId,
                orderInfo,
                redirectUrl,
                ipnUrl,
                lang = "vi",
                extraData,
                requestType,
                signature
            };
            var endpoint = "https://test-payment.momo.vn/v2/gateway/api/create"; // Replace with your actual VNPay endpoint
            var jsonRequest = JsonConvert.SerializeObject(requestData);
            var response = await execPostRequest(endpoint, jsonRequest);
            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response);
            string payUrl = jsonResponse.payUrl; 
            return payUrl;
        }
        private string ComputeHMACSHA256Hash(string rawData, string secretKey)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        private async Task<string> execPostRequest(string endpoint, string requestData)
        {
            var content = new StringContent(requestData, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(endpoint, content);
            return await response.Content.ReadAsStringAsync();
        }
        private static string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            StringBuilder sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(chars.Length);
                sb.Append(chars[index]);
            }

            return sb.ToString();
        }
    }
}
