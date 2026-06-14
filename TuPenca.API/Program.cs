using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System.Text;
using TuPenca.API.Hubs;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Application.Services;
using TuPenca.Domain.Entities;
using TuPenca.Domain.Interfaces;
using TuPenca.Domain.Interfaces.Repositories;
using TuPenca.Infrastructure.Data;
using TuPenca.Infrastructure.Data.Repositories;
using TuPenca.Infrastructure.Interfaces.Providers;
using TuPenca.Infrastructure.Middleware;
using TuPenca.Infrastructure.Providers;
using TuPenca.Infrastructure.Repositories;
using TuPenca.Infrastructure.Services;
// using TuPenca.Infrastructure.Data;
// revisar si es necesario

var builder = WebApplication.CreateBuilder(args);

// ─── Base de datos ───────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure();
        }));

// ─── Autenticación JWT ────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/chat"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

// ─── Multi-tenancy ────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISitioProvider, SitioProvider>();

// ─── Repositorios, Service y Unit of Work ─────────────────────────
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAdministradorRepository, AdministradorRepository>();
builder.Services.AddScoped<IPlantillaPencaRepository, PlantillaPencaRepository>();
builder.Services.AddScoped<IPencaRepository, PencaRepository>();
builder.Services.AddScoped<IPrediccionRepository, PrediccionRepository>();
builder.Services.AddScoped<IPuntajeUsuarioRepository, PuntajeUsuarioRepository>();
builder.Services.AddScoped<IPremioRepository, PremioRepository>();
builder.Services.AddScoped<IPartidoRepository, PartidoRepository>();
builder.Services.AddScoped<IParametrosSitioRepository, ParametrosSitioRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ISitioService, SitioService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<IEquipoService, EquipoService>();
builder.Services.AddScoped<IEventoDeportivoService, EventoDeportivoService>();
builder.Services.AddScoped<IPlantillaPencaService, PlantillaPencaService>();
builder.Services.AddScoped<IPencaService, PencaService>();
builder.Services.AddScoped<IPrediccionService, PrediccionService>();
builder.Services.AddScoped<IEstadisticasService, EstadisticasService>();
builder.Services.AddScoped<IInvitacionService, InvitacionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHttpClient<ISportsApiService, TheSportsDbService>();
builder.Services.AddScoped<IParametrosSitioService, ParametrosSitioService>();


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IFirebaseService, FirebaseService>();
builder.Services.AddSingleton<IEmailService, EmailService>();


// ─── Para Consultar API de Resultados Externa ───────────────────────────────────────────────

builder.Services.AddHttpClient<
    ISportsApiService,
    TheSportsDbService>();

// ─── AutoMapper ───────────────────────────────────────────────
//builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); 
//revisar pq da error

// ─── Service para actualizar automaticamente resultados de partidos ───
builder.Services.AddHostedService<ResultadoSyncBackgroundService>();
builder.Services.AddHostedService<RecordatorioPrediccionBackgroundService>();
builder.Services.AddHostedService<ResumenSemanalBackgroundService>();

// ─── SignalR ──────────────────────────────────────────────────
builder.Services.AddSignalR(); // revisar la logica

// ─── Controllers + Swagger ────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ─── MercadoPago Config de Prueba ─────────────────────────────
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// ─── CORS ─────────────────────────────────────────────────────
// Permite que el frontend y la app móvil consuman la API
builder.Services.AddCors(options =>
{
    options.AddPolicy("TenantCors", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrWhiteSpace(origin))
                    return false;

                var uri = new Uri(origin);

                if (uri.Host == "localhost" || uri.Host == "127.0.0.1")
                    return true;

                return uri.Scheme == "https";
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ─── Servicios de Application ────────────────────────────────
// Acá vas a ir registrando tus servicios a medida que los creés
// Ejemplo:
// builder.Services.AddScoped<IPencaService, PencaService>();

var firebaseJson = builder.Configuration["Firebase:ServiceAccountJson"];

GoogleCredential credential;

if (!string.IsNullOrWhiteSpace(firebaseJson))
{
    // Railway / Producción
    credential = GoogleCredential.FromJson(firebaseJson);
}
else
{
    // Desarrollo local
    var firebasePath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "tupencauy-key.json");

    credential = GoogleCredential.FromFile(firebasePath);
}

FirebaseApp.Create(new AppOptions
{
    Credential = credential
});

var app = builder.Build();

// ─── Middleware pipeline ──────────────────────────────────────
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

//app.UseCors("AllowAll");
app.UseCors("TenantCors");

app.UseAuthentication();   // siempre antes de Authorization

// ─── Middleware ───────────────────────────────────────────────
app.UseMiddleware<SitioResolverMiddleware>();

app.UseAuthorization();

app.MapControllers();

// ─── SignalR Hubs ─────────────────────────────────────────────
// app.MapHub<ResultadosHub>("/hubs/resultados");
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
