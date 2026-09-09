using System.Security.Claims;
using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs.Admin;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AutoMarket.Tests.Controllers;

public class AdminUsuariosControllerTests
{
    private static (AdminUsuariosController Controller, Mock<IDashboardService> Dashboard, Mock<IAdminUsuarioService> AdminUsuarios) CrearController(int adminId = 1)
    {
        var mockDashboard = new Mock<IDashboardService>();
        var mockAdminUsuarios = new Mock<IAdminUsuarioService>();
        var controller = new AdminUsuariosController(
            mockDashboard.Object,
            mockAdminUsuarios.Object);

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, adminId.ToString()) };
        var identity = new ClaimsIdentity(claims);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        return (controller, mockDashboard, mockAdminUsuarios);
    }

    [Fact]
    public async Task ListarUsuarios_DevuelveOkConLista()
    {
        var (controller, _, mockAdminUsuarios) = CrearController();

        var usuarios = new List<UsuarioAdminListDto>
        {
            new() { UsuarioId = 1, Nombre = "Raul", Apellido = "Diaz", Email = "raul@test.com", Rol = "Vendedor", IsActivo = true },
            new() { UsuarioId = 2, Nombre = "Ana", Apellido = "Lopez", Email = "ana@test.com", Rol = "Comprador", IsActivo = false }
        };

        mockAdminUsuarios
            .Setup(s => s.ListarUsuariosAsync())
            .ReturnsAsync(usuarios);

        var resultado = await controller.ListarUsuarios();

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        var lista = Assert.IsAssignableFrom<IEnumerable<UsuarioAdminListDto>>(okResult.Value);
        Assert.Equal(2, lista.Count());
    }

    [Fact]
    public async Task SuspenderUsuario_UsuarioNoEncontrado_DebeRetornarNotFound()
    {
        var (controller, _, mockAdminUsuarios) = CrearController();

        mockAdminUsuarios
            .Setup(s => s.SuspenderUsuarioAsync(99, It.IsAny<int>()))
            .ReturnsAsync((UsuarioAdminListDto?)null);

        var resultado = await controller.SuspenderUsuario(99);

        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task SuspenderUsuario_ReglaNegocio_DebeRetornarBadRequest()
    {
        var (controller, _, mockAdminUsuarios) = CrearController();

        mockAdminUsuarios
            .Setup(s => s.SuspenderUsuarioAsync(1, It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("No se puede suspender a un administrador."));

        var resultado = await controller.SuspenderUsuario(1);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task ReactivarUsuario_UsuarioNoEncontrado_DebeRetornarNotFound()
    {
        var (controller, _, mockAdminUsuarios) = CrearController();

        mockAdminUsuarios
            .Setup(s => s.ReactivarUsuarioAsync(99))
            .ReturnsAsync((UsuarioAdminListDto?)null);

        var resultado = await controller.ReactivarUsuario(99);

        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task ReactivarUsuario_ReglaNegocio_DebeRetornarBadRequest()
    {
        var (controller, _, mockAdminUsuarios) = CrearController();

        mockAdminUsuarios
            .Setup(s => s.ReactivarUsuarioAsync(1))
            .ThrowsAsync(new InvalidOperationException("El usuario no está suspendido."));

        var resultado = await controller.ReactivarUsuario(1);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task EliminarUsuario_UsuarioNoEncontrado_DebeRetornarNotFound()
    {
        var (controller, _, mockAdminUsuarios) = CrearController();

        mockAdminUsuarios
            .Setup(s => s.EliminarUsuarioAsync(99, It.IsAny<int>()))
            .ReturnsAsync((UsuarioAdminListDto?)null);

        var resultado = await controller.EliminarUsuario(99);

        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task EliminarUsuario_ReglaNegocio_DebeRetornarBadRequest()
    {
        var (controller, _, mockAdminUsuarios) = CrearController();

        mockAdminUsuarios
            .Setup(s => s.EliminarUsuarioAsync(1, It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("No se puede eliminar a un administrador."));

        var resultado = await controller.EliminarUsuario(1);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task CambiarRol_UsuarioNoEncontrado_DebeRetornarNotFound()
    {
        var (controller, _, mockAdminUsuarios) = CrearController();

        mockAdminUsuarios
            .Setup(s => s.CambiarRolAsync(99, It.IsAny<CambiarRolAdminDto>()))
            .ReturnsAsync((UsuarioAdminListDto?)null);

        var resultado = await controller.CambiarRol(99, new CambiarRolAdminDto { NuevoRol = "Comprador" });

        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task CambiarRol_ReglaNegocio_DebeRetornarBadRequest()
    {
        var (controller, _, mockAdminUsuarios) = CrearController();

        mockAdminUsuarios
            .Setup(s => s.CambiarRolAsync(1, It.IsAny<CambiarRolAdminDto>()))
            .ThrowsAsync(new InvalidOperationException("No se puede cambiar el rol de un administrador."));

        var resultado = await controller.CambiarRol(1, new CambiarRolAdminDto { NuevoRol = "Dealer" });

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task CambiarRol_UsuarioValido_DebeDelegarAlServicio()
    {
        var (controller, _, mockAdminUsuarios) = CrearController();
        var dto = new CambiarRolAdminDto { NuevoRol = "Vendedor" };

        var usuario = new UsuarioAdminListDto
        {
            UsuarioId = 5,
            Nombre = "Raul",
            Apellido = "Diaz",
            Email = "raul@test.com",
            Rol = "Vendedor",
            IsActivo = true
        };

        mockAdminUsuarios
            .Setup(s => s.CambiarRolAsync(5, dto))
            .ReturnsAsync(usuario);

        var resultado = await controller.CambiarRol(5, dto);

        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.NotNull(okResult);

        mockAdminUsuarios.Verify(
            s => s.CambiarRolAsync(5, dto),
            Times.Once);
    }
}
