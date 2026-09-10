using System.Security.Claims;
using BibliotekaKlasa.KlasePodataka.Modeli;
using BibliotekaKlasa.KlasePodataka.Repozitorijumi;
using BibliotekaKlasa.TehnoloskeKlase.PomocneFunkcije;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RVS_Aplikacija.ViewModels;

namespace RVS_Aplikacija.Controllers
{
    public class NalogController : Controller
    {
        private readonly KorisnikRepo _korisnikRepo;

        public NalogController(KorisnikRepo korisnikRepo)
        {
            _korisnikRepo = korisnikRepo;
        }

        [HttpGet]
        public IActionResult Registracija()
        {
            return View(new RegistracijaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registracija(
            RegistracijaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_korisnikRepo.DajPoEmailu(model.Email.Trim()) is not null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Korisnik sa ovom e-mail adresom već postoji.");

                return View(model);
            }

            var salt = FunkcijeLozinke.GenerisiSalt();
            var hash =
                FunkcijeLozinke.IzracunajHash(model.Lozinka, salt);

            _korisnikRepo.Dodaj(
                new KorisnikModel
                {
                    Ime = model.Ime.Trim(),
                    Prezime = model.Prezime.Trim(),
                    Email = model.Email.Trim(),
                    LozinkaSalt = salt,
                    LozinkaHash = hash,
                    Uloga = "Korisnik"
                });

            TempData["Poruka"] =
                "Registracija je uspešna. Sada se možete prijaviti.";

            TempData["PorukaTip"] = "uspeh";
            return RedirectToAction(nameof(Prijava));
        }

        [HttpGet]
        public IActionResult Prijava()
        {
            return View(new PrijavaNalogaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Prijava(
            PrijavaNalogaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var korisnik =
                _korisnikRepo.DajPoEmailu(model.Email.Trim());

            if (korisnik is null ||
                !FunkcijeLozinke.ProveriLozinku(
                    model.Lozinka,
                    korisnik.LozinkaSalt,
                    korisnik.LozinkaHash))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Neispravan e-mail ili lozinka.");

                return View(model);
            }

            var potrazivanja = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, korisnik.Id.ToString()),
                new(ClaimTypes.Name, korisnik.ImeIPrezime),
                new(ClaimTypes.Email, korisnik.Email),
                new(ClaimTypes.Role, korisnik.Uloga)
            };

            var identitet = new ClaimsIdentity(
                potrazivanja,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identitet));

            HttpContext.Session.SetString(
                "KorisnikUloga",
                korisnik.Uloga);

            return RedirectToAction("Index", "Pocetna");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OdjaviSe()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Pocetna");
        }

        public IActionResult ZabranjenPristup()
        {
            return View();
        }
    }
}
