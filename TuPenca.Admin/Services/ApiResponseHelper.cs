using System.Text.Json;

namespace TuPenca.Admin.Services;

public static class ApiResponseHelper
{
    public static async Task<string> LeerMensajeErrorAsync(HttpResponseMessage response)
    {
        var raw = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(raw))
            return "Ocurrió un error al comunicarse con la API.";

        return ExtraerMensaje(raw.Trim());
    }

    public static string ExtraerMensaje(string raw)
    {
        if (raw.StartsWith('{'))
        {
            try
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;

                if (root.TryGetProperty("mensaje", out var mensaje))
                    return mensaje.GetString() ?? raw;

                if (root.TryGetProperty("title", out var titulo))
                    return titulo.GetString() ?? raw;

                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    foreach (var propiedad in errors.EnumerateObject())
                    {
                        if (propiedad.Value.ValueKind == JsonValueKind.Array &&
                            propiedad.Value.GetArrayLength() > 0)
                        {
                            return propiedad.Value[0].GetString() ?? raw;
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Respuesta JSON no estándar: devolver texto crudo.
            }
        }

        if (raw.Length >= 2 && raw.StartsWith('"') && raw.EndsWith('"'))
        {
            try
            {
                var deserializado = JsonSerializer.Deserialize<string>(raw);
                if (!string.IsNullOrWhiteSpace(deserializado))
                    return deserializado;
            }
            catch (JsonException)
            {
                return raw.Trim('"');
            }
        }

        return raw;
    }
}
