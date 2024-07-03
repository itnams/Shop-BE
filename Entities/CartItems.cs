using System.ComponentModel.DataAnnotations;

namespace Shop_BE.Entities
{
    public class CartItems
    {
        [Key]
        public int CartItemId { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
