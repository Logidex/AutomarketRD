using AutoMarket.Application.Services;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using Moq;
using Xunit;

namespace AutoMarket.Tests.Services;

public class FavoritoServiceTests
{
    private readonly Mock<IFavoritoRepository> _mockFavoritoRepository;
    private readonly Mock<IAnuncioRepository> _mockAnuncioRepository;
    private readonly FavoritoService _service;

    public FavoritoServiceTests()
    {
        _mockFavoritoRepository = new Mock<IFavoritoRepository>();
        _mockAnuncioRepository = new Mock<IAnuncioRepository>();
        _service = new FavoritoService(_mockFavoritoRepository.Object, _mockAnuncioRepository.Object);
    }

    private static Anuncio CrearAnuncioSimulado(
        int usuarioId,
        string marca,
        string modelo,
        int anio,
        decimal precio,
        List<string>? fotos = null)
    {
        return new Anuncio(
            usuarioId,
            marca,
            modelo,
            "Sedan",
            "Blanco",
            "Negro",
            anio,
            precio,
            25000,
            "Automática",
            "Gasolina",
            fotos ?? new List<string>(),
            "Santo Domingo",
            "Vehículo de prueba"
        );
    }

    [Fact]
    public async Task AgregarFavoritoAsync_CuandoAnuncioNoExiste_DebeLanzarKeyNotFoundException()
    {
        _mockAnuncioRepository
            .Setup(r => r.ObtenerPorIdAsync(10))
            .ReturnsAsync((Anuncio?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.AgregarFavoritoAsync(123, 10));

        Assert.Equal("El anuncio no existe.", ex.Message);
        _mockFavoritoRepository.Verify(r => r.ObtenerAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _mockFavoritoRepository.Verify(r => r.AgregarAsync(It.IsAny<UsuarioFavorito>()), Times.Never);
    }

    [Fact]
    public async Task AgregarFavoritoAsync_CuandoYaExiste_DebeLanzarInvalidOperationException()
    {
        var anuncio = CrearAnuncioSimulado(10, "Toyota", "Corolla", 2022, 18500m);

        _mockAnuncioRepository
            .Setup(r => r.ObtenerPorIdAsync(10))
            .ReturnsAsync(anuncio);

        _mockFavoritoRepository
            .Setup(r => r.ObtenerAsync(123, 10))
            .ReturnsAsync(new UsuarioFavorito(123, 10));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AgregarFavoritoAsync(123, 10));

        Assert.Equal("El vehículo ya está en tus favoritos.", ex.Message);
        _mockFavoritoRepository.Verify(r => r.AgregarAsync(It.IsAny<UsuarioFavorito>()), Times.Never);
    }

    [Fact]
    public async Task AgregarFavoritoAsync_CuandoEsValido_DebeAgregarFavorito()
    {
        var anuncio = CrearAnuncioSimulado(10, "Toyota", "Corolla", 2022, 18500m);

        _mockAnuncioRepository
            .Setup(r => r.ObtenerPorIdAsync(10))
            .ReturnsAsync(anuncio);

        _mockFavoritoRepository
            .Setup(r => r.ObtenerAsync(123, 10))
            .ReturnsAsync((UsuarioFavorito?)null);

        _mockFavoritoRepository
            .Setup(r => r.AgregarAsync(It.IsAny<UsuarioFavorito>()))
            .Returns(Task.CompletedTask);

        await _service.AgregarFavoritoAsync(123, 10);

        _mockFavoritoRepository.Verify(r => r.AgregarAsync(It.Is<UsuarioFavorito>(f =>
            f.UsuarioId == 123 && f.AnuncioId == 10)), Times.Once);
    }

    [Fact]
    public async Task QuitarFavoritoAsync_CuandoNoExiste_DebeLanzarKeyNotFoundException()
    {
        _mockFavoritoRepository
            .Setup(r => r.ObtenerAsync(123, 10))
            .ReturnsAsync((UsuarioFavorito?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.QuitarFavoritoAsync(123, 10));

        Assert.Equal("El vehículo no estaba en tus favoritos.", ex.Message);
        _mockFavoritoRepository.Verify(r => r.EliminarAsync(It.IsAny<UsuarioFavorito>()), Times.Never);
    }

    [Fact]
    public async Task QuitarFavoritoAsync_CuandoExiste_DebeEliminarFavorito()
    {
        var favorito = new UsuarioFavorito(123, 10);

        _mockFavoritoRepository
            .Setup(r => r.ObtenerAsync(123, 10))
            .ReturnsAsync(favorito);

        _mockFavoritoRepository
            .Setup(r => r.EliminarAsync(favorito))
            .Returns(Task.CompletedTask);

        await _service.QuitarFavoritoAsync(123, 10);

        _mockFavoritoRepository.Verify(r => r.EliminarAsync(favorito), Times.Once);
    }

    [Fact]
    public async Task ObtenerFavoritosAsync_DebeMapearAnunciosAFavoritosDto()
    {
        var anuncios = new List<Anuncio>
        {
            CrearAnuncioSimulado(
                usuarioId: 1,
                marca: "Toyota",
                modelo: "Corolla",
                anio: 2022,
                precio: 18500m,
                fotos: new List<string> { "foto1.jpg", "foto2.jpg" }),

            CrearAnuncioSimulado(
                usuarioId: 2,
                marca: "Honda",
                modelo: "Civic",
                anio: 2021,
                precio: 17500m,
                fotos: new List<string>())
        };

        _mockFavoritoRepository
            .Setup(r => r.ObtenerAnunciosFavoritosAsync(123))
            .ReturnsAsync(anuncios);

        var resultado = await _service.ObtenerFavoritosAsync(123);

        var lista = resultado.ToList();

        Assert.Equal("Toyota", lista[0].Marca);
        Assert.Equal("Corolla", lista[0].Modelo);
        Assert.Equal(2022, lista[0].Anio);
        Assert.Equal(18500m, lista[0].Precio);

        Assert.Equal("Honda", lista[1].Marca);
        Assert.Equal("Civic", lista[1].Modelo);
        Assert.Equal(2021, lista[1].Anio);
        Assert.Equal(17500m, lista[1].Precio);
    }
}