USE RaftingTureDB;
GO
/*
    MIGRACIONA SKRIPTA

    Ovaj fajl je korišćen tokom razvoja za dopunu postojeće baze.
    Za novu instalaciju nije potrebno njegovo izvršavanje,
    jer su sva polja uključena u 01_BazaPodataka.sql.
*/

IF COL_LENGTH('dbo.Prijave', 'PotvrdaUplateOriginalniNaziv') IS NULL
BEGIN
    ALTER TABLE dbo.Prijave
    ADD PotvrdaUplateOriginalniNaziv NVARCHAR(255) NULL;
END
GO

IF COL_LENGTH('dbo.Prijave', 'PotvrdaUplateSacuvaniNaziv') IS NULL
BEGIN
    ALTER TABLE dbo.Prijave
    ADD PotvrdaUplateSacuvaniNaziv NVARCHAR(255) NULL;
END
GO

IF COL_LENGTH('dbo.Prijave', 'DatumOdobrenja') IS NULL
BEGIN
    ALTER TABLE dbo.Prijave
    ADD DatumOdobrenja DATETIME2 NULL;
END
GO

IF COL_LENGTH('dbo.Prijave', 'OdobrioKorisnikId') IS NULL
BEGIN
    ALTER TABLE dbo.Prijave
    ADD OdobrioKorisnikId INT NULL;
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Prijave_OdobrioKorisnik'
)
BEGIN
    ALTER TABLE dbo.Prijave
    ADD CONSTRAINT FK_Prijave_OdobrioKorisnik
        FOREIGN KEY (OdobrioKorisnikId)
        REFERENCES dbo.Korisnici(Id);
END
GO

/* Dodavanje statusa Odbijena */

IF EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Prijave_Status'
)
BEGIN
    ALTER TABLE dbo.Prijave
    DROP CONSTRAINT CK_Prijave_Status;
END
GO

ALTER TABLE dbo.Prijave
ADD CONSTRAINT CK_Prijave_Status
CHECK
(
    StatusPrijave IN
    (
        N'Kreirana',
        N'Potvrđena',
        N'Odbijena',
        N'Otkazana'
    )
);
GO

/* =========================================================
   DODATNA POLJA ZA UČESNIKA
   ========================================================= */

IF COL_LENGTH(
    'dbo.UcesniciPrijave',
    'PotvrdioDonosenjeIdentifikacionogDokumenta'
) IS NULL
BEGIN
    ALTER TABLE dbo.UcesniciPrijave
    ADD PotvrdioDonosenjeIdentifikacionogDokumenta BIT NOT NULL
        CONSTRAINT DF_Ucesnici_PotvrdioDokument DEFAULT (0);
END
GO

IF COL_LENGTH(
    'dbo.UcesniciPrijave',
    'PotpisanaIzjavaOriginalniNaziv'
) IS NULL
BEGIN
    ALTER TABLE dbo.UcesniciPrijave
    ADD PotpisanaIzjavaOriginalniNaziv NVARCHAR(255) NULL;
END
GO

IF COL_LENGTH(
    'dbo.UcesniciPrijave',
    'PotpisanaIzjavaSacuvaniNaziv'
) IS NULL
BEGIN
    ALTER TABLE dbo.UcesniciPrijave
    ADD PotpisanaIzjavaSacuvaniNaziv NVARCHAR(255) NULL;
END
GO

IF COL_LENGTH(
    'dbo.UcesniciPrijave',
    'SaglasnostOriginalniNaziv'
) IS NULL
BEGIN
    ALTER TABLE dbo.UcesniciPrijave
    ADD SaglasnostOriginalniNaziv NVARCHAR(255) NULL;
END
GO

IF COL_LENGTH(
    'dbo.UcesniciPrijave',
    'SaglasnostSacuvaniNaziv'
) IS NULL
BEGIN
    ALTER TABLE dbo.UcesniciPrijave
    ADD SaglasnostSacuvaniNaziv NVARCHAR(255) NULL;
END
GO

IF COL_LENGTH(
    'dbo.UcesniciPrijave',
    'IdentitetProveren'
) IS NULL
BEGIN
    ALTER TABLE dbo.UcesniciPrijave
    ADD IdentitetProveren BIT NOT NULL
        CONSTRAINT DF_Ucesnici_IdentitetProveren DEFAULT (0);
END
GO

IF COL_LENGTH(
    'dbo.UcesniciPrijave',
    'DatumProvereIdentiteta'
) IS NULL
BEGIN
    ALTER TABLE dbo.UcesniciPrijave
    ADD DatumProvereIdentiteta DATETIME2 NULL;
END
GO

IF COL_LENGTH(
    'dbo.UcesniciPrijave',
    'IdentitetProverioKorisnikId'
) IS NULL
BEGIN
    ALTER TABLE dbo.UcesniciPrijave
    ADD IdentitetProverioKorisnikId INT NULL;
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Ucesnici_IdentitetProverioKorisnik'
)
BEGIN
    ALTER TABLE dbo.UcesniciPrijave
    ADD CONSTRAINT FK_Ucesnici_IdentitetProverioKorisnik
        FOREIGN KEY (IdentitetProverioKorisnikId)
        REFERENCES dbo.Korisnici(Id);
END
GO