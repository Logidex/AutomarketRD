using AutoMarket.API.Controllers;
using AutoMarket.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Controllers;

public class ArchivosControllerTests
{
    private static ArchivosController CrearController(Mock<IAlmacenadorArchivos>? mockAlmacenador = null)
    {
        return new ArchivosController(
            (mockAlmacenador ?? new Mock<IAlmacenadorArchivos>()).Object);
    }

    [Fact]
    public async Task Obtener_ConClaveValida_DevuelveRedirectALaUrlFirmada()
    {
        // ARRANGE
        var mockAlmacenador = new Mock<IAlmacenadorArchivos>();
        const string clave = "uploads/foto.jpg";
        const string urlFirmada = "https://bucket.s3.region.amazonaws.com/uploads/foto.jpg?X-Amz-Expires=900";

        mockAlmacenador
            .Setup(a => a.GenerarUrlFirmadaAsync(clave))
            .ReturnsAsync(urlFirmada);

        var controller = CrearController(mockAlmacenador);

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
        var controller = CrearController();

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
        var mockAlmacenador = new Mock<IAlmacenadorArchivos>();
        const string urlLegada = "https://automarketrd-s3.s3.us-east-2.amazonaws.com/uploads/logo.png";
        const string urlFirmada = "https://bucket.s3.region.amazonaws.com/uploads/logo.png?X-Amz-Signature=x";

        mockAlmacenador
            .Setup(a => a.GenerarUrlFirmadaAsync(urlLegada))
            .ReturnsAsync(urlFirmada);

        var controller = CrearController(mockAlmacenador);

        // ACT
        var resultado = await controller.Obtener(urlLegada);

        // ASSERT
        var redirect = Assert.IsType<RedirectResult>(resultado);
        Assert.Equal(urlFirmada, redirect.Url);
    }
}
