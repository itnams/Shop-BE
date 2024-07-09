namespace Shop_BE.Request
{
    public class SearchOrdersRequest
    {
        public int? OrderId { get; set; }
        public string? OrderDate { get; set; }
        public string? Status { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? PaymentMethods { get; set; }
    }
}
