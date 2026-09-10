using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RVS_Aplikacija.ViewModels
{
    public class UcesnikPrijaveViewModel
    {
        public int Id { get; set; }
        public int RedniBroj { get; set; }

        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(50)]
        public string Ime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(50)]
        public string Prezime { get; set; } = string.Empty;

        [Required(ErrorMessage = "JMBG je obavezan.")]
        [RegularExpression(
            @"^\d{13}$",
            ErrorMessage = "JMBG mora imati tačno 13 cifara.")]
        public string JMBG { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum rođenja je obavezan.")]
        [DataType(DataType.Date)]
        public DateTime DatumRodjenja { get; set; }

        [Required(ErrorMessage = "Adresa je obavezna.")]
        [StringLength(150)]
        public string Adresa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kontakt telefon je obavezan.")]
        [RegularExpression(
            @"^[0-9+()\/\-\s]{6,30}$",
            ErrorMessage = "Kontakt telefon nije u ispravnom formatu.")]
        public string KontaktTelefon { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail je obavezan.")]
        [EmailAddress(ErrorMessage = "E-mail adresa nije ispravna.")]
        public string Email { get; set; } = string.Empty;

        public bool ZnaDaPliva { get; set; }
        [Range(
             typeof(bool),
             "true",
             "true",
             ErrorMessage = "Morate prihvatiti izjavu o odgovornosti.")]
        public bool PrihvatioIzjavuOdgovornosti { get; set; }
        public bool LicnaKarta { get; set; }
        public bool PotvrdaOUplati { get; set; }
        public bool PotpisanaIzjavaOdgovornosti { get; set; }
        public bool SaglasnostRoditeljaStaratelja { get; set; }

        [Range(
            typeof(bool),
            "true",
            "true",
            ErrorMessage =
                "Morate potvrditi da ćete poneti ličnu kartu ili pasoš.")]
        public bool PotvrdioDonosenjeIdentifikacionogDokumenta
        {
            get;
            set;
        }

        public IFormFile? PotpisanaIzjavaFajl { get; set; }

        public string? PostojecaPotpisanaIzjavaOriginalniNaziv
        {
            get;
            set;
        }

        public string? PostojecaPotpisanaIzjavaSacuvaniNaziv
        {
            get;
            set;
        }

        public IFormFile? SaglasnostFajl { get; set; }

        public string? PostojecaSaglasnostOriginalniNaziv
        {
            get;
            set;
        }

        public string? PostojecaSaglasnostSacuvaniNaziv
        {
            get;
            set;
        }

        public bool IdentitetProveren { get; set; }
        public DateTime? DatumProvereIdentiteta { get; set; }
    }
}
