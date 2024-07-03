namespace Shop_BE.Request
{
    public class AddProduct
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public float OldPrice { get; set; }
        public int CategoryId { get; set; }
        public List<IFormFile> Images { get; set; }

    }
}
