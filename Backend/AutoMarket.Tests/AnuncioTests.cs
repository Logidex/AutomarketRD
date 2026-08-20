using AutoMarket.Core.Entities;
using AutoMarket.Core.Exceptions;
using Xunit;

namespace AutoMarket.Tests.Entities;

public class AnuncioTests
{
    // =========================================================================
    // PRUEBA 01: Constructor - Moneda por defecto DOP
    // =========================================================================
    [Fact]
    public void Constructor_SinMoneda_DebeUsarDOP()
    {
        // Act
        var anuncio = CrearAnuncioBase(moneda: null);

        // Assert
        Assert.Equal("DOP", anuncio.Moneda);
    }

    // =========================================================================
    // PRUEBA 02: Constructor - Moneda USD normaliza a mayúsculas
    // =========================================================================
    [Fact]
    public void Constructor_MonedaUsdMinuscula_DebeNormalizarAUSD()
    {
        // Act
        var anuncio = CrearAnuncioBase(moneda: "usd");

        // Assert
        Assert.Equal("USD", anuncio.Moneda);
    }

    // =========================================================================
    // PRUEBA 03: Constructor - Moneda inválida lanza ArgumentException
    // =========================================================================
    [Fact]
    public void Constructor_MonedaInvalida_DebeLanzarArgumentException()
    {
        // Act & Assert
        var excepcion = Assert.Throws<ArgumentException>(() =>
            CrearAnuncioBase(moneda: "EUR"));

        Assert.Contains("moneda", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    // =========================================================================
    // PRUEBA 04: ActualizarInfo - Cambiar moneda a USD
    // =========================================================================
    [Fact]
    public void ActualizarInfo_CambiarMonedaADolar_DebeActualizar()
    {
        // Arrange
        var anuncio = CrearAnuncioBase(moneda: "DOP");

        // Act
        anuncio.ActualizarInfo(
            marca: "Toyota",
            modelo: "Corolla",
            version: "",
            tipoVehiculo: "Sedan",
            motor: "1.8L",
            traccion: "Delantera",
            colorExterior: "Blanco",
            colorInterior: "Negro",
            anio: 2022,
            precio: 15000m,
            moneda: "USD",
            kilometraje: 10000,
            transmision: "Automática",
            combustible: "Gasolina",
            accesorios: new List<string>(),
            ubicacion: "Santo Domingo",
            descripcion: "Vehículo de prueba");

        // Assert
        Assert.Equal("USD", anuncio.Moneda);
        Assert.Equal(15000m, anuncio.Precio);
    }

    // =========================================================================
    // PRUEBA 05: ActualizarInfo - Moneda inválida lanza ArgumentException
    // =========================================================================
    [Fact]
    public void ActualizarInfo_MonedaInvalida_DebeLanzarArgumentException()
    {
        // Arrange
        var anuncio = CrearAnuncioBase(moneda: "DOP");

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            anuncio.ActualizarInfo(
                marca: "Toyota",
                modelo: "Corolla",
                version: "",
                tipoVehiculo: "Sedan",
                motor: "1.8L",
                traccion: "Delantera",
                colorExterior: "Blanco",
                colorInterior: "Negro",
                anio: 2022,
                precio: 15000m,
                moneda: "EUR",
                kilometraje: 10000,
                transmision: "Automática",
                combustible: "Gasolina",
                accesorios: new List<string>(),
                ubicacion: "Santo Domingo",
                descripcion: "Vehículo de prueba"));
    }

    private static Anuncio CrearAnuncioBase(string? moneda)
    {
        return new Anuncio(
            usuarioId: 1,
            marca: "Toyota",
            modelo: "Corolla",
            version: "",
            tipoVehiculo: "Sedan",
            motor: "1.8L",
            traccion: "Delantera",
            colorExterior: "Blanco",
            colorInterior: "Negro",
            anio: 2022,
            precio: 600000m,
            moneda: moneda!,
            kilometraje: 10000,
            transmision: "Automática",
            combustible: "Gasolina",
            accesorios: new List<string>(),
            ubicacion: "Santo Domingo",
            descripcion: "Vehículo de prueba");
    }

    // =========================================================================
    // MoverFotoAlInicio - La primera foto es la portada
    // =========================================================================
    [Fact]
    public void MoverFotoAlInicio_DebeColocarLaFotoComoPrimera()
    {
        // Arrange
        var anuncio = CrearAnuncioBase(moneda: "DOP");
        anuncio.AgregarFotos(new List<string> { "uploads/f1.jpg", "uploads/f2.jpg", "uploads/f3.jpg" });

        // Act
        anuncio.MoverFotoAlInicio("uploads/f2.jpg");

        // Assert
        Assert.Equal("uploads/f2.jpg", anuncio.Fotos.First());
        Assert.Equal(new List<string> { "uploads/f2.jpg", "uploads/f1.jpg", "uploads/f3.jpg" }, anuncio.Fotos);
    }

    [Fact]
    public void MoverFotoAlInicio_YaEsPrimera_DebeMantenerOrden()
    {
        // Arrange
        var anuncio = CrearAnuncioBase(moneda: "DOP");
        anuncio.AgregarFotos(new List<string> { "uploads/f1.jpg", "uploads/f2.jpg" });

        // Act
        anuncio.MoverFotoAlInicio("uploads/f1.jpg");

        // Assert
        Assert.Equal(new List<string> { "uploads/f1.jpg", "uploads/f2.jpg" }, anuncio.Fotos);
    }

    [Fact]
    public void MoverFotoAlInicio_FotoNoPertenece_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        var anuncio = CrearAnuncioBase(moneda: "DOP");
        anuncio.AgregarFotos(new List<string> { "uploads/f1.jpg" });

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => anuncio.MoverFotoAlInicio("uploads/fantasma.jpg"));
    }

    // =========================================================================
    // Vigencia: Publicar asigna la fecha de vencimiento según los días del plan
    // =========================================================================
    [Fact]
    public void Publicar_AsignaFechaVencimientoSegunDias()
    {
        // Arrange
        var anuncio = CrearAnuncioBase(moneda: "DOP");
        anuncio.AgregarFotos(new List<string> { "f1", "f2", "f3", "f4", "f5" });

        // Act
        anuncio.Publicar(diasVigencia: 45);

        // Assert
        Assert.Equal("Publicado", anuncio.Estado);
        Assert.NotNull(anuncio.FechaVencimientoUtc);
        Assert.True(anuncio.FechaVencimientoUtc!.Value > DateTime.UtcNow.AddDays(44));
        Assert.True(anuncio.FechaVencimientoUtc!.Value <= DateTime.UtcNow.AddDays(45));
        Assert.False(anuncio.EstaVencido);
    }

    [Fact]
    public void EstaVencido_ConFechaPasada_DebeSerTrue()
    {
        // Arrange
        var anuncio = CrearAnuncioBase(moneda: "DOP");
        anuncio.AgregarFotos(new List<string> { "f1", "f2", "f3", "f4", "f5" });
        anuncio.Publicar(diasVigencia: -1); // vigencia en el pasado

        // Assert
        Assert.True(anuncio.EstaVencido);
    }

    [Fact]
    public void Publicar_AnuncioVencido_DebeRenovarVigencia()
    {
        // Arrange
        var anuncio = CrearAnuncioBase(moneda: "DOP");
        anuncio.AgregarFotos(new List<string> { "f1", "f2", "f3", "f4", "f5" });
        anuncio.Publicar(diasVigencia: -1); // queda vencido

        // Act
        anuncio.Publicar(diasVigencia: 30); // renovar

        // Assert
        Assert.Equal("Publicado", anuncio.Estado);
        Assert.False(anuncio.EstaVencido);
        Assert.True(anuncio.FechaVencimientoUtc!.Value > DateTime.UtcNow);
    }

    [Fact]
    public void Publicar_AnuncioPublicadoYVigente_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var anuncio = CrearAnuncioBase(moneda: "DOP");
        anuncio.AgregarFotos(new List<string> { "f1", "f2", "f3", "f4", "f5" });
        anuncio.Publicar(diasVigencia: 30);

        // Act & Assert
        Assert.Throws<BusinessRuleException>(() => anuncio.Publicar(diasVigencia: 30));
    }

    // =========================================================================
    // Fotos: límite máximo configurable por plan
    // =========================================================================
    [Fact]
    public void AgregarFotos_ExcedeMaximoPorPlan_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var anuncio = CrearAnuncioBase(moneda: "DOP");
        anuncio.AgregarFotos(new List<string> { "f1", "f2", "f3", "f4", "f5" });

        // Act & Assert (máximo 8, agregar 4 más supera el límite)
        Assert.Throws<BusinessRuleException>(() =>
            anuncio.AgregarFotos(new List<string> { "f6", "f7", "f8", "f9" }, maxFotos: 8));
    }

    [Fact]
    public void AgregarFotos_DentroDelMaximoPorPlan_DebeAgregar()
    {
        // Arrange
        var anuncio = CrearAnuncioBase(moneda: "DOP");
        anuncio.AgregarFotos(new List<string> { "f1", "f2", "f3", "f4", "f5" });

        // Act (máximo 8, agregar 3 más queda justo en el límite)
        anuncio.AgregarFotos(new List<string> { "f6", "f7", "f8" }, maxFotos: 8);

        // Assert
        Assert.Equal(8, anuncio.Fotos.Count);
    }
}