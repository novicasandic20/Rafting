using System.ComponentModel.DataAnnotations;

namespace RVS_Aplikacija.ViewModels
{
    public class PrijavaNalogaViewModel
    {
        [Required(ErrorMessage = "E-mail je obavezan.")]
        [EmailAddress(ErrorMessage = "E-mail adresa nije ispravna.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [DataType(DataType.Password)]
        public string Lozinka { get; set; } = string.Empty;
    }
}
