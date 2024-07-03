using System.ComponentModel.DataAnnotations;

namespace Shop_BE.Entities
{
    public class Reviews
    {
        [Key]
        public int ReviewId { get; set; }
        public int? ProductId { get; set; }
        public int? UserId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public string? ReviewDate { get; set; }

    }
}
