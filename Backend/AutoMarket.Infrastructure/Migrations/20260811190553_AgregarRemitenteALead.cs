using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRemitenteALead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsuarioIdRemitente",
                table: "Leads",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsuarioIdRemitente",
                table: "Leads");
        }
    }
}
