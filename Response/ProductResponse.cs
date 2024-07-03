using Shop_BE.Entities;
using System.Data;

namespace Shop_BE.Response
{
    public class ProductResponse
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int? CategoryId { get; set; }
        public int? PromotionId { get; set; }
        public List<ProductImageResponse> Images { get; set; }
        public ProductResponse(Products products, List<ProductImageResponse> images)
        {
            ProductId = products.ProductId; 
            ProductName = products.ProductName;
            Description = products.Description;
            Price = products.Price; 
            OldPrice = products.OldPrice;
            CategoryId = products.CategoryId;
            PromotionId = products.PromotionId;
            Images = images;
        }
    }
    public class ProductImageResponse
    {
        public int ImageId { get; set; }
        public string? ImagePath { get; set; }
        public ProductImageResponse(ProductImages productImages)
        {
            ImageId = productImages.ImageId;
            ImagePath = productImages.ImagePath;
        }
    }
}
