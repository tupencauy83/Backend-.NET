using System.Text.RegularExpressions;

namespace TuPenca.Application.Common;

public static class SitioUrlHelper
{
    public const string DominioBase = "tupenca.lat.uy";

    private static readonly Regex SlugValido = new(
        @"^[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?$",
        RegexOptions.Compiled);

    /// <summary>
    /// Extrae el subdominio (sin puntos) desde lo que escribió el usuario.
    /// Acepta "miclub" o "miclub.tupenca.lat.uy" por si pegan la URL completa.
    /// </summary>
    public static string NormalizarSubdominio(string? entrada)
    {
        if (string.IsNullOrWhiteSpace(entrada))
            return string.Empty;

        var valor = entrada.Trim().ToLowerInvariant();

        if (Uri.TryCreate(valor, UriKind.Absolute, out var uri))
            valor = uri.Host;
        else if (Uri.TryCreate($"https://{valor}", UriKind.Absolute, out var uriConEsquema))
            valor = uriConEsquema.Host;

        if (valor.StartsWith("www."))
            valor = valor[4..];

        var sufijo = $".{DominioBase}";
        if (valor.EndsWith(sufijo, StringComparison.Ordinal))
            valor = valor[..^sufijo.Length];

        if (valor.Contains('.'))
            throw new Exception("Ingresá solo el nombre del sitio, sin puntos ni dominio completo.");

        return valor;
    }

    public static string ConstruirUrlPropia(string? subdominio)
    {
        var slug = NormalizarSubdominio(subdominio);

        if (string.IsNullOrWhiteSpace(slug))
            throw new Exception("La dirección del sitio es obligatoria.");

        if (!SlugValido.IsMatch(slug))
            throw new Exception("Usá solo letras minúsculas, números y guiones. Debe empezar y terminar con letra o número.");

        return $"{slug}.{DominioBase}";
    }

    public static string NormalizarHost(string? host)
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
