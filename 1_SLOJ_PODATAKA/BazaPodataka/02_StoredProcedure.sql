USE RaftingTureDB;
GO

CREATE OR ALTER VIEW vwPrijaveZaStampu
AS
SELECT
    p.Id,
    p.BrojPrijave,
    p.DatumPrijave,
    p.StatusPrijave,
    rt.NazivTure,
    rt.DatumOdrzavanja,
    rt.LokacijaReka,
    nt.Naziv AS NivoTezine,
    rt.MaksimalanBrojUcesnika,
    rt.Cena,
    COUNT(up.Id) AS BrojUcesnika
FROM Prijave p
INNER JOIN RaftingTure rt ON p.RaftingTuraId = rt.Id
INNER JOIN NivoiTezine nt ON rt.NivoTezineId = nt.Id
LEFT JOIN UcesniciPrijave up ON p.Id = up.PrijavaId
GROUP BY
    p.Id, p.BrojPrijave, p.DatumPrijave, p.StatusPrijave,
    rt.NazivTure, rt.DatumOdrzavanja, rt.LokacijaReka,
    nt.Naziv, rt.MaksimalanBrojUcesnika, rt.Cena;
GO

CREATE OR ALTER PROCEDURE spDajUcesnikePoPrijavaId
    @PrijavaId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        PrijavaId,
        RedniBroj,
        Ime,
        Prezime,
        JMBG,
        DatumRodjenja,
        Adresa,
        KontaktTelefon,
        Email,
        ZnaDaPliva,
        PrihvatioIzjavuOdgovornosti,
        LicnaKarta,
        PotvrdaOUplati,
        PotpisanaIzjavaOdgovornosti,
        SaglasnostRoditeljaStaratelja,
        PotvrdioDonosenjeIdentifikacionogDokumenta,
        PotpisanaIzjavaOriginalniNaziv,
        PotpisanaIzjavaSacuvaniNaziv,
        SaglasnostOriginalniNaziv,
        SaglasnostSacuvaniNaziv,
        IdentitetProveren,
        DatumProvereIdentiteta,
        IdentitetProverioKorisnikId
    FROM UcesniciPrijave
    WHERE PrijavaId = @PrijavaId
    ORDER BY RedniBroj;
END
GO

CREATE OR ALTER PROCEDURE spDodajUcesnikaPrijave
    @PrijavaId INT,
    @RedniBroj INT,
    @Ime NVARCHAR(50),
    @Prezime NVARCHAR(50),
    @JMBG CHAR(13),
    @DatumRodjenja DATE,
    @Adresa NVARCHAR(150),
    @KontaktTelefon NVARCHAR(30),
    @Email NVARCHAR(128),
    @ZnaDaPliva BIT,
    @PrihvatioIzjavuOdgovornosti BIT,
    @LicnaKarta BIT,
    @PotvrdaOUplati BIT,
    @PotpisanaIzjavaOdgovornosti BIT,
    @SaglasnostRoditeljaStaratelja BIT,
    @PotvrdioDonosenjeIdentifikacionogDokumenta BIT,
    @PotpisanaIzjavaOriginalniNaziv NVARCHAR(255),
    @PotpisanaIzjavaSacuvaniNaziv NVARCHAR(255),
    @SaglasnostOriginalniNaziv NVARCHAR(255),
    @SaglasnostSacuvaniNaziv NVARCHAR(255),
    @IdentitetProveren BIT,
    @DatumProvereIdentiteta DATETIME2,
    @IdentitetProverioKorisnikId INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO UcesniciPrijave
    (
        PrijavaId, RedniBroj, Ime, Prezime, JMBG, DatumRodjenja,
        Adresa, KontaktTelefon, Email, ZnaDaPliva,
        PrihvatioIzjavuOdgovornosti, LicnaKarta, PotvrdaOUplati,
        PotpisanaIzjavaOdgovornosti, SaglasnostRoditeljaStaratelja, 
        PotvrdioDonosenjeIdentifikacionogDokumenta, PotpisanaIzjavaOriginalniNaziv,
        PotpisanaIzjavaSacuvaniNaziv, SaglasnostOriginalniNaziv,
        SaglasnostSacuvaniNaziv, IdentitetProveren,
        DatumProvereIdentiteta, IdentitetProverioKorisnikId
    )
    VALUES
    (
        @PrijavaId, @RedniBroj, @Ime, @Prezime, @JMBG, @DatumRodjenja,
        @Adresa, @KontaktTelefon, @Email, @ZnaDaPliva,
        @PrihvatioIzjavuOdgovornosti, @LicnaKarta, @PotvrdaOUplati,
        @PotpisanaIzjavaOdgovornosti, @SaglasnostRoditeljaStaratelja,
        @PotvrdioDonosenjeIdentifikacionogDokumenta, @PotpisanaIzjavaOriginalniNaziv,
        @PotpisanaIzjavaSacuvaniNaziv, @SaglasnostOriginalniNaziv,
        @SaglasnostSacuvaniNaziv, @IdentitetProveren,
        @DatumProvereIdentiteta, @IdentitetProverioKorisnikId
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE spObrisiUcesnikePoPrijavaId
    @PrijavaId INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM UcesniciPrijave
    WHERE PrijavaId = @PrijavaId;
END
GO

CREATE OR ALTER PROCEDURE spObrisiUcesnikaPrijave
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM UcesniciPrijave
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE spPostojiJMBGNaTuri
    @JMBG NVARCHAR(13),
    @RaftingTuraId INT,
    @PrijavaIdZaIzuzimanje INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*)
    FROM UcesniciPrijave up
    INNER JOIN Prijave p
        ON up.PrijavaId = p.Id
    WHERE
        up.JMBG = @JMBG
        AND p.RaftingTuraId = @RaftingTuraId
        AND p.StatusPrijave IN (N'Kreirana', N'Potvrđena')
        AND (
            @PrijavaIdZaIzuzimanje = 0
            OR p.Id <> @PrijavaIdZaIzuzimanje
        );
END;
GO