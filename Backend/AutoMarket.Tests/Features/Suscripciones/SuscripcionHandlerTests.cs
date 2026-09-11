using Xunit;
using Moq;
using AutoMarket.Application.Features.Suscripciones.Handlers;
using AutoMarket.Application.Features.Suscripciones.Commands;
using AutoMarket.Application.Features.Suscripciones.Queries;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs.Admin;
using AutoMarket.Application.DTOs.Suscripcion;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Tests.Features.Suscripciones;

public class SuscripcionHandlerTests
{
    private readonly Mock<ISuscripcionService> _mockService;
    private readonly SuscripcionCommandHandler _commandHandler;
    private readonly SuscripcionQueryHandler _queryHandler;

    public SuscripcionHandlerTests()
    {
        _mockService = new Mock<ISuscripcionService>();
        _commandHandler = new SuscripcionCommandHandler(_mockService.Object);
        _queryHandler = new SuscripcionQueryHandler(_mockService.Object);
    }

    // ==================== CommandHandler Tests ====================

    [Fact]
    public async Task Handle_AsignarPlanInicial_DebeLlamarServicio()
    {
        var command = new AsignarPlanInicialCommand(10, PlanNivel.Basico, CicloFacturacion.Mensual);
        _mockService
            .Setup(s => s.AsignarPlanInicialAsync(10, PlanNivel.Basico, CicloFacturacion.Mensual))
            .Returns(Task.CompletedTask);

        await _commandHandler.Handle(command, CancellationToken.None);

        _mockService.Verify(s => s.AsignarPlanInicialAsync(10, PlanNivel.Basico, CicloFacturacion.Mensual), Times.Once);
    }

    [Fact]
    public async Task Handle_CambiarPlan_DebeLlamarServicio()
    {
        var command = new CambiarPlanCommand(10, PlanNivel.Pro, CicloFacturacion.Trimestral);
        _mockService
            .Setup(s => s.CambiarPlanAsync(10, PlanNivel.Pro, CicloFacturacion.Trimestral))
            .Returns(Task.CompletedTask);

        await _commandHandler.Handle(command, CancellationToken.None);

        _mockService.Verify(s => s.CambiarPlanAsync(10, PlanNivel.Pro, CicloFacturacion.Trimestral), Times.Once);
    }

    [Fact]
    public async Task Handle_RenovarManual_DebeLlamarServicio()
    {
        var fecha = new DateTime(2027, 1, 1);
        var command = new RenovarManualCommand(10, fecha);
        _mockService
            .Setup(s => s.RenovarManualAsync(10, fecha))
            .Returns(Task.CompletedTask);

        await _commandHandler.Handle(command, CancellationToken.None);

        _mockService.Verify(s => s.RenovarManualAsync(10, fecha), Times.Once);
    }

    [Fact]
    public async Task Handle_ProcesarPagoSuscripcion_DebeLlamarServicio()
    {
        var command = new ProcesarPagoSuscripcionCommand(10, PlanNivel.Elite, CicloFacturacion.Anual);
        _mockService
            .Setup(s => s.ProcesarPagoSuscripcionAsync(10, PlanNivel.Elite, CicloFacturacion.Anual))
            .Returns(Task.CompletedTask);

        await _commandHandler.Handle(command, CancellationToken.None);

        _mockService.Verify(s => s.ProcesarPagoSuscripcionAsync(10, PlanNivel.Elite, CicloFacturacion.Anual), Times.Once);
    }

    [Fact]
    public async Task Handle_CancelarSuscripcion_DebeLlamarServicio()
    {
        var command = new CancelarSuscripcionCommand(10);
        _mockService
            .Setup(s => s.CancelarSuscripcionAsync(10))
            .Returns(Task.CompletedTask);

        await _commandHandler.Handle(command, CancellationToken.None);

        _mockService.Verify(s => s.CancelarSuscripcionAsync(10), Times.Once);
    }

    [Fact]
    public async Task Handle_ReembolsarPago_DebeRetornarMetodoPago()
    {
        var command = new ReembolsarPagoCommand(42);
        _mockService.Setup(s => s.ReembolsarPagoAsync(42)).ReturnsAsync(MetodoPago.PayPal);

        var result = await _commandHandler.Handle(command, CancellationToken.None);

        Assert.Equal(MetodoPago.PayPal, result);
        _mockService.Verify(s => s.ReembolsarPagoAsync(42), Times.Once);
    }

    [Fact]
    public async Task Handle_ReembolsarPago_Transferencia_DebeRetornarMetodoTransferencia()
    {
        var command = new ReembolsarPagoCommand(55);
        _mockService.Setup(s => s.ReembolsarPagoAsync(55)).ReturnsAsync(MetodoPago.Transferencia);

        var result = await _commandHandler.Handle(command, CancellationToken.None);

        Assert.Equal(MetodoPago.Transferencia, result);
        _mockService.Verify(s => s.ReembolsarPagoAsync(55), Times.Once);
    }

    [Fact]
    public async Task Handle_RegistrarPago_DebeLlamarServicio()
    {
        var command = new RegistrarPagoCommand(
            10, PlanNivel.Basico, CicloFacturacion.Mensual,
            500.00m, "USD", "order-123", "event-456", "capture-789", "Referencia test");
        _mockService
            .Setup(s => s.RegistrarPagoAsync(
                10, PlanNivel.Basico, CicloFacturacion.Mensual,
                500.00m, "USD", "order-123", "event-456", "capture-789", "Referencia test"))
            .Returns(Task.CompletedTask);

        await _commandHandler.Handle(command, CancellationToken.None);

        _mockService.Verify(s => s.RegistrarPagoAsync(
            10, PlanNivel.Basico, CicloFacturacion.Mensual,
            500.00m, "USD", "order-123", "event-456", "capture-789", "Referencia test"), Times.Once);
    }

    [Fact]
    public async Task Handle_RegistrarPagoTransferencia_DebeRetornarId()
    {
        var command = new RegistrarPagoTransferenciaCommand(
            10, PlanNivel.Pro, CicloFacturacion.Mensual,
            1500.00m, "DOP", "https://captura.com/img.jpg");
        _mockService
            .Setup(s => s.RegistrarPagoTransferenciaAsync(
                10, PlanNivel.Pro, CicloFacturacion.Mensual,
                1500.00m, "DOP", "https://captura.com/img.jpg"))
            .ReturnsAsync(77);

        var result = await _commandHandler.Handle(command, CancellationToken.None);

        Assert.Equal(77, result);
        _mockService.Verify(s => s.RegistrarPagoTransferenciaAsync(
            10, PlanNivel.Pro, CicloFacturacion.Mensual,
            1500.00m, "DOP", "https://captura.com/img.jpg"), Times.Once);
    }

    [Fact]
    public async Task Handle_AprobarTransferencia_DebeLlamarServicio()
    {
        var command = new AprobarTransferenciaCommand(30, "Pago verificado");
        _mockService
            .Setup(s => s.AprobarTransferenciaAsync(30, "Pago verificado"))
            .Returns(Task.CompletedTask);

        await _commandHandler.Handle(command, CancellationToken.None);

        _mockService.Verify(s => s.AprobarTransferenciaAsync(30, "Pago verificado"), Times.Once);
    }

    [Fact]
    public async Task Handle_AprobarTransferencia_SinNotas_DebeLlamarServicio()
    {
        var command = new AprobarTransferenciaCommand(31, null);
        _mockService
            .Setup(s => s.AprobarTransferenciaAsync(31, null))
            .Returns(Task.CompletedTask);

        await _commandHandler.Handle(command, CancellationToken.None);

        _mockService.Verify(s => s.AprobarTransferenciaAsync(31, null), Times.Once);
    }

    [Fact]
    public async Task Handle_RechazarTransferencia_DebeLlamarServicio()
    {
        var command = new RechazarTransferenciaCommand(30, "Documentacion invalida");
        _mockService
            .Setup(s => s.RechazarTransferenciaAsync(30, "Documentacion invalida"))
            .Returns(Task.CompletedTask);

        await _commandHandler.Handle(command, CancellationToken.None);

        _mockService.Verify(s => s.RechazarTransferenciaAsync(30, "Documentacion invalida"), Times.Once);
    }

    [Fact]
    public async Task Handle_RechazarTransferencia_SinNotas_DebeLlamarServicio()
    {
        var command = new RechazarTransferenciaCommand(32, null);
        _mockService
            .Setup(s => s.RechazarTransferenciaAsync(32, null))
            .Returns(Task.CompletedTask);

        await _commandHandler.Handle(command, CancellationToken.None);

        _mockService.Verify(s => s.RechazarTransferenciaAsync(32, null), Times.Once);
    }

    // ==================== QueryHandler Tests ====================

    [Fact]
    public async Task Handle_ObtenerHistorialPagos_DebeRetornarLista()
    {
        var command = new ObtenerHistorialPagosQuery(10);
        var expected = new List<PagoSuscripcionDto>
        {
            new() { Id = 1, PerfilDealerId = 10, Nivel = PlanNivel.Basico, Monto = 500 },
            new() { Id = 2, PerfilDealerId = 10, Nivel = PlanNivel.Pro, Monto = 1500 }
        };
        _mockService.Setup(s => s.ObtenerHistorialPagosAsync(10)).ReturnsAsync(expected);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(10, p.PerfilDealerId));
        _mockService.Verify(s => s.ObtenerHistorialPagosAsync(10), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerSuscripcion_DebeRetornarSuscripcion()
    {
        var command = new ObtenerSuscripcionQuery(10);
        var expected = new SuscripcionDealerDto
        {
            PerfilDealerId = 10,
            Nivel = PlanNivel.Pro,
            Ciclo = CicloFacturacion.Mensual,
            Estado = EstadoSuscripcion.Activa,
            Activa = true,
            DiasRestantes = 25
        };
        _mockService.Setup(s => s.ObtenerSuscripcionAsync(10)).ReturnsAsync(expected);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(10, result!.PerfilDealerId);
        Assert.Equal(PlanNivel.Pro, result.Nivel);
        Assert.True(result.Activa);
        _mockService.Verify(s => s.ObtenerSuscripcionAsync(10), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerSuscripcion_NoExiste_DebeRetornarNull()
    {
        var command = new ObtenerSuscripcionQuery(999);
        _mockService.Setup(s => s.ObtenerSuscripcionAsync(999)).ReturnsAsync((SuscripcionDealerDto?)null);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.Null(result);
        _mockService.Verify(s => s.ObtenerSuscripcionAsync(999), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerSuscripcionPorUsuarioId_DebeRetornarSuscripcion()
    {
        var command = new ObtenerSuscripcionPorUsuarioIdQuery(55);
        var expected = new SuscripcionDealerDto
        {
            PerfilDealerId = 10,
            Nivel = PlanNivel.Elite,
            Estado = EstadoSuscripcion.Activa
        };
        _mockService.Setup(s => s.ObtenerSuscripcionPorUsuarioIdAsync(55)).ReturnsAsync(expected);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(PlanNivel.Elite, result!.Nivel);
        _mockService.Verify(s => s.ObtenerSuscripcionPorUsuarioIdAsync(55), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerSuscripcionPorUsuarioId_NoExiste_DebeRetornarNull()
    {
        var command = new ObtenerSuscripcionPorUsuarioIdQuery(999);
        _mockService
            .Setup(s => s.ObtenerSuscripcionPorUsuarioIdAsync(999))
            .ReturnsAsync((SuscripcionDealerDto?)null);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.Null(result);
        _mockService.Verify(s => s.ObtenerSuscripcionPorUsuarioIdAsync(999), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerPagosAdmin_DebeRetornarLista()
    {
        var command = new ObtenerPagosAdminQuery();
        var expected = new List<PagoAdminDto>
        {
            new() { Id = 1, PerfilDealerId = 10, Nivel = PlanNivel.Basico, Monto = 500 },
            new() { Id = 2, PerfilDealerId = 20, Nivel = PlanNivel.Pro, Monto = 1500 }
        };
        _mockService.Setup(s => s.ObtenerPagosAdminAsync()).ReturnsAsync(expected);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        _mockService.Verify(s => s.ObtenerPagosAdminAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerTransferenciasPendientes_DebeRetornarLista()
    {
        var command = new ObtenerTransferenciasPendientesQuery();
        var expected = new List<PagoAdminDto>
        {
            new() { Id = 1, PerfilDealerId = 10, Metodo = MetodoPago.Transferencia, EstadoTransferencia = EstadoTransferencia.Pendiente }
        };
        _mockService
            .Setup(s => s.ObtenerTransferenciasPendientesAsync())
            .ReturnsAsync(expected);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.All(result, p => Assert.Equal(MetodoPago.Transferencia, p.Metodo));
        _mockService.Verify(s => s.ObtenerTransferenciasPendientesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistePagoPorEvento_DebeRetornarTrue()
    {
        var command = new ExistePagoPorEventoQuery("evt-123");
        _mockService.Setup(s => s.ExistePagoPorEventoAsync("evt-123")).ReturnsAsync(true);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.True(result);
        _mockService.Verify(s => s.ExistePagoPorEventoAsync("evt-123"), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistePagoPorEvento_NoExiste_DebeRetornarFalse()
    {
        var command = new ExistePagoPorEventoQuery("evt-notfound");
        _mockService.Setup(s => s.ExistePagoPorEventoAsync("evt-notfound")).ReturnsAsync(false);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.False(result);
        _mockService.Verify(s => s.ExistePagoPorEventoAsync("evt-notfound"), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistePagoPorOrden_DebeRetornarTrue()
    {
        var command = new ExistePagoPorOrdenQuery("ord-456");
        _mockService.Setup(s => s.ExistePagoPorOrdenAsync("ord-456")).ReturnsAsync(true);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.True(result);
        _mockService.Verify(s => s.ExistePagoPorOrdenAsync("ord-456"), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistePagoPorOrden_NoExiste_DebeRetornarFalse()
    {
        var command = new ExistePagoPorOrdenQuery("ord-notfound");
        _mockService.Setup(s => s.ExistePagoPorOrdenAsync("ord-notfound")).ReturnsAsync(false);

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.False(result);
        _mockService.Verify(s => s.ExistePagoPorOrdenAsync("ord-notfound"), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerHistorialPagos_Vacio_DebeRetornarListaVacia()
    {
        var command = new ObtenerHistorialPagosQuery(999);
        _mockService
            .Setup(s => s.ObtenerHistorialPagosAsync(999))
            .ReturnsAsync(new List<PagoSuscripcionDto>());

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
        _mockService.Verify(s => s.ObtenerHistorialPagosAsync(999), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerPagosAdmin_Vacio_DebeRetornarListaVacia()
    {
        var command = new ObtenerPagosAdminQuery();
        _mockService
            .Setup(s => s.ObtenerPagosAdminAsync())
            .ReturnsAsync(new List<PagoAdminDto>());

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
        _mockService.Verify(s => s.ObtenerPagosAdminAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerTransferenciasPendientes_Vacio_DebeRetornarListaVacia()
    {
        var command = new ObtenerTransferenciasPendientesQuery();
        _mockService
            .Setup(s => s.ObtenerTransferenciasPendientesAsync())
            .ReturnsAsync(new List<PagoAdminDto>());

        var result = await _queryHandler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
        _mockService.Verify(s => s.ObtenerTransferenciasPendientesAsync(), Times.Once);
    }
}
