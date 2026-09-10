using System.Diagnostics;
using BibliotekaKlasa.KlasePodataka.Repozitorijumi;
using Microsoft.AspNetCore.Mvc;
using RVS_Aplikacija.ViewModels;

namespace RVS_Aplikacija.Controllers
{
    public class PocetnaController : Controller
    {
        private readonly RaftingTuraRepo _raftingTuraRepo;
        private readonly PrijavaRepo _prijavaRepo;

        public PocetnaController(RaftingTuraRepo raftingTuraRepo, PrijavaRepo prijavaRepo)
        {
            _raftingTuraRepo = raftingTuraRepo;
            _prijavaRepo = prijavaRepo;
        }

        public IActionResult Index()
        {
            var sveTure = _raftingTuraRepo
                .DajSve(samoAktivne: true)
                .Where(x =>
                    x.Aktivna &&
                    x.DatumOdrzavanja.Date >= DateTime.Today)
                .OrderBy(x => x.DatumOdrzavanja)
                .ToList();

            var model = new PocetnaViewModel
            {
                Ture = sveTure
                    .Take(6)
                    .ToList(),

                AktivneTure = sveTure.Count
            };

            if (User.IsInRole("Admin"))
            {
                var prijave = _prijavaRepo.DajSve();

                model.PrijaveNaCekanju =
                    prijave.Count(x =>
                        x.StatusPrijave == "Kreirana");

                model.PotvrdjenePrijave =
                    prijave.Count(x =>
                        x.StatusPrijave == "Potvrđena");

                model.UkupnoUcesnika =
                    prijave
                        .Where(x =>
                            x.StatusPrijave == "Kreirana" ||
                            x.StatusPrijave == "Potvrđena")
                        .Sum(x => x.BrojUcesnikaIzUpita);
            }

            return View(model);
        }

        public IActionResult Privatnost()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Greska()
        {
            return View(
                new GreskaViewModel
                {
                    RequestId =
                        Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
                });
        }
    }
}
