using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSuscripcionDealerSaaS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SuscripcionDealers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfilDealerId = table.Column<int>(type: "integer", nullable: false),
                    Nivel = table.Column<int>(type: "integer", nullable: false),
                    Ciclo = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaInicioUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaVencimientoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuscripcionDealers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuscripcionDealers_PerfilesDealers_PerfilDealerId",
                        column: x => x.PerfilDealerId,
                        principalTable: "PerfilesDealers",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionDealer_Vencimiento_Estado",
                table: "SuscripcionDealers",
                columns: new[] { "FechaVencimientoUtc", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionDealers_PerfilDealerId",
                table: "SuscripcionDealers",
                column: "PerfilDealerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SuscripcionDealers");
        }
    }
}
