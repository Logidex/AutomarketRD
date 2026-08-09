using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHistorialPagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRecordatorioEnviadoUtc",
                table: "SuscripcionDealers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PagosSuscripcion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfilDealerId = table.Column<int>(type: "integer", nullable: false),
                    Nivel = table.Column<int>(type: "integer", nullable: false),
                    Ciclo = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    OrderIdPayPal = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    EventoIdPayPal = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Referencia = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FechaUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosSuscripcion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosSuscripcion_PerfilesDealers_PerfilDealerId",
                        column: x => x.PerfilDealerId,
                        principalTable: "PerfilesDealers",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PagosSuscripcion_FechaUtc",
                table: "PagosSuscripcion",
                column: "FechaUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PagosSuscripcion_PerfilDealerId",
                table: "PagosSuscripcion",
                column: "PerfilDealerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PagosSuscripcion");

            migrationBuilder.DropColumn(
                name: "FechaRecordatorioEnviadoUtc",
                table: "SuscripcionDealers");
        }
    }
}
