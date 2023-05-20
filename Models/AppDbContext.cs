using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Musteri> TblMusteri { get; set; }
        public DbSet<Sepet> TblSepet { get; set; }
        public DbSet<SepetUrun> TblSepetUrun { get; set; }

    }
}
