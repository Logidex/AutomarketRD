using System.Security.Claims;
using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.Features.Anuncios.Commands;
using AutoMarket.Application.Features.Anuncios.Queries;
using AutoMarket.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AutoMarket.Tests.Controllers;

public class AnunciosControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IAnuncioService> _mockServicio;
    private readonly AnunciosController _controller;

    public AnunciosControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockServicio = new Mock<IAnuncioService>();
        _controller = new AnunciosController(_mockMediator.Object, _mockServicio.Object);
    }

    // =========================================================================
    // HELPERS
    // =========================================================================

    private void SimularUsuarioAutenticado(string usuarioId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private void SimularVisitanteAnonimo()
    {
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private AnuncioCreateDto CrearDtoValido()
    {
        return new AnuncioCreateDto
        {
            Marca = "Toyota",
            Modelo = "Corolla",
            Version = "1.8L",
            TipoVehiculo = "Sedan",
            Motor = "1.8L",
            Traccion = "Delantera",
            ColorExterior = "Blanco",
            ColorInterior = "Negro",
            Anio = 2022,
            Precio = 600000,
            Kilometraje = 50000,
            Transmision = "Automática",
            Combustible = "Gasolina",
            Accesorios = new List<string>(),
            Ubicacion = "Santo Domingo",
            Descripcion = "Vehículo en buen estado"
        };
    }

    private static IFormFile CrearFormFile(long tamanio = 1024)
    {
        var mockArchivo = new Mock<IFormFile>();
        mockArchivo.Setup(f => f.Length).Returns(tamanio);
        mockArchivo.Setup(f => f.FileName).Returns("foto.jpg");
        return mockArchivo.Object;
    }

    // =========================================================================
    // 1. CREAR ANUNCIO
    // =========================================================================

    [Fact]
    public async Task Crear_DatosValidos_RetornaOkConId()
    {
        SimularUsuarioAutenticado("15");
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CrearAnuncioCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);

        var resultado = await _controller.Crear(CrearDtoValido());

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var id = ok.Value?.GetType().GetProperty("id")?.GetValue(ok.Value);
        Assert.Equal(7, id);
    }

    // =========================================================================
    // 2. OBTENER POR ID
    // =========================================================================

    [Fact]
    public async Task ObtenerPorId_NoExiste_DebeRetornarNotFound()
    {
        SimularVisitanteAnonimo();
        _mockMediator
            .Setup(m => m.Send(It.IsAny<ObtenerAnuncioPorIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AnuncioDto?)null);

        var resultado = await _controller.ObtenerPorId(99);

        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task ObtenerPorId_Existe_DebeRetornarOk()
    {
        SimularVisitanteAnonimo();
        _mockMediator
            .Setup(m => m.Send(It.IsAny<ObtenerAnuncioPorIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AnuncioDto { Id = 5, Marca = "Honda" });

        var resultado = await _controller.ObtenerPorId(5);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.IsType<AnuncioDto>(ok.Value);
    }

    // =========================================================================
    // 3. PUBLICAR
    // =========================================================================

    [Fact]
    public async Task Publicar_NoEncontrado_DebeRetornarNotFound()
    {
        SimularUsuarioAutenticado("15");
        _mockMediator
            .Setup(m => m.Send(It.IsAny<PublicarAnuncioCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var resultado = await _controller.Publicar(99);

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task Publicar_Exitoso_DebeRetornarOk()
    {
        SimularUsuarioAutenticado("15");
        _mockMediator
            .Setup(m => m.Send(It.IsAny<PublicarAnuncioCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var resultado = await _controller.Publicar(5);

        Assert.IsType<OkObjectResult>(resultado);
    }

    // =========================================================================
    // 4. SUBIR IMÁGENES (usa IAnuncioService directamente)
    // =========================================================================

    [Fact]
    public async Task SubirImagenes_SinImagenes_DebeRetornarBadRequest()
    {
        SimularUsuarioAutenticado("15");

        var resultado = await _controller.SubirImagenes(5, new List<IFormFile>());

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task SubirImagenes_MasDeDiez_DebeRetornarBadRequest()
    {
        SimularUsuarioAutenticado("15");
        var demasiadas = Enumerable.Range(0, 11).Select(_ => CrearFormFile()).ToList();

        var resultado = await _controller.SubirImagenes(5, demasiadas);

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    // =========================================================================
    // 4b. ESTABLECER FOTO PRINCIPAL
    // =========================================================================

    [Fact]
    public async Task EstablecerFotoPrincipal_UrlVacia_DebeRetornarBadRequest()
    {
        SimularUsuarioAutenticado("15");
        var dto = new AnuncioFotoPrincipalDto { UrlImagen = "" };

        var resultado = await _controller.EstablecerFotoPrincipal(5, dto);

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task EstablecerFotoPrincipal_AnuncioNoExiste_DebeRetornarNotFound()
    {
        SimularUsuarioAutenticado("15");
        _mockServicio
            .Setup(s => s.EstablecerFotoPrincipalAsync(99, 15, "uploads/f2.jpg"))
            .ReturnsAsync(false);

        var resultado = await _controller.EstablecerFotoPrincipal(99, new AnuncioFotoPrincipalDto { UrlImagen = "uploads/f2.jpg" });

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task EstablecerFotoPrincipal_Exitoso_DebeRetornarOk()
    {
        SimularUsuarioAutenticado("15");
        _mockServicio
            .Setup(s => s.EstablecerFotoPrincipalAsync(5, 15, "uploads/f2.jpg"))
            .ReturnsAsync(true);

        var resultado = await _controller.EstablecerFotoPrincipal(5, new AnuncioFotoPrincipalDto { UrlImagen = "uploads/f2.jpg" });

        Assert.IsType<OkObjectResult>(resultado);
    }

    [Fact]
    public async Task EstablecerFotoPrincipal_NoEsElDueno_DebeRetornar403()
    {
        SimularUsuarioAutenticado("15");
        _mockServicio
            .Setup(s => s.EstablecerFotoPrincipalAsync(5, 15, "uploads/f2.jpg"))
            .ThrowsAsync(new UnauthorizedAccessException());

        var resultado = await _controller.EstablecerFotoPrincipal(5, new AnuncioFotoPrincipalDto { UrlImagen = "uploads/f2.jpg" });

        var status = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(403, status.StatusCode);
    }

    // =========================================================================
    // 5. CAMBIAR ESTADO
    // =========================================================================

    [Fact]
    public async Task CambiarEstado_EstadoVacio_DebeRetornarBadRequest()
    {
        SimularUsuarioAutenticado("15");
        var dto = new AnuncioEstadoDto { Estado = "   " };

        var resultado = await _controller.CambiarEstado(5, dto);

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task CambiarEstado_NoEncontrado_DebeRetornarNotFound()
    {
        SimularUsuarioAutenticado("15");
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CambiarEstadoAnuncioCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var resultado = await _controller.CambiarEstado(99, new AnuncioEstadoDto { Estado = "Pausado" });

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task CambiarEstado_Exitoso_DebeRetornarOk()
    {
        SimularUsuarioAutenticado("15");
        _mockMediator
            .Setup(m => m.Send(It.IsAny<CambiarEstadoAnuncioCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var resultado = await _controller.CambiarEstado(5, new AnuncioEstadoDto { Estado = "Pausado" });

        Assert.IsType<OkObjectResult>(resultado);
    }

    // =========================================================================
    // 6. ELIMINAR IMAGEN
    // =========================================================================

    [Fact]
    public async Task EliminarImagen_SinUrl_DebeRetornarBadRequest()
    {
        SimularUsuarioAutenticado("15");
        var dto = new EliminarImagenDto { UrlImagen = "" };

        var resultado = await _controller.EliminarImagen(5, dto);

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task EliminarImagen_NoEsElDueno_DebeRetornar403()
    {
        SimularUsuarioAutenticado("15");
        _mockServicio
            .Setup(s => s.EliminarImagenAsync(5, 15, "https://s3/url"))
            .ThrowsAsync(new UnauthorizedAccessException());

        var resultado = await _controller.EliminarImagen(5, new EliminarImagenDto { UrlImagen = "https://s3/url" });

        var status = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(403, status.StatusCode);
    }

    [Fact]
    public async Task EliminarImagen_Exitoso_DebeRetornarOk()
    {
        SimularUsuarioAutenticado("15");

        var resultado = await _controller.EliminarImagen(5, new EliminarImagenDto { UrlImagen = "https://s3/url" });

        Assert.IsType<OkObjectResult>(resultado);
    }

    // =========================================================================
    // 7. REGISTRAR VISTA
    // =========================================================================

    [Fact]
    public async Task RegistrarVista_AnuncioNoExiste_DebeRetornarNotFound()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<RegistrarVistaCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        var resultado = await _controller.RegistrarVista(99);

        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task RegistrarVista_Exitoso_DebeRetornarOk()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<RegistrarVistaCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var resultado = await _controller.RegistrarVista(5);

        Assert.IsType<OkObjectResult>(resultado);
    }

    // =========================================================================
    // 8. ELIMINAR ANUNCIO
    // =========================================================================

    [Fact]
    public async Task Eliminar_NoEncontrado_DebeRetornarNotFound()
    {
        SimularUsuarioAutenticado("15");
        _mockMediator
            .Setup(m => m.Send(It.IsAny<EliminarAnuncioCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var resultado = await _controller.Eliminar(99);

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task Eliminar_Exitoso_DebeRetornarOk()
    {
        SimularUsuarioAutenticado("15");
        _mockMediator
            .Setup(m => m.Send(It.IsAny<EliminarAnuncioCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var resultado = await _controller.Eliminar(5);

        Assert.IsType<OkObjectResult>(resultado);
    }
}
