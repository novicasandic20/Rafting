using BibliotekaKlasa.KlasePodatakaEF.ModeliEF;
using Microsoft.EntityFrameworkCore;

namespace BibliotekaKlasa.KlasePodatakaEF.KontekstEF
{
    // Nasleđivanje standardne Entity Framework klase DbContext.
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opcije)
            : base(opcije)
        {
        }

        public DbSet<NivoTezineEntityModel> NivoiTezine =>
            Set<NivoTezineEntityModel>();
    }
}
