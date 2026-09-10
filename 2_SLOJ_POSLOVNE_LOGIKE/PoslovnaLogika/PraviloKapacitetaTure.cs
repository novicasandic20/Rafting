using System.Net.Http.Json;
using BibliotekaKlasa.KlasePodataka.Repozitorijumi;
using PoslovnaLogika.Modeli;

namespace PoslovnaLogika
{
    public class PraviloKapacitetaTure
    {
        private readonly HttpClient _httpKlijent;
        private readonly RaftingTuraRepo _raftingTuraRepo;
        private readonly PrijavaRepo _prijavaRepo;

        public PraviloKapacitetaTure(
            HttpClient httpKlijent,
            RaftingTuraRepo raftingTuraRepo,
            PrijavaRepo prijavaRepo)
        {
            _httpKlijent = httpKlijent;
            _raftingTuraRepo = raftingTuraRepo;
            _prijavaRepo = prijavaRepo;
        }

        public async Task<RezultatProvereKapaciteta> ProveriAsync(
            int raftingTuraId,
            int brojNovihUcesnika,
            int prijavaIdZaIzuzimanje = 0)
        {
            if (brojNovihUcesnika <= 0)
            {
                return new RezultatProvereKapaciteta
                {
                    Dozvoljeno = false,
                    NoviUcesnici = brojNovihUcesnika,
                    Poruka = "Prijava mora sadržati najmanje jednog učesnika."
                };
            }

            var tura = _raftingTuraRepo.DajPoId(raftingTuraId);

            if (tura is null)
            {
                return new RezultatProvereKapaciteta
                {
                    Dozvoljeno = false,
                    NoviUcesnici = brojNovihUcesnika,
                    Poruka = "Izabrana rafting tura ne postoji."
                };
            }

            var (parametri, izServisa) =
                await UcitajParametreAsync();

            var procenat = Math.Clamp(
                parametri.ProcenatKapaciteta,
                1m,
                100m);

            var efektivniKapacitet = Math.Max(
                0,
                (int)Math.Floor(
                    tura.MaksimalanBrojUcesnika * procenat / 100m)
                - Math.Max(0, parametri.RezervisanaMesta));

            var trenutnoPrijavljenih =
                _prijavaRepo.DajBrojPrijavljenihUcesnika(
                    raftingTuraId,
                    parametri.StatusiKojiSeRacunaju,
                    prijavaIdZaIzuzimanje);

            var preostalo = Math.Max(
                0,
                efektivniKapacitet - trenutnoPrijavljenih);

            var dozvoljeno =
                trenutnoPrijavljenih + brojNovihUcesnika
                <= efektivniKapacitet;

            return new RezultatProvereKapaciteta
            {
                Dozvoljeno = dozvoljeno,
                MaksimalanKapacitet =
                    tura.MaksimalanBrojUcesnika,
                EfektivniKapacitet = efektivniKapacitet,
                TrenutnoPrijavljenih = trenutnoPrijavljenih,
                NoviUcesnici = brojNovihUcesnika,
                PreostaloMesta = preostalo,
                ParametriUcitanIzServisa = izServisa,
                Poruka = dozvoljeno
                    ? $"Prijava je dozvoljena. Preostalo mesta nakon upisa: " +
                      $"{Math.Max(0, preostalo - brojNovihUcesnika)}."
                    : $"Nije moguće kreirati prijavu. Tura je popunjena ili " +
                      $"nema dovoljno mesta. Trenutno prijavljenih: " +
                      $"{trenutnoPrijavljenih}, raspoloživo mesta: {preostalo}, " +
                      $"traženo novih mesta: {brojNovihUcesnika}."
            };
        }

        private async Task<(ParametriKapaciteta Parametri, bool IzServisa)>
            UcitajParametreAsync()
        {
            try
            {
                var parametri =
                    await _httpKlijent.GetFromJsonAsync<ParametriKapaciteta>(
                        "api/parametri/kapacitet");

                if (parametri is not null)
                {
                    return (parametri, true);
                }
            }
            catch (HttpRequestException)
            {
                // Aplikacija ostaje upotrebljiva i kada servis nije pokrenut.
            }
            catch (TaskCanceledException)
            {
                // Vremensko ograničenje poziva servisa.
            }

            return (new ParametriKapaciteta(), false);
        }
    }
}
