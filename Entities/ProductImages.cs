using System.ComponentModel.DataAnnotations;

namespace Shop_BE.Entities
{
    public class ProductImages
    {
        [Key]
        public int ImageId { get; set; }
        public int ProductId { get; set; }
        public string ImagePath { get; set; }

    }
}
