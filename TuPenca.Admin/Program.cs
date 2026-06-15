using TuPenca.Admin.Components;
using TuPenca.Admin.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Razor Components ─────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ─── API Components ─────────────────────────────────────────

builder.Services.AddHttpClient<AuthApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("ApiSettings:BaseUrl no está configurado.");

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<SitiosApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("ApiSettings:BaseUrl no está configurado.");

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<EstadisticasApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("ApiSettings:BaseUrl no está configurado.");

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<EquiposApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("ApiSettings:BaseUrl no está configurado.");

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<EventosApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("ApiSettings:BaseUrl no está configurado.");

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<PlantillasApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("ApiSettings:BaseUrl no está configurado.");

    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient<PencasApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("ApiSettings:BaseUrl no está configurado.");

    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
