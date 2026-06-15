using System.Net.Http.Headers;
using TuPenca.Admin.DTOs.Sitio;
using TuPenca.Admin.Models.Enums;

namespace TuPenca.Admin.Services;

public class SitiosApiClient
{
    private readonly HttpClient _httpClient;

    public SitiosApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<SitioRequestDto>> ObtenerTodosAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<SitioRequestDto>>("api/sitio/obtener/todos");
        return result ?? new List<SitioRequestDto>();
    }

    public async Task<SitioResponseDto> AprobarAsync(Guid id)
    {
        var request = new SitioActualizarEstadoRequest
        {
            Id = id,
            Estado = EstadoSitio.Activo
        };

        var response = await _httpClient.PostAsJsonAsync("api/sitio/actualizar/estado", request);
        return await LeerRespuesta<SitioResponseDto>(response);
    }

    public async Task<SitioResponseDto> RechazarAsync(Guid id)
    {
        var request = new SitioActualizarEstadoRequest
        {
            Id = id,
            Estado = EstadoSitio.Inactivo
        };

        var response = await _httpClient.PostAsJsonAsync("api/sitio/actualizar/estado", request);
        return await LeerRespuesta<SitioResponseDto>(response);
    }

    public async Task<SitioResponseDto> EliminarAsync(Guid id)
    {
        var response = await _httpClient.PostAsync($"api/sitio/eliminar/{id}", null);
        return await LeerRespuesta<SitioResponseDto>(response);
    }

    public async Task<SitioResponseDto> SolicitarAsync(SitioPendienteRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/sitio/solicitar", request);
        return await LeerRespuesta<SitioResponseDto>(response);
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