using System.Net.Http.Headers;
using System.Net.Http.Json;
using TuPenca.Admin.DTOs.Evento;
using TuPenca.Admin.DTOs.Plantillas;

namespace TuPenca.Admin.Services;

public class PlantillasApiClient
{
    private readonly HttpClient _httpClient;

    public PlantillasApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<PlantillaResponseDto>> ObtenerTodosAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<PlantillaResponseDto>>("api/plantillapenca");
        return result ?? new List<PlantillaResponseDto>();
    }

    public async Task<List<CatalogoSimpleDto>> ObtenerEventosAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<CatalogoSimpleDto>>("api/EventoDeportivo");
        return result ?? new List<CatalogoSimpleDto>();
    }

    public async Task<PlantillaResponseDto> CrearAsync(PlantillaRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/plantillapenca/crear", request);
        return await LeerRespuesta<PlantillaResponseDto>(response);
    }

    public async Task EliminarAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/plantillapenca/{id}");
        await ValidarRespuesta(response);
    }

    private static async Task ValidarRespuesta(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var error = await response.Content.ReadAsStringAsync();

        throw new Exception(string.IsNullOrWhiteSpace(error)
            ? "Ocurrió un error al comunicarse con la API."
            : error);
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