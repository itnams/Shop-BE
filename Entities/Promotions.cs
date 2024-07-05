using System.ComponentModel.DataAnnotations;

namespace Shop_BE.Entities
{
    public class Promotions
    {
        [Key]
        public int? PromotionId { get; set; }
        public string? PromotionName { get; set; }
        public decimal? Discount { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public byte[]? Image { get; set; }
        public DateTime? StartDateTime
        {
            get
            {
                return DateTime.TryParseExact(StartDate, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime tempDate) ? tempDate : (DateTime?)null;
            }
        }

        public DateTime? EndDateTime
        {
            get
            {
                return DateTime.TryParseExact(EndDate, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime tempDate) ? tempDate : (DateTime?)null;
            }
        }
    }
}
