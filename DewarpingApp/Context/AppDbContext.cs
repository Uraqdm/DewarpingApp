using DewarpingApp.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DewarpingApp.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<ImageFile> ImageFiles { get; set; }
    }
}
