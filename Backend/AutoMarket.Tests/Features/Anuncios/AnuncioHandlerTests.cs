using Xunit;
using Moq;
using MediatR;
using AutoMarket.Application.Features.Anuncios.Handlers;
using AutoMarket.Application.Features.Anuncios.Commands;
using AutoMarket.Application.Features.Anuncios.Queries;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs;

namespace AutoMarket.Tests.Features.Anuncios;

public class AnuncioHandlerTests
{
    private readonly Mock<IAnuncioService> _mockService;
    private readonly AnuncioCommandHandler _commandHandler;
    private readonly AnuncioQueryHandler _queryHandler;

    public AnuncioHandlerTests()
    {
        _mockService = new Mock<IAnuncioService>();
        _commandHandler = new AnuncioCommandHandler(_mockService.Object);
        _queryHandler = new AnuncioQueryHandler(_mockService.Object);
    }

    // ==================== CommandHandler Tests ====================

    [Fact]
    public async Task Handle_CrearAnuncio_DebeRetornarId()
    {
        // Arrange
        var dto = new AnuncioCreateDto { Marca = "Toyota", Modelo = "Corolla" };
        var command = new CrearAnuncioCommand { Dto = dto };
        _mockService.Setup(s => s.CrearAnuncioAsync(dto)).ReturnsAsync(1);

        // Act
        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, resultado);
        _mockService.Verify(s => s.CrearAnuncioAsync(dto), Times.Once);
    }

    [Fact]
    public async Task Handle_ActualizarAnuncio_DebeRetornarDto()
    {
        // Arrange
        var id = 1;
        var usuarioId = 10;
        var dto = new AnuncioUpdateDto { Marca = "Honda", Modelo = "Civic" };
        var command = new ActualizarAnuncioCommand { Id = id, UsuarioId = usuarioId, Dto = dto };
        var expectedDto = new AnuncioUpdateDto { Marca = "Honda", Modelo = "Civic" };
        _mockService.Setup(s => s.ActualizarAsync(id, usuarioId, dto)).ReturnsAsync(expectedDto);

        // Act
        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Honda", resultado!.Marca);
        Assert.Equal("Civic", resultado.Modelo);
        _mockService.Verify(s => s.ActualizarAsync(id, usuarioId, dto), Times.Once);
    }

    [Fact]
    public async Task Handle_PublicarAnuncio_DebeRetornarTrue()
    {
        // Arrange
        var id = 1;
        var usuarioId = 10;
        var command = new PublicarAnuncioCommand { Id = id, UsuarioId = usuarioId };
        _mockService.Setup(s => s.PublicarAnuncioAsync(id, usuarioId)).ReturnsAsync(true);

        // Act
        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(resultado);
        _mockService.Verify(s => s.PublicarAnuncioAsync(id, usuarioId), Times.Once);
    }

    [Fact]
    public async Task Handle_CambiarEstadoAnuncio_DebeRetornarTrue()
    {
        // Arrange
        var id = 1;
        var usuarioId = 10;
        var estado = "Publicado";
        var command = new CambiarEstadoAnuncioCommand { Id = id, UsuarioId = usuarioId, Estado = estado };
        _mockService.Setup(s => s.CambiarEstadoAsync(id, usuarioId, estado)).ReturnsAsync(true);

        // Act
        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(resultado);
        _mockService.Verify(s => s.CambiarEstadoAsync(id, usuarioId, estado), Times.Once);
    }

    [Fact]
    public async Task Handle_EliminarAnuncio_DebeRetornarTrue()
    {
        // Arrange
        var id = 1;
        var usuarioId = 10;
        var command = new EliminarAnuncioCommand { Id = id, UsuarioId = usuarioId };
        _mockService.Setup(s => s.EliminarAnuncioAsync(id, usuarioId)).ReturnsAsync(true);

        // Act
        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(resultado);
        _mockService.Verify(s => s.EliminarAnuncioAsync(id, usuarioId), Times.Once);
    }

    [Fact]
    public async Task Handle_RenovarAnuncioGratis_DebeRetornarTrue()
    {
        // Arrange
        var id = 1;
        var usuarioId = 10;
        var command = new RenovarAnuncioGratisCommand { Id = id, UsuarioId = usuarioId };
        _mockService.Setup(s => s.RenovarAnuncioGratisAsync(id, usuarioId)).ReturnsAsync(true);

        // Act
        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(resultado);
        _mockService.Verify(s => s.RenovarAnuncioGratisAsync(id, usuarioId), Times.Once);
    }

    [Fact]
    public async Task Handle_MarcarDestacado_DebeRetornarTrue()
    {
        // Arrange
        var id = 1;
        var usuarioId = 10;
        var command = new MarcarDestacadoCommand { Id = id, UsuarioId = usuarioId };
        _mockService.Setup(s => s.MarcarComoDestacadoAsync(id, usuarioId)).ReturnsAsync(true);

        // Act
        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(resultado);
        _mockService.Verify(s => s.MarcarComoDestacadoAsync(id, usuarioId), Times.Once);
    }

    [Fact]
    public async Task Handle_QuitarDestacado_DebeRetornarTrue()
    {
        // Arrange
        var id = 1;
        var usuarioId = 10;
        var command = new QuitarDestacadoCommand { Id = id, UsuarioId = usuarioId };
        _mockService.Setup(s => s.QuitarDestacadoAsync(id, usuarioId)).ReturnsAsync(true);

        // Act
        var resultado = await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(resultado);
        _mockService.Verify(s => s.QuitarDestacadoAsync(id, usuarioId), Times.Once);
    }

    [Fact]
    public async Task Handle_RegistrarVista_DebeLlamarServicio()
    {
        // Arrange
        var anuncioId = 1;
        var command = new RegistrarVistaCommand { AnuncioId = anuncioId };
        _mockService.Setup(s => s.RegistrarVistaAsync(anuncioId)).Returns(Task.CompletedTask);

        // Act
        await _commandHandler.Handle(command, CancellationToken.None);

        // Assert
        _mockService.Verify(s => s.RegistrarVistaAsync(anuncioId), Times.Once);
    }

    // ==================== QueryHandler Tests ====================

    [Fact]
    public async Task Handle_ObtenerAnuncioPorId_DebeRetornarDto()
    {
        // Arrange
        var id = 1;
        var usuarioId = 10;
        var command = new ObtenerAnuncioPorIdQuery { Id = id, UsuarioId = usuarioId };
        var expectedDto = new AnuncioDto { Id = id, Marca = "Toyota", Modelo = "Corolla" };
        _mockService.Setup(s => s.ObtenerAnuncioPorIdAsync(id, usuarioId)).ReturnsAsync(expectedDto);

        // Act
        var resultado = await _queryHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(id, resultado!.Id);
        Assert.Equal("Toyota", resultado.Marca);
        _mockService.Verify(s => s.ObtenerAnuncioPorIdAsync(id, usuarioId), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerTodosLosAnuncios_DebeRetornarLista()
    {
        // Arrange
        var command = new ObtenerTodosLosAnunciosQuery();
        var expectedList = new List<AnuncioListadoDto>
        {
            new() { Id = 1, Marca = "Toyota" },
            new() { Id = 2, Marca = "Honda" }
        };
        _mockService.Setup(s => s.ObtenerTodosLosAnuncios()).ReturnsAsync(expectedList);

        // Act
        var resultado = await _queryHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count);
        _mockService.Verify(s => s.ObtenerTodosLosAnuncios(), Times.Once);
    }

    [Fact]
    public async Task Handle_BuscarAnuncios_DebeRetornarPagedResult()
    {
        // Arrange
        var searchDto = new AnuncioSearchDto { Marca = "Toyota", PaginaActual = 1 };
        var command = new BuscarAnunciosQuery { Dto = searchDto };
        var expected = new PagedResult<AnuncioListadoDto>(
            new List<AnuncioListadoDto> { new() { Id = 1, Marca = "Toyota" } },
            1, 1, 10);
        _mockService.Setup(s => s.BuscarAnunciosAsync(searchDto)).ReturnsAsync(expected);

        // Act
        var resultado = await _queryHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.TotalRegistros);
        _mockService.Verify(s => s.BuscarAnunciosAsync(searchDto), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerDestacados_DebeRetornarPagedResult()
    {
        // Arrange
        var command = new ObtenerDestacadosQuery { Pagina = 1, TamanoPagina = 6 };
        var expected = new PagedResult<AnuncioListadoDto>(
            new List<AnuncioListadoDto> { new() { Id = 1, EsDestacado = true } },
            1, 1, 6);
        _mockService.Setup(s => s.ObtenerDestacadosAsync(1, 6)).ReturnsAsync(expected);

        // Act
        var resultado = await _queryHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.TotalRegistros);
        Assert.True(resultado.Items.First().EsDestacado);
        _mockService.Verify(s => s.ObtenerDestacadosAsync(1, 6), Times.Once);
    }
}
