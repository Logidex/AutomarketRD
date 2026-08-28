using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEncuestas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Encuestas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encuestas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EncuestasPreguntas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EncuestaId = table.Column<int>(type: "integer", nullable: false),
                    Texto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncuestasPreguntas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EncuestasPreguntas_Encuestas_EncuestaId",
                        column: x => x.EncuestaId,
                        principalTable: "Encuestas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EncuestasRespuestas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EncuestaId = table.Column<int>(type: "integer", nullable: false),
                    PreguntaId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    ValorEscala = table.Column<int>(type: "integer", nullable: true),
                    ValorTexto = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FechaUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncuestasRespuestas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EncuestasRespuestas_EncuestasPreguntas_PreguntaId",
                        column: x => x.PreguntaId,
                        principalTable: "EncuestasPreguntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EncuestasRespuestas_Encuestas_EncuestaId",
                        column: x => x.EncuestaId,
                        principalTable: "Encuestas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EncuestasRespuestas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Encuestas_Activa",
                table: "Encuestas",
                column: "Activa");

            migrationBuilder.CreateIndex(
                name: "IX_EncuestasPreguntas_EncuestaId_Orden",
                table: "EncuestasPreguntas",
                columns: new[] { "EncuestaId", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_EncuestasRespuestas_EncuestaId_UsuarioId",
                table: "EncuestasRespuestas",
                columns: new[] { "EncuestaId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_EncuestasRespuestas_PreguntaId",
                table: "EncuestasRespuestas",
                column: "PreguntaId");

            migrationBuilder.CreateIndex(
                name: "IX_EncuestasRespuestas_Unicas",
                table: "EncuestasRespuestas",
                columns: new[] { "EncuestaId", "UsuarioId", "PreguntaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EncuestasRespuestas_UsuarioId",
                table: "EncuestasRespuestas",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EncuestasRespuestas");

            migrationBuilder.DropTable(
                name: "EncuestasPreguntas");

            migrationBuilder.DropTable(
                name: "Encuestas");
        }
    }
}
