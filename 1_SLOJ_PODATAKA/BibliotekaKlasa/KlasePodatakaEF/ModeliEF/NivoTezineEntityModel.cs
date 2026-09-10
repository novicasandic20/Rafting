using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BibliotekaKlasa.KlasePodatakaEF.ModeliEF
{
    [Table("NivoiTezine")]
    public class NivoTezineEntityModel
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Naziv { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Opis { get; set; }
    }
}
