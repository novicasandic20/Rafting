using Microsoft.AspNetCore.Mvc;

namespace RVS_REST_API.Controllers
{
    [ApiController]
    [Route("api/parametri")]
    public class ParametriController : ControllerBase
    {
        private readonly IWebHostEnvironment _okruzenje;

        public ParametriController(IWebHostEnvironment okruzenje)
        {
            _okruzenje = okruzenje;
        }

        [HttpGet("kapacitet")]
        public async Task<IActionResult> DajParametreKapaciteta()
        {
            var putanja = Path.Combine(
                _okruzenje.ContentRootPath,
                "Ogranicenja",
                "praviloKapaciteta.json");

            if (!System.IO.File.Exists(putanja))
            {
                return NotFound(
                    "Datoteka sa parametrima poslovnog pravila ne postoji.");
            }

            var json = await System.IO.File.ReadAllTextAsync(putanja);
            return Content(json, "application/json");
        }
    }
}
