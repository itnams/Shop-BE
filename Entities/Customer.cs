using System.ComponentModel.DataAnnotations;

namespace Shop_BE.Entities
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
    }
}
