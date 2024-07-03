using System.ComponentModel.DataAnnotations;

namespace Shop_BE.Entities
{
    public class ProductCategories
    {
        [Key]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
