using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDestacadosYCuotasPlanes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlanCatalogoId",
                table: "SuscripcionDealers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CuotaDestacados",
                table: "PlanesCatalogo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EsDestacado",
                table: "Anuncios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaDestacadoHasta",
                table: "Anuncios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionDealers_PlanCatalogoId",
                table: "SuscripcionDealers",
                column: "PlanCatalogoId");

            migrationBuilder.AddForeignKey(
                name: "FK_SuscripcionDealers_PlanesCatalogo_PlanCatalogoId",
                table: "SuscripcionDealers",
                column: "PlanCatalogoId",
                principalTable: "PlanesCatalogo",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SuscripcionDealers_PlanesCatalogo_PlanCatalogoId",
                table: "SuscripcionDealers");

            migrationBuilder.DropIndex(
                name: "IX_SuscripcionDealers_PlanCatalogoId",
                table: "SuscripcionDealers");

            migrationBuilder.DropColumn(
                name: "PlanCatalogoId",
                table: "SuscripcionDealers");

            migrationBuilder.DropColumn(
                name: "CuotaDestacados",
                table: "PlanesCatalogo");

            migrationBuilder.DropColumn(
                name: "EsDestacado",
                table: "Anuncios");

            migrationBuilder.DropColumn(
                name: "FechaDestacadoHasta",
                table: "Anuncios");
        }
    }
}
