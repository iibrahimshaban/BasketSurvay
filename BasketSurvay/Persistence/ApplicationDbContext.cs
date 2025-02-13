

using BasketSurvay.Persistence.EntitiesConfigurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection;

namespace BasketSurvay.Persistence
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        :IdentityDbContext<ApplicationUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<Poll> Polls { get; set; }
    }
}
