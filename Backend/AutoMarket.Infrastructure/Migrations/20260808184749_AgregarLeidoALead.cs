using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarLeidoALead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Leido",
                table: "Leads",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Leido",
                table: "Leads");
        }
    }
}
