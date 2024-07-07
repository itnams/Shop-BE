using Shop_BE.Entities;

namespace Shop_BE.Response
{
    public class ReviewResponse
    {
        public int ReviewId { get; set; }
        public int? ProductId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public string? ReviewDate { get; set; }
        public CustomerResponse? User { get; set; }

        public ReviewResponse(Reviews review, CustomerResponse customer) { 
            ReviewId = review.ReviewId;
            ProductId = review.ProductId;
            Rating = review.Rating;
            Comment = review.Comment;
            ReviewDate = review.ReviewDate;
            User = customer;
        }
    }
}
