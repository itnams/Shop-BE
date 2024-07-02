using Microsoft.EntityFrameworkCore;
using Shop_BE.Entities;

namespace Shop_BE.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        { }
        public DbSet<Customer> Customer { get; set; }
    }
}
