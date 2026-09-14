using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarBusquedaFullText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Columna tsvector generada automáticamente con pesos por campo
            migrationBuilder.Sql(@"
                ALTER TABLE ""Anuncios""
                ADD COLUMN ""SearchVector"" tsvector
                GENERATED ALWAYS AS (
                    setweight(to_tsvector('spanish', coalesce(""Marca"", '')), 'A') ||
                    setweight(to_tsvector('spanish', coalesce(""Modelo"", '')), 'A') ||
                    setweight(to_tsvector('spanish', coalesce(""Version"", '')), 'B') ||
                    setweight(to_tsvector('spanish', coalesce(""Descripcion"", '')), 'C')
                ) STORED;
            ");

            // Índice GIN para búsquedas full-text rápidas
            migrationBuilder.Sql(@"
                CREATE INDEX ""IX_Anuncios_SearchVector""
                ON ""Anuncios""
                USING GIN (""SearchVector"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Anuncios_SearchVector"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Anuncios"" DROP COLUMN IF EXISTS ""SearchVector"";");
        }
    }
}
