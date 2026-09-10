namespace PoslovnaLogika.Modeli
{
    public class RezultatProvereKapaciteta
    {
        public bool Dozvoljeno { get; set; }
        public int MaksimalanKapacitet { get; set; }
        public int EfektivniKapacitet { get; set; }
        public int TrenutnoPrijavljenih { get; set; }
        public int NoviUcesnici { get; set; }
        public int PreostaloMesta { get; set; }
        public bool ParametriUcitanIzServisa { get; set; }
        public string Poruka { get; set; } = string.Empty;
    }
}
