using PowerDapp_UserArea.Database.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PowerDapp_UserArea.Database
{
    public class DBEntities : IdentityDbContext<User>
    {
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<ExchangeRate> ExchangeRates { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<BlockNumberCache> BlockNumberCache { get; set; }

        public DBEntities(DbContextOptions<DBEntities> options) : base(options)
        {
            
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}
