using BibliotekaKlasa.KlasePodataka.Modeli;
using BibliotekaKlasa.KlasePodataka.Repozitorijumi;
using BibliotekaKlasa.KlasePodatakaEF.RepozitorijumiEF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RVS_Aplikacija.ViewModels;

namespace RVS_Aplikacija.Controllers
{
    public class RaftingTureController : Controller
    {
        private readonly RaftingTuraRepo _raftingTuraRepo;
        private readonly NivoTezineRepo _nivoTezineRepo;

        public RaftingTureController(
            RaftingTuraRepo raftingTuraRepo,
            NivoTezineRepo nivoTezineRepo)
        {
            _raftingTuraRepo = raftingTuraRepo;
            _nivoTezineRepo = nivoTezineRepo;
        }

        public IActionResult Index(string? filter)
        {
            ViewData["Filter"] = filter;
            return View(_raftingTuraRepo.DajSve(filter));
        }

        public IActionResult Detalji(int id)
        {
            var tura = _raftingTuraRepo.DajPoId(id);
            return tura is null ? NotFound() : View(tura);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Dodaj()
        {
            await PopuniNivoeTezineAsync();
            return View(new RaftingTuraViewModel());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dodaj(
            RaftingTuraViewModel model)
        {
            ProveriDatumTure(model);

            if (!ModelState.IsValid)
            {
                await PopuniNivoeTezineAsync();
                return View(model);
            }

            _raftingTuraRepo.Dodaj(Mapiraj(model));

            TempData["Poruka"] = "Rafting tura je uspešno dodata.";
            TempData["PorukaTip"] = "uspeh";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Izmeni(int id)
        {
            var tura = _raftingTuraRepo.DajPoId(id);

            if (tura is null)
            {
                return NotFound();
            }

            await PopuniNivoeTezineAsync();

            return View(
                new RaftingTuraViewModel
                {
                    Id = tura.Id,
                    NazivTure = tura.NazivTure,
                    DatumOdrzavanja = tura.DatumOdrzavanja,
                    LokacijaReka = tura.LokacijaReka,
                    NivoTezineId = tura.NivoTezineId,
                    MaksimalanBrojUcesnika =
                        tura.MaksimalanBrojUcesnika,
                    Cena = tura.Cena,
                    Aktivna = tura.Aktivna
                });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Izmeni(
            RaftingTuraViewModel model)
        {
            ProveriDatumTure(model, dozvoliProsliDatum: true);

            var postojecaTura = _raftingTuraRepo.DajPoId(model.Id);
            if (postojecaTura is null)
            {
                return NotFound();
            }

            if (model.MaksimalanBrojUcesnika <
                postojecaTura.BrojPrijavljenihUcesnika)
            {
                ModelState.AddModelError(
                    nameof(model.MaksimalanBrojUcesnika),
                    "Kapacitet ne može biti manji od broja već prijavljenih učesnika.");
            }

            if (!ModelState.IsValid)
            {
                await PopuniNivoeTezineAsync();
                return View(model);
            }

            _raftingTuraRepo.Izmeni(Mapiraj(model));

            TempData["Poruka"] = "Rafting tura je uspešno izmenjena.";
            TempData["PorukaTip"] = "uspeh";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Obrisi(int id)
        {
            try
            {
                _raftingTuraRepo.Obrisi(id);
                TempData["Poruka"] = "Rafting tura je obrisana.";
                TempData["PorukaTip"] = "uspeh";
            }
            catch (SqlException)
            {
                TempData["Poruka"] =
                    "Tura ima prijave i ne može se obrisati. " +
                    "Možete je označiti kao neaktivnu.";

                TempData["PorukaTip"] = "greska";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopuniNivoeTezineAsync()
        {
            ViewBag.NivoiTezine =
                await _nivoTezineRepo.DajSveAsync();
        }

        private void ProveriDatumTure(
            RaftingTuraViewModel model,
            bool dozvoliProsliDatum = false)
        {
            if (!dozvoliProsliDatum &&
                model.DatumOdrzavanja.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.DatumOdrzavanja),
                    "Datum nove ture ne može biti u prošlosti.");
            }
        }

        private static RaftingTuraModel Mapiraj(
            RaftingTuraViewModel model)
        {
            return new RaftingTuraModel
            {
                Id = model.Id,
                NazivTure = model.NazivTure.Trim(),
                DatumOdrzavanja = model.DatumOdrzavanja.Date,
                LokacijaReka = model.LokacijaReka.Trim(),
                NivoTezineId = model.NivoTezineId,
                MaksimalanBrojUcesnika =
                    model.MaksimalanBrojUcesnika,
                Cena = model.Cena,
                Aktivna = model.Aktivna
            };
        }
    }
}
