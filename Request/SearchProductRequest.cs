namespace Shop_BE.Request
{
    public class SearchProductRequest
    {
        public string? ProductName { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? CategoryId { get; set;}
    }
}
