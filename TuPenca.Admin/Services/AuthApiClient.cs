using System.Net;
using TuPenca.Admin.DTOs.Auth;

namespace TuPenca.Admin.Services;

public class AuthApiClient
{
    private readonly HttpClient _httpClient;

    public AuthApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResponseDto?> LoginAdminAsync(LoginRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return null;

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(string.IsNullOrWhiteSpace(error)
                ? "No se pudo iniciar sesión."
                : error);
        }

        return await response.Content.ReadFromJsonAsync<LoginResponseDto>();
    }
}