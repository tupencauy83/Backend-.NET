using System.Globalization;

namespace TuPenca.Admin.Helpers;

public static class UruguayTimeHelper
{
    private static readonly Lazy<TimeZoneInfo> UruguayTz = new(GetUruguayTimeZone);
    private static readonly CultureInfo EsUy = CultureInfo.GetCultureInfo("es-UY");

    public static DateTime FromUtcToUruguayLocal(DateTime utc)
    {
        var normalized = utc.Kind switch
        {
            DateTimeKind.Utc => utc,
            DateTimeKind.Local => utc.ToUniversalTime(),
            _ => DateTime.SpecifyKind(utc, DateTimeKind.Utc)
        };

        return TimeZoneInfo.ConvertTimeFromUtc(normalized, UruguayTz.Value);
    }

    public static DateTime NowInUruguay() => FromUtcToUruguayLocal(DateTime.UtcNow);

    public static string Formatear(DateTime utcAlmacenado) =>
        FromUtcToUruguayLocal(utcAlmacenado).ToString("g", EsUy);

    public static string HoyComoInputDate() =>
        NowInUruguay().ToString("yyyy-MM-dd");

    public static string FormatearSoloFecha(DateTime utcAlmacenado) =>
        FromUtcToUruguayLocal(utcAlmacenado).ToString("d", EsUy);

    private static TimeZoneInfo GetUruguayTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Montevideo");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Montevideo Standard Time");
        }
    }
}
