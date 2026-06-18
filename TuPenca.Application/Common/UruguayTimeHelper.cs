namespace TuPenca.Application.Common;

/// <summary>
/// Fechas de eventos/partidos: la UI usa hora de Uruguay; en BD y comparaciones se usa UTC.
/// </summary>
public static class UruguayTimeHelper
{
    private static readonly Lazy<TimeZoneInfo> UruguayTz = new(GetUruguayTimeZone);

    public static DateTime FromUruguayLocalToUtc(DateTime uruguayLocal)
    {
        var local = DateTime.SpecifyKind(uruguayLocal, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(local, UruguayTz.Value);
    }

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

    public static DateTime AsUtc(DateTime storedUtc)
    {
        return storedUtc.Kind switch
        {
            DateTimeKind.Utc => storedUtc,
            DateTimeKind.Local => storedUtc.ToUniversalTime(),
            _ => DateTime.SpecifyKind(storedUtc, DateTimeKind.Utc)
        };
    }

    public static DateTime NowInUruguay() => FromUtcToUruguayLocal(DateTime.UtcNow);

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
