APLIKACIJA: EVIDENCIJA PRIJAVA NA RAFTING TURU
================================================

Tehnologije:
- ASP.NET Core MVC (.NET 8)
- MS SQL Server / SQL Server Express
- Microsoft.Data.SqlClient
- Entity Framework Core
- REST servisi
- Bootstrap
- JavaScript
- Razor View


1. KREIRANJE BAZE
-----------------

U SQL Server Management Studio-u redom pokrenuti:

1. 1_SLOJ_PODATAKA\BazaPodataka\01_BazaPodataka.sql
2. 1_SLOJ_PODATAKA\BazaPodataka\02_StoredProcedure.sql

Baza podataka se zove:

RaftingTureDB

Fajl DokumentiOdobravanje.sql predstavlja migracionu skriptu
koja je korišćena tokom razvoja aplikacije i nije potrebna
prilikom kreiranja nove baze podataka.

Ako SQL Server instanca nije .\SQLEXPRESS,
potrebno je promeniti konekcioni string u:

- 4_PREZENTACIONI_SLOJ\RVS_Aplikacija\appsettings.json
- 3_SLOJ_SERVISA\REST_SERVIS_CRUD_Operacija\appsettings.json


2. OTVARANJE I POKRETANJE REŠENJA
---------------------------------

Otvoriti:

RaftingTure.sln

u programu Visual Studio 2022.

U:

Solution Properties -> Startup Project

izabrati:

Multiple startup projects

i podesiti:

1. RVS_REST_API                         Start
2. REST_SERVIS_CRUD_Operacija           Start
3. RVS_Aplikacija                       Start


Podrazumevani portovi:

- REST servis parametara poslovnog pravila:
  http://localhost:5057

- REST CRUD servis nivoa težine:
  http://localhost:5058

- MVC aplikacija:
  http://localhost:5059


3. PROBNI NALOZI
----------------

Administrator:

E-mail:
admin@rafting.rs

Lozinka:
Admin123!


Korisnik:

E-mail:
korisnik@rafting.rs

Lozinka:
Korisnik123!


Moguće je registrovati i novi korisnički nalog.


4. POSLOVNO PRAVILO
-------------------

Osnovno poslovno pravilo aplikacije odnosi se na
kontrolu kapaciteta rafting ture.

AKO bi dodavanje učesnika iz nove prijave dovelo do
prekoračenja dozvoljenog kapaciteta izabrane rafting ture,
ONDA sistem ne dozvoljava kreiranje prijave.

Ograničenje je parametrizovano.

Parametri poslovnog pravila čitaju se preko REST servisa
iz JSON datoteke:

3_SLOJ_SERVISA\RVS_REST_API\Ogranicenja\
praviloKapaciteta.json

Podrazumevani parametri su:

- procenatKapaciteta = 100
- rezervisanaMesta = 0
- statusi koji se računaju:
  Kreirana i Potvrđena

Prijave sa statusom Odbijena ili Otkazana
ne zauzimaju kapacitet rafting ture.

Ako je servis parametara privremeno nedostupan,
aplikacija koristi bezbedne podrazumevane vrednosti.


5. DODATNA PRAVILA VALIDACIJE
-----------------------------

Pored kontrole kapaciteta, aplikacija sadrži i
validaciju jedinstvenosti učesnika.

Isti JMBG ne može biti unet više puta u okviru
iste prijave.

Takođe, učesnik sa istim JMBG-om ne može biti
prijavljen kroz različite aktivne prijave za istu
rafting turu.

Pri ovoj proveri računaju se prijave sa statusom:

- Kreirana
- Potvrđena

Ukoliko je prethodna prijava Odbijena ili Otkazana,
učesnik se može ponovo prijaviti na istu turu.


6. GLAVNE FUNKCIJE
------------------

Aplikacija omogućava:

- registraciju korisnika
- prijavu korisnika
- odjavu korisnika

- prikaz rafting tura
- CRUD operacije nad rafting turama

- CRUD operacije nad nivoima težine
  preko REST servisa i Entity Framework-a

- kreiranje prijave za rafting turu

- unos više učesnika u okviru jedne prijave
  na istoj ekranskoj formi

- master-detail odnos:
  Prijava -> Učesnici prijave

- čuvanje master-detail celine
  primenom SQL transakcije

- izmenu prijave i njenih učesnika

- brisanje prijave

- pregled pojedinačne prijave
  sa svim pripadajućim učesnicima

- tabelarni prikaz prijava

- filtriranje prijava po:
  broju prijave,
  rafting turi,
  lokaciji,
  imenu,
  prezimenu i
  JMBG-u

- filtriranje prema statusu prijave

- štampu svih prijava

- štampu filtriranog spiska prijava

- parametarsku štampu
  pojedinačnog master-detail dokumenta

- upload potvrde o uplati

- upload potpisane izjave o odgovornosti

- upload saglasnosti roditelja/staratelja
  za maloletnog učesnika

- potvrdu da će učesnik na dan ture
  poneti važeći identifikacioni dokument

- JavaScript validacije

- validacije pomoću regularnih izraza
  za JMBG, e-mail i kontakt telefon

- kontrolu kapaciteta kroz
  sloj poslovne logike

- administratorsko potvrđivanje
  i odbijanje prijava


7. ORGANIZACIJA SLOJEVA
-----------------------

Aplikacija je realizovana kao četvoroslojno rešenje.


1_SLOJ_PODATAKA

Sadrži:

- modele podataka
- Repository pattern
- SQL upite preko TabelaKlasa
- standardne SQL Client klase
- stored procedure
- Entity Framework Core

Različiti načini pristupa podacima demonstrirani su kroz:

1. standardne SQL Client klase i stored procedure
2. nasleđivanje TabelaKlasa i primenu SQL upita
3. Entity Framework Core


2_SLOJ_POSLOVNE_LOGIKE

Sadrži:

- realizaciju poslovnog pravila
  kontrole kapaciteta

- čitanje parametara odlučivanja
  pozivom REST servisa

- pozive klasa iz sloja za rad sa podacima

- izračunavanje raspoloživog kapaciteta ture


3_SLOJ_SERVISA

Sadrži dva REST servisa:

RVS_REST_API

- obezbeđuje parametre za poslovnu logiku
- učitava parametre iz JSON datoteke


REST_SERVIS_CRUD_Operacija

- omogućava CRUD operacije
  nad nivoima težine
- koristi Entity Framework Core


4_PREZENTACIONI_SLOJ

Realizovan je pomoću ASP.NET Core MVC-a.

Sadrži:

- Controllers
- ViewModel klase
- Razor Views
- View Components
- Bootstrap korisnički interfejs
- JavaScript validacije


8. MASTER-DETAIL I TRANSAKCIJE
------------------------------

Glavni dokument u aplikaciji je:

Prijava

Dok detaljni deo predstavljaju:

UčesniciPrijave

Jedna prijava može sadržati jednog ili više učesnika.

Prilikom kreiranja prijave, podaci glavnog dokumenta
i svi učesnici čuvaju se kao jedna celina primenom
SQL transakcije.

Na taj način se obezbeđuje da se kompletna prijava
sačuva samo ako su sve operacije uspešno izvršene.


9. VALIDACIJE
-------------

Aplikacija koristi validacije na više nivoa:

- DataAnnotations validacije
- serverske validacije
- JavaScript validacije
- regularne izraze
- SQL CHECK ograničenja
- UNIQUE ograničenja

Proveravaju se, između ostalog:

- obavezna polja
- dužina podataka
- tip podatka
- JMBG
- e-mail adresa
- kontakt telefon
- datum rođenja
- budući datum rođenja
- duplikat JMBG-a
- dokumentacija učesnika
- dokumentacija maloletnog učesnika
- raspoloživ kapacitet rafting ture


10. NAPOMENA
------------

Za pravilno funkcionisanje poslovnog pravila
potrebno je da REST servis RVS_REST_API bude pokrenut.

Za CRUD operacije nad nivoima težine potrebno je
da REST_SERVIS_CRUD_Operacija bude pokrenut.

MVC aplikacija se pokreće na:

http://localhost:5059