using Microsoft.EntityFrameworkCore;
using MultipleImageHandling.Models;

namespace MultipleImageHandling.Services
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions options):base(options)
        {
            
        }

        public DbSet<Item> Items { get; set; }

    }
}
