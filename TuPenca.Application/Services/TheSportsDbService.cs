using System.Text.Json;
using TuPenca.Application.DTOs;
using TuPenca.Application.DTOs.SportsApi;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Domain.Interfaces.Repositories;

public class TheSportsDbService : ISportsApiService
{
    private readonly HttpClient _httpClient;

    public TheSportsDbService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ResultadoExternoDto?> ObtenerResultadoAsync(
        string externalMatchId)
    {
        var url =
            $"https://www.thesportsdb.com/api/v1/json/3/lookupevent.php?id={externalMatchId}";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();

        var resultado =
            JsonSerializer.Deserialize<TheSportsDbResponse>(
                json);

        var evento = resultado?.Events?.FirstOrDefault();

        if (evento == null)
            return null;

        if (evento.StrStatus != "FT")
            return new ResultadoExternoDto
            {
                Finalizado = false
            };

        return new ResultadoExternoDto
        {
            GolesLocal = int.Parse(evento.IntHomeScore ?? "0"),
            GolesVisitante = int.Parse(evento.IntAwayScore ?? "0"),
            Finalizado = true
        };
    }
}