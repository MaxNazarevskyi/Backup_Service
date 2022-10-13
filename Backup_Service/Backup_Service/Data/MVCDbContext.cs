using Backup_Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Backup_Service.Data
{
    public class MVCDbContext : DbContext
    {
        public DbSet<FileModel> Files { get; set; }
        public MVCDbContext(DbContextOptions<MVCDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
