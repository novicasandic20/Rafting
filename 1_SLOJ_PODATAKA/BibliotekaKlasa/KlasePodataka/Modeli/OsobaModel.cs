namespace BibliotekaKlasa.KlasePodataka.Modeli
{
    // Bazna klasa demonstrira nasleđivanje u objektno-orijentisanom rešenju.
    public abstract class OsobaModel
    {
        public int Id { get; set; }
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string ImeIPrezime => $"{Ime} {Prezime}".Trim();
    }
}
