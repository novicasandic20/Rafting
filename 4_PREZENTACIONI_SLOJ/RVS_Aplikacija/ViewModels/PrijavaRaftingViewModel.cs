using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RVS_Aplikacija.ViewModels
{
    public class PrijavaRaftingViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "Broj prijave")]
        public string BrojPrijave { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum prijave je obavezan.")]
        [DataType(DataType.Date)]
        [Display(Name = "Datum prijave")]
        public DateTime DatumPrijave { get; set; } = DateTime.Today;

        [Range(1, int.MaxValue, ErrorMessage = "Izaberite rafting turu.")]
        [Display(Name = "Rafting tura")]
        public int RaftingTuraId { get; set; }

        [Required]
        [Display(Name = "Status prijave")]
        public string StatusPrijave { get; set; } = "Kreirana";

        public List<UcesnikPrijaveViewModel> Ucesnici { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            var ucesnici = Ucesnici ?? new List<UcesnikPrijaveViewModel>();

            if (ucesnici.Count == 0)
            {
                yield return new ValidationResult(
                    "Prijava mora sadržati najmanje jednog učesnika.",
                    new[] { nameof(Ucesnici) });
                yield break;
            }

            var duplikati = ucesnici
                .Where(x => !string.IsNullOrWhiteSpace(x.JMBG))
                .GroupBy(x => x.JMBG)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplikati.Count > 0)
            {
                yield return new ValidationResult(
                    "Isti JMBG ne može biti unet više puta u jednoj prijavi.",
                    new[] { nameof(Ucesnici) });
            }

            if (PotvrdaUplateFajl is null &&
                string.IsNullOrWhiteSpace(
                    PostojecaPotvrdaUplateSacuvaniNaziv))
            {
                yield return new ValidationResult(
                    "Potrebno je priložiti potvrdu o uplati.",
                    new[] { nameof(PotvrdaUplateFajl) });
            }


            for (var i = 0; i < ucesnici.Count; i++)
            {
                var ucesnik = ucesnici[i];
                    
                if (ucesnik.DatumRodjenja.Date > DateTime.Today)
                {
                    yield return new ValidationResult(
                        $"Datum rođenja učesnika {i + 1} ne može biti u budućnosti.",
                        new[] { $"Ucesnici[{i}].DatumRodjenja" });
                }

                var maloletan =
                    ucesnik.DatumRodjenja.Date >
                    DateTime.Today.AddYears(-18);

                if (maloletan &&
                    !ucesnik.SaglasnostRoditeljaStaratelja)
                {
                    yield return new ValidationResult(
                        $"Za maloletnog učesnika {i + 1} potrebna je saglasnost roditelja ili staratelja.",
                        new[] { $"Ucesnici[{i}].SaglasnostRoditeljaStaratelja" });
                }

                if (!ucesnik.PotvrdioDonosenjeIdentifikacionogDokumenta)
                {
                    yield return new ValidationResult(
                        $"Učesnik {i + 1} mora potvrditi da će poneti ličnu kartu ili pasoš.",
                        new[] { $"Ucesnici[{i}].PotvrdioDonosenjeIdentifikacionogDokumenta" });
                }

                if (!ucesnik.PrihvatioIzjavuOdgovornosti)
                {
                    yield return new ValidationResult(
                        $"Učesnik {i + 1} mora prihvatiti izjavu o odgovornosti.",
                        new[] { $"Ucesnici[{i}].PrihvatioIzjavuOdgovornosti" });
                }

                if (ucesnik.PotpisanaIzjavaFajl is null &&
                    string.IsNullOrWhiteSpace(
                        ucesnik.PostojecaPotpisanaIzjavaSacuvaniNaziv))
                {
                    yield return new ValidationResult(
                        $"Za učesnika {i + 1} potrebno je priložiti potpisanu izjavu.",
                        new[] { $"Ucesnici[{i}].PotpisanaIzjavaFajl"});
                }

                if (maloletan &&
                    ucesnik.SaglasnostFajl is null &&
                    string.IsNullOrWhiteSpace(
                        ucesnik.PostojecaSaglasnostSacuvaniNaziv))
                {
                    yield return new ValidationResult(
                        $"Za maloletnog učesnika {i + 1} potrebno je priložiti saglasnost roditelja ili staratelja.",
                        new[] {$"Ucesnici[{i}].SaglasnostFajl"});
                }
            }
        }

        [Display(Name = "Potvrda o uplati")]
        public IFormFile? PotvrdaUplateFajl { get; set; }

        public string? PostojecaPotvrdaUplateOriginalniNaziv
        {
            get;
            set;
        }

        public string? PostojecaPotvrdaUplateSacuvaniNaziv
        {
            get;
            set;
        }
    }
}
