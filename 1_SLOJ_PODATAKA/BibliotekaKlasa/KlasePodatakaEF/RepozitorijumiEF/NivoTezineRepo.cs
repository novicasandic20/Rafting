using BibliotekaKlasa.KlasePodataka.Modeli;
using BibliotekaKlasa.KlasePodatakaEF.KontekstEF;
using BibliotekaKlasa.KlasePodatakaEF.ModeliEF;
using Microsoft.EntityFrameworkCore;

namespace BibliotekaKlasa.KlasePodatakaEF.RepozitorijumiEF
{
    // Primer rada sa bazom primenom Entity Framework Core-a.
    public class NivoTezineRepo
    {
        private readonly AppDbContext _kontekst;

        public NivoTezineRepo(AppDbContext kontekst)
        {
            _kontekst = kontekst;
        }

        public async Task<List<NivoTezineModel>> DajSveAsync()
        {
            return await _kontekst.NivoiTezine
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => new NivoTezineModel
                {
                    Id = x.Id,
                    Naziv = x.Naziv,
                    Opis = x.Opis
                })
                .ToListAsync();
        }

        public async Task<NivoTezineModel?> DajPoIdAsync(int id)
        {
            return await _kontekst.NivoiTezine
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new NivoTezineModel
                {
                    Id = x.Id,
                    Naziv = x.Naziv,
                    Opis = x.Opis
                })
                .FirstOrDefaultAsync();
        }

        public async Task<NivoTezineModel> DodajAsync(
            NivoTezineModel nivo)
        {
            var entitet = new NivoTezineEntityModel
            {
                Naziv = nivo.Naziv,
                Opis = nivo.Opis
            };

            _kontekst.NivoiTezine.Add(entitet);
            await _kontekst.SaveChangesAsync();

            nivo.Id = entitet.Id;
            return nivo;
        }

        public async Task<bool> IzmeniAsync(NivoTezineModel nivo)
        {
            var entitet = await _kontekst.NivoiTezine
                .FirstOrDefaultAsync(x => x.Id == nivo.Id);

            if (entitet is null)
            {
                return false;
            }

            entitet.Naziv = nivo.Naziv;
            entitet.Opis = nivo.Opis;
            await _kontekst.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ObrisiAsync(int id)
        {
            var entitet = await _kontekst.NivoiTezine
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entitet is null)
            {
                return false;
            }

            _kontekst.NivoiTezine.Remove(entitet);
            await _kontekst.SaveChangesAsync();

            return true;
        }
    }
}
