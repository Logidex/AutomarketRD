using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTransferenciaBancaria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstadoTransferencia",
                table: "PagosSuscripcion",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaConfirmacionUtc",
                table: "PagosSuscripcion",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Metodo",
                table: "PagosSuscripcion",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NotasAdmin",
                table: "PagosSuscripcion",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlCapturaTransferencia",
                table: "PagosSuscripcion",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstadoTransferencia",
                table: "PagosSuscripcion");

            migrationBuilder.DropColumn(
                name: "FechaConfirmacionUtc",
                table: "PagosSuscripcion");

            migrationBuilder.DropColumn(
                name: "Metodo",
                table: "PagosSuscripcion");

            migrationBuilder.DropColumn(
                name: "NotasAdmin",
                table: "PagosSuscripcion");

            migrationBuilder.DropColumn(
                name: "UrlCapturaTransferencia",
                table: "PagosSuscripcion");
        }
    }
}
