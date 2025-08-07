using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Data.Entity;

namespace PetShop.Models
{
    public class MyDbContext:DbContext
    {
        public DbSet<Member> member { get; set; }
    }
}