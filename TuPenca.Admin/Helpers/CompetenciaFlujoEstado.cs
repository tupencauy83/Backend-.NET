namespace TuPenca.Admin.Helpers;

public static class CompetenciaFlujoEstado
{
    public static string EtiquetaPartidos(int cantidad) =>
        cantidad == 0 ? "Sin partidos" : cantidad == 1 ? "1 partido" : $"{cantidad} partidos";

    public static string EtiquetaPlantillas(int cantidad) =>
        cantidad == 0 ? "Sin plantilla" : cantidad == 1 ? "1 plantilla" : $"{cantidad} plantillas";

    public static string ClaseBadgePartidos(int cantidad) =>
        cantidad > 0 ? "workflow-badge workflow-badge--ok" : "workflow-badge workflow-badge--pending";

    public static string ClaseBadgePlantillas(int cantidad) =>
        cantidad > 0 ? "workflow-badge workflow-badge--ok" : "workflow-badge workflow-badge--pending";
}
