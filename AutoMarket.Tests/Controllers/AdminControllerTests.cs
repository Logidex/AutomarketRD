using AutoMarket.API.Controllers;
using AutoMarket.Application.Interfaces;
using AutoMarket.Application.Services;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Controllers;

public class AdminControllerTests
{
    [Fact]
    public async Task EliminarAnuncioForzoso_ConFotos_LlamaAlServicioS3YRepositorio()
    {
        // 1. ARRANGE (Preparar el escenario)
        var mockRepo = new Mock<IAnuncioRepository>();
        var mockS3 = new Mock<IAlmacenadorArchivos>();
        var mockDashboard = new Mock<IDashboardService>();
        var mockUsuarios = new Mock<IUsuarioRepository>();

        // Crear un anuncio falso con 2 fotos
        var anuncio = new Anuncio(1, "Toyota", "Corolla", "Sedan", "Rojo", "Negro", 2020, 15000, 10000, "Auto", "Gasolina", new List<string>(), "N/A", "Test");
        anuncio.AgregarFotos(new List<string> { "foto1.jpg", "foto2.jpg" });

        // Fingir que la BD encuentra el anuncio
        mockRepo.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(anuncio);

        var controller = new AdminController(
            mockDashboard.Object, mockUsuarios.Object, mockRepo.Object, mockS3.Object);

        // 2. ACT (Ejecutar la acción)
        var resultado = await controller.EliminarAnuncioForzoso(1);

        // 3. ASSERT (Verificar que hizo lo correcto)
        Assert.IsType<OkObjectResult>(resultado);
        
        // Verificar que llamó al S3 exactamente 2 veces (una por cada foto)
        mockS3.Verify(s => s.EliminarArchivoAsync("foto1.jpg"), Times.Once);
        mockS3.Verify(s => s.EliminarArchivoAsync("foto2.jpg"), Times.Once);

        // Verificar que borró el anuncio de la BD
        mockRepo.Verify(r => r.Eliminar(anuncio), Times.Once);
        mockRepo.Verify(r => r.GuardarCambiosAsync(), Times.Once);
    }
}