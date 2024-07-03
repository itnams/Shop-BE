namespace Shop_BE.Request
{
    public class AddProduct
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public int CategoryId { get; set; }
        public List<IFormFile> Images { get; set; }

    }
}
