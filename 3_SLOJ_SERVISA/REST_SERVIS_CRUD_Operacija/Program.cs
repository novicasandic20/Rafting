using BibliotekaKlasa.KlasePodatakaEF.KontekstEF;
using BibliotekaKlasa.KlasePodatakaEF.RepozitorijumiEF;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(opcije =>
    opcije.UseSqlServer(
        builder.Configuration.GetConnectionString("KonekcioniString")));

builder.Services.AddScoped<NivoTezineRepo>();

var app = builder.Build();

app.MapControllers();

app.Run();
