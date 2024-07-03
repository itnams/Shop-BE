using System.ComponentModel.DataAnnotations;

namespace Shop_BE.Entities
{
    public class Promotions
    {
        [Key]
        public int? PromotionId { get; set; }
        public string? PromotionName { get; set; }
        public float? Discount { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }
}
