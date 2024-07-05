using Shop_BE.Response;

namespace Shop_BE.Request
{
    public class UpdateCartItemRequest
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}
