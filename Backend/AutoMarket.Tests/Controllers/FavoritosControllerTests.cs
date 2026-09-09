using System.Security.Claims;
using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs.Favorito;
using AutoMarket.Application.Features.Favoritos.Commands;
using AutoMarket.Application.Features.Favoritos.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Controllers;

public class FavoritosControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly FavoritosController _controller;

    public FavoritosControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new FavoritosController(_mockMediator.Object);
        ConfigurarUsuario(123);
    }

    private void ConfigurarUsuario(int usuarioId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };
    }

    [Fact]
    public async Task AgregarFavorito_CuandoEsExitoso_DebeRetornarOk()
    {
        _mockMediator
            .Setup(s => s.Send(It.IsAny<AgregarFavoritoCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var resultado = await _controller.AgregarFavorito(10);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.NotNull(ok.Value);

        var tipo = ok.Value!.GetType();
        var exito = (bool?)tipo.GetProperty("exito")?.GetValue(ok.Value);
        var mensaje = tipo.GetProperty("mensaje")?.GetValue(ok.Value)?.ToString();

        Assert.True(exito);
        Assert.Equal("Vehículo agregado a favoritos.", mensaje);

        _mockMediator.Verify(s => s.Send(It.IsAny<AgregarFavoritoCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AgregarFavorito_CuandoAnuncioNoExiste_DebeRetornarNotFound()
    {
        _mockMediator
            .Setup(s => s.Send(It.IsAny<AgregarFavoritoCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("El anuncio no existe."));

        var resultado = await _controller.AgregarFavorito(10);

        var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
        Assert.NotNull(notFound.Value);

        var mensaje = notFound.Value!.GetType().GetProperty("mensaje")?.GetValue(notFound.Value)?.ToString();
        Assert.Equal("El anuncio no existe.", mensaje);
    }

    [Fact]
    public async Task AgregarFavorito_CuandoYaExiste_DebeRetornarBadRequest()
    {
        _mockMediator
            .Setup(s => s.Send(It.IsAny<AgregarFavoritoCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("El vehículo ya está en tus favoritos."));

        var resultado = await _controller.AgregarFavorito(10);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.NotNull(badRequest.Value);

        var mensaje = badRequest.Value!.GetType().GetProperty("mensaje")?.GetValue(badRequest.Value)?.ToString();
        Assert.Equal("El vehículo ya está en tus favoritos.", mensaje);
    }

    [Fact]
    public async Task QuitarFavorito_CuandoEsExitoso_DebeRetornarOk()
    {
        _mockMediator
            .Setup(s => s.Send(It.IsAny<QuitarFavoritoCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var resultado = await _controller.QuitarFavorito(10);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.NotNull(ok.Value);

        var tipo = ok.Value!.GetType();
        var exito = (bool?)tipo.GetProperty("exito")?.GetValue(ok.Value);
        var mensaje = tipo.GetProperty("mensaje")?.GetValue(ok.Value)?.ToString();

        Assert.True(exito);
        Assert.Equal("Vehículo removido de favoritos.", mensaje);

        _mockMediator.Verify(s => s.Send(It.IsAny<QuitarFavoritoCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task QuitarFavorito_CuandoNoExiste_DebeRetornarNotFound()
    {
        _mockMediator
            .Setup(s => s.Send(It.IsAny<QuitarFavoritoCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("El vehículo no estaba en tus favoritos."));

        var resultado = await _controller.QuitarFavorito(10);

        var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
        Assert.NotNull(notFound.Value);

        var mensaje = notFound.Value!.GetType().GetProperty("mensaje")?.GetValue(notFound.Value)?.ToString();
        Assert.Equal("El vehículo no estaba en tus favoritos.", mensaje);
    }

    [Fact]
    public async Task ObtenerMisFavoritos_DebeRetornarOkConLista()
    {
        var favoritos = new List<AnuncioFavoritoDto>
        {
            new AnuncioFavoritoDto
            {
                Id = 1,
                Marca = "Toyota",
                Modelo = "Corolla",
                Anio = 2022,
                Precio = 18500m,
                FotoPrincipal = "foto1.jpg"
            },
            new AnuncioFavoritoDto
            {
                Id = 2,
                Marca = "Honda",
                Modelo = "Civic",
                Anio = 2021,
                Precio = 17500m,
                FotoPrincipal = null
            }
        };

        _mockMediator
            .Setup(s => s.Send(It.IsAny<ObtenerFavoritosQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(favoritos);

        var resultado = await _controller.ObtenerMisFavoritos();

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var valor = Assert.IsAssignableFrom<IEnumerable<AnuncioFavoritoDto>>(ok.Value);
        Assert.Equal(2, valor.Count());

        _mockMediator.Verify(s => s.Send(It.IsAny<ObtenerFavoritosQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
