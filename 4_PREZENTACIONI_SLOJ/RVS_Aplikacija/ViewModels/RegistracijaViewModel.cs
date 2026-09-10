using System.ComponentModel.DataAnnotations;

namespace RVS_Aplikacija.ViewModels
{
    public class RegistracijaViewModel
    {
        [Required(ErrorMessage = "Ime je obavezno.")]
        [StringLength(40)]
        public string Ime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Prezime je obavezno.")]
        [StringLength(40)]
        public string Prezime { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail je obavezan.")]
        [EmailAddress(ErrorMessage = "E-mail adresa nije ispravna.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [MinLength(8, ErrorMessage = "Lozinka mora imati najmanje 8 znakova.")]
        [DataType(DataType.Password)]
        public string Lozinka { get; set; } = string.Empty;
        

        [Required(ErrorMessage = "Potvrda lozinke je obavezna.")]
        [Compare(nameof(Lozinka), ErrorMessage = "Lozinke se ne podudaraju.")]
        [Display(Name = "Potvrdite lozinku")]
        [DataType(DataType.Password)]
        public string LozinkaPotvrda { get; set; } = string.Empty;
    }
}
