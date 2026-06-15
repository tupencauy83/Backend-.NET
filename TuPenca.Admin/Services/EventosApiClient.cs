using System.Net.Http.Headers;
using System.Net.Http.Json;
using TuPenca.Admin.DTOs.Evento;
using TuPenca.Admin.DTOs.Partido;

namespace TuPenca.Admin.Services;

public class EventosApiClient
{
    private readonly HttpClient _httpClient;

    public EventosApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<EventoResponseDto>> ObtenerTodosAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<EventoResponseDto>>("api/EventoDeportivo");
        return result ?? new List<EventoResponseDto>();
    }

    public async Task<EventoResponseDto> CrearAsync(EventoRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/EventoDeportivo/crear", request);
        return await LeerRespuesta<EventoResponseDto>(response);
    }

    public async Task<List<CatalogoSimpleDto>> ObtenerDeportesAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<CatalogoSimpleDto>>("api/Deporte");
        return result ?? new List<CatalogoSimpleDto>();
    }

    public async Task<CatalogoSimpleDto> CrearDeporteAsync(string nombre)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Deporte/crear", nombre);
        return await LeerRespuesta<CatalogoSimpleDto>(response);
    }

    public async Task<List<CatalogoSimpleDto>> ObtenerTiposCompetenciaAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<CatalogoSimpleDto>>("api/TipoCompetencia");
        return result ?? new List<CatalogoSimpleDto>();
    }

    public async Task<CatalogoSimpleDto> CrearTipoCompetenciaAsync(string nombre)
    {
        var response = await _httpClient.PostAsJsonAsync("api/TipoCompetencia/crear", nombre);
        return await LeerRespuesta<CatalogoSimpleDto>(response);
    }

    public async Task<EventoResponseDto?> ObtenerDetalleAsync(Guid eventoId)
    {
        return await _httpClient.GetFromJsonAsync<EventoResponseDto>($"api/EventoDeportivo/{eventoId}");
    }

    public async Task<PartidoResponseDto> AgregarPartidoAsync(PartidoRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/EventoDeportivo/partido/agregar", request);
        return await LeerRespuesta<PartidoResponseDto>(response);
    }

    public async Task<ResultadoResponseDto> CargarResultadoAsync(ResultadoRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/EventoDeportivo/resultado/cargar", request);
        return await LeerRespuesta<ResultadoResponseDto>(response);
    }

    private static async Task<T> LeerRespuesta<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<T>();

            if (data is null)
                throw new Exception("La API devolvió una respuesta vacía.");

            return data;
        }

        var error = await response.Content.ReadAsStringAsync();

        throw new Exception(string.IsNullOrWhiteSpace(error)
            ? "Ocurrió un error al comunicarse con la API."
            : error);
    }
}