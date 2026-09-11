using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdSlotSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Ubicacion = table.Column<int>(type: "integer", nullable: false),
                    AnchoPx = table.Column<int>(type: "integer", nullable: false),
                    AltoPx = table.Column<int>(type: "integer", nullable: false),
                    IntervaloRotacionSeg = table.Column<int>(type: "integer", nullable: false),
                    MaxAnunciosSimultaneos = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdSlots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdSlotsAnuncios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdSlotId = table.Column<int>(type: "integer", nullable: false),
                    PerfilDealerId = table.Column<int>(type: "integer", nullable: false),
                    ImagenOriginalUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ImagenRedimensionadaUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Enlace = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Titulo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    FechaInicioUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFinUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    MetodoPago = table.Column<int>(type: "integer", nullable: false),
                    MontoPagado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    OrderIdPayPal = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UrlCapturaTransferencia = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EstadoTransferencia = table.Column<int>(type: "integer", nullable: true),
                    Prioridad = table.Column<int>(type: "integer", nullable: false),
                    Impresiones = table.Column<int>(type: "integer", nullable: false),
                    Clicks = table.Column<int>(type: "integer", nullable: false),
                    FechaRecordatorioEnviadoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdSlotsAnuncios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdSlotsAnuncios_AdSlots_AdSlotId",
                        column: x => x.AdSlotId,
                        principalTable: "AdSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdSlotsAnuncios_PerfilesDealers_PerfilDealerId",
                        column: x => x.PerfilDealerId,
                        principalTable: "PerfilesDealers",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdSlotsPrecios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdSlotId = table.Column<int>(type: "integer", nullable: false),
                    DuracionDias = table.Column<int>(type: "integer", nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DescuentoProElitePorcentaje = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdSlotsPrecios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdSlotsPrecios_AdSlots_AdSlotId",
                        column: x => x.AdSlotId,
                        principalTable: "AdSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdSlots_Activo_Orden",
                table: "AdSlots",
                columns: new[] { "Activo", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_AdSlots_Ubicacion",
                table: "AdSlots",
                column: "Ubicacion");

            migrationBuilder.CreateIndex(
                name: "IX_AdSlotsAnuncios_Estado_FechaFin",
                table: "AdSlotsAnuncios",
                columns: new[] { "Estado", "FechaFinUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AdSlotsAnuncios_PerfilDealerId",
                table: "AdSlotsAnuncios",
                column: "PerfilDealerId");

            migrationBuilder.CreateIndex(
                name: "IX_AdSlotsAnuncios_Recordatorio",
                table: "AdSlotsAnuncios",
                column: "FechaRecordatorioEnviadoUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AdSlotsAnuncios_Slot_Estado",
                table: "AdSlotsAnuncios",
                columns: new[] { "AdSlotId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_AdSlotsPrecios_Slot_Duracion",
                table: "AdSlotsPrecios",
                columns: new[] { "AdSlotId", "DuracionDias" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdSlotsAnuncios");

            migrationBuilder.DropTable(
                name: "AdSlotsPrecios");

            migrationBuilder.DropTable(
                name: "AdSlots");
        }
    }
}
