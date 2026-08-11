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
    private static AdminController CrearController(
        Mock<IAnuncioRepository>? mockRepo = null,
        Mock<IAlmacenadorArchivos>? mockS3 = null,
        Mock<IDashboardService>? mockDashboard = null,
        Mock<IUsuarioRepository>? mockUsuarios = null,
        Mock<ISuscripcionService>? mockSuscripcionService = null,
        Mock<IPlanCatalogoService>? mockPlanCatalogo = null,
        Mock<IUsuarioCuentaService>? mockUsuarioCuenta = null)
    {
        return new AdminController(
            (mockDashboard ?? new Mock<IDashboardService>()).Object,
            (mockUsuarios ?? new Mock<IUsuarioRepository>()).Object,
            (mockRepo ?? new Mock<IAnuncioRepository>()).Object,
            (mockS3 ?? new Mock<IAlmacenadorArchivos>()).Object,
            (mockSuscripcionService ?? new Mock<ISuscripcionService>()).Object,
            (mockPlanCatalogo ?? new Mock<IPlanCatalogoService>()).Object,
            (mockUsuarioCuenta ?? new Mock<IUsuarioCuentaService>()).Object);
    }

    [Fact]
    public async Task EliminarAnuncioForzoso_ConFotos_LlamaAlServicioS3YRepositorio()
    {
        // 1. ARRANGE
        var mockRepo = new Mock<IAnuncioRepository>();
        var mockS3 = new Mock<IAlmacenadorArchivos>();
        var mockDashboard = new Mock<IDashboardService>();
        var mockUsuarios = new Mock<IUsuarioRepository>();
        var mockSuscripcionService = new Mock<ISuscripcionService>();
        var mockPlanCatalogo = new Mock<IPlanCatalogoService>();

        var anuncio = new Anuncio(
            1,
            "Toyota",
            "Corolla",
            "",
            "Sedan",
            "1.8L",
            "Delantera",
            "Rojo",
            "Negro",
            2020,
            15000,
            10000,
            "Auto",
            "Gasolina",
            new List<string>(),
            "N/A",
            "Test");

        anuncio.AgregarFotos(new List<string> { "foto1.jpg", "foto2.jpg" });

        mockRepo.Setup(r => r.ObtenerPorIdAsync(1))
            .ReturnsAsync(anuncio);

        var controller = CrearController(
            mockRepo: mockRepo,
            mockS3: mockS3,
            mockDashboard: mockDashboard,
            mockUsuarios: mockUsuarios,
            mockSuscripcionService: mockSuscripcionService,
            mockPlanCatalogo: mockPlanCatalogo);

        // 2. ACT
        var resultado = await controller.EliminarAnuncioForzoso(1);

        // 3. ASSERT
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.NotNull(okResult);

        mockS3.Verify(s => s.EliminarArchivoAsync("foto1.jpg"), Times.Once);
        mockS3.Verify(s => s.EliminarArchivoAsync("foto2.jpg"), Times.Once);

        mockRepo.Verify(r => r.Eliminar(anuncio), Times.Once);
        mockRepo.Verify(r => r.GuardarCambiosAsync(), Times.Once);
    }

    [Fact]
    public async Task CambiarRol_UsuarioNoEncontrado_DebeRetornarNotFound()
    {
        var mockUsuarios = new Mock<IUsuarioRepository>();
        mockUsuarios.Setup(r => r.ObtenerPorIdAsync(99)).ReturnsAsync((Usuario?)null);

        var controller = CrearController(mockUsuarios: mockUsuarios);

        var resultado = await controller.CambiarRol(99, new AutoMarket.Application.DTOs.Admin.CambiarRolAdminDto
        {
            NuevoRol = "Comprador"
        });

        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task CambiarRol_UsuarioAdmin_DebeRetornarBadRequest()
    {
        var usuario = Usuario.CrearAdministradorInterno("Admin", "Sistema", "admin@test.com", "hash");
        typeof(Usuario).GetProperty("UsuarioId")?.SetValue(usuario, 1);

        var mockUsuarios = new Mock<IUsuarioRepository>();
        mockUsuarios.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(usuario);

        var controller = CrearController(mockUsuarios: mockUsuarios);

        var resultado = await controller.CambiarRol(1, new AutoMarket.Application.DTOs.Admin.CambiarRolAdminDto
        {
            NuevoRol = "Dealer"
        });

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task CambiarRol_UsuarioValido_DebeDelegarAlServicio()
    {
        var usuario = new Usuario("Raul", "Diaz", "raul@test.com", "hash", "8090000000", "Comprador");
        typeof(Usuario).GetProperty("UsuarioId")?.SetValue(usuario, 5);

        var mockUsuarios = new Mock<IUsuarioRepository>();
        mockUsuarios.Setup(r => r.ObtenerPorIdAsync(5)).ReturnsAsync(usuario);

        var mockUsuarioCuenta = new Mock<IUsuarioCuentaService>();
        mockUsuarioCuenta
            .Setup(s => s.CambiarRolAdminAsync(5, It.IsAny<AutoMarket.Application.DTOs.Admin.CambiarRolAdminDto>()))
            .ReturnsAsync(new AutoMarket.Application.DTOs.Usuario.UsuarioCuentaDto
            {
                UsuarioId = 5,
                Nombre = "Raul",
                Apellido = "Diaz",
                Email = "raul@test.com",
                Rol = "Vendedor"
            });

        var controller = CrearController(mockUsuarios: mockUsuarios, mockUsuarioCuenta: mockUsuarioCuenta);

        var resultado = await controller.CambiarRol(5, new AutoMarket.Application.DTOs.Admin.CambiarRolAdminDto
        {
            NuevoRol = "Vendedor"
        });

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.NotNull(okResult);

        mockUsuarioCuenta.Verify(
            s => s.CambiarRolAdminAsync(5, It.IsAny<AutoMarket.Application.DTOs.Admin.CambiarRolAdminDto>()),
            Times.Once);
    }
}