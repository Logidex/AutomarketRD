using Moq;
using Xunit;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.Services;
using AutoMarket.Core.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;

namespace AutoMarket.Tests.Services;

public class SuscripcionServiceTests
{
    private readonly Mock<ISuscripcionRepository> _mockRepo;
    private readonly Mock<IAnuncioRepository> _mockAnuncioRepo;
    private readonly Mock<IPayPalService> _mockPayPalService;
    private readonly Mock<IPlanCatalogoRepository> _mockPlanCatalogoRepo;
    private readonly SuscripcionService _servicio;

    public SuscripcionServiceTests()
    {
        _mockRepo = new Mock<ISuscripcionRepository>();
        _mockAnuncioRepo = new Mock<IAnuncioRepository>();
        _mockPayPalService = new Mock<IPayPalService>();
        _mockPlanCatalogoRepo = new Mock<IPlanCatalogoRepository>();
        _servicio = new SuscripcionService(_mockRepo.Object, _mockAnuncioRepo.Object, _mockPayPalService.Object, _mockPlanCatalogoRepo.Object);
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
    // PRUEBA 10: Procesar Pago - Suscripción cancelada se reactiva
    // =========================================================================
    [Fact]
    public async Task ProcesarPagoSuscripcionAsync_SuscripcionCancelada_DebeReactivar()
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

        // Act
        await _servicio.ProcesarPagoSuscripcionAsync(perfilId, PlanNivel.Elite, CicloFacturacion.Anual);

        // Assert
        Assert.Equal(EstadoSuscripcion.Activa, suscripcionCancelada.Estado);
        Assert.Equal(PlanNivel.Elite, suscripcionCancelada.Nivel);
        Assert.Equal(CicloFacturacion.Anual, suscripcionCancelada.Ciclo);
        _mockRepo.Verify(r => r.ActualizarAsync(suscripcionCancelada), Times.Once);
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

    // =========================================================================
    // PRUEBA 15: Cancelar - Fallo si no existe suscripción
    // =========================================================================
    [Fact]
    public async Task CancelarSuscripcionAsync_SuscripcionNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        int perfilId = 30;
        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync((SuscripcionDealer?)null);

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _servicio.CancelarSuscripcionAsync(perfilId));

        Assert.Equal("No se encontró una suscripción para este dealer.", excepcion.Message);
        _mockRepo.Verify(r => r.ActualizarAsync(It.IsAny<SuscripcionDealer>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 16: Cancelar - Éxito
    // =========================================================================
    [Fact]
    public async Task CancelarSuscripcionAsync_SuscripcionActiva_DebeMarcarComoCanceladaYGuardar()
    {
        // Arrange
        int perfilId = 31;
        var suscripcion = CrearSuscripcionSimulada(perfilId, PlanNivel.Pro, EstadoSuscripcion.Activa);

        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync(suscripcion);

        // Act
        await _servicio.CancelarSuscripcionAsync(perfilId);

        // Assert
        Assert.Equal(EstadoSuscripcion.Cancelada, suscripcion.Estado);
        _mockRepo.Verify(r => r.ActualizarAsync(suscripcion), Times.Once);
    }

    // =========================================================================
    // PRUEBA 17: Cancelar - Fallo si ya está cancelada
    // =========================================================================
    [Fact]
    public async Task CancelarSuscripcionAsync_SuscripcionYaCancelada_DebeLanzarBusinessRuleException()
    {
        // Arrange
        int perfilId = 32;
        var suscripcion = CrearSuscripcionSimulada(perfilId, PlanNivel.Basico, EstadoSuscripcion.Cancelada);

        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync(suscripcion);

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _servicio.CancelarSuscripcionAsync(perfilId));

        Assert.Equal("La suscripción ya se encuentra cancelada.", excepcion.Message);
        _mockRepo.Verify(r => r.ActualizarAsync(It.IsAny<SuscripcionDealer>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 18: Obtener Suscripción - No existe retorna null
    // =========================================================================
    [Fact]
    public async Task ObtenerSuscripcionAsync_SuscripcionNoExiste_DebeRetornarNull()
    {
        // Arrange
        int perfilId = 33;
        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync((SuscripcionDealer?)null);

        // Act
        var resultado = await _servicio.ObtenerSuscripcionAsync(perfilId);

        // Assert
        Assert.Null(resultado);
    }

    // =========================================================================
    // PRUEBA 19: Obtener Suscripción - Éxito mapea DTO
    // =========================================================================
    [Fact]
    public async Task ObtenerSuscripcionAsync_SuscripcionActiva_DebeRetornarDtoCorrecto()
    {
        // Arrange
        int perfilId = 34;
        var suscripcion = CrearSuscripcionSimulada(perfilId, PlanNivel.Elite, EstadoSuscripcion.Activa);

        _mockRepo.Setup(r => r.ObtenerPorDealerIdAsync(perfilId))
            .ReturnsAsync(suscripcion);

        // Act
        var resultado = await _servicio.ObtenerSuscripcionAsync(perfilId);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(perfilId, resultado!.PerfilDealerId);
        Assert.Equal(PlanNivel.Elite, resultado.Nivel);
        Assert.Equal(EstadoSuscripcion.Activa, resultado.Estado);
        Assert.Equal(PlanConfig.LimiteAnuncios(PlanNivel.Elite), resultado.LimiteAnuncios);
        Assert.True(resultado.Activa);
    }

    // =========================================================================
    // PRUEBA 20: Registrar Pago - Éxito guarda todos los datos
    // =========================================================================
    [Fact]
    public async Task RegistrarPagoAsync_DatosValidos_DebeGuardarPagoEnRepositorio()
    {
        // Arrange
        int perfilId = 40;

        // Act
        await _servicio.RegistrarPagoAsync(
            perfilId,
            PlanNivel.Elite,
            CicloFacturacion.Anual,
            120m,
            "usd",
            "ORDER-123",
            "EVENTO-1",
            "CAPTURE-1",
            "DEALER-40-PLAN-Elite-CICLO-Anual");

        // Assert
        _mockRepo.Verify(r => r.AgregarPagoAsync(It.Is<PagoSuscripcion>(p =>
            p.PerfilDealerId == perfilId &&
            p.Nivel == PlanNivel.Elite &&
            p.Ciclo == CicloFacturacion.Anual &&
            p.Monto == 120m &&
            p.Moneda == "USD" &&
            p.OrderIdPayPal == "ORDER-123" &&
            p.EventoIdPayPal == "EVENTO-1" &&
            p.CaptureIdPayPal == "CAPTURE-1" &&
            p.Estado == EstadoPago.Completado)), Times.Once);
    }

    // =========================================================================
    // PRUEBA 21: Registrar Pago - Fallo si el monto no es positivo
    // =========================================================================
    [Fact]
    public async Task RegistrarPagoAsync_MontoInvalido_DebeLanzarArgumentException()
    {
        // Arrange, Act & Assert
        var excepcion = await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.RegistrarPagoAsync(
                40,
                PlanNivel.Basico,
                CicloFacturacion.Mensual,
                0m,
                "USD",
                null,
                null,
                null,
                null));

        Assert.Equal("monto", excepcion.ParamName);
        _mockRepo.Verify(r => r.AgregarPagoAsync(It.IsAny<PagoSuscripcion>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 22: Obtener Historial de Pagos - Mapea el DTO correctamente
    // =========================================================================
    [Fact]
    public async Task ObtenerHistorialPagosAsync_DebeRetornarDtoMapeado()
    {
        // Arrange
        int perfilId = 41;
        var pago = new PagoSuscripcion(
            perfilId,
            PlanNivel.Pro,
            CicloFacturacion.Trimestral,
            50m,
            "USD",
            "ORDER-77",
            null,
            null,
            "DEALER-41-PLAN-Pro-CICLO-Trimestral");

        _mockRepo.Setup(r => r.ObtenerHistorialPagosAsync(perfilId))
            .ReturnsAsync(new[] { pago });

        // Act
        var resultado = await _servicio.ObtenerHistorialPagosAsync(perfilId);

        // Assert
        var dto = Assert.Single(resultado);
        Assert.Equal(perfilId, dto.PerfilDealerId);
        Assert.Equal(PlanNivel.Pro, dto.Nivel);
        Assert.Equal(CicloFacturacion.Trimestral, dto.Ciclo);
        Assert.Equal(EstadoPago.Completado, dto.Estado);
        Assert.Equal(50m, dto.Monto);
        Assert.Equal("USD", dto.Moneda);
        Assert.Equal("ORDER-77", dto.OrdenIdPayPal);
    }

    // =========================================================================
    // PRUEBA 23: Reembolsar Pago - Con CaptureId, reembolsa y guarda
    // =========================================================================
    [Fact]
    public async Task ReembolsarPagoAsync_PagoConCaptureId_DebeReembolsarYGuardar()
    {
        // Arrange
        var pago = new PagoSuscripcion(
            15,
            PlanNivel.Pro,
            CicloFacturacion.Mensual,
            30m,
            "USD",
            "ord-999",
            "evt-1",
            "cap-999",
            "DEALER-15-PLAN-PRO-CICLO-MENSUAL");

        _mockRepo.Setup(r => r.ObtenerPagoPorIdAsync(15)).ReturnsAsync(pago);
        _mockPayPalService.Setup(s => s.ReembolsarAsync("cap-999", 30m, "USD")).ReturnsAsync(true);

        // Act
        await _servicio.ReembolsarPagoAsync(15);

        // Assert
        Assert.Equal(EstadoPago.Reembolsado, pago.Estado);
        _mockPayPalService.Verify(s => s.ReembolsarAsync("cap-999", 30m, "USD"), Times.Once);
        _mockRepo.Verify(r => r.ActualizarPagoAsync(pago), Times.Once);
    }

    // =========================================================================
    // PRUEBA 24: Reembolsar Pago - Sin capture ni orden, lanza regla de negocio
    // =========================================================================
    [Fact]
    public async Task ReembolsarPagoAsync_SinCaptureIdNiOrden_DebeLanzarBusinessRuleException()
    {
        // Arrange
        var pago = new PagoSuscripcion(
            15,
            PlanNivel.Pro,
            CicloFacturacion.Mensual,
            30m,
            "USD",
            null,
            null,
            null,
            null);

        _mockRepo.Setup(r => r.ObtenerPagoPorIdAsync(15)).ReturnsAsync(pago);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _servicio.ReembolsarPagoAsync(15));
        Assert.Contains("captura", ex.Message, StringComparison.OrdinalIgnoreCase);
        _mockRepo.Verify(r => r.ActualizarPagoAsync(It.IsAny<PagoSuscripcion>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 25: Reembolsar Pago - Sin capture pero con orden, recupera de PayPal
    // =========================================================================
    [Fact]
    public async Task ReembolsarPagoAsync_SinCaptureIdConOrden_DebeRecuperarDePayPalYReembolsar()
    {
        // Arrange
        var pago = new PagoSuscripcion(
            15,
            PlanNivel.Pro,
            CicloFacturacion.Mensual,
            30m,
            "USD",
            "ord-999",
            null,
            null,
            "DEALER-15-PLAN-PRO-CICLO-MENSUAL");

        _mockRepo.Setup(r => r.ObtenerPagoPorIdAsync(15)).ReturnsAsync(pago);
        _mockPayPalService.Setup(s => s.ObtenerCaptureIdDeOrdenAsync("ord-999")).ReturnsAsync("cap-rec");
        _mockPayPalService.Setup(s => s.ReembolsarAsync("cap-rec", 30m, "USD")).ReturnsAsync(true);

        // Act
        await _servicio.ReembolsarPagoAsync(15);

        // Assert
        Assert.Equal(EstadoPago.Reembolsado, pago.Estado);
        Assert.Equal("cap-rec", pago.CaptureIdPayPal);
        _mockRepo.Verify(r => r.ActualizarPagoAsync(pago), Times.Once);
    }

    // =========================================================================
    // PRUEBA 26: Reembolsar Pago - Ya reembolsado, lanza regla de negocio
    // =========================================================================
    [Fact]
    public async Task ReembolsarPagoAsync_YaReembolsado_DebeLanzarBusinessRuleException()
    {
        // Arrange
        var pago = new PagoSuscripcion(
            15,
            PlanNivel.Pro,
            CicloFacturacion.Mensual,
            30m,
            "USD",
            "ord-999",
            null,
            "cap-999",
            null);

        pago.MarcarComoReembolsado();
        _mockRepo.Setup(r => r.ObtenerPagoPorIdAsync(15)).ReturnsAsync(pago);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _servicio.ReembolsarPagoAsync(15));
        Assert.Contains("reembolsado", ex.Message, StringComparison.OrdinalIgnoreCase);
        _mockPayPalService.Verify(s => s.ReembolsarAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 27: Reembolsar Pago - PayPal rechaza, lanza regla de negocio
    // =========================================================================
    [Fact]
    public async Task ReembolsarPagoAsync_PayPalRechaza_DebeLanzarBusinessRuleException()
    {
        // Arrange
        var pago = new PagoSuscripcion(
            15,
            PlanNivel.Pro,
            CicloFacturacion.Mensual,
            30m,
            "USD",
            "ord-999",
            null,
            "cap-999",
            null);

        _mockRepo.Setup(r => r.ObtenerPagoPorIdAsync(15)).ReturnsAsync(pago);
        _mockPayPalService.Setup(s => s.ReembolsarAsync("cap-999", 30m, "USD")).ReturnsAsync(false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _servicio.ReembolsarPagoAsync(15));
        Assert.Contains("rechazó", ex.Message, StringComparison.OrdinalIgnoreCase);
        _mockRepo.Verify(r => r.ActualizarPagoAsync(It.IsAny<PagoSuscripcion>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 28: Reembolsar Pago - Pago inexistente, lanza KeyNotFoundException
    // =========================================================================
    [Fact]
    public async Task ReembolsarPagoAsync_PagoNoExiste_DebeLanzarKeyNotFoundException()
    {
        _mockRepo.Setup(r => r.ObtenerPagoPorIdAsync(99)).ReturnsAsync((PagoSuscripcion?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _servicio.ReembolsarPagoAsync(99));
    }

    // =========================================================================
    // PRUEBA 29: Obtener Pagos Admin - Mapea los datos del dealer
    // =========================================================================
    [Fact]
    public async Task ObtenerPagosAdminAsync_DebeRetornarDtoConDatosDelDealer()
    {
        // Arrange
        var usuario = new Usuario(
            nombre: "Erick",
            apellido: "Hipolito",
            email: "erick@test.com",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("ClaveSegura123"),
            rol: "Dealer",
            telefonoPersonal: "8095555555"
        );

        var perfilDealer = (PerfilDealer)Activator.CreateInstance(typeof(PerfilDealer), nonPublic: true)!;
        typeof(PerfilDealer).GetProperty("UsuarioId")?.SetValue(perfilDealer, 15);
        typeof(PerfilDealer).GetProperty("NombreAgencia")?.SetValue(perfilDealer, "Agencia Erick");
        typeof(PerfilDealer).GetProperty("Usuario")?.SetValue(perfilDealer, usuario);
        typeof(Usuario).GetProperty("PerfilDealer")?.SetValue(usuario, perfilDealer);

        var pago = new PagoSuscripcion(
            15,
            PlanNivel.Pro,
            CicloFacturacion.Mensual,
            30m,
            "USD",
            "ord-999",
            "evt-1",
            "cap-999",
            "DEALER-15-PLAN-PRO-CICLO-MENSUAL");

        typeof(PagoSuscripcion).GetProperty("Id")?.SetValue(pago, 7);
        typeof(PagoSuscripcion).GetProperty("PerfilDealer")?.SetValue(pago, perfilDealer);

        _mockRepo.Setup(r => r.ObtenerTodosLosPagosAsync()).ReturnsAsync(new[] { pago });

        // Act
        var resultado = await _servicio.ObtenerPagosAdminAsync();

        // Assert
        var dto = Assert.Single(resultado);
        Assert.Equal(7, dto.Id);
        Assert.Equal(15, dto.PerfilDealerId);
        Assert.Equal("Agencia Erick", dto.DealerNombreAgencia);
        Assert.Equal("erick@test.com", dto.DealerEmail);
        Assert.Equal(PlanNivel.Pro, dto.Nivel);
        Assert.Equal(CicloFacturacion.Mensual, dto.Ciclo);
        Assert.Equal(EstadoPago.Completado, dto.Estado);
        Assert.Equal(30m, dto.Monto);
        Assert.Equal("USD", dto.Moneda);
        Assert.Equal("ord-999", dto.OrdenIdPayPal);
        Assert.Equal("cap-999", dto.CaptureIdPayPal);
    }
}