using System.ComponentModel.DataAnnotations;

namespace Shop_BE.Entities
{
    public class Products
    {
        [Key]
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int? CategoryId { get; set; }
        public int? PromotionId { get; set; }
    }
}
