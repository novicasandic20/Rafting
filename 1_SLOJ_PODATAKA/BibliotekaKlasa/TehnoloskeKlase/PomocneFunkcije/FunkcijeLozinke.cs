using System.Security.Cryptography;
using System.Text;

namespace BibliotekaKlasa.TehnoloskeKlase.PomocneFunkcije
{
    public static class FunkcijeLozinke
    {
        public static string GenerisiSalt(int duzina = 16)
        {
            var bajtovi = RandomNumberGenerator.GetBytes(duzina);
            return Convert.ToBase64String(bajtovi);
        }

        public static string IzracunajHash(string lozinka, string salt)
        {
            using var sha256 = SHA256.Create();
            var ulaz = Encoding.UTF8.GetBytes(lozinka + salt);
            return Convert.ToBase64String(sha256.ComputeHash(ulaz));
        }

        public static bool ProveriLozinku(
            string lozinka,
            string salt,
            string ocekivaniHash)
        {
            var stvarniHash = IzracunajHash(lozinka, salt);
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(stvarniHash),
                Convert.FromBase64String(ocekivaniHash));
        }
    }
}
