using System.Data.Entity;

namespace PetShop.Models
{
    public class MyDbContext : DbContext
    {
        public DbSet<Member> Member { get; set; }
    }
}