using BibliotekaKlasa.KlasePodataka.Modeli;
using BibliotekaKlasa.TehnoloskeKlase;
using Microsoft.Data.SqlClient;

namespace BibliotekaKlasa.KlasePodataka.Repozitorijumi
{
    public class KorisnikRepo
    {
        private readonly KonekcijaKlasa _konekcijaObjekat;

        public KorisnikRepo(KonekcijaKlasa konekcijaObjekat)
        {
            _konekcijaObjekat = konekcijaObjekat;
        }

        public KorisnikModel? DajPoEmailu(string email)
        {
            const string upit = """
                SELECT Id, Ime, Prezime, Email, LozinkaHash, LozinkaSalt, Uloga
                FROM Korisnici
                WHERE Email = @Email
                """;

            using var konekcija = _konekcijaObjekat.KreirajOtvorenuKonekciju();
            using var komanda = new SqlCommand(upit, konekcija);
            komanda.Parameters.AddWithValue("@Email", email);

            using var citac = komanda.ExecuteReader();

            if (!citac.Read())
            {
                return null;
            }

            return Mapiraj(citac);
        }

        public List<KorisnikModel> DajSve()
        {
            const string upit = """
                SELECT Id, Ime, Prezime, Email, LozinkaHash, LozinkaSalt, Uloga
                FROM Korisnici
                ORDER BY Prezime, Ime
                """;

            var lista = new List<KorisnikModel>();

            using var konekcija = _konekcijaObjekat.KreirajOtvorenuKonekciju();
            using var komanda = new SqlCommand(upit, konekcija);
            using var citac = komanda.ExecuteReader();

            while (citac.Read())
            {
                lista.Add(Mapiraj(citac));
            }

            return lista;
        }

        public void Dodaj(KorisnikModel korisnik)
        {
            const string upit = """
                INSERT INTO Korisnici
                    (Ime, Prezime, Email, LozinkaHash, LozinkaSalt, Uloga)
                VALUES
                    (@Ime, @Prezime, @Email, @LozinkaHash, @LozinkaSalt, @Uloga)
                """;

            using var konekcija = _konekcijaObjekat.KreirajOtvorenuKonekciju();
            using var komanda = new SqlCommand(upit, konekcija);

            komanda.Parameters.AddWithValue("@Ime", korisnik.Ime);
            komanda.Parameters.AddWithValue("@Prezime", korisnik.Prezime);
            komanda.Parameters.AddWithValue("@Email", korisnik.Email);
            komanda.Parameters.AddWithValue("@LozinkaHash", korisnik.LozinkaHash);
            komanda.Parameters.AddWithValue("@LozinkaSalt", korisnik.LozinkaSalt);
            komanda.Parameters.AddWithValue("@Uloga", korisnik.Uloga);

            komanda.ExecuteNonQuery();
        }

        private static KorisnikModel Mapiraj(SqlDataReader citac)
        {
            return new KorisnikModel
            {
                Id = citac.GetInt32(0),
                Ime = citac.GetString(1),
                Prezime = citac.GetString(2),
                Email = citac.GetString(3),
                LozinkaHash = citac.GetString(4),
                LozinkaSalt = citac.GetString(5),
                Uloga = citac.GetString(6)
            };
        }
    }
}
