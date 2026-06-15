using System.Net.Http.Headers;
using System.Net.Http.Json;
using TuPenca.Admin.DTOs.Estadisticas;

namespace TuPenca.Admin.Services;

public class EstadisticasApiClient
{
    private readonly HttpClient _httpClient;

    public EstadisticasApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<EstadisticasGlobalesDto?> ObtenerGlobalesAsync()
    {
        var response = await _httpClient.GetAsync("api/estadisticas/global");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(string.IsNullOrWhiteSpace(error)
                ? "No se pudieron obtener las estadísticas."
                : error);
        }

        return await response.Content.ReadFromJsonAsync<EstadisticasGlobalesDto>();
    }
}