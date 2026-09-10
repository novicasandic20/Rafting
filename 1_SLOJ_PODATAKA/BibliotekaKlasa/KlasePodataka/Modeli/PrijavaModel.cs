namespace BibliotekaKlasa.KlasePodataka.Modeli
{
    public class PrijavaModel
    {
        public int Id { get; set; }
        public string BrojPrijave { get; set; } = string.Empty;
        public DateTime DatumPrijave { get; set; }
        public int RaftingTuraId { get; set; }
        public int KorisnikId { get; set; }
        public string StatusPrijave { get; set; } = "Kreirana";
        public RaftingTuraModel? RaftingTuraObjekat { get; set; }
        public KorisnikModel? KorisnikObjekat { get; set; }
        public List<UcesnikPrijaveModel> Ucesnici { get; set; } = new();

        public int BrojUcesnika =>
            Ucesnici.Count > 0 ? Ucesnici.Count : BrojUcesnikaIzUpita;

        public int BrojUcesnikaIzUpita { get; set; }

        public string? PotvrdaUplateOriginalniNaziv { get; set; }
        public string? PotvrdaUplateSacuvaniNaziv { get; set; }

        public DateTime? DatumOdobrenja { get; set; }
        public int? OdobrioKorisnikId { get; set; }
    }
}
