namespace BibliotekaKlasa.KlasePodataka.Modeli
{
    public class UcesnikPrijaveModel : OsobaModel
    {
        public int PrijavaId { get; set; }
        public int RedniBroj { get; set; }
        public string JMBG { get; set; } = string.Empty;
        public DateTime DatumRodjenja { get; set; }
        public string Adresa { get; set; } = string.Empty;
        public string KontaktTelefon { get; set; } = string.Empty;
        public bool ZnaDaPliva { get; set; }
        public bool PrihvatioIzjavuOdgovornosti { get; set; }
        public bool LicnaKarta { get; set; }
        public bool PotvrdaOUplati { get; set; }
        public bool PotpisanaIzjavaOdgovornosti { get; set; }
        public bool SaglasnostRoditeljaStaratelja { get; set; }

        public bool Maloletan =>
            DatumRodjenja.Date > DateTime.Today.AddYears(-18);

        public bool PotvrdioDonosenjeIdentifikacionogDokumenta
        {
            get;
            set;
        }

        public string? PotpisanaIzjavaOriginalniNaziv { get; set; }
        public string? PotpisanaIzjavaSacuvaniNaziv { get; set; }

        public string? SaglasnostOriginalniNaziv { get; set; }
        public string? SaglasnostSacuvaniNaziv { get; set; }

        public bool IdentitetProveren { get; set; }
        public DateTime? DatumProvereIdentiteta { get; set; }
        public int? IdentitetProverioKorisnikId { get; set; }
    }
}
