using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuPenca.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizarFechasEventosUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Datos previos: hora cargada en Uruguay guardada sin conversión a UTC.
            migrationBuilder.Sql("""
                UPDATE EventosDeportivos
                SET FechaInicio = DATEADD(hour, 3, FechaInicio),
                    FechaFin = DATEADD(hour, 3, FechaFin);

                UPDATE Partidos
                SET Fecha = DATEADD(hour, 3, Fecha);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE EventosDeportivos
                SET FechaInicio = DATEADD(hour, -3, FechaInicio),
                    FechaFin = DATEADD(hour, -3, FechaFin);

                UPDATE Partidos
                SET Fecha = DATEADD(hour, -3, Fecha);
                """);
        }
    }
}
