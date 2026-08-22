using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CascadaEliminacionUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketMensajes_Usuarios_AutorId",
                table: "TicketMensajes");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMensajes_Usuarios_AutorId",
                table: "TicketMensajes",
                column: "AutorId",
                principalTable: "Usuarios",
                principalColumn: "UsuarioId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketMensajes_Usuarios_AutorId",
                table: "TicketMensajes");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMensajes_Usuarios_AutorId",
                table: "TicketMensajes",
                column: "AutorId",
                principalTable: "Usuarios",
                principalColumn: "UsuarioId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
