using System.Data;
using BibliotekaKlasa.KlasePodataka.Modeli;
using BibliotekaKlasa.TehnoloskeKlase;
using Microsoft.Data.SqlClient;

namespace BibliotekaKlasa.KlasePodataka.Repozitorijumi
{
    // Glavna tabela dokumenta. Repository nasleđuje TabelaKlasa.
    public class PrijavaRepo : TabelaKlasa
    {
        public PrijavaRepo(KonekcijaKlasa konekcijaObjekat)
            : base(konekcijaObjekat)
        {
        }

        public List<PrijavaModel> DajSve(string? filter = null)
        {
            var where = string.Empty;
            var parametri = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                where = """
            WHERE
                p.BrojPrijave LIKE @Filter OR
                rt.NazivTure LIKE @Filter OR
                rt.LokacijaReka LIKE @Filter OR
                EXISTS
                (
                    SELECT 1
                    FROM UcesniciPrijave uf
                    WHERE uf.PrijavaId = p.Id
                      AND
                      (
                          uf.Ime LIKE @Filter OR
                          uf.Prezime LIKE @Filter OR
                          uf.JMBG LIKE @Filter
                      )
                )
            """;

                parametri.Add(
                    new SqlParameter(
                        "@Filter",
                        $"%{filter.Trim()}%"));
            }

            var upit = $"""
        SELECT
            p.Id,
            p.BrojPrijave,
            p.DatumPrijave,
            p.RaftingTuraId,
            p.KorisnikId,
            p.StatusPrijave,

            rt.NazivTure,
            rt.DatumOdrzavanja,
            rt.LokacijaReka,
            rt.NivoTezineId,

            nt.Naziv,
            nt.Opis,

            rt.MaksimalanBrojUcesnika,
            rt.Cena,
            rt.Aktivna,

            k.Ime,
            k.Prezime,
            k.Email,
            k.Uloga,

            COUNT(DISTINCT up.Id) AS BrojUcesnika,

            p.PotvrdaUplateOriginalniNaziv,
            p.PotvrdaUplateSacuvaniNaziv,
            p.DatumOdobrenja,
            p.OdobrioKorisnikId

        FROM Prijave p

        INNER JOIN RaftingTure rt
            ON p.RaftingTuraId = rt.Id

        INNER JOIN NivoiTezine nt
            ON rt.NivoTezineId = nt.Id

        INNER JOIN Korisnici k
            ON p.KorisnikId = k.Id

        LEFT JOIN UcesniciPrijave up
            ON p.Id = up.PrijavaId

        {where}

        GROUP BY
            p.Id,
            p.BrojPrijave,
            p.DatumPrijave,
            p.RaftingTuraId,
            p.KorisnikId,
            p.StatusPrijave,

            rt.NazivTure,
            rt.DatumOdrzavanja,
            rt.LokacijaReka,
            rt.NivoTezineId,

            nt.Naziv,
            nt.Opis,

            rt.MaksimalanBrojUcesnika,
            rt.Cena,
            rt.Aktivna,

            k.Ime,
            k.Prezime,
            k.Email,
            k.Uloga,

            p.PotvrdaUplateOriginalniNaziv,
            p.PotvrdaUplateSacuvaniNaziv,
            p.DatumOdobrenja,
            p.OdobrioKorisnikId

        ORDER BY
            p.DatumPrijave DESC,
            p.Id DESC
        """;

            var tabela =
                DajPodatke(
                    upit,
                    parametri.ToArray());

            return tabela.Rows
                .Cast<DataRow>()
                .Select(Mapiraj)
                .ToList();
        }

        public PrijavaModel? DajPoId(int id)
        {
            const string upit = """
                SELECT
                    p.Id,
                    p.BrojPrijave,
                    p.DatumPrijave,
                    p.RaftingTuraId,
                    p.KorisnikId,
                    p.StatusPrijave,
                    rt.NazivTure,
                    rt.DatumOdrzavanja,
                    rt.LokacijaReka,
                    rt.NivoTezineId,
                    nt.Naziv,
                    nt.Opis,
                    rt.MaksimalanBrojUcesnika,
                    rt.Cena,
                    rt.Aktivna,
                    k.Ime,
                    k.Prezime,
                    k.Email,
                    k.Uloga,
                    COUNT(up.Id) AS BrojUcesnika,
                    p.PotvrdaUplateOriginalniNaziv,
                    p.PotvrdaUplateSacuvaniNaziv,
                    p.DatumOdobrenja,
                    p.OdobrioKorisnikId
                FROM Prijave p
                INNER JOIN RaftingTure rt ON p.RaftingTuraId = rt.Id
                INNER JOIN NivoiTezine nt ON rt.NivoTezineId = nt.Id
                INNER JOIN Korisnici k ON p.KorisnikId = k.Id
                LEFT JOIN UcesniciPrijave up ON p.Id = up.PrijavaId
                WHERE p.Id = @Id
                GROUP BY
                    p.Id, p.BrojPrijave, p.DatumPrijave,
                    p.RaftingTuraId, p.KorisnikId, p.StatusPrijave,
                    rt.NazivTure, rt.DatumOdrzavanja, rt.LokacijaReka,
                    rt.NivoTezineId, nt.Naziv, nt.Opis,
                    rt.MaksimalanBrojUcesnika, rt.Cena, rt.Aktivna,
                    k.Ime, k.Prezime, k.Email, k.Uloga,
                    p.PotvrdaUplateOriginalniNaziv,
                    p.PotvrdaUplateSacuvaniNaziv,
                    p.DatumOdobrenja,
                    p.OdobrioKorisnikId
                """;

            var tabela = DajPodatke(
                upit,
                new SqlParameter("@Id", id));

            return tabela.Rows.Count == 0
                ? null
                : Mapiraj(tabela.Rows[0]);
        }

        public int Dodaj(
            PrijavaModel prijava,
            SqlConnection konekcija,
            SqlTransaction transakcija)
        {
            const string insertUpit = """
                    INSERT INTO Prijave
                        (
                            BrojPrijave,
                            DatumPrijave,
                            RaftingTuraId,
                            KorisnikId,
                            StatusPrijave,
                            PotvrdaUplateOriginalniNaziv,
                            PotvrdaUplateSacuvaniNaziv
                        )
                    OUTPUT INSERTED.Id
                    VALUES
                        (
                            @PrivremeniBroj,
                            @DatumPrijave,
                            @RaftingTuraId,
                            @KorisnikId,
                            @StatusPrijave,
                            @PotvrdaUplateOriginalniNaziv,
                            @PotvrdaUplateSacuvaniNaziv
                        )
                    """;

            using var insertKomanda =
                new SqlCommand(insertUpit, konekcija, transakcija);

            var privremeniBroj =
                $"TEMP-{Guid.NewGuid():N}".Substring(0, 30);

            insertKomanda.Parameters
                .Add(
                    "@PrivremeniBroj",
                    System.Data.SqlDbType.NVarChar,
                    30)
                .Value = privremeniBroj;

            insertKomanda.Parameters.AddWithValue(
                "@DatumPrijave",
                prijava.DatumPrijave.Date);

            insertKomanda.Parameters.AddWithValue(
                "@RaftingTuraId",
                prijava.RaftingTuraId);

            insertKomanda.Parameters.AddWithValue(
                "@KorisnikId",
                prijava.KorisnikId);

            insertKomanda.Parameters.AddWithValue(
                "@StatusPrijave",
                prijava.StatusPrijave);

            insertKomanda.Parameters.Add(
                "@PotvrdaUplateOriginalniNaziv",
                SqlDbType.NVarChar,
                255)
                .Value =
                    (object?)prijava.PotvrdaUplateOriginalniNaziv
                    ?? DBNull.Value;

            insertKomanda.Parameters.Add(
                "@PotvrdaUplateSacuvaniNaziv",
                SqlDbType.NVarChar,
                255)
                .Value =
                    (object?)prijava.PotvrdaUplateSacuvaniNaziv
                    ?? DBNull.Value;

            var id = Convert.ToInt32(insertKomanda.ExecuteScalar());
            var brojPrijave =
                $"PR-{prijava.DatumPrijave:yyyy}-{id:000000}";

            const string updateUpit = """
                UPDATE Prijave
                SET BrojPrijave = @BrojPrijave
                WHERE Id = @Id
                """;

            using var updateKomanda =
                new SqlCommand(updateUpit, konekcija, transakcija);

            updateKomanda.Parameters.AddWithValue(
                "@BrojPrijave",
                brojPrijave);

            updateKomanda.Parameters.AddWithValue("@Id", id);
            updateKomanda.ExecuteNonQuery();

            prijava.Id = id;
            prijava.BrojPrijave = brojPrijave;

            return id;
        }

        public void Izmeni(
            PrijavaModel prijava,
            SqlConnection konekcija,
            SqlTransaction transakcija)
        {
            const string upit = """
                UPDATE Prijave
                SET
                    DatumPrijave = @DatumPrijave,
                    RaftingTuraId = @RaftingTuraId,
                    StatusPrijave = @StatusPrijave,
                    PotvrdaUplateOriginalniNaziv =
                        @PotvrdaUplateOriginalniNaziv,
                    PotvrdaUplateSacuvaniNaziv =
                        @PotvrdaUplateSacuvaniNaziv
                WHERE Id = @Id
                """;

            using var komanda =
                new SqlCommand(upit, konekcija, transakcija);

            komanda.Parameters.AddWithValue("@Id", prijava.Id);
            komanda.Parameters.AddWithValue(
                "@DatumPrijave",
                prijava.DatumPrijave.Date);

            komanda.Parameters.AddWithValue(
                "@RaftingTuraId",
                prijava.RaftingTuraId);

            komanda.Parameters.AddWithValue(
                "@StatusPrijave",
                prijava.StatusPrijave);

            komanda.Parameters.Add(
                "@PotvrdaUplateOriginalniNaziv",
                SqlDbType.NVarChar,
                255)
                .Value =
                    (object?)prijava.PotvrdaUplateOriginalniNaziv
                    ?? DBNull.Value;

            komanda.Parameters.Add(
                "@PotvrdaUplateSacuvaniNaziv",
                SqlDbType.NVarChar,
                255)
                .Value =
                    (object?)prijava.PotvrdaUplateSacuvaniNaziv
                    ?? DBNull.Value;

            komanda.ExecuteNonQuery();
        }

        public void Obrisi(int id)
        {
            const string upit = "DELETE FROM Prijave WHERE Id = @Id";

            IzvrsiAzuriranje(
                upit,
                new SqlParameter("@Id", id));
        }

        public int DajBrojPrijavljenihUcesnika(
            int raftingTuraId,
            IReadOnlyCollection<string> statusiKojiSeRacunaju,
            int prijavaIdZaIzuzimanje = 0)
        {
            var statusi = statusiKojiSeRacunaju.Count > 0
                ? statusiKojiSeRacunaju
                : new[] { "Kreirana", "Potvrđena" };

            var statusParametri = statusi
                .Select((_, indeks) => $"@Status{indeks}")
                .ToList();

            var upit = $"""
                SELECT COUNT(up.Id)
                FROM UcesniciPrijave up
                INNER JOIN Prijave p ON up.PrijavaId = p.Id
                WHERE
                    p.RaftingTuraId = @RaftingTuraId
                    AND p.StatusPrijave IN ({string.Join(", ", statusParametri)})
                    AND (@PrijavaIdZaIzuzimanje = 0
                         OR p.Id <> @PrijavaIdZaIzuzimanje)
                """;

            var parametri = new List<SqlParameter>
            {
                new("@RaftingTuraId", raftingTuraId),
                new("@PrijavaIdZaIzuzimanje", prijavaIdZaIzuzimanje)
            };

            parametri.AddRange(
                statusi.Select(
                    (status, indeks) =>
                        new SqlParameter($"@Status{indeks}", status)));

            var rezultat = IzvrsiSkalar(upit, parametri.ToArray());
            return Convert.ToInt32(rezultat);
        }

        private static PrijavaModel Mapiraj(DataRow red)
        {
            return new PrijavaModel
            {
                Id = Convert.ToInt32(red[0]),
                BrojPrijave = Convert.ToString(red[1]) ?? string.Empty,
                DatumPrijave = Convert.ToDateTime(red[2]),
                RaftingTuraId = Convert.ToInt32(red[3]),
                KorisnikId = Convert.ToInt32(red[4]),
                StatusPrijave = Convert.ToString(red[5]) ?? string.Empty,
                RaftingTuraObjekat = new RaftingTuraModel
                {
                    Id = Convert.ToInt32(red[3]),
                    NazivTure = Convert.ToString(red[6]) ?? string.Empty,
                    DatumOdrzavanja = Convert.ToDateTime(red[7]),
                    LokacijaReka = Convert.ToString(red[8]) ?? string.Empty,
                    NivoTezineId = Convert.ToInt32(red[9]),
                    NivoTezineObjekat = new NivoTezineModel
                    {
                        Id = Convert.ToInt32(red[9]),
                        Naziv = Convert.ToString(red[10]) ?? string.Empty,
                        Opis = red.IsNull(11)
                            ? null
                            : Convert.ToString(red[11])
                    },
                    MaksimalanBrojUcesnika = Convert.ToInt32(red[12]),
                    Cena = Convert.ToDecimal(red[13]),
                    Aktivna = Convert.ToBoolean(red[14])
                },
                KorisnikObjekat = new KorisnikModel
                {
                    Id = Convert.ToInt32(red[4]),
                    Ime = Convert.ToString(red[15]) ?? string.Empty,
                    Prezime = Convert.ToString(red[16]) ?? string.Empty,
                    Email = Convert.ToString(red[17]) ?? string.Empty,
                    Uloga = Convert.ToString(red[18]) ?? string.Empty
                },
                BrojUcesnikaIzUpita = Convert.ToInt32(red[19]),

                PotvrdaUplateOriginalniNaziv =
                    red.IsNull("PotvrdaUplateOriginalniNaziv")
                        ? null
                        : Convert.ToString(
                            red["PotvrdaUplateOriginalniNaziv"]),

                 PotvrdaUplateSacuvaniNaziv =
                    red.IsNull("PotvrdaUplateSacuvaniNaziv")
                        ? null
                        : Convert.ToString(
                            red["PotvrdaUplateSacuvaniNaziv"]),

                 DatumOdobrenja =
                    red.IsNull("DatumOdobrenja")
                        ? null
                        : Convert.ToDateTime(
                            red["DatumOdobrenja"]),

                 OdobrioKorisnikId =
                    red.IsNull("OdobrioKorisnikId")
                        ? null
                        : Convert.ToInt32(
                            red["OdobrioKorisnikId"])
            };
        }

        public void PromeniStatus(
    int prijavaId,
    string noviStatus,
    int adminKorisnikId)
        {
            const string upit = """
        UPDATE Prijave
        SET
            StatusPrijave = @StatusPrijave,
            DatumOdobrenja =
                CASE
                    WHEN @StatusPrijave = N'Potvrđena'
                    THEN SYSDATETIME()
                    ELSE NULL
                END,
            OdobrioKorisnikId =
                CASE
                    WHEN @StatusPrijave = N'Potvrđena'
                    THEN @AdminKorisnikId
                    ELSE NULL
                END
        WHERE Id = @PrijavaId
        """;

            IzvrsiAzuriranje(
                upit,
                new SqlParameter("@StatusPrijave", noviStatus),
                new SqlParameter("@AdminKorisnikId", adminKorisnikId),
                new SqlParameter("@PrijavaId", prijavaId));
        }

    }
}
