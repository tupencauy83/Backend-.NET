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

    public async Task<EstadisticasGlobalesDto?> ObtenerGlobalesAsync(EstadisticasGlobalesFiltroDto? filtro = null)
    {
        var url = "api/estadisticas/global" + ConstruirQuery(filtro);
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(string.IsNullOrWhiteSpace(error)
                ? "No se pudieron obtener las estadísticas."
                : error);
        }

        return await response.Content.ReadFromJsonAsync<EstadisticasGlobalesDto>();
    }

    private static string ConstruirQuery(EstadisticasGlobalesFiltroDto? filtro)
    {
        if (filtro == null)
            return string.Empty;

        var parts = new List<string>();

        if (filtro.FechaDesde.HasValue)
            parts.Add($"fechaDesde={filtro.FechaDesde.Value:yyyy-MM-dd}");

        if (filtro.FechaHasta.HasValue)
            parts.Add($"fechaHasta={filtro.FechaHasta.Value:yyyy-MM-dd}");

        if (filtro.EstadoSitio.HasValue)
            parts.Add($"estadoSitio={(int)filtro.EstadoSitio.Value}");

        if (!string.IsNullOrWhiteSpace(filtro.Buscar))
            parts.Add($"buscar={Uri.EscapeDataString(filtro.Buscar.Trim())}");

        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }
}
