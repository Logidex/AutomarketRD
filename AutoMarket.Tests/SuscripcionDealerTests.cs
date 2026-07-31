using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using Xunit;

namespace AutoMarket.Tests.Entities;

public class SuscripcionDealerTests
{
    // =========================================================================
    // PRUEBA 01: Constructor - Éxito
    // =========================================================================
    [Fact]
    public void Constructor_DatosValidos_DebeCrearSuscripcionActiva()
    {
        // Arrange
        int perfilDealerId = 10;

        // Act
        var suscripcion = new SuscripcionDealer(
            perfilDealerId,
            PlanNivel.Pro,
            CicloFacturacion.Mensual);

        // Assert
        Assert.Equal(perfilDealerId, suscripcion.PerfilDealerId);
        Assert.Equal(PlanNivel.Pro, suscripcion.Nivel);
        Assert.Equal(CicloFacturacion.Mensual, suscripcion.Ciclo);
        Assert.Equal(EstadoSuscripcion.Activa, suscripcion.Estado);
        Assert.True(suscripcion.FechaInicioUtc <= DateTime.UtcNow);
        Assert.True(suscripcion.FechaVencimientoUtc > DateTime.UtcNow);
    }

    // =========================================================================
    // PRUEBA 02: Constructor - Fallo por perfil inválido
    // =========================================================================
    [Fact]
    public void Constructor_PerfilDealerIdInvalido_DebeLanzarArgumentException()
    {
        // Act & Assert
        var excepcion = Assert.Throws<ArgumentException>(() =>
            new SuscripcionDealer(0, PlanNivel.Basico, CicloFacturacion.Mensual));

        Assert.Equal("perfilDealerId", excepcion.ParamName);
        Assert.Contains("inválido", excepcion.Message);
    }

    // =========================================================================
    // PRUEBA 03: PermiteNuevosAnuncios - Éxito
    // =========================================================================
    [Fact]
    public void PermiteNuevosAnuncios_SuscripcionActivaYConEspacio_DebeRetornarTrue()
    {
        // Arrange
        var suscripcion = new SuscripcionDealer(1, PlanNivel.Pro, CicloFacturacion.Mensual);

        // Act
        var resultado = suscripcion.PermiteNuevosAnuncios((int)PlanNivel.Pro - 1);

        // Assert
        Assert.True(resultado);
    }

    // =========================================================================
    // PRUEBA 04: PermiteNuevosAnuncios - Falla por límite alcanzado
    // =========================================================================
    [Fact]
    public void PermiteNuevosAnuncios_LimiteAlcanzado_DebeRetornarFalse()
    {
        // Arrange
        var suscripcion = new SuscripcionDealer(1, PlanNivel.Basico, CicloFacturacion.Mensual);

        // Act
        var resultado = suscripcion.PermiteNuevosAnuncios((int)PlanNivel.Basico);

        // Assert
        Assert.False(resultado);
    }

    // =========================================================================
    // PRUEBA 05: PermiteNuevosAnuncios - Falla por suscripción cancelada
    // =========================================================================
    [Fact]
    public void PermiteNuevosAnuncios_SuscripcionCancelada_DebeRetornarFalse()
    {
        // Arrange
        var suscripcion = new SuscripcionDealer(1, PlanNivel.Pro, CicloFacturacion.Mensual);

        var propEstado = typeof(SuscripcionDealer).GetProperty("Estado");
        propEstado?.SetValue(suscripcion, EstadoSuscripcion.Cancelada);

        // Act
        var resultado = suscripcion.PermiteNuevosAnuncios(0);

        // Assert
        Assert.False(resultado);
    }

    // =========================================================================
    // PRUEBA 06: PermiteNuevosAnuncios - Falla por vencimiento
    // =========================================================================
    [Fact]
    public void PermiteNuevosAnuncios_SuscripcionVencida_DebeRetornarFalse()
    {
        // Arrange
        var suscripcion = new SuscripcionDealer(1, PlanNivel.Pro, CicloFacturacion.Mensual);

        var propFechaVencimiento = typeof(SuscripcionDealer).GetProperty("FechaVencimientoUtc");
        propFechaVencimiento?.SetValue(suscripcion, DateTime.UtcNow.AddMinutes(-1));

        // Act
        var resultado = suscripcion.PermiteNuevosAnuncios(0);

        // Assert
        Assert.False(resultado);
    }

    // =========================================================================
    // PRUEBA 07: CambiarPlan - Éxito
    // =========================================================================
    [Fact]
    public void CambiarPlan_DatosValidos_DebeActualizarPlanYCiclo()
    {
        // Arrange
        var suscripcion = new SuscripcionDealer(5, PlanNivel.Basico, CicloFacturacion.Mensual);

        // Act
        suscripcion.CambiarPlan(PlanNivel.Elite, CicloFacturacion.Anual);

        // Assert
        Assert.Equal(PlanNivel.Elite, suscripcion.Nivel);
        Assert.Equal(CicloFacturacion.Anual, suscripcion.Ciclo);
        Assert.Equal(EstadoSuscripcion.Activa, suscripcion.Estado);
        Assert.True(suscripcion.FechaVencimientoUtc > DateTime.UtcNow);
    }

    // =========================================================================
    // PRUEBA 08: CambiarPlan - Fallo por suscripción cancelada
    // =========================================================================
    [Fact]
    public void CambiarPlan_SuscripcionCancelada_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var suscripcion = new SuscripcionDealer(5, PlanNivel.Basico, CicloFacturacion.Mensual);

        var propEstado = typeof(SuscripcionDealer).GetProperty("Estado");
        propEstado?.SetValue(suscripcion, EstadoSuscripcion.Cancelada);

        // Act & Assert
        var excepcion = Assert.Throws<InvalidOperationException>(() =>
            suscripcion.CambiarPlan(PlanNivel.Pro, CicloFacturacion.Trimestral));

        Assert.Contains("suscripción actual se encuentra cancelada", excepcion.Message);
    }

    // =========================================================================
    // PRUEBA 09: RenovarManualmente - Éxito
    // =========================================================================
    [Fact]
    public void RenovarManualmente_FechaFutura_DebeActualizarVencimientoYActivar()
    {
        // Arrange
        var suscripcion = new SuscripcionDealer(2, PlanNivel.Pro, CicloFacturacion.Mensual);
        var nuevaFecha = DateTime.UtcNow.AddMonths(6);

        var propEstado = typeof(SuscripcionDealer).GetProperty("Estado");
        propEstado?.SetValue(suscripcion, EstadoSuscripcion.Cancelada);

        // Act
        suscripcion.RenovarManualmente(nuevaFecha);

        // Assert
        Assert.Equal(nuevaFecha, suscripcion.FechaVencimientoUtc);
        Assert.Equal(EstadoSuscripcion.Activa, suscripcion.Estado);
    }

    // =========================================================================
    // PRUEBA 10: RenovarManualmente - Fallo por fecha pasada
    // =========================================================================
    [Fact]
    public void RenovarManualmente_FechaPasada_DebeLanzarArgumentException()
    {
        // Arrange
        var suscripcion = new SuscripcionDealer(2, PlanNivel.Pro, CicloFacturacion.Mensual);
        var fechaInvalida = DateTime.UtcNow.AddMinutes(-5);

        // Act & Assert
        var excepcion = Assert.Throws<ArgumentException>(() =>
            suscripcion.RenovarManualmente(fechaInvalida));

        Assert.Equal("nuevaFechaVencimiento", excepcion.ParamName);
        Assert.Contains("debe ser en el futuro", excepcion.Message);
    }
}