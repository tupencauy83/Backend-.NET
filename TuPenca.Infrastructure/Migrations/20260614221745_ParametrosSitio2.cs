using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuPenca.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ParametrosSitio2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParametrosSitio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SitioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotifRecordatorioPrediccion = table.Column<bool>(type: "bit", nullable: false),
                    HorasAntesRecordatorio = table.Column<int>(type: "int", nullable: false),
                    NotifResumenSemanal = table.Column<bool>(type: "bit", nullable: false),
                    DiaResumenSemanal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoraResumenSemanal = table.Column<int>(type: "int", nullable: false),
                    NotifResultadoPartido = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParametrosSitio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParametrosSitio_Sitios_SitioId",
                        column: x => x.SitioId,
                        principalTable: "Sitios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParametrosSitio_SitioId",
                table: "ParametrosSitio",
                column: "SitioId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParametrosSitio");
        }
    }
}
