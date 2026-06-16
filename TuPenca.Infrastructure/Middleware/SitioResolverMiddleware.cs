using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace TuPenca.Infrastructure.Middleware
{
    public class SitioResolverMiddleware
    {
        private readonly RequestDelegate _next;

        public SitioResolverMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, AppDbContext db)
        {
            var host = NormalizarHost(context.Request.Headers["X-Sitio"].FirstOrDefault() ?? context.Request.Host.Host);

            var sitio = await db.Sitios.FirstOrDefaultAsync(t => t.UrlPropia == host);

            if (sitio != null)
            {
                context.Items["Sitio"] = sitio;
            }

            await _next(context);
        }

        private static string NormalizarHost(string? host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return string.Empty;

            var valor = host.Trim().ToLowerInvariant();

            if (Uri.TryCreate(valor, UriKind.Absolute, out var uri))
                valor = uri.Host;
            else if (Uri.TryCreate($"https://{valor}", UriKind.Absolute, out var uriConEsquema))
                valor = uriConEsquema.Host;

            if (valor.StartsWith("www."))
                valor = valor[4..];

            return valor;
        }
    }
}
