using System.ComponentModel.DataAnnotations;

namespace Shop_BE.Entities
{
    public class Orders
    {
        [Key]
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? PaymentMethods { get; set; }
    }
}
