using System.ComponentModel.DataAnnotations;

namespace RVS_Aplikacija.ViewModels
{
    public class NivoTezineViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Naziv nivoa težine je obavezan.")]
        [StringLength(50)]
        public string Naziv { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Opis { get; set; }
    }
}
