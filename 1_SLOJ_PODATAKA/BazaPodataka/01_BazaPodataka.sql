CREATE DATABASE RaftingTureDB;

USE RaftingTureDB;
GO

IF OBJECT_ID(N'dbo.UcesniciPrijave', N'U') IS NOT NULL DROP TABLE dbo.UcesniciPrijave;
IF OBJECT_ID(N'dbo.Prijave', N'U') IS NOT NULL DROP TABLE dbo.Prijave;
IF OBJECT_ID(N'dbo.RaftingTure', N'U') IS NOT NULL DROP TABLE dbo.RaftingTure;
IF OBJECT_ID(N'dbo.NivoiTezine', N'U') IS NOT NULL DROP TABLE dbo.NivoiTezine;
IF OBJECT_ID(N'dbo.Korisnici', N'U') IS NOT NULL DROP TABLE dbo.Korisnici;
GO

CREATE TABLE Korisnici
(
    Id INT IDENTITY(1,1) CONSTRAINT PK_Korisnici PRIMARY KEY,
    Ime NVARCHAR(40) NOT NULL,
    Prezime NVARCHAR(40) NOT NULL,
    Email NVARCHAR(128) NOT NULL CONSTRAINT UQ_Korisnici_Email UNIQUE,
    LozinkaHash NVARCHAR(512) NOT NULL,
    LozinkaSalt NVARCHAR(512) NOT NULL,
    Uloga NVARCHAR(32) NOT NULL
        CONSTRAINT CK_Korisnici_Uloga CHECK (Uloga IN (N'Admin', N'Korisnik'))
);
GO

CREATE TABLE NivoiTezine
(
    Id INT IDENTITY(1,1) CONSTRAINT PK_NivoiTezine PRIMARY KEY,
    Naziv NVARCHAR(50) NOT NULL CONSTRAINT UQ_NivoiTezine_Naziv UNIQUE,
    Opis NVARCHAR(250) NULL
);
GO

CREATE TABLE RaftingTure
(
    Id INT IDENTITY(1,1) CONSTRAINT PK_RaftingTure PRIMARY KEY,
    NazivTure NVARCHAR(100) NOT NULL,
    DatumOdrzavanja DATE NOT NULL,
    LokacijaReka NVARCHAR(100) NOT NULL,
    NivoTezineId INT NOT NULL,
    MaksimalanBrojUcesnika INT NOT NULL,
    Cena DECIMAL(12,2) NOT NULL,
    Aktivna BIT NOT NULL CONSTRAINT DF_RaftingTure_Aktivna DEFAULT (1),

    CONSTRAINT FK_RaftingTure_NivoiTezine
        FOREIGN KEY (NivoTezineId) REFERENCES NivoiTezine(Id),

    CONSTRAINT CK_RaftingTure_Kapacitet
        CHECK (MaksimalanBrojUcesnika BETWEEN 1 AND 500),

    CONSTRAINT CK_RaftingTure_Cena
        CHECK (Cena >= 0)
);
GO

CREATE TABLE Prijave
(
    Id INT IDENTITY(1,1)
        CONSTRAINT PK_Prijave PRIMARY KEY,

    BrojPrijave NVARCHAR(30) NOT NULL
        CONSTRAINT UQ_Prijave_BrojPrijave UNIQUE,

    DatumPrijave DATE NOT NULL,

    RaftingTuraId INT NOT NULL,

    KorisnikId INT NOT NULL,

    StatusPrijave NVARCHAR(30) NOT NULL
        CONSTRAINT DF_Prijave_Status
        DEFAULT (N'Kreirana'),

    PotvrdaUplateOriginalniNaziv NVARCHAR(255) NULL,

    PotvrdaUplateSacuvaniNaziv NVARCHAR(255) NULL,

    DatumOdobrenja DATETIME2 NULL,

    OdobrioKorisnikId INT NULL,


    CONSTRAINT FK_Prijave_RaftingTure
        FOREIGN KEY (RaftingTuraId)
        REFERENCES RaftingTure(Id),

    CONSTRAINT FK_Prijave_Korisnici
        FOREIGN KEY (KorisnikId)
        REFERENCES Korisnici(Id),

    CONSTRAINT FK_Prijave_OdobrioKorisnik
        FOREIGN KEY (OdobrioKorisnikId)
        REFERENCES Korisnici(Id),

    CONSTRAINT CK_Prijave_Status
        CHECK
        (
            StatusPrijave IN
            (
                N'Kreirana',
                N'Potvrđena',
                N'Odbijena',
                N'Otkazana'
            )
        )
);
GO

CREATE TABLE UcesniciPrijave
(
    Id INT IDENTITY(1,1)
        CONSTRAINT PK_UcesniciPrijave PRIMARY KEY,

    PrijavaId INT NOT NULL,

    RedniBroj INT NOT NULL,

    Ime NVARCHAR(50) NOT NULL,

    Prezime NVARCHAR(50) NOT NULL,

    JMBG CHAR(13) NOT NULL,

    DatumRodjenja DATE NOT NULL,

    Adresa NVARCHAR(150) NOT NULL,

    KontaktTelefon NVARCHAR(30) NOT NULL,

    Email NVARCHAR(128) NOT NULL,

    ZnaDaPliva BIT NOT NULL,

    PrihvatioIzjavuOdgovornosti BIT NOT NULL,

    /* Stara polja ostaju zbog postojeće implementacije */
    LicnaKarta BIT NOT NULL,
    PotvrdaOUplati BIT NOT NULL,
    PotpisanaIzjavaOdgovornosti BIT NOT NULL,
    SaglasnostRoditeljaStaratelja BIT NOT NULL,


    /* Nova dokumentacija */

    PotvrdioDonosenjeIdentifikacionogDokumenta
        BIT NOT NULL
        CONSTRAINT DF_Ucesnici_PotvrdioDokument
        DEFAULT (0),

    PotpisanaIzjavaOriginalniNaziv
        NVARCHAR(255) NULL,

    PotpisanaIzjavaSacuvaniNaziv
        NVARCHAR(255) NULL,

    SaglasnostOriginalniNaziv
        NVARCHAR(255) NULL,

    SaglasnostSacuvaniNaziv
        NVARCHAR(255) NULL,


    /* Polja ostaju zbog trenutnih modela/repozitorijuma */

    IdentitetProveren
        BIT NOT NULL
        CONSTRAINT DF_Ucesnici_IdentitetProveren
        DEFAULT (0),

    DatumProvereIdentiteta
        DATETIME2 NULL,

    IdentitetProverioKorisnikId
        INT NULL,


    CONSTRAINT FK_UcesniciPrijave_Prijave
        FOREIGN KEY (PrijavaId)
        REFERENCES Prijave(Id)
        ON DELETE CASCADE,

    CONSTRAINT FK_Ucesnici_IdentitetProverioKorisnik
        FOREIGN KEY (IdentitetProverioKorisnikId)
        REFERENCES Korisnici(Id),

    CONSTRAINT UQ_UcesniciPrijave_RedniBroj
        UNIQUE (PrijavaId, RedniBroj),

    CONSTRAINT UQ_UcesniciPrijave_JMBG
        UNIQUE (PrijavaId, JMBG),

    CONSTRAINT CK_UcesniciPrijave_JMBG
        CHECK
        (
            JMBG NOT LIKE '%[^0-9]%'
            AND LEN(JMBG) = 13
        ),

    CONSTRAINT CK_UcesniciPrijave_RedniBroj
        CHECK (RedniBroj > 0)
);
GO

CREATE INDEX IX_Prijave_RaftingTuraId ON Prijave(RaftingTuraId);
CREATE INDEX IX_UcesniciPrijave_PrijavaId ON UcesniciPrijave(PrijavaId);
GO

INSERT INTO NivoiTezine (Naziv, Opis)
VALUES
(N'Laka', N'Mirniji tok, pogodna za početnike i porodice.'),
(N'Srednja', N'Umeren tok i povremeni brzaci; potrebna osnovna fizička spremnost.'),
(N'Teška', N'Zahtevni brzaci; preporučuje se prethodno iskustvo.');
GO

DECLARE @Laka INT = (SELECT Id FROM NivoiTezine WHERE Naziv = N'Laka');
DECLARE @Srednja INT = (SELECT Id FROM NivoiTezine WHERE Naziv = N'Srednja');
DECLARE @Teska INT = (SELECT Id FROM NivoiTezine WHERE Naziv = N'Teška');

INSERT INTO RaftingTure
    (NazivTure, DatumOdrzavanja, LokacijaReka, NivoTezineId, MaksimalanBrojUcesnika, Cena, Aktivna)
VALUES
(N'Porodični rafting Tarom', DATEADD(DAY, 30, CAST(GETDATE() AS DATE)), N'Tara', @Laka, 24, 6500.00, 1),
(N'Vikend avantura Drinom', DATEADD(DAY, 45, CAST(GETDATE() AS DATE)), N'Drina', @Srednja, 18, 8200.00, 1),
(N'Adrenalinski spust Limom', DATEADD(DAY, 60, CAST(GETDATE() AS DATE)), N'Lim', @Teska, 12, 9900.00, 1);
GO

INSERT INTO Korisnici
    (Ime, Prezime, Email, LozinkaHash, LozinkaSalt, Uloga)
VALUES
(N'Admin', N'Rafting', N'admin@rafting.rs',
 N'ppvTeCYKNJoNMYQ2N1tsw3oAjY6RlPkDnOV804EO2cE=',
 N'958RcR+8sqk8zuu+PnpwSg==',
 N'Admin'),
(N'Probni', N'Korisnik', N'korisnik@rafting.rs',
 N'N+GCkU+KZcaXogJPmWi/JedDbRRoqFB0hudVKnCJZ9w=',
 N'9jTxsZeCgMemWaK/JccpmQ==',
 N'Korisnik');
GO
