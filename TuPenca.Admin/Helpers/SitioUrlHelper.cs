using System.Text.RegularExpressions;

namespace TuPenca.Admin.Helpers;

public static class SitioUrlHelper
{
    public const string DominioBase = "tupenca.lat.uy";

    private static readonly Regex SlugValido = new(
        @"^[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?$",
        RegexOptions.Compiled);

    public static string NormalizarSubdominio(string? entrada)
    {
        if (string.IsNullOrWhiteSpace(entrada))
            return string.Empty;

        var valor = entrada.Trim().ToLowerInvariant()
            .Replace("https://", "")
            .Replace("http://", "")
            .TrimEnd('/');

        var sufijo = $".{DominioBase}";
        if (valor.EndsWith(sufijo, StringComparison.Ordinal))
            valor = valor[..^sufijo.Length];

        var soloSlug = new string(valor.Where(c => c is >= 'a' and <= 'z' or >= '0' and <= '9' or '-').ToArray());
        return soloSlug;
    }

    public static string? ConstruirUrlPropia(string? subdominio)
    {
        var slug = NormalizarSubdominio(subdominio);
        if (string.IsNullOrWhiteSpace(slug) || !SlugValido.IsMatch(slug))
            return null;

        return $"{slug}.{DominioBase}";
    }
}
