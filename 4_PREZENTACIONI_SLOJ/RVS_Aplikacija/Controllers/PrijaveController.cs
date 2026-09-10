using System.Security.Claims;
using BibliotekaKlasa.KlasePodataka.Modeli;
using BibliotekaKlasa.KlasePodataka.Repozitorijumi;
using BibliotekaKlasa.TehnoloskeKlase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PoslovnaLogika;
using RVS_Aplikacija.ViewModels;

namespace RVS_Aplikacija.Controllers
{
    [Authorize]
    public class PrijaveController : Controller
    {
        private readonly PrijavaRepo _prijavaRepo;
        private readonly UcesnikPrijaveRepo _ucesnikRepo;
        private readonly RaftingTuraRepo _raftingTuraRepo;
        private readonly KonekcijaKlasa _konekcijaObjekat;
        private readonly PraviloKapacitetaTure _praviloKapaciteta;
        private readonly IWebHostEnvironment _okruzenje;


        public PrijaveController(
            PrijavaRepo prijavaRepo,
            UcesnikPrijaveRepo ucesnikRepo,
            RaftingTuraRepo raftingTuraRepo,
            KonekcijaKlasa konekcijaObjekat,
            PraviloKapacitetaTure praviloKapaciteta,
            IWebHostEnvironment okruzenje)
        {
            _prijavaRepo = prijavaRepo;
            _ucesnikRepo = ucesnikRepo;
            _raftingTuraRepo = raftingTuraRepo;
            _konekcijaObjekat = konekcijaObjekat;
            _praviloKapaciteta = praviloKapaciteta;
            _okruzenje = okruzenje;
        }

        public IActionResult Index(
    string? filter,
    string? status)
        {
            ViewData["Filter"] = filter;
            ViewData["Status"] = status;

            var lista = _prijavaRepo.DajSve(filter);

            if (!User.IsInRole("Admin"))
            {
                var korisnikId = DajKorisnikId();

                lista = lista
                    .Where(x => x.KorisnikId == korisnikId)
                    .ToList();
            }

            // Brojevi se računaju pre statusnog filtriranja
            ViewData["BrojSve"] = lista.Count;

            ViewData["BrojKreirane"] =
                lista.Count(x => x.StatusPrijave == "Kreirana");

            ViewData["BrojPotvrdjene"] =
                lista.Count(x => x.StatusPrijave == "Potvrđena");

            ViewData["BrojOdbijene"] =
                lista.Count(x => x.StatusPrijave == "Odbijena");

            ViewData["BrojOtkazane"] =
                lista.Count(x => x.StatusPrijave == "Otkazana");


            if (!string.IsNullOrWhiteSpace(status))
            {
                lista = lista
                    .Where(x =>
                        x.StatusPrijave.Equals(
                            status,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return View(lista);
        }

        [HttpGet]
        public IActionResult Dodaj(int? raftingTuraId)
        {
            PopuniTure(raftingTuraId ?? 0);

            return View(
                new PrijavaRaftingViewModel
                {
                    DatumPrijave = DateTime.Today,
                    RaftingTuraId = raftingTuraId ?? 0,
                    StatusPrijave = "Kreirana",
                    Ucesnici =
                    {
                        new UcesnikPrijaveViewModel
                        {
                            RedniBroj = 1,
                            DatumRodjenja =
                                DateTime.Today.AddYears(-18)
                        }
                    }
                });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Odobri(int id)
        {
            var prijava = UcitajCelinu(id);

            if (prijava is null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(
                prijava.PotvrdaUplateSacuvaniNaziv))
            {
                TempData["Poruka"] =
                    "Prijava ne može biti odobrena jer nije priložena potvrda o uplati.";

                TempData["PorukaTip"] = "greska";

                return RedirectToAction(
                    nameof(Detalji),
                    new { id });
            }

            foreach (var ucesnik in prijava.Ucesnici)
            {
                if (!ucesnik
                    .PotvrdioDonosenjeIdentifikacionogDokumenta)
                {
                    TempData["Poruka"] =
                        $"Učesnik {ucesnik.ImeIPrezime} nije potvrdio donošenje identifikacionog dokumenta.";

                    TempData["PorukaTip"] = "greska";

                    return RedirectToAction(
                        nameof(Detalji),
                        new { id });
                }

                if (string.IsNullOrWhiteSpace(
                    ucesnik.PotpisanaIzjavaSacuvaniNaziv))
                {
                    TempData["Poruka"] =
                        $"Za učesnika {ucesnik.ImeIPrezime} nije priložena potpisana izjava.";

                    TempData["PorukaTip"] = "greska";

                    return RedirectToAction(
                        nameof(Detalji),
                        new { id });
                }

                if (ucesnik.Maloletan &&
                    string.IsNullOrWhiteSpace(
                        ucesnik.SaglasnostSacuvaniNaziv))
                {
                    TempData["Poruka"] =
                        $"Za maloletnog učesnika {ucesnik.ImeIPrezime} nije priložena saglasnost.";

                    TempData["PorukaTip"] = "greska";

                    return RedirectToAction(
                        nameof(Detalji),
                        new { id });
                }
            }

            _prijavaRepo.PromeniStatus(
                id,
                "Potvrđena",
                DajKorisnikId());

            TempData["Poruka"] =
                "Prijava je odobrena.";

            TempData["PorukaTip"] = "uspeh";

            return RedirectToAction(
                nameof(Detalji),
                new { id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Odbij(int id)
        {
            var prijava = _prijavaRepo.DajPoId(id);

            if (prijava is null)
            {
                return NotFound();
            }

            _prijavaRepo.PromeniStatus(
                id,
                "Odbijena",
                DajKorisnikId());

            TempData["Poruka"] =
                "Prijava je odbijena i mesta su ponovo oslobođena.";

            TempData["PorukaTip"] = "upozorenje";

            return RedirectToAction(
                nameof(Detalji),
                new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dodaj(
            PrijavaRaftingViewModel model)
        {
            model.StatusPrijave = "Kreirana";

            OcistiUcesnike(model);

            ModelState.Clear();
            TryValidateModel(model);

            if (!ModelState.IsValid)
            {
                PopuniTure(model.RaftingTuraId);
                return View(model);
            }


            foreach (var ucesnik in model.Ucesnici)
            {
                if (_ucesnikRepo.PostojiJmbgNaTuri(
                    ucesnik.JMBG,
                    model.RaftingTuraId))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Učesnik sa JMBG-om {ucesnik.JMBG} " +
                        "je već prijavljen na izabranu rafting turu.");
                }
            }

            if (!ModelState.IsValid)
            {
                PopuniTure(model.RaftingTuraId);
                return View(model);
            }


            var rezultat = await _praviloKapaciteta.ProveriAsync(
                model.RaftingTuraId,
                model.Ucesnici.Count);

            if (!rezultat.Dozvoljeno)
            {
                ModelState.AddModelError(
                    string.Empty,
                    rezultat.Poruka);

                PopuniTure(model.RaftingTuraId);
                return View(model);
            }

            try
            {
                // Čuvanje potvrde o uplati
                if (model.PotvrdaUplateFajl is not null)
                {
                    var potvrdaUplate =
                        await SacuvajFajlAsync(
                            model.PotvrdaUplateFajl);

                    model.PostojecaPotvrdaUplateOriginalniNaziv =
                        potvrdaUplate.OriginalniNaziv;

                    model.PostojecaPotvrdaUplateSacuvaniNaziv =
                        potvrdaUplate.SacuvaniNaziv;
                }

                // Čuvanje dokumenata svih učesnika
                foreach (var ucesnik in model.Ucesnici)
                {
                    if (ucesnik.PotpisanaIzjavaFajl is not null)
                    {
                        var izjava =
                            await SacuvajFajlAsync(
                                ucesnik.PotpisanaIzjavaFajl);

                        ucesnik.PostojecaPotpisanaIzjavaOriginalniNaziv =
                            izjava.OriginalniNaziv;

                        ucesnik.PostojecaPotpisanaIzjavaSacuvaniNaziv =
                            izjava.SacuvaniNaziv;
                    }

                    var maloletan =
                        ucesnik.DatumRodjenja.Date >
                        DateTime.Today.AddYears(-18);

                    if (maloletan &&
                        ucesnik.SaglasnostFajl is not null)
                    {
                        var saglasnost =
                            await SacuvajFajlAsync(
                                ucesnik.SaglasnostFajl);

                        ucesnik.PostojecaSaglasnostOriginalniNaziv =
                            saglasnost.OriginalniNaziv;

                        ucesnik.PostojecaSaglasnostSacuvaniNaziv =
                            saglasnost.SacuvaniNaziv;
                    }
                }

                // Mapiranje se radi tek kada su fajlovi sačuvani
                var prijava = Mapiraj(model);
                prijava.KorisnikId = DajKorisnikId();

                using var celina =
                    new TransakcijaKlasa(_konekcijaObjekat);

                var prijavaId = _prijavaRepo.Dodaj(
                    prijava,
                    celina.Konekcija,
                    celina.Transakcija);

                foreach (var ucesnik in prijava.Ucesnici)
                {
                    ucesnik.PrijavaId = prijavaId;

                    _ucesnikRepo.Dodaj(
                        ucesnik,
                        celina.Konekcija,
                        celina.Transakcija);
                }

                celina.Potvrdi();

                TempData["Poruka"] =
                    $"Prijava {prijava.BrojPrijave} je uspešno kreirana.";

                TempData["PorukaTip"] = "uspeh";

                if (!rezultat.ParametriUcitanIzServisa)
                {
                    TempData["Upozorenje"] =
                        "Servis parametara nije bio dostupan, pa je primenjeno " +
                        "podrazumevano ograničenje od 100% kapaciteta.";
                }

                return RedirectToAction(
                    nameof(Detalji),
                    new { id = prijavaId });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                PopuniTure(model.RaftingTuraId);
                return View(model);
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Greška pri upisu prijave u bazu podataka: " +
                    ex.Message);

                PopuniTure(model.RaftingTuraId);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Izmeni(int id)
        {
            var prijava = UcitajCelinu(id);

            if (prijava is null)
            {
                return NotFound();
            }

            if (!MozeDaMenja(prijava))
            {
                return Forbid();
            }

            PopuniTure(prijava.RaftingTuraId);

            return View(Mapiraj(prijava));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Izmeni(
    PrijavaRaftingViewModel model)
        {
            var postojeca = UcitajCelinu(model.Id);

            if (postojeca is null)
            {
                return NotFound();
            }

            if (!MozeDaMenja(postojeca))
            {
                return Forbid();
            }

            if (!User.IsInRole("Admin"))
            {
                model.StatusPrijave = postojeca.StatusPrijave;
            }

            OcistiUcesnike(model);

            ModelState.Clear();
            TryValidateModel(model);

            if (!ModelState.IsValid)
            {
                PopuniTure(model.RaftingTuraId);
                return View(model);
            }

            foreach (var ucesnik in model.Ucesnici)
            {
                if (_ucesnikRepo.PostojiJmbgNaTuri(
                    ucesnik.JMBG,
                    model.RaftingTuraId,
                    model.Id))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Učesnik sa JMBG-om {ucesnik.JMBG} " +
                        "je već prijavljen na izabranu rafting turu.");
                }
            }

            if (!ModelState.IsValid)
            {
                PopuniTure(model.RaftingTuraId);
                return View(model);
            }

            if (!string.Equals(
                    model.StatusPrijave,
                    "Otkazana",
                    StringComparison.OrdinalIgnoreCase))
            {
                var rezultat = await _praviloKapaciteta.ProveriAsync(
                    model.RaftingTuraId,
                    model.Ucesnici.Count,
                    model.Id);

                if (!rezultat.Dozvoljeno)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        rezultat.Poruka);

                    PopuniTure(model.RaftingTuraId);
                    return View(model);
                }
            }

            try
            {
                // =====================================================
                // 1. POTVRDA O UPLATI
                // =====================================================

                if (model.PotvrdaUplateFajl is not null)
                {
                    var noviFajl =
                        await SacuvajFajlAsync(
                            model.PotvrdaUplateFajl);

                    model.PostojecaPotvrdaUplateOriginalniNaziv =
                        noviFajl.OriginalniNaziv;

                    model.PostojecaPotvrdaUplateSacuvaniNaziv =
                        noviFajl.SacuvaniNaziv;
                }
                else
                {
                    // Ako novi fajl nije izabran,
                    // zadržavamo postojeću potvrdu iz baze.

                    model.PostojecaPotvrdaUplateOriginalniNaziv =
                        postojeca.PotvrdaUplateOriginalniNaziv;

                    model.PostojecaPotvrdaUplateSacuvaniNaziv =
                        postojeca.PotvrdaUplateSacuvaniNaziv;
                }


                // =====================================================
                // 2. DOKUMENTI UČESNIKA
                // =====================================================

                foreach (var ucesnik in model.Ucesnici)
                {
                    // Tražimo postojećeg učesnika po ID-u.
                    var postojeciUcesnik =
                        postojeca.Ucesnici
                            .FirstOrDefault(x => x.Id == ucesnik.Id);


                    // ------------------------------
                    // POTPISANA IZJAVA
                    // ------------------------------

                    if (ucesnik.PotpisanaIzjavaFajl is not null)
                    {
                        var novaIzjava =
                            await SacuvajFajlAsync(
                                ucesnik.PotpisanaIzjavaFajl);

                        ucesnik.PostojecaPotpisanaIzjavaOriginalniNaziv =
                            novaIzjava.OriginalniNaziv;

                        ucesnik.PostojecaPotpisanaIzjavaSacuvaniNaziv =
                            novaIzjava.SacuvaniNaziv;
                    }
                    else if (postojeciUcesnik is not null)
                    {
                        // Nema novog fajla:
                        // ostaje postojeća izjava.

                        ucesnik.PostojecaPotpisanaIzjavaOriginalniNaziv =
                            postojeciUcesnik
                                .PotpisanaIzjavaOriginalniNaziv;

                        ucesnik.PostojecaPotpisanaIzjavaSacuvaniNaziv =
                            postojeciUcesnik
                                .PotpisanaIzjavaSacuvaniNaziv;
                    }


                    // ------------------------------
                    // SAGLASNOST RODITELJA / STARATELJA
                    // ------------------------------

                    var maloletan =
                        ucesnik.DatumRodjenja.Date >
                        DateTime.Today.AddYears(-18);

                    if (maloletan)
                    {
                        if (ucesnik.SaglasnostFajl is not null)
                        {
                            var novaSaglasnost =
                                await SacuvajFajlAsync(
                                    ucesnik.SaglasnostFajl);

                            ucesnik.PostojecaSaglasnostOriginalniNaziv =
                                novaSaglasnost.OriginalniNaziv;

                            ucesnik.PostojecaSaglasnostSacuvaniNaziv =
                                novaSaglasnost.SacuvaniNaziv;
                        }
                        else if (postojeciUcesnik is not null)
                        {
                            // Nema novog fajla:
                            // ostaje postojeća saglasnost.

                            ucesnik.PostojecaSaglasnostOriginalniNaziv =
                                postojeciUcesnik
                                    .SaglasnostOriginalniNaziv;

                            ucesnik.PostojecaSaglasnostSacuvaniNaziv =
                                postojeciUcesnik
                                    .SaglasnostSacuvaniNaziv;
                        }
                    }
                    else
                    {
                        // Ako učesnik više nije maloletan,
                        // saglasnost mu više nije potrebna.

                        ucesnik.PostojecaSaglasnostOriginalniNaziv = null;
                        ucesnik.PostojecaSaglasnostSacuvaniNaziv = null;
                    }
                }


                // =====================================================
                // 3. MAPIRANJE TEK POSLE OBRADE FAJLOVA
                // =====================================================

                var prijava = Mapiraj(model);

                prijava.KorisnikId = postojeca.KorisnikId;
                prijava.BrojPrijave = postojeca.BrojPrijave;


                // =====================================================
                // 4. ČUVANJE U BAZU
                // =====================================================

                using var celina =
                    new TransakcijaKlasa(_konekcijaObjekat);

                _prijavaRepo.Izmeni(
                    prijava,
                    celina.Konekcija,
                    celina.Transakcija);

                _ucesnikRepo.ObrisiZaPrijavu(
                    prijava.Id,
                    celina.Konekcija,
                    celina.Transakcija);

                foreach (var ucesnik in prijava.Ucesnici)
                {
                    ucesnik.PrijavaId = prijava.Id;

                    _ucesnikRepo.Dodaj(
                        ucesnik,
                        celina.Konekcija,
                        celina.Transakcija);
                }

                celina.Potvrdi();

                TempData["Poruka"] =
                    "Prijava je uspešno izmenjena.";

                TempData["PorukaTip"] =
                    "uspeh";

                return RedirectToAction(
                    nameof(Detalji),
                    new { id = prijava.Id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                PopuniTure(model.RaftingTuraId);

                return View(model);
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Greška pri izmeni prijave: " +
                    ex.Message);

                PopuniTure(model.RaftingTuraId);

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Obrisi(int id)
        {
            var prijava = _prijavaRepo.DajPoId(id);

            if (prijava is null)
            {
                return NotFound();
            }

            if (!MozeDaMenja(prijava))
            {
                return Forbid();
            }

            _prijavaRepo.Obrisi(id);

            TempData["Poruka"] = "Prijava je uspešno obrisana.";
            TempData["PorukaTip"] = "uspeh";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Detalji(int id)
        {
            var prijava = UcitajCelinu(id);

            if (prijava is null)
            {
                return NotFound();
            }

            if (!MozeDaVidi(prijava))
            {
                return Forbid();
            }

            return View(prijava);
        }

        public IActionResult ParametarskaStampa(int id)
        {
            var prijava = UcitajCelinu(id);

            if (prijava is null)
            {
                return NotFound();
            }

            if (!MozeDaVidi(prijava))
            {
                return Forbid();
            }

            return View(prijava);
        }

        public IActionResult Stampa(string? filter)
        {
            var lista = _prijavaRepo.DajSve(filter);

            if (!User.IsInRole("Admin"))
            {
                var korisnikId = DajKorisnikId();
                lista = lista
                    .Where(x => x.KorisnikId == korisnikId)
                    .ToList();
            }

            ViewData["Filter"] = filter;
            return View(lista);
        }

        private PrijavaModel? UcitajCelinu(int id)
        {
            var prijava = _prijavaRepo.DajPoId(id);

            if (prijava is null)
            {
                return null;
            }

            prijava.Ucesnici =
                _ucesnikRepo.DajPoPrijavaId(id);

            return prijava;
        }

        private void PopuniTure(int ukljuciTuruId = 0)
        {
            ViewBag.Ture = _raftingTuraRepo
                .DajSve()
                .Where(x =>
                    (x.Aktivna &&
                     x.DatumOdrzavanja.Date >= DateTime.Today)
                    || x.Id == ukljuciTuruId)
                .ToList();
        }

        private int DajKorisnikId()
        {
            var vrednost =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(vrednost, out var id) ? id : 0;
        }

        private bool MozeDaVidi(PrijavaModel prijava)
        {
            return User.IsInRole("Admin")
                || prijava.KorisnikId == DajKorisnikId();
        }

        private bool MozeDaMenja(PrijavaModel prijava)
        {
            return MozeDaVidi(prijava);
        }

        private static void OcistiUcesnike(
            PrijavaRaftingViewModel model)
        {
            model.Ucesnici ??= new List<UcesnikPrijaveViewModel>();

            model.Ucesnici = model.Ucesnici
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Ime)
                    || !string.IsNullOrWhiteSpace(x.Prezime)
                    || !string.IsNullOrWhiteSpace(x.JMBG))
                .ToList();

            for (var i = 0; i < model.Ucesnici.Count; i++)
            {
                model.Ucesnici[i].RedniBroj = i + 1;
            }
        }

        private static PrijavaModel Mapiraj(
                PrijavaRaftingViewModel model)
        {
            return new PrijavaModel
            {
                Id = model.Id,
                BrojPrijave = model.BrojPrijave,
                DatumPrijave = model.DatumPrijave.Date,
                RaftingTuraId = model.RaftingTuraId,
                StatusPrijave = model.StatusPrijave,

                // Potvrda o uplati za celu prijavu
                PotvrdaUplateOriginalniNaziv =
                    model.PostojecaPotvrdaUplateOriginalniNaziv,

                PotvrdaUplateSacuvaniNaziv =
                    model.PostojecaPotvrdaUplateSacuvaniNaziv,

                Ucesnici = model.Ucesnici
                    .Select((x, indeks) => new UcesnikPrijaveModel
                    {
                        Id = x.Id,
                        RedniBroj = indeks + 1,
                        Ime = x.Ime.Trim(),
                        Prezime = x.Prezime.Trim(),
                        JMBG = x.JMBG.Trim(),
                        DatumRodjenja = x.DatumRodjenja.Date,
                        Adresa = x.Adresa.Trim(),
                        KontaktTelefon = x.KontaktTelefon.Trim(),
                        Email = x.Email.Trim(),
                        ZnaDaPliva = x.ZnaDaPliva,

                        PrihvatioIzjavuOdgovornosti =
                            x.PrihvatioIzjavuOdgovornosti,

                        // Potvrda da će poneti ličnu kartu ili pasoš
                        PotvrdioDonosenjeIdentifikacionogDokumenta =
                            x.PotvrdioDonosenjeIdentifikacionogDokumenta,

                        // Potpisana izjava
                        PotpisanaIzjavaOriginalniNaziv =
                            x.PostojecaPotpisanaIzjavaOriginalniNaziv,

                        PotpisanaIzjavaSacuvaniNaziv =
                            x.PostojecaPotpisanaIzjavaSacuvaniNaziv,

                        // Saglasnost roditelja/staratelja
                        SaglasnostOriginalniNaziv =
                            x.PostojecaSaglasnostOriginalniNaziv,

                        SaglasnostSacuvaniNaziv =
                            x.PostojecaSaglasnostSacuvaniNaziv,

                        IdentitetProveren =
                            x.IdentitetProveren,

                        DatumProvereIdentiteta =
                            x.DatumProvereIdentiteta,

                        // Stara polja privremeno ostavljamo zbog baze
                        LicnaKarta =
                            x.PotvrdioDonosenjeIdentifikacionogDokumenta,

                        PotvrdaOUplati =
                            !string.IsNullOrWhiteSpace(
                                model.PostojecaPotvrdaUplateSacuvaniNaziv),

                        PotpisanaIzjavaOdgovornosti =
                            !string.IsNullOrWhiteSpace(
                                x.PostojecaPotpisanaIzjavaSacuvaniNaziv),

                        SaglasnostRoditeljaStaratelja =
                            !string.IsNullOrWhiteSpace(
                                x.PostojecaSaglasnostSacuvaniNaziv)
                    })
                    .ToList()
            };
        }

        private async Task<(string OriginalniNaziv, string SacuvaniNaziv)>
            SacuvajFajlAsync(IFormFile fajl)
        {
            const long maksimalnaVelicina = 5 * 1024 * 1024;

            var dozvoljeneEkstenzije = new[]
            {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png"
    };

            if (fajl.Length == 0)
            {
                throw new InvalidOperationException(
                    "Izabrani dokument je prazan.");
            }

            if (fajl.Length > maksimalnaVelicina)
            {
                throw new InvalidOperationException(
                    "Dokument ne sme biti veći od 5 MB.");
            }

            var ekstenzija = Path
                .GetExtension(fajl.FileName)
                .ToLowerInvariant();

            if (!dozvoljeneEkstenzije.Contains(ekstenzija))
            {
                throw new InvalidOperationException(
                    "Dozvoljeni formati su PDF, JPG, JPEG i PNG.");
            }

            var folder = Path.Combine(
                _okruzenje.ContentRootPath,
                "App_Data",
                "DokumentiPrijava");

            Directory.CreateDirectory(folder);

            var sacuvaniNaziv =
                $"{Guid.NewGuid():N}{ekstenzija}";

            var putanja = Path.Combine(
                folder,
                sacuvaniNaziv);

            await using var tok = new FileStream(
                putanja,
                FileMode.CreateNew);

            await fajl.CopyToAsync(tok);

            return
            (
                Path.GetFileName(fajl.FileName),
                sacuvaniNaziv
            );
        }

        private static PrijavaRaftingViewModel Mapiraj(
    PrijavaModel model)
        {
            return new PrijavaRaftingViewModel
            {
                Id = model.Id,
                BrojPrijave = model.BrojPrijave,
                DatumPrijave = model.DatumPrijave,
                RaftingTuraId = model.RaftingTuraId,
                StatusPrijave = model.StatusPrijave,

                // Postojeća potvrda o uplati
                PostojecaPotvrdaUplateOriginalniNaziv =
                    model.PotvrdaUplateOriginalniNaziv,

                PostojecaPotvrdaUplateSacuvaniNaziv =
                    model.PotvrdaUplateSacuvaniNaziv,

                Ucesnici = model.Ucesnici
                    .Select(x => new UcesnikPrijaveViewModel
                    {
                        Id = x.Id,
                        RedniBroj = x.RedniBroj,
                        Ime = x.Ime,
                        Prezime = x.Prezime,
                        JMBG = x.JMBG,
                        DatumRodjenja = x.DatumRodjenja,
                        Adresa = x.Adresa,
                        KontaktTelefon = x.KontaktTelefon,
                        Email = x.Email,
                        ZnaDaPliva = x.ZnaDaPliva,

                        PrihvatioIzjavuOdgovornosti =
                            x.PrihvatioIzjavuOdgovornosti,

                        // Potvrda da će poneti ličnu kartu/pasoš
                        PotvrdioDonosenjeIdentifikacionogDokumenta =
                            x.PotvrdioDonosenjeIdentifikacionogDokumenta,

                        // Postojeća potpisana izjava
                        PostojecaPotpisanaIzjavaOriginalniNaziv =
                            x.PotpisanaIzjavaOriginalniNaziv,

                        PostojecaPotpisanaIzjavaSacuvaniNaziv =
                            x.PotpisanaIzjavaSacuvaniNaziv,

                        // Postojeća saglasnost roditelja/staratelja
                        PostojecaSaglasnostOriginalniNaziv =
                            x.SaglasnostOriginalniNaziv,

                        PostojecaSaglasnostSacuvaniNaziv =
                            x.SaglasnostSacuvaniNaziv,

                        IdentitetProveren =
                            x.IdentitetProveren,

                        DatumProvereIdentiteta =
                            x.DatumProvereIdentiteta,

                        // Stara polja ostaju zbog postojeće baze
                        LicnaKarta = x.LicnaKarta,
                        PotvrdaOUplati = x.PotvrdaOUplati,

                        PotpisanaIzjavaOdgovornosti =
                            x.PotpisanaIzjavaOdgovornosti,

                        SaglasnostRoditeljaStaratelja =
                            x.SaglasnostRoditeljaStaratelja
                    })
                    .ToList()
            };
        }
    }
}
