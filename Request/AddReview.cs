namespace Shop_BE.Request
{
    public class AddReview
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
