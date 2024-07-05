using Shop_BE.Entities;

namespace Shop_BE.Response
{
    public class OrdersResponse
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public string? Address { get; set; }
        public string? PaymentMethods { get; set; }
        public IEnumerable<OrdersDetailResponse> Items { get; set; }
        public OrdersResponse(Orders orders, IEnumerable<OrdersDetailResponse> items)
        {
            OrderId = orders.OrderId;
            UserId = orders.UserId;
            OrderDate = orders.OrderDate;
            TotalAmount = orders.TotalAmount;
            Status = orders.Status;
            Address = orders.Address;
            PaymentMethods = orders.PaymentMethods;
            Items = items;
        }
    }
    public class OrdersDetailResponse
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public ProductResponse Product { get; set; }
        public OrdersDetailResponse(OrderDetails ordersDetail, ProductResponse product)
        {
            OrderDetailId = ordersDetail.OrderId;
            OrderId = ordersDetail.OrderId;
            Quantity = ordersDetail.Quantity;
            Price = ordersDetail.Price;
            Product = product;
        }
    }
}
