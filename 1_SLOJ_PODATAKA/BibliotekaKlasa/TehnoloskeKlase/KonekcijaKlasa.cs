using Microsoft.Data.SqlClient;

namespace BibliotekaKlasa.TehnoloskeKlase
{
    public class KonekcijaKlasa
    {
        public string KonekcioniString { get; }

        public KonekcijaKlasa(string konekcioniString)
        {
            if (string.IsNullOrWhiteSpace(konekcioniString))
            {
                throw new ArgumentException(
                    "Konekcioni string nije podešen.",
                    nameof(konekcioniString));
            }

            KonekcioniString = konekcioniString;
        }

        public SqlConnection KreirajKonekciju()
        {
            return new SqlConnection(KonekcioniString);
        }

        public SqlConnection KreirajOtvorenuKonekciju()
        {
            var konekcija = KreirajKonekciju();
                konekcija.Open();
            return konekcija;
        }
    }
}
