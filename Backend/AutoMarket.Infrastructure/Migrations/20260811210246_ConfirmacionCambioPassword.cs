using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfirmacionCambioPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CodigoPasswordExpiracionUtc",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoPasswordHash",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordPendienteHash",
                table: "Usuarios",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoPasswordExpiracionUtc",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CodigoPasswordHash",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "PasswordPendienteHash",
                table: "Usuarios");
        }
    }
}
