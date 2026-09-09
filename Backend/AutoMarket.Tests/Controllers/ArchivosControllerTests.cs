using AutoMarket.API.Controllers;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AutoMarket.Tests.Controllers;

public class ArchivosControllerTests
{
    private static (ArchivosController Controller, Mock<IArchivoService> Service) CrearController()
    {
        var mockArchivoService = new Mock<IArchivoService>();
        var controller = new ArchivosController(mockArchivoService.Object);
        return (controller, mockArchivoService);
    }

    [Fact]
    public async Task Obtener_ConClaveRegistrada_DevuelveRedirectALaUrlFirmada()
    {
        var (controller, mockService) = CrearController();
        const string clave = "uploads/foto.jpg";
        const string urlFirmada = "https://bucket.s3.region.amazonaws.com/uploads/foto.jpg?X-Amz-Expires=900";

        mockService
            .Setup(s => s.ObtenerUrlFirmadaSiExisteAsync(clave))
            .ReturnsAsync(urlFirmada);

        var resultado = await controller.Obtener(clave);

        var redirect = Assert.IsType<RedirectResult>(resultado);
        Assert.Equal(urlFirmada, redirect.Url);
        mockService.Verify(
            s => s.ObtenerUrlFirmadaSiExisteAsync(clave),
            Times.Once);
    }

    [Fact]
    public async Task Obtener_ConClaveVacia_DevuelveBadRequest()
    {
        var (controller, _) = CrearController();

        var resultado = await controller.Obtener("   ");

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task Obtener_ConClaveNoRegistrada_DevuelveNotFound()
    {
        var (controller, mockService) = CrearController();
        const string claveAjena = "uploads/backups/dump.sql";

        mockService
            .Setup(s => s.ObtenerUrlFirmadaSiExisteAsync(claveAjena))
            .ReturnsAsync((string?)null);

        var resultado = await controller.Obtener(claveAjena);

        Assert.IsType<NotFoundObjectResult>(resultado);
    }
}
