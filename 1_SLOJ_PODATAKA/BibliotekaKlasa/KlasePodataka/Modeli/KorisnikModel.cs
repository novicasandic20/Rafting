namespace BibliotekaKlasa.KlasePodataka.Modeli
{
    public class KorisnikModel : OsobaModel
    {
        public string LozinkaHash { get; set; } = string.Empty;
        public string LozinkaSalt { get; set; } = string.Empty;
        public string Uloga { get; set; } = "Korisnik";
    }
}
