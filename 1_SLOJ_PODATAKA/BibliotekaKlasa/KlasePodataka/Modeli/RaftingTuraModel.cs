namespace BibliotekaKlasa.KlasePodataka.Modeli
{
    public class RaftingTuraModel
    {
        public int Id { get; set; }
        public string NazivTure { get; set; } = string.Empty;
        public DateTime DatumOdrzavanja { get; set; }
        public string LokacijaReka { get; set; } = string.Empty;
        public int NivoTezineId { get; set; }
        public NivoTezineModel? NivoTezineObjekat { get; set; }
        public int MaksimalanBrojUcesnika { get; set; }
        public decimal Cena { get; set; }
        public bool Aktivna { get; set; }
        public int BrojPrijavljenihUcesnika { get; set; }

        public int PreostaloMesta =>
            Math.Max(0, MaksimalanBrojUcesnika - BrojPrijavljenihUcesnika);
    }
}
