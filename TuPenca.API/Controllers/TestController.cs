using Microsoft.AspNetCore.Mvc;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Application.Services;
using TuPenca.Domain.Interfaces.Repositories;

namespace TuPenca.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SportsTestController : ControllerBase
    {
        private readonly ISportsApiService _sportsApiService;
        private readonly IEventoDeportivoService _eventoDeportivoService;

        public SportsTestController(
            ISportsApiService sportsApiService, IEventoDeportivoService eventoDeportivoService)
        {
            _sportsApiService = sportsApiService;
            _eventoDeportivoService = eventoDeportivoService;
        }

        [HttpGet("{externalMatchId}")]
        public async Task<IActionResult> ObtenerResultado(
            string externalMatchId)
        {
            var resultado =
                await _sportsApiService
                    .ObtenerResultadoAsync(externalMatchId);

            if (resultado == null)
                return NotFound(
                    "No se encontró el partido en TheSportsDB");

            return Ok(resultado);
        }

        [HttpPost("sync/{partidoId}")]
        public async Task<IActionResult>
    SincronizarPartido(Guid partidoId)
        {
            var resultado =
                await _eventoDeportivoService
                    .SincronizarPartidoAsync(partidoId);

            return Ok(resultado);
        }


    }


}