using System.Security.Claims;
using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs.Suscripcion;
using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Features.Dashboard.Queries;
using AutoMarket.Application.Features.PerfilDealer.Commands;
using AutoMarket.Application.Features.PerfilDealer.Queries;
using AutoMarket.Application.Features.Suscripciones.Commands;
using AutoMarket.Application.Features.Suscripciones.Queries;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AutoMarket.Tests.Controllers;

public class DealersControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly DealersController _controller;

    public DealersControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new DealersController(_mockMediator.Object);
    }

    // =========================================================================
    // HELPERS
    // =========================================================================

    private void SimularUsuarioAutenticado(string usuarioId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private static PerfilDealerPublicoDto CrearPerfilDto()
    {
        return new PerfilDealerPublicoDto
        {
            Id = 15,
            NombreAgencia = "AutoMarket RD",
            Ubicacion = "Santo Domingo",
            TelefonoAgencia = "8095554444",
            Descripcion = "Dealer de prueba"
        };
    }

    private static SuscripcionDealerDto CrearSuscripcionDto()
    {
        return new SuscripcionDealerDto
        {
            PerfilDealerId = 15,
            Nivel = PlanNivel.Pro,
            Ciclo = CicloFacturacion.Mensual,
            Estado = EstadoSuscripcion.Activa,
            LimiteAnuncios = (int)PlanNivel.Pro,
            FechaInicioUtc = DateTime.UtcNow,
            FechaVencimientoUtc = DateTime.UtcNow.AddMonths(1),
            Activa = true
        };
    }

    // =========================================================================
    // 1. OBTENER PERFIL PÚBLICO (anónimo)
    // =========================================================================

    [Fact]
    public async Task ObtenerPerfilPublico_NoExiste_DebeRetornarNotFound()
    {
        // Arrange
        _mockMediator
            .Setup(s => s.Send(It.IsAny<ObtenerPerfilPublicoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PerfilDealerPublicoDto?)null);

        // Act
        var resultado = await _controller.ObtenerPerfilPublico(99);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task ObtenerPerfilPublico_Existe_DebeRetornarOk()
    {
        // Arrange
        _mockMediator
            .Setup(s => s.Send(It.IsAny<ObtenerPerfilPublicoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CrearPerfilDto());

        // Act
        var resultado = await _controller.ObtenerPerfilPublico(15);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.IsType<PerfilDealerPublicoDto>(ok.Value);
    }

    // =========================================================================
    // 2. ACTUALIZAR MIS PERFIL
    // =========================================================================

    [Fact]
    public async Task ActualizarMiPerfil_SinToken_DebeRetornarUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };

        // Act
        var resultado = await _controller.ActualizarMiPerfil(new PerfilDealerUpdateDto());

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(resultado);
    }

    [Fact]
    public async Task ActualizarMiPerfil_SinPerfilAsociado_DebeRetornarNotFound()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockMediator
            .Setup(s => s.Send(It.IsAny<ActualizarMiPerfilCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PerfilDealerPublicoDto?)null);

        // Act
        var resultado = await _controller.ActualizarMiPerfil(new PerfilDealerUpdateDto());

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task ActualizarMiPerfil_DatosInvalidos_DebeRetornarBadRequest()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockMediator
            .Setup(s => s.Send(It.IsAny<ActualizarMiPerfilCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("El nombre de la agencia es obligatorio."));

        // Act
        var resultado = await _controller.ActualizarMiPerfil(new PerfilDealerUpdateDto());

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task ActualizarMiPerfil_Exitoso_DebeRetornarOk()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockMediator
            .Setup(s => s.Send(It.IsAny<ActualizarMiPerfilCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CrearPerfilDto());

        // Act
        var resultado = await _controller.ActualizarMiPerfil(new PerfilDealerUpdateDto());

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.IsType<PerfilDealerPublicoDto>(ok.Value);
    }

    // =========================================================================
    // 3. DASHBOARD RESUMEN
    // =========================================================================

    [Fact]
    public async Task ObtenerDashboardResumen_SinToken_DebeRetornarUnauthorized()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        // Act & Assert
        Assert.IsType<UnauthorizedObjectResult>(await _controller.ObtenerDashboardResumen());
    }

    [Fact]
    public async Task ObtenerDashboardResumen_ConToken_DebeRetornarResumen()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockMediator
            .Setup(s => s.Send(It.IsAny<ObtenerResumenDashboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AutoMarket.Application.DTOs.Admin.DashboardResumenDto());

        // Act
        var resultado = await _controller.ObtenerDashboardResumen();

        // Assert
        Assert.IsType<OkObjectResult>(resultado);
        _mockMediator.Verify(s => s.Send(It.IsAny<ObtenerResumenDashboardQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // =========================================================================
    // 4. OBTENER MI SUSCRIPCIÓN
    // =========================================================================

    [Fact]
    public async Task ObtenerMiSuscripcion_SinRegistro_DebeRetornarNotFound()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockMediator
            .Setup(s => s.Send(It.IsAny<ObtenerSuscripcionPorUsuarioIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SuscripcionDealerDto?)null);

        // Act
        var resultado = await _controller.ObtenerMiSuscripcion();

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task ObtenerMiSuscripcion_ConSuscripcion_DebeRetornarOk()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockMediator
            .Setup(s => s.Send(It.IsAny<ObtenerSuscripcionPorUsuarioIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CrearSuscripcionDto());

        // Act
        var resultado = await _controller.ObtenerMiSuscripcion();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.IsType<SuscripcionDealerDto>(ok.Value);
    }

    // =========================================================================
    // 5. HISTORIAL DE PAGOS
    // =========================================================================

    [Fact]
    public async Task ObtenerHistorialPagos_ConPagos_DebeRetornarLista()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockMediator
            .Setup(s => s.Send(It.IsAny<ObtenerHistorialPagosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PagoSuscripcionDto>
            {
                new PagoSuscripcionDto { Id = 1, PerfilDealerId = 15, Nivel = PlanNivel.Pro, Monto = 3000m, Moneda = "USD" }
            });

        // Act
        var resultado = await _controller.ObtenerHistorialPagos();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var lista = Assert.IsType<List<PagoSuscripcionDto>>(ok.Value);
        Assert.Single(lista);
    }

    // =========================================================================
    // 6. CANCELAR SUSCRIPCIÓN
    // =========================================================================

    [Fact]
    public async Task CancelarMiSuscripcion_NoEncontrada_DebeRetornarNotFound()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockMediator
            .Setup(s => s.Send(It.IsAny<CancelarSuscripcionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("No existe suscripción."));

        // Act
        var resultado = await _controller.CancelarMiSuscripcion();

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task CancelarMiSuscripcion_ViolacionReglaDeNegocio_DebeRetornarBadRequest()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockMediator
            .Setup(s => s.Send(It.IsAny<CancelarSuscripcionCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleException("La suscripción ya está cancelada."));

        // Act
        var resultado = await _controller.CancelarMiSuscripcion();

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task CancelarMiSuscripcion_Exitoso_DebeRetornarOk()
    {
        // Arrange
        SimularUsuarioAutenticado("15");

        // Act
        var resultado = await _controller.CancelarMiSuscripcion();

        // Assert
        Assert.IsType<OkObjectResult>(resultado);
        _mockMediator.Verify(s => s.Send(It.IsAny<CancelarSuscripcionCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
