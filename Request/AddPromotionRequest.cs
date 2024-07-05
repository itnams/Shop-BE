namespace Shop_BE.Request
{
    public class AddPromotionRequest
    {
        public string PromotionName { get; set; }
        public decimal Discount { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public IFormFile Image { get; set; }
    }
}
