using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecuperacionPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CodigoRecuperacionExpiracionUtc",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoRecuperacionHash",
                table: "Usuarios",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoRecuperacionExpiracionUtc",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CodigoRecuperacionHash",
                table: "Usuarios");
        }
    }
}
