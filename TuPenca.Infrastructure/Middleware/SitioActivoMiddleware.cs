using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TuPenca.Domain.Enums;

namespace TuPenca.Infrastructure.Middleware
{
    /// <summary>
    /// Bloquea requests autenticados de tenant cuando el sitio no está Activo.
    /// </summary>
    public class SitioActivoMiddleware
    {
        private static readonly string[] RutasExentas =
        [
            "/api/auth/login",
            "/api/auth/firebase",
            "/api/auth/registro",
            "/api/sitio/publicos",
            "/api/sitio/solicitar",
        ];

        private readonly RequestDelegate _next;

        public SitioActivoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, AppDbContext db)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var sitioIdClaim = context.User.FindFirst("sitioId")?.Value;
                var path = context.Request.Path.Value ?? string.Empty;

                if (sitioIdClaim != null
                    && Guid.TryParse(sitioIdClaim, out var sitioId)
                    && !RutaExenta(path))
                {
                    var sitio = await db.Sitios
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == sitioId);

                    if (sitio == null || sitio.Estado != EstadoSitio.Activo)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.Headers["X-Sitio-Inactivo"] = "1";
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("\"Este sitio fue desactivado, eliminado o no está disponible.\"");
                        return;
                    }
                }
            }

            await _next(context);
        }

        private static bool RutaExenta(string path)
        {
            foreach (var ruta in RutasExentas)
            {
                if (path.StartsWith(ruta, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
