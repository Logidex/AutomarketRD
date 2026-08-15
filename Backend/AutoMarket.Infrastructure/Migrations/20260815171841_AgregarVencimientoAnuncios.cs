using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarVencimientoAnuncios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimientoUtc",
                table: "Anuncios",
                type: "timestamp with time zone",
                nullable: true);

            // Backfill: los anuncios ya publicados reciben una vigencia base de 30 días
            // desde ahora. Así entran a la regla de vencimiento sin esperar a que el
            // vendedor los vuelva a publicar.
            migrationBuilder.Sql(
                "UPDATE \"Anuncios\" SET \"FechaVencimientoUtc\" = NOW() + INTERVAL '30 days' " +
                "WHERE \"Estado\" = 'Publicado' AND \"FechaVencimientoUtc\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaVencimientoUtc",
                table: "Anuncios");
        }
    }
}
