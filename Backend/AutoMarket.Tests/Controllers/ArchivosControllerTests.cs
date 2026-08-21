using AutoMarket.API.Controllers;
using AutoMarket.Application.Services;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Controllers;

public class ArchivosControllerTests
{
    private static (ArchivosController Controller, Mock<IAlmacenadorArchivos> Almacenador, Mock<IAnuncioRepository> Repositorio) CrearController()
    {
        var mockAlmacenador = new Mock<IAlmacenadorArchivos>();
        var mockRepositorio = new Mock<IAnuncioRepository>();

        var controller = new ArchivosController(
            mockAlmacenador.Object,
            mockRepositorio.Object);

        return (controller, mockAlmacenador, mockRepositorio);
    }

    [Fact]
    public async Task Obtener_ConClaveRegistrada_DevuelveRedirectALaUrlFirmada()
    {
        // ARRANGE
        var (controller, mockAlmacenador, mockRepositorio) = CrearController();
        const string clave = "uploads/foto.jpg";
        const string urlFirmada = "https://bucket.s3.region.amazonaws.com/uploads/foto.jpg?X-Amz-Expires=900";

        mockRepositorio
            .Setup(r => r.ExisteFotoAsync(clave))
            .ReturnsAsync(true);

        mockAlmacenador
            .Setup(a => a.GenerarUrlFirmadaAsync(clave))
            .ReturnsAsync(urlFirmada);

        // ACT
        var resultado = await controller.Obtener(clave);

        // ASSERT
        var redirect = Assert.IsType<RedirectResult>(resultado);
        Assert.Equal(urlFirmada, redirect.Url);
        mockAlmacenador.Verify(
            a => a.GenerarUrlFirmadaAsync(clave),
            Times.Once);
    }

    [Fact]
    public async Task Obtener_ConClaveVacia_DevuelveBadRequest()
    {
        // ARRANGE
        var (controller, _, _) = CrearController();

        // ACT
        var resultado = await controller.Obtener("   ");

        // ASSERT
        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task Obtener_SoportaUrlsPublicasLegadas()
    {
        // ARRANGE
        var (controller, mockAlmacenador, mockRepositorio) = CrearController();
        const string urlLegada = "https://automarketrd-s3.s3.us-east-2.amazonaws.com/uploads/logo.png";
        const string urlFirmada = "https://bucket.s3.region.amazonaws.com/uploads/logo.png?X-Amz-Signature=x";

        mockRepositorio
            .Setup(r => r.ExisteFotoAsync(urlLegada))
            .ReturnsAsync(true);

        mockAlmacenador
            .Setup(a => a.GenerarUrlFirmadaAsync(urlLegada))
            .ReturnsAsync(urlFirmada);

        // ACT
        var resultado = await controller.Obtener(urlLegada);

        // ASSERT
        var redirect = Assert.IsType<RedirectResult>(resultado);
        Assert.Equal(urlFirmada, redirect.Url);
    }

    [Fact]
    public async Task Obtener_ConClaveNoRegistrada_DevuelveNotFoundYNoFirmaUrl()
    {
        // ARRANGE
        var (controller, mockAlmacenador, mockRepositorio) = CrearController();
        const string claveAjena = "uploads/backups/dump.sql";

        mockRepositorio
            .Setup(r => r.ExisteFotoAsync(claveAjena))
            .ReturnsAsync(false);

        // ACT
        var resultado = await controller.Obtener(claveAjena);

        // ASSERT
        Assert.IsType<NotFoundObjectResult>(resultado);
        mockAlmacenador.Verify(
            a => a.GenerarUrlFirmadaAsync(It.IsAny<string>()),
            Times.Never);
    }
}
