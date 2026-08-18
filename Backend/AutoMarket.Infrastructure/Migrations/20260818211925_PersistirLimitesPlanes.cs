using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PersistirLimitesPlanes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiasVigencia",
                table: "PlanesCatalogo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LimiteAnuncios",
                table: "PlanesCatalogo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxFotos",
                table: "PlanesCatalogo",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiasVigencia",
                table: "PlanesCatalogo");

            migrationBuilder.DropColumn(
                name: "LimiteAnuncios",
                table: "PlanesCatalogo");

            migrationBuilder.DropColumn(
                name: "MaxFotos",
                table: "PlanesCatalogo");
        }
    }
}
