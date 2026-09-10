using BibliotekaKlasa.KlasePodataka.Repozitorijumi;
using Microsoft.AspNetCore.Mvc;

namespace RVS_Aplikacija.ViewComponents
{
    public class RaftingTureMeniViewComponent : ViewComponent
    {
        private readonly RaftingTuraRepo _raftingTuraRepo;

        public RaftingTureMeniViewComponent(
            RaftingTuraRepo raftingTuraRepo)
        {
            _raftingTuraRepo = raftingTuraRepo;
        }

        public IViewComponentResult Invoke()
        {
            var ture = _raftingTuraRepo
                .DajSve(samoAktivne: true)
                .Where(x =>
                    x.Aktivna &&
                    x.DatumOdrzavanja.Date >= DateTime.Today)
                .OrderBy(x => x.DatumOdrzavanja)
                .Take(5)
                .ToList();

            return View(ture);
        }
    }
}