using System.ComponentModel.DataAnnotations;

namespace RVS_Aplikacija.ViewModels
{
    public class RaftingTuraViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv ture je obavezan.")]
        [StringLength(100)]
        [Display(Name = "Naziv ture")]
        public string NazivTure { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum održavanja je obavezan.")]
        [DataType(DataType.Date)]
        [Display(Name = "Datum održavanja")]
        public DateTime DatumOdrzavanja { get; set; } =
            DateTime.Today.AddDays(30);

        [Required(ErrorMessage = "Lokacija ili reka je obavezna.")]
        [StringLength(100)]
        [Display(Name = "Lokacija / reka")]
        public string LokacijaReka { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Izaberite nivo težine.")]
        [Display(Name = "Nivo težine")]
        public int NivoTezineId { get; set; }

        [Range(1, 500, ErrorMessage = "Kapacitet mora biti od 1 do 500.")]
        [Display(Name = "Maksimalan broj učesnika")]
        public int MaksimalanBrojUcesnika { get; set; }

        [Range(0, 1000000, ErrorMessage = "Cena nije ispravna.")]
        [Display(Name = "Cena ture")]
        public decimal Cena { get; set; }

        [Display(Name = "Aktivna tura")]
        public bool Aktivna { get; set; } = true;
    }
}
