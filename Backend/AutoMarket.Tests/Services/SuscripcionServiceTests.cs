using Moq;
using Xunit;
using AutoMarket.Application.Services;
using AutoMarket.Core.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;

namespace AutoMarket.Tests.Services;

public class SuscripcionServiceTests
{
    private readonly Mock<ISuscripcionRepository> _mockRepo;
    private readonly SuscripcionService _servicio;

    public SuscripcionServiceTests()
    {
        _mockRepo = new Mock<ISuscripcionRepository>();
        _servicio = new SuscripcionService(_mockRepo.Object);
    }

    // =========================================================================
    // HELPER: Crear Entidades Encapsuladas para Tests
    // =========================================================================
    private SuscripcionDealer CrearSuscripcionSimulada(
        int perfilDealerId,
        PlanNivel nivel,
        EstadoSuscripcion estado,
        CicloFacturacion ciclo = CicloFacturacion.Mensual)
    {
        var suscripcion = new SuscripcionDealer(perfilDealerId, nivel, ciclo);

        var propEstado = typeof(SuscripcionDealer).GetProperty("Estado");
        propEstado?.SetValue(suscripcion, estado);

        return suscripcion;
    }

    // =========================================================================
    // PRUEBA 01: Asignar Plan Inicial - Fallo (Ya tiene suscripción)
    // =========================================================================
    [Fact]
    public async Task AsignarPlanInicialAsync_SuscripcionExistente_DebeLanzarBusinessRuleException()
    {
        // Arrange
        int perfilId = 1;
        var suscripcionExistente = CrearSuscripcionSimulada(perfilId, PlanNivel.Basico, EstadoSuscripcion.Activa);

        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync(suscripcionExistente);

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _servicio.AsignarPlanInicialAsync(perfilId, PlanNivel.Basico, CicloFacturacion.Mensual));

        Assert.Equal("El dealer ya posee una suscripción registrada.", excepcion.Message);
        _mockRepo.Verify(r => r.AgregarAsync(It.IsAny<SuscripcionDealer>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 02: Asignar Plan Inicial - Éxito
    // =========================================================================
    [Fact]
    public async Task AsignarPlanInicialAsync_SinSuscripcionPrevia_DebeAgregarSuscripcion()
    {
        // Arrange
        int perfilId = 2;
        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync((SuscripcionDealer?)null);

        // Act
        await _servicio.AsignarPlanInicialAsync(perfilId, PlanNivel.Basico, CicloFacturacion.Mensual);

        // Assert
        _mockRepo.Verify(r => r.AgregarAsync(It.Is<SuscripcionDealer>(s =>
            s.PerfilDealerId == perfilId &&
            s.Nivel == PlanNivel.Basico &&
            s.Ciclo == CicloFacturacion.Mensual)), Times.Once);
    }

    // =========================================================================
    // PRUEBA 03: Cambiar Plan - Fallo (Regla 1: El Fantasma)
    // =========================================================================
    [Fact]
    public async Task CambiarPlanAsync_SuscripcionNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        int perfilId = 3;
        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync((SuscripcionDealer?)null);

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _servicio.CambiarPlanAsync(perfilId, PlanNivel.Pro, CicloFacturacion.Mensual));

        Assert.Equal("No se encontró una suscripción activa para este dealer.", excepcion.Message);
    }

    // =========================================================================
    // PRUEBA 04: Cambiar Plan - Fallo (Regla 2: El Cobro Doble)
    // =========================================================================
    [Fact]
    public async Task CambiarPlanAsync_MismoPlan_DebeLanzarBusinessRuleException()
    {
        // Arrange
        int perfilId = 4;
        var suscripcionActual = CrearSuscripcionSimulada(perfilId, PlanNivel.Pro, EstadoSuscripcion.Activa);

        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync(suscripcionActual);

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _servicio.CambiarPlanAsync(perfilId, PlanNivel.Pro, CicloFacturacion.Mensual));

        Assert.Contains("ya se encuentra suscrito a este plan", excepcion.Message);
        Assert.Contains("mismo ciclo", excepcion.Message);
        _mockRepo.Verify(r => r.ActualizarAsync(It.IsAny<SuscripcionDealer>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 05: Cambiar Plan - Fallo (Regla 3: El Moroso)
    // =========================================================================
    [Fact]
    public async Task CambiarPlanAsync_SuscripcionCancelada_DebeLanzarBusinessRuleException()
    {
        // Arrange
        int perfilId = 5;
        var suscripcionMorosa = CrearSuscripcionSimulada(perfilId, PlanNivel.Basico, EstadoSuscripcion.Cancelada);

        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync(suscripcionMorosa);

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _servicio.CambiarPlanAsync(perfilId, PlanNivel.Pro, CicloFacturacion.Mensual));

        Assert.Equal("La suscripción está cancelada. Debe adquirir una nueva en lugar de cambiar de plan.", excepcion.Message);
    }

    // =========================================================================
    // PRUEBA 06: Cambiar Plan - Éxito (Mutación Segura)
    // =========================================================================
    [Fact]
    public async Task CambiarPlanAsync_DatosValidos_DebeActualizarNivelYCicloYGuardar()
    {
        // Arrange
        int perfilId = 6;
        var suscripcionValida = CrearSuscripcionSimulada(perfilId, PlanNivel.Basico, EstadoSuscripcion.Activa);

        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync(suscripcionValida);

        // Act
        await _servicio.CambiarPlanAsync(perfilId, PlanNivel.Pro, CicloFacturacion.Anual);

        // Assert
        Assert.Equal(PlanNivel.Pro, suscripcionValida.Nivel);
        Assert.Equal(CicloFacturacion.Anual, suscripcionValida.Ciclo);
        _mockRepo.Verify(r => r.ActualizarAsync(suscripcionValida), Times.Once);
    }

    // =========================================================================
    // PRUEBA 07: Procesar Pago - Sin suscripción previa crea nueva
    // =========================================================================
    [Fact]
    public async Task ProcesarPagoSuscripcionAsync_SinSuscripcion_DebeCrearNueva()
    {
        // Arrange
        int perfilId = 10;

        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync((SuscripcionDealer?)null);

        // Act
        await _servicio.ProcesarPagoSuscripcionAsync(perfilId, PlanNivel.Pro, CicloFacturacion.Mensual);

        // Assert
        _mockRepo.Verify(r => r.AgregarAsync(It.Is<SuscripcionDealer>(s =>
            s.PerfilDealerId == perfilId &&
            s.Nivel == PlanNivel.Pro &&
            s.Ciclo == CicloFacturacion.Mensual)), Times.Once);

        _mockRepo.Verify(r => r.ActualizarAsync(It.IsAny<SuscripcionDealer>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 08: Procesar Pago - Mismo plan y ciclo renueva
    // =========================================================================
    [Fact]
    public async Task ProcesarPagoSuscripcionAsync_MismoPlanYCiclo_DebeRenovarYActualizar()
    {
        // Arrange
        int perfilId = 11;
        var suscripcion = CrearSuscripcionSimulada(
            perfilId,
            PlanNivel.Pro,
            EstadoSuscripcion.Activa,
            CicloFacturacion.Mensual);

        var vencimientoAnterior = suscripcion.FechaVencimientoUtc;

        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync(suscripcion);

        // Act
        await _servicio.ProcesarPagoSuscripcionAsync(perfilId, PlanNivel.Pro, CicloFacturacion.Mensual);

        // Assert
        Assert.True(suscripcion.FechaVencimientoUtc >= vencimientoAnterior);
        _mockRepo.Verify(r => r.ActualizarAsync(suscripcion), Times.Once);
        _mockRepo.Verify(r => r.AgregarAsync(It.IsAny<SuscripcionDealer>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 09: Procesar Pago - Plan o ciclo distinto cambia plan
    // =========================================================================
    [Fact]
    public async Task ProcesarPagoSuscripcionAsync_PlanOCicloDistinto_DebeCambiarPlanYActualizar()
    {
        // Arrange
        int perfilId = 12;
        var suscripcion = CrearSuscripcionSimulada(
            perfilId,
            PlanNivel.Basico,
            EstadoSuscripcion.Activa,
            CicloFacturacion.Mensual);

        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync(suscripcion);

        // Act
        await _servicio.ProcesarPagoSuscripcionAsync(perfilId, PlanNivel.Elite, CicloFacturacion.Anual);

        // Assert
        Assert.Equal(PlanNivel.Elite, suscripcion.Nivel);
        Assert.Equal(CicloFacturacion.Anual, suscripcion.Ciclo);
        _mockRepo.Verify(r => r.ActualizarAsync(suscripcion), Times.Once);
    }

    // =========================================================================
    // PRUEBA 10: Procesar Pago - Suscripción cancelada falla
    // =========================================================================
    [Fact]
    public async Task ProcesarPagoSuscripcionAsync_SuscripcionCancelada_DebeLanzarBusinessRuleException()
    {
        // Arrange
        int perfilId = 13;
        var suscripcionCancelada = CrearSuscripcionSimulada(
            perfilId,
            PlanNivel.Basico,
            EstadoSuscripcion.Cancelada,
            CicloFacturacion.Mensual);

        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync(suscripcionCancelada);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _servicio.ProcesarPagoSuscripcionAsync(perfilId, PlanNivel.Basico, CicloFacturacion.Mensual));

        _mockRepo.Verify(r => r.ActualizarAsync(It.IsAny<SuscripcionDealer>()), Times.Never);
        _mockRepo.Verify(r => r.AgregarAsync(It.IsAny<SuscripcionDealer>()), Times.Never);
    }

        // =========================================================================
    // PRUEBA 11: Renovar Manual - Fallo si no existe suscripción
    // =========================================================================
    [Fact]
    public async Task RenovarManualAsync_SuscripcionNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        int perfilId = 20;
        var nuevaFecha = DateTime.UtcNow.AddMonths(1);

        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync((SuscripcionDealer?)null);

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _servicio.RenovarManualAsync(perfilId, nuevaFecha));

        Assert.Equal("No se encontró una suscripción para este dealer.", excepcion.Message);
        _mockRepo.Verify(r => r.ActualizarAsync(It.IsAny<SuscripcionDealer>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 12: Renovar Manual - Éxito
    // =========================================================================
    [Fact]
    public async Task RenovarManualAsync_SuscripcionExiste_DebeActualizarVencimientoYGuardar()
    {
        // Arrange
        int perfilId = 21;
        var suscripcion = CrearSuscripcionSimulada(
            perfilId,
            PlanNivel.Pro,
            EstadoSuscripcion.Activa,
            CicloFacturacion.Mensual);

        var nuevaFecha = DateTime.UtcNow.AddMonths(4);

        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync(suscripcion);

        // Act
        await _servicio.RenovarManualAsync(perfilId, nuevaFecha);

        // Assert
        Assert.Equal(nuevaFecha, suscripcion.FechaVencimientoUtc);
        Assert.Equal(EstadoSuscripcion.Activa, suscripcion.Estado);
        _mockRepo.Verify(r => r.ActualizarAsync(suscripcion), Times.Once);
    }

    // =========================================================================
    // PRUEBA 13: Procesar Pago - Mismo plan y ciclo con suscripción vencida
    // =========================================================================
    [Fact]
    public async Task ProcesarPagoSuscripcionAsync_MismoPlanYCiclo_ConSuscripcionVencida_DebeRenovarDesdeAhora()
    {
        // Arrange
        int perfilId = 22;
        var suscripcion = CrearSuscripcionSimulada(
            perfilId,
            PlanNivel.Pro,
            EstadoSuscripcion.Activa,
            CicloFacturacion.Mensual);

        var propFechaVencimiento = typeof(SuscripcionDealer).GetProperty("FechaVencimientoUtc");
        propFechaVencimiento?.SetValue(suscripcion, DateTime.UtcNow.AddDays(-10));

        var antesDeProcesar = DateTime.UtcNow;

        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync(suscripcion);

        // Act
        await _servicio.ProcesarPagoSuscripcionAsync(perfilId, PlanNivel.Pro, CicloFacturacion.Mensual);

        // Assert
        Assert.True(suscripcion.FechaVencimientoUtc > antesDeProcesar);
        _mockRepo.Verify(r => r.ActualizarAsync(suscripcion), Times.Once);
        _mockRepo.Verify(r => r.AgregarAsync(It.IsAny<SuscripcionDealer>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 14: Renovar Manual - Fallo si la nueva fecha no es futura
    // =========================================================================
    [Fact]
    public async Task RenovarManualAsync_FechaInvalida_DebePropagarArgumentException()
    {
        // Arrange
        int perfilId = 23;
        var suscripcion = CrearSuscripcionSimulada(
            perfilId,
            PlanNivel.Basico,
            EstadoSuscripcion.Activa,
            CicloFacturacion.Mensual);

        var fechaInvalida = DateTime.UtcNow.AddMinutes(-1);

        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync(suscripcion);

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.RenovarManualAsync(perfilId, fechaInvalida));

        Assert.Equal("nuevaFechaVencimiento", excepcion.ParamName);
        Assert.Contains("debe ser en el futuro", excepcion.Message);
        _mockRepo.Verify(r => r.ActualizarAsync(It.IsAny<SuscripcionDealer>()), Times.Never);
    }
}