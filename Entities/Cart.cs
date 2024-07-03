using System.ComponentModel.DataAnnotations;

namespace Shop_BE.Entities
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }
        public int UserId { get; set; }
    }
}
