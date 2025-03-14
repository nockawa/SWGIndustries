using Microsoft.EntityFrameworkCore;

namespace SWGIndustries.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<ApplicationUser> Users { get; set; }

    public ApplicationDbContext()
    {
        
    }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }   
}