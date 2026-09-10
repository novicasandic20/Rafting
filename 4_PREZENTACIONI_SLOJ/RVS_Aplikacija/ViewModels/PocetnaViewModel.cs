using BibliotekaKlasa.KlasePodataka.Modeli;

namespace RVS_Aplikacija.ViewModels
{
    public class PocetnaViewModel
    {
        public List<RaftingTuraModel> Ture { get; set; }
            = new();

        public int PrijaveNaCekanju { get; set; }

        public int PotvrdjenePrijave { get; set; }

        public int AktivneTure { get; set; }

        public int UkupnoUcesnika { get; set; }
    }
}