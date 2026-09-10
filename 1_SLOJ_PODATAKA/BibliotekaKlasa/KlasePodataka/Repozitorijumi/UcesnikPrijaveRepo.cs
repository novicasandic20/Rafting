using System.Data;
using BibliotekaKlasa.KlasePodataka.Modeli;
using BibliotekaKlasa.TehnoloskeKlase;
using Microsoft.Data.SqlClient;

namespace BibliotekaKlasa.KlasePodataka.Repozitorijumi
{
    // Primer standardnih SQL client klasa i stored procedura.
    public class UcesnikPrijaveRepo
    {
        private readonly KonekcijaKlasa _konekcijaObjekat;

        public UcesnikPrijaveRepo(KonekcijaKlasa konekcijaObjekat)
        {
            _konekcijaObjekat = konekcijaObjekat;
        }

        public List<UcesnikPrijaveModel> DajPoPrijavaId(int prijavaId)
        {
            var lista = new List<UcesnikPrijaveModel>();

            using var konekcija =
                _konekcijaObjekat.KreirajOtvorenuKonekciju();

            using var komanda =
                new SqlCommand("spDajUcesnikePoPrijavaId", konekcija)
                {
                    CommandType = CommandType.StoredProcedure
                };

            komanda.Parameters.AddWithValue("@PrijavaId", prijavaId);

            using var citac = komanda.ExecuteReader();

            while (citac.Read())
            {
                lista.Add(Mapiraj(citac));
            }

            return lista;
        }

        public bool PostojiJmbgNaTuri(
    string jmbg,
    int raftingTuraId,
    int prijavaIdZaIzuzimanje = 0)
        {
            using var konekcija =
                _konekcijaObjekat.KreirajOtvorenuKonekciju();

            using var komanda =
                new SqlCommand(
                    "spPostojiJMBGNaTuri",
                    konekcija)
                {
                    CommandType = CommandType.StoredProcedure
                };

            komanda.Parameters.Add(
                "@JMBG",
                SqlDbType.NVarChar,
                13).Value = jmbg.Trim();

            komanda.Parameters.Add(
                "@RaftingTuraId",
                SqlDbType.Int).Value = raftingTuraId;

            komanda.Parameters.Add(
                "@PrijavaIdZaIzuzimanje",
                SqlDbType.Int).Value = prijavaIdZaIzuzimanje;

            var broj =
                Convert.ToInt32(
                    komanda.ExecuteScalar());

            return broj > 0;
        }   

        public int Dodaj(
            UcesnikPrijaveModel ucesnik,
            SqlConnection konekcija,
            SqlTransaction transakcija)
        {
            using var komanda =
                new SqlCommand(
                    "spDodajUcesnikaPrijave",
                    konekcija,
                    transakcija)
                {
                    CommandType = CommandType.StoredProcedure
                };

            DodajParametre(komanda, ucesnik);

            return Convert.ToInt32(komanda.ExecuteScalar());
        }

        public void ObrisiZaPrijavu(
            int prijavaId,
            SqlConnection konekcija,
            SqlTransaction transakcija)
        {
            using var komanda =
                new SqlCommand(
                    "spObrisiUcesnikePoPrijavaId",
                    konekcija,
                    transakcija)
                {
                    CommandType = CommandType.StoredProcedure
                };

            komanda.Parameters.AddWithValue("@PrijavaId", prijavaId);
            komanda.ExecuteNonQuery();
        }

        private static void DodajParametre(
            SqlCommand komanda,
            UcesnikPrijaveModel ucesnik)
        {
            komanda.Parameters.AddWithValue(
                "@PrijavaId",
                ucesnik.PrijavaId);

            komanda.Parameters.AddWithValue(
                "@RedniBroj",
                ucesnik.RedniBroj);

            komanda.Parameters.AddWithValue("@Ime", ucesnik.Ime);
            komanda.Parameters.AddWithValue("@Prezime", ucesnik.Prezime);
            komanda.Parameters.AddWithValue("@JMBG", ucesnik.JMBG);
            komanda.Parameters.AddWithValue(
                "@DatumRodjenja",
                ucesnik.DatumRodjenja.Date);

            komanda.Parameters.AddWithValue("@Adresa", ucesnik.Adresa);
            komanda.Parameters.AddWithValue(
                "@KontaktTelefon",
                ucesnik.KontaktTelefon);

            komanda.Parameters.AddWithValue("@Email", ucesnik.Email);
            komanda.Parameters.AddWithValue(
                "@ZnaDaPliva",
                ucesnik.ZnaDaPliva);

            komanda.Parameters.AddWithValue(
                "@PrihvatioIzjavuOdgovornosti",
                ucesnik.PrihvatioIzjavuOdgovornosti);

            komanda.Parameters.AddWithValue(
                "@LicnaKarta",
                ucesnik.LicnaKarta);

            komanda.Parameters.AddWithValue(
                "@PotvrdaOUplati",
                ucesnik.PotvrdaOUplati);

            komanda.Parameters.AddWithValue(
                "@PotpisanaIzjavaOdgovornosti",
                ucesnik.PotpisanaIzjavaOdgovornosti);

            komanda.Parameters.AddWithValue(
                "@SaglasnostRoditeljaStaratelja",
                ucesnik.SaglasnostRoditeljaStaratelja);

            komanda.Parameters.Add(
                    "@PotvrdioDonosenjeIdentifikacionogDokumenta",
                    SqlDbType.Bit)
                    .Value =
                ucesnik.PotvrdioDonosenjeIdentifikacionogDokumenta;

            komanda.Parameters.Add(
                "@PotpisanaIzjavaOriginalniNaziv",
                SqlDbType.NVarChar,
                255)
                .Value =
                    (object?)ucesnik.PotpisanaIzjavaOriginalniNaziv
                    ?? DBNull.Value;

            komanda.Parameters.Add(
                "@PotpisanaIzjavaSacuvaniNaziv",
                SqlDbType.NVarChar,
                255)
                .Value =
                    (object?)ucesnik.PotpisanaIzjavaSacuvaniNaziv
                    ?? DBNull.Value;

            komanda.Parameters.Add(
                "@SaglasnostOriginalniNaziv",
                SqlDbType.NVarChar,
                255)
                .Value =
                    (object?)ucesnik.SaglasnostOriginalniNaziv
                    ?? DBNull.Value;

            komanda.Parameters.Add(
                "@SaglasnostSacuvaniNaziv",
                SqlDbType.NVarChar,
                255)
                .Value =
                    (object?)ucesnik.SaglasnostSacuvaniNaziv
                    ?? DBNull.Value;

            komanda.Parameters.Add(
                "@IdentitetProveren",
                SqlDbType.Bit)
                .Value =
                    ucesnik.IdentitetProveren;

            komanda.Parameters.Add(
                "@DatumProvereIdentiteta",
                SqlDbType.DateTime2)
                .Value =
                    (object?)ucesnik.DatumProvereIdentiteta
                    ?? DBNull.Value;

            komanda.Parameters.Add(
                "@IdentitetProverioKorisnikId",
                SqlDbType.Int)
                .Value =
                    (object?)ucesnik.IdentitetProverioKorisnikId
                    ?? DBNull.Value;
        }

        private static UcesnikPrijaveModel Mapiraj(
            SqlDataReader citac)
        {
            return new UcesnikPrijaveModel
            {
                Id = citac.GetInt32(0),
                PrijavaId = citac.GetInt32(1),
                RedniBroj = citac.GetInt32(2),
                Ime = citac.GetString(3),
                Prezime = citac.GetString(4),
                JMBG = citac.GetString(5),
                DatumRodjenja = citac.GetDateTime(6),
                Adresa = citac.GetString(7),
                KontaktTelefon = citac.GetString(8),
                Email = citac.GetString(9),
                ZnaDaPliva = citac.GetBoolean(10),
                PrihvatioIzjavuOdgovornosti = citac.GetBoolean(11),
                LicnaKarta = citac.GetBoolean(12),
                PotvrdaOUplati = citac.GetBoolean(13),
                PotpisanaIzjavaOdgovornosti = citac.GetBoolean(14),
                SaglasnostRoditeljaStaratelja = citac.GetBoolean(15),
                PotvrdioDonosenjeIdentifikacionogDokumenta = citac.GetBoolean(16),
                PotpisanaIzjavaOriginalniNaziv = citac.IsDBNull(17) ? null : citac.GetString(17),
                PotpisanaIzjavaSacuvaniNaziv = citac.IsDBNull(18) ? null : citac.GetString(18),
                SaglasnostOriginalniNaziv = citac.IsDBNull(19) ? null : citac.GetString(19),
                SaglasnostSacuvaniNaziv =  citac.IsDBNull(20) ? null : citac.GetString(20),
                IdentitetProveren = citac.GetBoolean(21),
                DatumProvereIdentiteta = citac.IsDBNull(22) ? null : citac.GetDateTime(22),
                IdentitetProverioKorisnikId = citac.IsDBNull(23) ? null : citac.GetInt32(23)
            };
        }
    }
}
