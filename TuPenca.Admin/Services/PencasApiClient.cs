using System.Net.Http.Headers;
using System.Net.Http.Json;
using TuPenca.Admin.DTOs.Penca;
using TuPenca.Admin.Models.Enums;

namespace TuPenca.Admin.Services;

public class PencasApiClient
{
    private readonly HttpClient _httpClient;

    public PencasApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<PencaResponseDto>> ObtenerTodasAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<PencaResponseDto>>("api/penca");
        return result ?? new List<PencaResponseDto>();
    }

    public async Task<PencaResponseDto> CambiarEstadoAsync(Guid pencaId, EstadoPenca nuevoEstado)
    {
        var request = new CambiarEstadoPencaDto
        {
            PencaId = pencaId,
            NuevoEstado = nuevoEstado
        };

        var response = await _httpClient.PostAsJsonAsync("api/penca/cambiar-estado", request);
        return await LeerRespuesta<PencaResponseDto>(response);
    }

    public async Task<List<PremioResponseDto>> ObtenerGanadoresAsync(Guid pencaId)
    {
        var result = await _httpClient.GetFromJsonAsync<List<PremioResponseDto>>($"api/penca/{pencaId}/ganadores");
        return result ?? new List<PremioResponseDto>();
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