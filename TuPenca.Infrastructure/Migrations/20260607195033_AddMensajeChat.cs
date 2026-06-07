using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuPenca.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMensajeChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaHora",
                table: "MensajesChat");

            migrationBuilder.RenameColumn(
                name: "Contenido",
                table: "MensajesChat",
                newName: "Mensaje");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Mensaje",
                table: "MensajesChat",
                newName: "Contenido");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaHora",
                table: "MensajesChat",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
