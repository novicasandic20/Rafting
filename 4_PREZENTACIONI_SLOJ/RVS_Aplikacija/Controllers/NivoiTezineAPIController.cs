using System.Net;
using System.Net.Http.Json;
using BibliotekaKlasa.KlasePodataka.Modeli;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVS_Aplikacija.ViewModels;

namespace RVS_Aplikacija.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NivoiTezineAPIController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public NivoiTezineAPIController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var klijent =
                    _httpClientFactory.CreateClient("NivoiTezineApi");

                var lista =
                    await klijent.GetFromJsonAsync<List<NivoTezineModel>>(
                        "api/nivoi-tezine")
                    ?? new List<NivoTezineModel>();

                return View(lista);
            }
            catch (HttpRequestException)
            {
                TempData["Poruka"] =
                    "REST servis za nivoe težine nije dostupan. " +
                    "Pokrenite projekat REST_SERVIS_CRUD_Operacija.";

                TempData["PorukaTip"] = "greska";
                return View(new List<NivoTezineModel>());
            }
        }

        [HttpGet]
        public IActionResult Dodaj()
        {
            return View(new NivoTezineViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dodaj(
            NivoTezineViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var klijent =
                _httpClientFactory.CreateClient("NivoiTezineApi");

            try
            {
                var odgovor = await klijent.PostAsJsonAsync(
                    "api/nivoi-tezine",
                    Mapiraj(model));

                if (odgovor.StatusCode == HttpStatusCode.Conflict)
                {
                    ModelState.AddModelError(
                        nameof(model.Naziv),
                        await odgovor.Content.ReadAsStringAsync());

                    return View(model);
                }

                odgovor.EnsureSuccessStatusCode();

                TempData["Poruka"] =
                    "Nivo težine je dodat preko REST servisa.";

                TempData["PorukaTip"] = "uspeh";
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "REST servis nije dostupan.");

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Izmeni(int id)
        {
            var klijent =
                _httpClientFactory.CreateClient("NivoiTezineApi");

            try
            {
                var nivo =
                    await klijent.GetFromJsonAsync<NivoTezineModel>(
                        $"api/nivoi-tezine/{id}");

                if (nivo is null)
                {
                    return NotFound();
                }

                return View(
                    new NivoTezineViewModel
                    {
                        Id = nivo.Id,
                        Naziv = nivo.Naziv,
                        Opis = nivo.Opis
                    });
            }
            catch (HttpRequestException)
            {
                TempData["Poruka"] = "REST servis nije dostupan.";
                TempData["PorukaTip"] = "greska";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Izmeni(
            NivoTezineViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var klijent =
                _httpClientFactory.CreateClient("NivoiTezineApi");

            try
            {
                var odgovor = await klijent.PutAsJsonAsync(
                    $"api/nivoi-tezine/{model.Id}",
                    Mapiraj(model));

                if (odgovor.StatusCode == HttpStatusCode.Conflict)
                {
                    ModelState.AddModelError(
                        nameof(model.Naziv),
                        await odgovor.Content.ReadAsStringAsync());

                    return View(model);
                }

                odgovor.EnsureSuccessStatusCode();

                TempData["Poruka"] =
                    "Nivo težine je izmenjen preko REST servisa.";

                TempData["PorukaTip"] = "uspeh";
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "REST servis nije dostupan.");

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Obrisi(int id)
        {
            var klijent =
                _httpClientFactory.CreateClient("NivoiTezineApi");

            try
            {
                var odgovor = await klijent.DeleteAsync(
                    $"api/nivoi-tezine/{id}");

                if (odgovor.StatusCode == HttpStatusCode.Conflict)
                {
                    TempData["Poruka"] =
                        await odgovor.Content.ReadAsStringAsync();

                    TempData["PorukaTip"] = "greska";
                }
                else
                {
                    odgovor.EnsureSuccessStatusCode();
                    TempData["Poruka"] =
                        "Nivo težine je obrisan preko REST servisa.";

                    TempData["PorukaTip"] = "uspeh";
                }
            }
            catch (HttpRequestException)
            {
                TempData["Poruka"] = "REST servis nije dostupan.";
                TempData["PorukaTip"] = "greska";
            }

            return RedirectToAction(nameof(Index));
        }

        private static NivoTezineModel Mapiraj(
            NivoTezineViewModel model)
        {
            return new NivoTezineModel
            {
                Id = model.Id,
                Naziv = model.Naziv.Trim(),
                Opis = model.Opis?.Trim()
            };
        }
    }
}
