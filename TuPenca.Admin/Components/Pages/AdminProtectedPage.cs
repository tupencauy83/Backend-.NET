using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace TuPenca.Admin.Components.Pages;

public abstract class AdminProtectedPage : ComponentBase
{
    [Inject] protected IJSRuntime JS { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected bool VerificandoSesion { get; set; } = true;
    protected bool SesionValida { get; set; }
    protected string? AdminToken { get; private set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        var token = await JS.InvokeAsync<string?>("localStorage.getItem", "adminToken");
        var rol = await JS.InvokeAsync<string?>("localStorage.getItem", "adminRol");

        if (string.IsNullOrWhiteSpace(token) ||
            rol != "AdministradorPlataforma" ||
            TokenExpirado(token))
        {
            await LimpiarSesion();
            Navigation.NavigateTo("/login-admin", forceLoad: true);
            return;
        }

        SesionValida = true;
        VerificandoSesion = false;
        AdminToken = token;

        await OnSesionValidaAsync();

        StateHasChanged();
    }

    protected virtual Task OnSesionValidaAsync()
    {
        return Task.CompletedTask;
    }

    private async Task LimpiarSesion()
    {
        await JS.InvokeVoidAsync("localStorage.removeItem", "adminToken");
        await JS.InvokeVoidAsync("localStorage.removeItem", "adminNombre");
        await JS.InvokeVoidAsync("localStorage.removeItem", "adminRol");
    }

    private static bool TokenExpirado(string token)
    {
        try
        {
            var partes = token.Split('.');

            if (partes.Length != 3)
                return true;

            var payload = partes[1];
            var jsonBytes = Convert.FromBase64String(NormalizarBase64(payload));
            var json = JsonSerializer.Deserialize<JsonElement>(jsonBytes);

            if (!json.TryGetProperty("exp", out var expClaim))
                return true;

            var expUnix = expClaim.GetInt64();
            var expira = DateTimeOffset.FromUnixTimeSeconds(expUnix);

            return expira <= DateTimeOffset.UtcNow;
        }
        catch
        {
            return true;
        }
    }

    private static string NormalizarBase64(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');

        return base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
    }
}