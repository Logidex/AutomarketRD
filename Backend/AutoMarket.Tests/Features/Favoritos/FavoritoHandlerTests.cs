using Xunit;
using Moq;
using AutoMarket.Application.Features.Favoritos.Handlers;
using AutoMarket.Application.Features.Favoritos.Commands;
using AutoMarket.Application.Features.Favoritos.Queries;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.DTOs.Favorito;

namespace AutoMarket.Tests.Features.Favoritos;

public class FavoritoHandlerTests
{
    private readonly Mock<IFavoritoService> _mockService;
    private readonly FavoritoCommandHandler _commandHandler;
    private readonly FavoritoQueryHandler _queryHandler;

    public FavoritoHandlerTests()
    {
        _mockService = new Mock<IFavoritoService>();
        _commandHandler = new FavoritoCommandHandler(_mockService.Object);
        _queryHandler = new FavoritoQueryHandler(_mockService.Object);
    }

    [Fact]
    public async Task Handle_AgregarFavorito_DebeLlamarServicio()
    {
        var command = new AgregarFavoritoCommand(UsuarioId: 1, AnuncioId: 10);
        _mockService
            .Setup(s => s.AgregarFavoritoAsync(1, 10))
            .Returns(Task.CompletedTask);

        await _commandHandler.Handle(command, CancellationToken.None);

        _mockService.Verify(s => s.AgregarFavoritoAsync(1, 10), Times.Once);
    }

    [Fact]
    public async Task Handle_QuitarFavorito_DebeLlamarServicio()
    {
        var command = new QuitarFavoritoCommand(UsuarioId: 1, AnuncioId: 10);
        _mockService
            .Setup(s => s.QuitarFavoritoAsync(1, 10))
            .Returns(Task.CompletedTask);

        await _commandHandler.Handle(command, CancellationToken.None);

        _mockService.Verify(s => s.QuitarFavoritoAsync(1, 10), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerFavoritos_DebeRetornarLista()
    {
        var query = new ObtenerFavoritosQuery(UsuarioId: 1);
        var expected = new List<AnuncioFavoritoDto>
        {
            new() { Id = 1, Marca = "Toyota", Modelo = "Corolla", Anio = 2023, Precio = 1200000 },
            new() { Id = 2, Marca = "Honda", Modelo = "Civic", Anio = 2022, Precio = 950000 }
        };
        _mockService
            .Setup(s => s.ObtenerFavoritosAsync(1))
            .ReturnsAsync(expected);

        var resultado = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Count());
        _mockService.Verify(s => s.ObtenerFavoritosAsync(1), Times.Once);
    }

    [Fact]
    public async Task Handle_ObtenerFavoritos_Vacia_DebeRetornarListaVacia()
    {
        var query = new ObtenerFavoritosQuery(UsuarioId: 99);
        _mockService
            .Setup(s => s.ObtenerFavoritosAsync(99))
            .ReturnsAsync(new List<AnuncioFavoritoDto>());

        var resultado = await _queryHandler.Handle(query, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Empty(resultado);
        _mockService.Verify(s => s.ObtenerFavoritosAsync(99), Times.Once);
    }
}
