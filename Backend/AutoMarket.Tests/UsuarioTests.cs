using AutoMarket.Core.Entities;
using Xunit;

namespace AutoMarket.Tests.Entities;

public class UsuarioTests
{
    [Fact]
    public void CambiarPassword_HashValido_DebeActualizarElPasswordHash()
    {
        // Arrange
        var admin = Usuario.CrearAdministradorInterno("Administrador", "Supremo", "admin@automarket.do", "hash-anterior");

        // Act
        admin.CambiarPassword("hash-nuevo");

        // Assert
        var hashActual = typeof(Usuario).GetProperty("PasswordHash")?.GetValue(admin);
        Assert.Equal("hash-nuevo", hashActual);
    }

    [Fact]
    public void CambiarPassword_HashVacio_DebeLanzarArgumentException()
    {
        // Arrange
        var admin = Usuario.CrearAdministradorInterno("Administrador", "Supremo", "admin@automarket.do", "hash-anterior");

        // Act & Assert
        var excepcion = Assert.Throws<ArgumentException>(() => admin.CambiarPassword(null));

        Assert.Equal("nuevoPasswordHash", excepcion.ParamName);
    }

    [Fact]
    public void ConstructorPublico_DebeAsignarLaFechaDeCreacionActual()
    {
        // Arrange
        var antes = DateTime.UtcNow.AddSeconds(-5);

        // Act
        var usuario = new Usuario(
            "Juan",
            "Perez",
            "juan@test.com",
            "hash-123",
            "8095550000",
            "Comprador");

        // Assert
        var despues = DateTime.UtcNow.AddSeconds(5);
        Assert.InRange(usuario.CreatedAt, antes, despues);
    }
}