using BibliotekaKlasa.KlasePodataka.Modeli;
using BibliotekaKlasa.KlasePodatakaEF.RepozitorijumiEF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace REST_SERVIS_CRUD_Operacija.Controllers
{
    [ApiController]
    [Route("api/nivoi-tezine")]
    public class NivoiTezineController : ControllerBase
    {
        private readonly NivoTezineRepo _repozitorijum;

        public NivoiTezineController(NivoTezineRepo repozitorijum)
        {
            _repozitorijum = repozitorijum;
        }

        [HttpGet]
        public async Task<ActionResult<List<NivoTezineModel>>> DajSve()
        {
            return Ok(await _repozitorijum.DajSveAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<NivoTezineModel>> DajPoId(int id)
        {
            var nivo = await _repozitorijum.DajPoIdAsync(id);
            return nivo is null ? NotFound() : Ok(nivo);
        }

        [HttpPost]
        public async Task<ActionResult<NivoTezineModel>> Dodaj(
            NivoTezineModel nivo)
        {
            if (string.IsNullOrWhiteSpace(nivo.Naziv))
            {
                return BadRequest("Naziv nivoa težine je obavezan.");
            }

            try
            {
                var dodat = await _repozitorijum.DodajAsync(nivo);
                return CreatedAtAction(
                    nameof(DajPoId),
                    new { id = dodat.Id },
                    dodat);
            }
            catch (DbUpdateException)
            {
                return Conflict("Nivo težine sa tim nazivom već postoji.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Izmeni(
            int id,
            NivoTezineModel nivo)
        {
            if (id != nivo.Id)
            {
                return BadRequest("ID u ruti i telu zahteva nisu isti.");
            }

            try
            {
                var izmenjen = await _repozitorijum.IzmeniAsync(nivo);
                return izmenjen ? NoContent() : NotFound();
            }
            catch (DbUpdateException)
            {
                return Conflict("Nivo težine sa tim nazivom već postoji.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Obrisi(int id)
        {
            try
            {
                var obrisan = await _repozitorijum.ObrisiAsync(id);
                return obrisan ? NoContent() : NotFound();
            }
            catch (DbUpdateException)
            {
                return Conflict(
                    "Nivo težine se koristi u rafting turama i ne može se obrisati.");
            }
        }
    }
}
