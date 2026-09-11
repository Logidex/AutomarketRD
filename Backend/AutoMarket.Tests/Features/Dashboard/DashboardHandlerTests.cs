using Xunit;
using Moq;
using AutoMarket.Application.Features.Dashboard.Handlers;
using AutoMarket.Application.Features.Dashboard.Queries;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs.Admin;

namespace AutoMarket.Tests.Features.Dashboard;

public class DashboardHandlerTests
{
    private readonly Mock<IDashboardService> _mockService;
    private readonly DashboardQueryHandler _queryHandler;

    public DashboardHandlerTests()
    {
        _mockService = new Mock<IDashboardService>();
        _queryHandler = new DashboardQueryHandler(_mockService.Object);
    }

    [Fact]
    public async Task Handle_ObtenerResumen_Global_DebeRetornarDto()
    {
        var query = new ObtenerResumenDashboardQuery(DealerUsuarioId: null);
        var expected = new DashboardResumenDto
        {
            TotalUsuarios = 100,
            TotalAnuncios = 250,
            TotalLeads = 50,
            AnunciosActivos = 180,
            AnunciosBorrador = 30,
            AnunciosVendidos = 40,
            AnunciosPausados = 0,
            LeadsNoLeidos = 15
        };
        _mockService
            .Setup(s => s.ObtenerResumenAsync())
            .ReturnsAsync(expected);

        var resultado = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal(100, resultado!.TotalUsuarios);
        Assert.Equal(250, resultado.TotalAnuncios);
        _mockService.Verify(s => s.ObtenerResumenAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerResumen_PorDealer_DebeRetornarDto()
    {
        var query = new ObtenerResumenDashboardQuery(DealerUsuarioId: 5);
        var expected = new DashboardResumenDto
        {
            TotalUsuarios = 0,
            TotalAnuncios = 10,
            TotalLeads = 3,
            PlanActual = "Pro",
            DiasRestantesSuscripcion = 25,
            LimiteAnuncios = 50,
            CuotaDestacados = 5,
            DestacadosActivos = 2
        };
        _mockService
            .Setup(s => s.ObtenerResumenAsync(5))
            .ReturnsAsync(expected);

        var resultado = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal(10, resultado!.TotalAnuncios);
        Assert.Equal("Pro", resultado.PlanActual);
        Assert.Equal(25, resultado.DiasRestantesSuscripcion);
        _mockService.Verify(s => s.ObtenerResumenAsync(5), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerResumen_Global_ListasVacias_DebeRetornarDto()
    {
        var query = new ObtenerResumenDashboardQuery();
        var expected = new DashboardResumenDto
        {
            AnunciosMasVistos = new List<AnuncioMasVistoDto>()
        };
        _mockService
            .Setup(s => s.ObtenerResumenAsync())
            .ReturnsAsync(expected);

        var resultado = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Empty(resultado!.AnunciosMasVistos);
        _mockService.Verify(s => s.ObtenerResumenAsync(), Times.Once);
    }
}
