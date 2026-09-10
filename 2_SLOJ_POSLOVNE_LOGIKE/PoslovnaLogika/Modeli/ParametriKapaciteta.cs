namespace PoslovnaLogika.Modeli
{
    public class ParametriKapaciteta
    {
        public decimal ProcenatKapaciteta { get; set; } = 100m;
        public int RezervisanaMesta { get; set; }
        public List<string> StatusiKojiSeRacunaju { get; set; } =
            new() { "Kreirana", "Potvrđena" };
    }
}
