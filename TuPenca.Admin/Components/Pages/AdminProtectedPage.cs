using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TuPenca.Admin.Helpers;

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
            JwtTokenHelper.TokenExpirado(token))
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

}