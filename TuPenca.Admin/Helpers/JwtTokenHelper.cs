using System.Text.Json;

namespace TuPenca.Admin.Helpers
{
    public static class JwtTokenHelper
    {
        public static bool TokenExpirado(string token)
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
}
