using Shop_BE.Entities;
using static System.Net.Mime.MediaTypeNames;

namespace Shop_BE.Response
{
    public class CartResponse
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public IEnumerable<CartItemsResponse> Items { get; set; }
        public CartResponse(Cart cart, IEnumerable<CartItemsResponse> Items)
        {
            CartId = cart.CartId;
            UserId = cart.UserId;
            this.Items = Items;
        }
    }
    public class CartItemsResponse
    {
        public int CartItemId { get; set; }
        public int CartId { get; set; }
        public int Quantity { get; set; }
        public ProductResponse Product { get; set; }
        public CartItemsResponse(CartItems cartItems, ProductResponse Items)
        {
            CartItemId = cartItems.CartItemId;
            CartId = cartItems.CartId;
            Quantity = cartItems.Quantity;
            Product = Items;
        }
    }
}
