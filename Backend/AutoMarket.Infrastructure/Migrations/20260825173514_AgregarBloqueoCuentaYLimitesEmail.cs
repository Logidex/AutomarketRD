using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarBloqueoCuentaYLimitesEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BloqueadoHastaUtc",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmailsEnviadosHoy",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IntentosFallidos",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoEnvioCambioEmailUtc",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoEnvioCambioPasswordUtc",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoEnvioConfirmacionCuentaUtc",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoEnvioRecuperacionUtc",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VentanaEmailsInicioUtc",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BloqueadoHastaUtc",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EmailsEnviadosHoy",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "IntentosFallidos",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UltimoEnvioCambioEmailUtc",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UltimoEnvioCambioPasswordUtc",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UltimoEnvioConfirmacionCuentaUtc",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UltimoEnvioRecuperacionUtc",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "VentanaEmailsInicioUtc",
                table: "Usuarios");
        }
    }
}
