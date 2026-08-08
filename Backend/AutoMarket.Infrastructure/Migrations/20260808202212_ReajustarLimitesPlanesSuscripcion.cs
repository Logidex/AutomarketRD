using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReajustarLimitesPlanesSuscripcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Reajuste de límites de planes:
            //  - Gratis pasó de 5 anuncios a 1.
            //  - Elite pasó de 999999 (ilimitado) a 500.
            // Los valores antiguos almacenados en BD se migran a los nuevos.
            migrationBuilder.Sql(
                "UPDATE \"SuscripcionDealers\" SET \"Nivel\" = 1 WHERE \"Nivel\" = 5;");

            migrationBuilder.Sql(
                "UPDATE \"SuscripcionDealers\" SET \"Nivel\" = 500 WHERE \"Nivel\" = 999999;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir: restaurar los valores antiguos (5 y 999999).
            migrationBuilder.Sql(
                "UPDATE \"SuscripcionDealers\" SET \"Nivel\" = 5 WHERE \"Nivel\" = 1;");

            migrationBuilder.Sql(
                "UPDATE \"SuscripcionDealers\" SET \"Nivel\" = 999999 WHERE \"Nivel\" = 500;");
        }
    }
}
