namespace Shop_BE.Request
{
    public class AddOrders {
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string PaymentMethods { get; set; }
        public List<CartItem> CartItems { get; set; }
    }
    public class CartItem {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
    }
}
