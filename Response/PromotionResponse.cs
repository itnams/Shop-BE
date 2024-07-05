using Shop_BE.Entities;

namespace Shop_BE.Response
{
    public class PromotionResponse
    {
        public int? PromotionId { get; set; }
        public string? PromotionName { get; set; }
        public decimal? Discount { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? Image { get; set; }
        public PromotionResponse(Promotions promotions)
        {
            PromotionId = promotions.PromotionId;
            PromotionName = promotions.PromotionName;
            Discount = promotions.Discount;
            StartDate = promotions.StartDate;
            EndDate = promotions.EndDate;
            Image = promotions.Image;
        }
    }
}
