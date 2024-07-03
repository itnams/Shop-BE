namespace Shop_BE.Entities
{
    public class CustomerResponse
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public CustomerResponse(Customer customer)
        {
            Id = customer.Id;
            UserName = customer.UserName;
            Password = customer.Password;
            Role = customer.Role;
        }
    }
}
