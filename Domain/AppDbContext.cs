
using Microsoft.EntityFrameworkCore;
namespace Domain
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.RegisterAllEntities(DomainsBuilder.Assembly);

            base.OnModelCreating(modelBuilder);
        }        
    }
}