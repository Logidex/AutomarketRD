using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizarBusquedaAnuncios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"UPDATE ""Anuncios"" SET
                    ""Marca"" = BTRIM(TRANSLATE(""Marca"", 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNaeiouun')),
                    ""Modelo"" = BTRIM(TRANSLATE(""Modelo"", 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNaeiouun')),
                    ""Version"" = BTRIM(TRANSLATE(""Version"", 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNaeiouun')),
                    ""TipoVehiculo"" = BTRIM(TRANSLATE(""TipoVehiculo"", 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNaeiouun')),
                    ""Motor"" = BTRIM(TRANSLATE(""Motor"", 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNaeiouun')),
                    ""Traccion"" = BTRIM(TRANSLATE(""Traccion"", 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNaeiouun')),
                    ""ColorExterior"" = BTRIM(TRANSLATE(""ColorExterior"", 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNaeiouun')),
                    ""ColorInterior"" = BTRIM(TRANSLATE(""ColorInterior"", 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNaeiouun')),
                    ""Transmision"" = BTRIM(TRANSLATE(""Transmision"", 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNaeiouun')),
                    ""Combustible"" = BTRIM(TRANSLATE(""Combustible"", 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNaeiouun')),
                    ""Ubicacion"" = BTRIM(TRANSLATE(""Ubicacion"", 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNaeiouun'))
                WHERE ""Marca"" ~ '[ÁÉÍÓÚÜÑáéíóúüñ]'
                   OR ""Modelo"" ~ '[ÁÉÍÓÚÜÑáéíóúüñ]'
                   OR ""Version"" ~ '[ÁÉÍÓÚÜÑáéíóúüñ]'
                   OR ""TipoVehiculo"" ~ '[ÁÉÍÓÚÜÑáéíóúüñ]'
                   OR ""Motor"" ~ '[ÁÉÍÓÚÜÑáéíóúüñ]'
                   OR ""Traccion"" ~ '[ÁÉÍÓÚÜÑáéíóúüñ]'
                   OR ""ColorExterior"" ~ '[ÁÉÍÓÚÜÑáéíóúüñ]'
                   OR ""ColorInterior"" ~ '[ÁÉÍÓÚÜÑáéíóúüñ]'
                   OR ""Transmision"" ~ '[ÁÉÍÓÚÜÑáéíóúüñ]'
                   OR ""Combustible"" ~ '[ÁÉÍÓÚÜÑáéíóúüñ]'
                   OR ""Ubicacion"" ~ '[ÁÉÍÓÚÜÑáéíóúüñ]'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No se puede restaurar de forma confiable los acentos originales.
        }
    }
}
