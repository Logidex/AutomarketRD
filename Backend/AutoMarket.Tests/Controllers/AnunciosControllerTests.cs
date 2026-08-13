using System.Security.Claims;
using AutoMarket.API.Controllers;
using AutoMarket.Application.DTOs;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AutoMarket.Tests.Controllers;

public class AnunciosControllerTests
{
    private readonly Mock<IAnuncioService> _mockServicio;
    private readonly AnunciosController _controller;

    public AnunciosControllerTests()
    {
        _mockServicio = new Mock<IAnuncioService>();
        _controller = new AnunciosController(_mockServicio.Object);
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
    public async Task CrearAnuncio_TokenInvalido_LanzaExcepcionNoAutorizado()
    {
        // Arrange
        SimularUsuarioAutenticado("abc");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.CrearAnuncio(CrearDtoValido()));
    }

    [Fact]
    public async Task CrearAnuncio_DatosValidos_AsignaUsuarioYRetornaOkConId()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockServicio
            .Setup(s => s.CrearAnuncioAsync(It.IsAny<AnuncioCreateDto>()))
            .ReturnsAsync(7);

        var dto = CrearDtoValido();

        // Act
        var resultado = await _controller.CrearAnuncio(dto);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var id = ok.Value?.GetType().GetProperty("id")?.GetValue(ok.Value);
        Assert.Equal(7, id);
        Assert.Equal(15, dto.UsuarioId);
    }

    // =========================================================================
    // 2. OBTENER POR ID
    // =========================================================================

    [Fact]
    public async Task ObtenerPorId_NoExiste_DebeRetornarNotFound()
    {
        // Arrange
        SimularVisitanteAnonimo();
        _mockServicio
            .Setup(s => s.ObtenerAnuncioPorIdAsync(99, It.IsAny<int?>()))
            .ReturnsAsync((AnuncioDto?)null);

        // Act
        var resultado = await _controller.ObtenerPorId(99);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task ObtenerPorId_Existe_DebeRetornarOk()
    {
        // Arrange
        SimularVisitanteAnonimo();
        _mockServicio
            .Setup(s => s.ObtenerAnuncioPorIdAsync(5, It.IsAny<int?>()))
            .ReturnsAsync(new AnuncioDto { Id = 5, Marca = "Honda" });

        // Act
        var resultado = await _controller.ObtenerPorId(5);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.IsType<AnuncioDto>(ok.Value);
    }

    // =========================================================================
    // 3. PUBLICAR
    // =========================================================================

    [Fact]
    public async Task Publicar_NoEncontrado_DebeRetornarNotFound()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockServicio.Setup(s => s.PublicarAnuncioAsync(99, 15)).ReturnsAsync(false);

        // Act
        var resultado = await _controller.Publicar(99);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task Publicar_Exitoso_DebeRetornarOk()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockServicio.Setup(s => s.PublicarAnuncioAsync(5, 15)).ReturnsAsync(true);

        // Act
        var resultado = await _controller.Publicar(5);

        // Assert
        Assert.IsType<OkObjectResult>(resultado);
        _mockServicio.Verify(s => s.PublicarAnuncioAsync(5, 15), Times.Once);
    }

    // =========================================================================
    // 4. SUBIR IMÁGENES
    // =========================================================================

    [Fact]
    public async Task SubirImagenes_SinImagenes_DebeRetornarBadRequest()
    {
        // Arrange
        SimularUsuarioAutenticado("15");

        // Act
        var resultado = await _controller.SubirImagenes(5, new List<IFormFile>());

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task SubirImagenes_MasDeDiez_DebeRetornarBadRequest()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        var demasiadas = Enumerable.Range(0, 11).Select(_ => CrearFormFile()).ToList();

        // Act
        var resultado = await _controller.SubirImagenes(5, demasiadas);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(400, badRequest.StatusCode);
    }

    // =========================================================================
    // 4b. ESTABLECER FOTO PRINCIPAL
    // =========================================================================

    [Fact]
    public async Task EstablecerFotoPrincipal_UrlVacia_DebeRetornarBadRequest()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        var dto = new AnuncioFotoPrincipalDto { UrlImagen = "" };

        // Act
        var resultado = await _controller.EstablecerFotoPrincipal(5, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
        _mockServicio.Verify(
            s => s.EstablecerFotoPrincipalAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task EstablecerFotoPrincipal_AnuncioNoExiste_DebeRetornarNotFound()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockServicio
            .Setup(s => s.EstablecerFotoPrincipalAsync(99, 15, "uploads/f2.jpg"))
            .ReturnsAsync(false);

        // Act
        var resultado = await _controller.EstablecerFotoPrincipal(99, new AnuncioFotoPrincipalDto { UrlImagen = "uploads/f2.jpg" });

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task EstablecerFotoPrincipal_Exitoso_DebeRetornarOk()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockServicio
            .Setup(s => s.EstablecerFotoPrincipalAsync(5, 15, "uploads/f2.jpg"))
            .ReturnsAsync(true);

        // Act
        var resultado = await _controller.EstablecerFotoPrincipal(5, new AnuncioFotoPrincipalDto { UrlImagen = "uploads/f2.jpg" });

        // Assert
        Assert.IsType<OkObjectResult>(resultado);
        _mockServicio.Verify(s => s.EstablecerFotoPrincipalAsync(5, 15, "uploads/f2.jpg"), Times.Once);
    }

    [Fact]
    public async Task EstablecerFotoPrincipal_NoEsElDueno_DebeRetornar403()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockServicio
            .Setup(s => s.EstablecerFotoPrincipalAsync(5, 15, "uploads/f2.jpg"))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var resultado = await _controller.EstablecerFotoPrincipal(5, new AnuncioFotoPrincipalDto { UrlImagen = "uploads/f2.jpg" });

        // Assert
        var status = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(403, status.StatusCode);
    }

    // =========================================================================
    // 5. CAMBIAR ESTADO
    // =========================================================================

    [Fact]
    public async Task CambiarEstado_EstadoVacio_DebeRetornarBadRequest()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        var dto = new AnuncioEstadoDto { Estado = "   " };

        // Act
        var resultado = await _controller.CambiarEstado(5, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
        _mockServicio.Verify(
            s => s.CambiarEstadoAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task CambiarEstado_NoEncontrado_DebeRetornarNotFound()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockServicio.Setup(s => s.CambiarEstadoAsync(99, 15, "Pausado")).ReturnsAsync(false);

        // Act
        var resultado = await _controller.CambiarEstado(99, new AnuncioEstadoDto { Estado = "Pausado" });

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task CambiarEstado_Exitoso_DebeRetornarOk()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockServicio.Setup(s => s.CambiarEstadoAsync(5, 15, "Pausado")).ReturnsAsync(true);

        // Act
        var resultado = await _controller.CambiarEstado(5, new AnuncioEstadoDto { Estado = "Pausado" });

        // Assert
        Assert.IsType<OkObjectResult>(resultado);
        _mockServicio.Verify(s => s.CambiarEstadoAsync(5, 15, "Pausado"), Times.Once);
    }

    // =========================================================================
    // 6. ELIMINAR IMAGEN
    // =========================================================================

    [Fact]
    public async Task EliminarImagen_SinUrl_DebeRetornarBadRequest()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        var dto = new EliminarImagenDto { UrlImagen = "" };

        // Act
        var resultado = await _controller.EliminarImagen(5, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task EliminarImagen_NoEsElDueno_DebeRetornar403()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockServicio
            .Setup(s => s.EliminarImagenAsync(5, 15, "https://s3/url"))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var resultado = await _controller.EliminarImagen(5, new EliminarImagenDto { UrlImagen = "https://s3/url" });

        // Assert
        var status = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(403, status.StatusCode);
    }

    [Fact]
    public async Task EliminarImagen_Exitoso_DebeRetornarOk()
    {
        // Arrange
        SimularUsuarioAutenticado("15");

        // Act
        var resultado = await _controller.EliminarImagen(5, new EliminarImagenDto { UrlImagen = "https://s3/url" });

        // Assert
        Assert.IsType<OkObjectResult>(resultado);
        _mockServicio.Verify(s => s.EliminarImagenAsync(5, 15, "https://s3/url"), Times.Once);
    }

    // =========================================================================
    // 7. REGISTRAR VISTA
    // =========================================================================

    [Fact]
    public async Task RegistrarVista_AnuncioNoExiste_DebeRetornarNotFound()
    {
        // Arrange
        _mockServicio
            .Setup(s => s.RegistrarVistaAsync(99))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        var resultado = await _controller.RegistrarVista(99);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task RegistrarVista_Exitoso_DebeRetornarOk()
    {
        // Arrange
        _mockServicio.Setup(s => s.RegistrarVistaAsync(5)).Returns(Task.CompletedTask);

        // Act
        var resultado = await _controller.RegistrarVista(5);

        // Assert
        Assert.IsType<OkObjectResult>(resultado);
    }

    // =========================================================================
    // 8. ELIMINAR ANUNCIO
    // =========================================================================

    [Fact]
    public async Task EliminarAnuncio_NoEncontrado_DebeRetornarNotFound()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockServicio.Setup(s => s.EliminarAnuncioAsync(99, 15)).ReturnsAsync(false);

        // Act
        var resultado = await _controller.EliminarAnuncio(99);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task EliminarAnuncio_Exitoso_DebeRetornarOk()
    {
        // Arrange
        SimularUsuarioAutenticado("15");
        _mockServicio.Setup(s => s.EliminarAnuncioAsync(5, 15)).ReturnsAsync(true);

        // Act
        var resultado = await _controller.EliminarAnuncio(5);

        // Assert
        Assert.IsType<OkObjectResult>(resultado);
    }
}