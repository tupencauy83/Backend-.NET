using System.Net.Http.Headers;
using System.Net.Http.Json;
using TuPenca.Admin.DTOs.Equipo;

namespace TuPenca.Admin.Services;

public class EquiposApiClient
{
    private readonly HttpClient _httpClient;

    public EquiposApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<EquipoResponseDto>> ObtenerTodosAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<EquipoResponseDto>>("api/equipo");
        return result ?? new List<EquipoResponseDto>();
    }

    public async Task<List<EquipoResponseDto>> CrearVariosAsync(EquipoRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/equipo/crear", request);
        return await LeerRespuesta<List<EquipoResponseDto>>(response);
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