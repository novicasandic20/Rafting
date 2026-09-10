using System.Data;
using BibliotekaKlasa.KlasePodataka.Modeli;
using BibliotekaKlasa.TehnoloskeKlase;
using Microsoft.Data.SqlClient;

namespace BibliotekaKlasa.KlasePodataka.Repozitorijumi
{
    // Primer rada nasleđivanjem klase TabelaKlasa i izvršavanjem SQL upita.
    public class RaftingTuraRepo : TabelaKlasa
    {
        public RaftingTuraRepo(KonekcijaKlasa konekcijaObjekat)
            : base(konekcijaObjekat)
        {
        }

        public List<RaftingTuraModel> DajSve(
            string? filter = null,
            bool samoAktivne = false)
        {
            var uslovi = new List<string>();
            var parametri = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                uslovi.Add(
                    "(rt.NazivTure LIKE @Filter OR " +
                    "rt.LokacijaReka LIKE @Filter OR nt.Naziv LIKE @Filter)");

                parametri.Add(
                    new SqlParameter("@Filter", $"%{filter.Trim()}%"));
            }

            if (samoAktivne)
            {
                uslovi.Add("rt.Aktivna = 1");
            }

            var where = uslovi.Count > 0
                ? "WHERE " + string.Join(" AND ", uslovi)
                : string.Empty;

            var upit = $"""
                SELECT
                    rt.Id,
                    rt.NazivTure,
                    rt.DatumOdrzavanja,
                    rt.LokacijaReka,
                    rt.NivoTezineId,
                    nt.Naziv,
                    nt.Opis,
                    rt.MaksimalanBrojUcesnika,
                    rt.Cena,
                    rt.Aktivna,
                    COALESCE(SUM(
                        CASE WHEN p.StatusPrijave IN (N'Kreirana', N'Potvrđena')
                                  AND up.Id IS NOT NULL
                             THEN 1 ELSE 0 END
                    ), 0) AS BrojPrijavljenihUcesnika
                FROM RaftingTure rt
                INNER JOIN NivoiTezine nt ON rt.NivoTezineId = nt.Id
                LEFT JOIN Prijave p ON p.RaftingTuraId = rt.Id
                LEFT JOIN UcesniciPrijave up ON up.PrijavaId = p.Id
                {where}
                GROUP BY
                    rt.Id, rt.NazivTure, rt.DatumOdrzavanja, rt.LokacijaReka,
                    rt.NivoTezineId, nt.Naziv, nt.Opis,
                    rt.MaksimalanBrojUcesnika, rt.Cena, rt.Aktivna
                ORDER BY rt.DatumOdrzavanja, rt.NazivTure
                """;

            var tabela = DajPodatke(upit, parametri.ToArray());
            return tabela.Rows
                .Cast<DataRow>()
                .Select(Mapiraj)
                .ToList();
        }

        public RaftingTuraModel? DajPoId(int id)
        {
            const string upit = """
                SELECT
                    rt.Id,
                    rt.NazivTure,
                    rt.DatumOdrzavanja,
                    rt.LokacijaReka,
                    rt.NivoTezineId,
                    nt.Naziv,
                    nt.Opis,
                    rt.MaksimalanBrojUcesnika,
                    rt.Cena,
                    rt.Aktivna,
                    COALESCE(SUM(
                        CASE WHEN p.StatusPrijave IN (N'Kreirana', N'Potvrđena')
                                  AND up.Id IS NOT NULL
                             THEN 1 ELSE 0 END
                    ), 0) AS BrojPrijavljenihUcesnika
                FROM RaftingTure rt
                INNER JOIN NivoiTezine nt ON rt.NivoTezineId = nt.Id
                LEFT JOIN Prijave p ON p.RaftingTuraId = rt.Id
                LEFT JOIN UcesniciPrijave up ON up.PrijavaId = p.Id
                WHERE rt.Id = @Id
                GROUP BY
                    rt.Id, rt.NazivTure, rt.DatumOdrzavanja, rt.LokacijaReka,
                    rt.NivoTezineId, nt.Naziv, nt.Opis,
                    rt.MaksimalanBrojUcesnika, rt.Cena, rt.Aktivna
                """;

            var tabela = DajPodatke(
                upit,
                new SqlParameter("@Id", id));

            if (tabela.Rows.Count == 0)
            {
                return null;
            }

            return Mapiraj(tabela.Rows[0]);
        }

        public int Dodaj(RaftingTuraModel tura)
        {
            const string upit = """
                INSERT INTO RaftingTure
                    (NazivTure, DatumOdrzavanja, LokacijaReka,
                     NivoTezineId, MaksimalanBrojUcesnika, Cena, Aktivna)
                OUTPUT INSERTED.Id
                VALUES
                    (@NazivTure, @DatumOdrzavanja, @LokacijaReka,
                     @NivoTezineId, @MaksimalanBrojUcesnika, @Cena, @Aktivna)
                """;

            var rezultat = IzvrsiSkalar(
                upit,
                new SqlParameter("@NazivTure", tura.NazivTure),
                new SqlParameter("@DatumOdrzavanja", tura.DatumOdrzavanja.Date),
                new SqlParameter("@LokacijaReka", tura.LokacijaReka),
                new SqlParameter("@NivoTezineId", tura.NivoTezineId),
                new SqlParameter(
                    "@MaksimalanBrojUcesnika",
                    tura.MaksimalanBrojUcesnika),
                new SqlParameter("@Cena", tura.Cena),
                new SqlParameter("@Aktivna", tura.Aktivna));

            return Convert.ToInt32(rezultat);
        }

        public void Izmeni(RaftingTuraModel tura)
        {
            const string upit = """
                UPDATE RaftingTure
                SET
                    NazivTure = @NazivTure,
                    DatumOdrzavanja = @DatumOdrzavanja,
                    LokacijaReka = @LokacijaReka,
                    NivoTezineId = @NivoTezineId,
                    MaksimalanBrojUcesnika = @MaksimalanBrojUcesnika,
                    Cena = @Cena,
                    Aktivna = @Aktivna
                WHERE Id = @Id
                """;

            IzvrsiAzuriranje(
                upit,
                new SqlParameter("@Id", tura.Id),
                new SqlParameter("@NazivTure", tura.NazivTure),
                new SqlParameter("@DatumOdrzavanja", tura.DatumOdrzavanja.Date),
                new SqlParameter("@LokacijaReka", tura.LokacijaReka),
                new SqlParameter("@NivoTezineId", tura.NivoTezineId),
                new SqlParameter(
                    "@MaksimalanBrojUcesnika",
                    tura.MaksimalanBrojUcesnika),
                new SqlParameter("@Cena", tura.Cena),
                new SqlParameter("@Aktivna", tura.Aktivna));
        }

        public void Obrisi(int id)
        {
            const string upit = "DELETE FROM RaftingTure WHERE Id = @Id";
            IzvrsiAzuriranje(
                upit,
                new SqlParameter("@Id", id));
        }

        private static RaftingTuraModel Mapiraj(DataRow red)
        {
            return new RaftingTuraModel
            {
                Id = Convert.ToInt32(red[0]),
                NazivTure = Convert.ToString(red[1]) ?? string.Empty,
                DatumOdrzavanja = Convert.ToDateTime(red[2]),
                LokacijaReka = Convert.ToString(red[3]) ?? string.Empty,
                NivoTezineId = Convert.ToInt32(red[4]),
                NivoTezineObjekat = new NivoTezineModel
                {
                    Id = Convert.ToInt32(red[4]),
                    Naziv = Convert.ToString(red[5]) ?? string.Empty,
                    Opis = red.IsNull(6) ? null : Convert.ToString(red[6])
                },
                MaksimalanBrojUcesnika = Convert.ToInt32(red[7]),
                Cena = Convert.ToDecimal(red[8]),
                Aktivna = Convert.ToBoolean(red[9]),
                BrojPrijavljenihUcesnika = Convert.ToInt32(red[10])
            };
        }
    }
}
