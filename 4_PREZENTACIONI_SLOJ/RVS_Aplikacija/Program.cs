using BibliotekaKlasa.KlasePodataka.Repozitorijumi;
using BibliotekaKlasa.KlasePodatakaEF.KontekstEF;
using BibliotekaKlasa.KlasePodatakaEF.RepozitorijumiEF;
using BibliotekaKlasa.TehnoloskeKlase;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PoslovnaLogika;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var konekcioniString =
    builder.Configuration.GetConnectionString("KonekcioniString")
    ?? throw new InvalidOperationException(
        "Nedostaje ConnectionStrings:KonekcioniString.");

builder.Services.AddSingleton(
    new KonekcijaKlasa(konekcioniString));

builder.Services.AddScoped<KorisnikRepo>();
builder.Services.AddScoped<RaftingTuraRepo>();
builder.Services.AddScoped<PrijavaRepo>();
builder.Services.AddScoped<UcesnikPrijaveRepo>();

builder.Services.AddDbContext<AppDbContext>(opcije =>
    opcije.UseSqlServer(konekcioniString));

builder.Services.AddScoped<NivoTezineRepo>();

var parametriUrl =
    builder.Configuration["Servisi:ParametriPoslovnogPravilaUrl"]
    ?? "http://localhost:5057/";

builder.Services.AddHttpClient<PraviloKapacitetaTure>(klijent =>
{
    klijent.BaseAddress = new Uri(parametriUrl);
    klijent.Timeout = TimeSpan.FromSeconds(3);
});

var nivoiApiUrl =
    builder.Configuration["Servisi:NivoiTezineApiUrl"]
    ?? "http://localhost:5058/";

builder.Services.AddHttpClient("NivoiTezineApi", klijent =>
{
    klijent.BaseAddress = new Uri(nivoiApiUrl);
    klijent.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opcije =>
    {
        opcije.LoginPath = "/Nalog/Prijava";
        opcije.LogoutPath = "/Nalog/OdjaviSe";
        opcije.AccessDeniedPath = "/Nalog/ZabranjenPristup";
    });

builder.Services.AddAuthorization();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opcije =>
{
    opcije.IdleTimeout = TimeSpan.FromMinutes(30);
    opcije.Cookie.HttpOnly = true;
    opcije.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Pocetna/Greska");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pocetna}/{action=Index}/{id?}");

app.Run();
