using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using TuPenca.Admin.Components;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Application.Services;
using TuPenca.Domain.Interfaces;
using TuPenca.Domain.Interfaces.Repositories;
using TuPenca.Infrastructure.Data;
using TuPenca.Infrastructure.Data.Repositories;
using TuPenca.Infrastructure.Interfaces.Providers;
using TuPenca.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Razor Components ─────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ─── Base de datos ────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Repositorios y Unit of Work ──────────────────────────────
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAdministradorRepository, AdministradorRepository>();
builder.Services.AddScoped<IPlantillaPencaRepository, PlantillaPencaRepository>();
builder.Services.AddScoped<IPencaRepository, PencaRepository>();
builder.Services.AddScoped<IPrediccionRepository, PrediccionRepository>();
builder.Services.AddScoped<IPuntajeUsuarioRepository, PuntajeUsuarioRepository>();
builder.Services.AddScoped<IPremioRepository, PremioRepository>();
builder.Services.AddScoped<IPartidoRepository, PartidoRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ─── Servicios de Application ─────────────────────────────────
builder.Services.AddScoped<IEquipoService, EquipoService>();
builder.Services.AddScoped<IEventoDeportivoService, EventoDeportivoService>();
builder.Services.AddScoped<IPlantillaPencaService, PlantillaPencaService>();
builder.Services.AddScoped<IPencaService, PencaService>();
builder.Services.AddScoped<ISitioService, SitioService>();
builder.Services.AddScoped<IEstadisticasService, EstadisticasService>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IFirebaseService, FirebaseService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHttpClient<ISportsApiService, TheSportsDbService>();

// ─── Service para actualizar automaticamente resultados de partidos ───
builder.Services.AddHostedService<ResultadoSyncBackgroundService>();
builder.Services.AddHostedService<RecordatorioPrediccionBackgroundService>();
builder.Services.AddHostedService<ResumenSemanalBackgroundService>();


// ─── SitioProvider (null para admin plataforma) ───────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISitioProvider>(sp => new AdminSitioProvider());


// FIREBASE

var firebaseJson = builder.Configuration["Firebase:ServiceAccountJson"];

GoogleCredential credential;

if (!string.IsNullOrWhiteSpace(firebaseJson))
{
    credential = GoogleCredential.FromJson(firebaseJson);
}
else
{
    credential = GoogleCredential.FromFile("tupencauy-key.json");
}

if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = credential
    });
}

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

// Provider que siempre devuelve null (admin plataforma no pertenece a ningún sitio)
public class AdminSitioProvider : ISitioProvider
{
    public Guid? GetSitioId() => null;
    public bool EsAdminPlataforma() => true;
}
