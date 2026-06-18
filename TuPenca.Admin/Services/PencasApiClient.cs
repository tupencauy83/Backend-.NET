using System.Net.Http.Headers;
using System.Net.Http.Json;
using TuPenca.Admin.DTOs.Penca;

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

    public async Task<List<PremioResponseDto>> ObtenerGanadoresAsync(Guid pencaId)
    {
        var result = await _httpClient.GetFromJsonAsync<List<PremioResponseDto>>($"api/penca/{pencaId}/ganadores");
        return result ?? new List<PremioResponseDto>();
    }
}