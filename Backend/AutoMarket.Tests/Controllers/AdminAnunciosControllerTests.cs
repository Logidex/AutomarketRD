using AutoMarket.API.Controllers;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AutoMarket.Tests.Controllers;

public class AdminAnunciosControllerTests
{
    [Fact]
    public async Task EliminarAnuncioForzoso_ConFotos_LlamaAlServicio()
    {
        var mockService = new Mock<IAdminAnuncioService>();
        mockService.Setup(s => s.EliminarAnuncioForzosoAsync(1)).ReturnsAsync(true);

        var controller = new AdminAnunciosController(mockService.Object);

        var resultado = await controller.EliminarAnuncioForzoso(1);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.NotNull(okResult);
        mockService.Verify(s => s.EliminarAnuncioForzosoAsync(1), Times.Once);
    }
}
