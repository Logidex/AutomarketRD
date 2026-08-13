using Moq;
using Xunit;
using AutoMarket.Application.Services;
using AutoMarket.Core.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Application.DTOs;
using Microsoft.AspNetCore.Http;
using AutoMarket.Core.Exceptions;
using System.Reflection;
using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Tests.Services;

public class AnuncioServiceTests
{
    // Mocks centralizados para evitar repetir código en cada prueba
    private readonly Mock<IAnuncioRepository> _mockRepo;
    private readonly Mock<IAlmacenadorArchivos> _mockArchivos;
    private readonly Mock<IUsuarioRepository> _mockUsuarioRepo; // 🌟 Nueva dependencia agregada
    private readonly AnuncioService _servicio;

    // El constructor corre automáticamente ANTES de cada prueba individual
    public AnuncioServiceTests()
    {
        _mockRepo = new Mock<IAnuncioRepository>();
        _mockArchivos = new Mock<IAlmacenadorArchivos>();
        _mockUsuarioRepo = new Mock<IUsuarioRepository>(); // Inicializamos el nuevo Mock

        // Instanciamos el servicio UNA SOLA VEZ pasándole los 3 parámetros requeridos
        _servicio = new AnuncioService(_mockRepo.Object, _mockArchivos.Object, _mockUsuarioRepo.Object);
    }

    // =========================================================================
    // PRUEBA 8: Obtener Todos los Anuncios (Éxito)
    // =========================================================================
    [Fact]
    public async Task ObtenerTodosLosAnuncios_DebeRetornarListaDeDtos()
    {
        // 1. ARRANGE
        var listaSimulada = new List<Anuncio>
        {
            new Anuncio(1, "Toyota", "Corolla", "", "Sedan", "1.8L", "Delantera", "Blanco", "Negro", 2015, 600000, "DOP", 80000, "Automática", "Gasolina", new List<string> { "Ninguno" }, "Santo Domingo", "Excelente estado")
        };

        // Configuramos el mock centralizado
        _mockRepo.Setup(r => r.ObtenerTodosLosAnuncios()).ReturnsAsync(listaSimulada);

        // 2. ACT (Usamos el _servicio centralizado)
        var resultado = await _servicio.ObtenerTodosLosAnuncios();

        // 3. ASSERT
        Assert.NotNull(resultado);
        Assert.Single(resultado);
        Assert.Equal(600000, resultado.First().Precio);
    }

    // =========================================================================
    // PRUEBA 9: Obtener Por Id - Fallo por no encontrado
    // =========================================================================
    [Fact]
    public async Task ObtenerAnuncioPorIdAsync_NoEncontrado_DebeRetornarNull()
    {
        // 1. ARRANGE
        var idFalso = 999;
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(idFalso)).ReturnsAsync((Anuncio?)null);

        // 2. ACT
        var resultado = await _servicio.ObtenerAnuncioPorIdAsync(idFalso);

        // 3. ASSERT
        Assert.Null(resultado);
    }

// =========================================================================
    // PRUEBA 10: Obtener Por Id - Éxito al mapear el DTO (el dueño ve su borrador)
    // =========================================================================
    [Fact]
    public async Task ObtenerAnuncioPorIdAsync_Encontrado_DebeRetornarDto()
    {
        // 1. ARRANGE
        var idReal = 5;
        var anuncioEnBD = new Anuncio(1, "Honda", "Civic", "", "Sedan", "1.8L", "Delantera", "Rojo", "Gris", 2022, 1200000, "DOP", 15000, "Automática", "Gasolina", new List<string> { "Sunroof" }, "Santiago", "Casi nuevo");

        _mockRepo.Setup(r => r.ObtenerPorIdAsync(idReal)).ReturnsAsync(anuncioEnBD);

        // 2. ACT (El usuario autenticado es el dueño del anuncio -> puede ver su borrador)
        var resultado = await _servicio.ObtenerAnuncioPorIdAsync(idReal, usuarioId: 1);

        // 3. ASSERT
        Assert.NotNull(resultado);
        Assert.Equal("Honda", resultado.Marca);
        Assert.Equal("Civic", resultado.Modelo);
        Assert.Equal(2022, resultado.Anio);
    }

    // =========================================================================
    // PRUEBA 10b: Obtener Por Id - No publicado + No es el dueño -> null (404)
    // =========================================================================
    [Fact]
    public async Task ObtenerAnuncioPorIdAsync_BorradorDeOtroUsuario_DebeRetornarNull()
    {
        // 1. ARRANGE
        var idReal = 5;
        var anuncioEnBD = new Anuncio(1, "Honda", "Civic", "", "Sedan", "1.8L", "Delantera", "Rojo", "Gris", 2022, 1200000, "DOP", 15000, "Automática", "Gasolina", new List<string> { "Sunroof" }, "Santiago", "Casi nuevo");

        _mockRepo.Setup(r => r.ObtenerPorIdAsync(idReal)).ReturnsAsync(anuncioEnBD);

        // 2. ACT (Un usuario autenticado cualquiera, que NO es el dueño)
        var resultado = await _servicio.ObtenerAnuncioPorIdAsync(idReal, usuarioId: 99);

        // 3. ASSERT
        Assert.Null(resultado);
    }

    // =========================================================================
    // PRUEBA 10c: Obtener Por Id - Público sin sesión NO ve borradores de nadie
    // =========================================================================
    [Fact]
    public async Task ObtenerAnuncioPorIdAsync_SinSesionYNoPublicado_DebeRetornarNull()
    {
        // 1. ARRANGE
        var idBorrador = 7;
        var anuncioBorrador = new Anuncio(3, "Toyota", "Corolla", "", "Sedan", "1.8L", "Delantera", "Blanco", "Negro", 2021, 900000, "DOP", 20000, "Automática", "Gasolina", new List<string>(), "SDQ", "Nuevo");

        _mockRepo.Setup(r => r.ObtenerPorIdAsync(idBorrador)).ReturnsAsync(anuncioBorrador);

        // 2. ACT (Sin token, visitante anónimo)
        var resultado = await _servicio.ObtenerAnuncioPorIdAsync(idBorrador);

        // 3. ASSERT
        Assert.Null(resultado);
    }

    // =========================================================================
    // PRUEBA 10d: Obtener Por Id - Vendedor particular marca EsVendedorParticular
    // =========================================================================
    [Fact]
    public async Task ObtenerAnuncioPorIdAsync_VendedorParticular_DebeMarcarEsVendedorParticular()
    {
        // ARRANGE
        var idReal = 6;
        var anuncio = new Anuncio(6, "Ford", "Mustang", "", "Deportivo", "5.0L", "Trasera", "Rojo", "Negro", 2020, 1500000, "DOP", 30000, "Automática", "Gasolina", new List<string>(), "Santo Domingo", "Único dueño");

        _mockRepo.Setup(r => r.ObtenerPorIdAsync(idReal)).ReturnsAsync(anuncio);

        var vendedor = new Usuario(
            nombre: "Juan",
            apellido: "Perez",
            email: "jperez@gmail.com",
            passwordHash: "hash",
            telefonoPersonal: "8290001234",
            rol: "Vendedor",
            emailConfirmado: true
        );

        typeof(Usuario).GetProperty("UsuarioId")?.SetValue(vendedor, 6);

        _mockUsuarioRepo
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(6))
            .ReturnsAsync(vendedor);

        // ACT (el dueño ve su anuncio sin importar el estado)
        var resultado = await _servicio.ObtenerAnuncioPorIdAsync(idReal, usuarioId: 6);

        // ASSERT
        Assert.NotNull(resultado);
        Assert.Equal("Juan Perez", resultado.NombreVendedor);
        Assert.True(resultado.EsVendedorParticular);
    }

    // =========================================================================
    // PRUEBA 10e: Obtener Por Id - Dealer (agencia) NO es vendedor particular
    // =========================================================================
    [Fact]
    public async Task ObtenerAnuncioPorIdAsync_Dealer_DebeMarcarEsVendedorParticularFalso()
    {
        // ARRANGE
        var idReal = 5;
        var anuncio = new Anuncio(5, "Honda", "Civic", "", "Sedan", "1.8L", "Delantera", "Blanco", "Negro", 2022, 1200000, "DOP", 15000, "Automática", "Gasolina", new List<string>(), "Santiago", "Casi nuevo");

        _mockRepo.Setup(r => r.ObtenerPorIdAsync(idReal)).ReturnsAsync(anuncio);

        var dealer = CrearUsuarioDealerSinSuscripcion(5);

        _mockUsuarioRepo
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(5))
            .ReturnsAsync(dealer);

        // ACT
        var resultado = await _servicio.ObtenerAnuncioPorIdAsync(idReal, usuarioId: 5);

        // ASSERT
        Assert.NotNull(resultado);
        Assert.False(resultado.EsVendedorParticular);
    }

    // =========================================================================
    // PRUEBA 11: Crear Anuncio - Éxito al instanciar y guardar
    // =========================================================================
    [Fact]
    public async Task CrearAnuncioAsync_DatosValidos_DebeGuardarEnRepositorio()
    {
        // 1. ARRANGE
        var dto = new AnuncioCreateDto
        {
            UsuarioId = 10,
            Marca = "Toyota",
            Modelo = "Corolla",
            TipoVehiculo = "Sedan",
            Motor = "1.8L",
            Traccion = "Delantera",
            ColorExterior = "Blanco",
            ColorInterior = "Negro",
            Anio = 2020,
            Precio = 800000,
            Kilometraje = 45000,
            Transmision = "Automática",
            Combustible = "Gasolina",
            Accesorios = new List<string> { "Aros de magnesio", "Radio Android" },
            Ubicacion = "Santo Domingo",
            Descripcion = "Vehículo en excelentes condiciones"
        };

        var usuarioValido = CrearUsuarioSimulado(10, esDealer: false);
        _mockUsuarioRepo.Setup(r => r.ObtenerDealerConPerfilPorIdAsync(10)).ReturnsAsync(usuarioValido);

        // 2. ACT
        await _servicio.CrearAnuncioAsync(dto);

        // 3. ASSERT
        _mockRepo.Verify(r => r.AgregarAsync(It.Is<Anuncio>(a =>
            a.UsuarioId == dto.UsuarioId &&
            a.Marca == dto.Marca &&
            a.Modelo == dto.Modelo &&
            a.Precio == dto.Precio
        )), Times.Once);

        _mockRepo.Verify(r => r.GuardarCambiosAsync(), Times.Once);
    }

    // =========================================================================
    // PRUEBA 11b: Crear Anuncio - Normaliza acentos en campos de búsqueda
    // =========================================================================
    [Fact]
    public async Task CrearAnuncioAsync_NormalizaAcentosEnCamposDeBusqueda()
    {
        // 1. ARRANGE
        var dto = new AnuncioCreateDto
        {
            UsuarioId = 11,
            Marca = "Renault",
            Modelo = "Clío",
            TipoVehiculo = "Sedán",
            Motor = "1.2L",
            Traccion = "Delantera",
            ColorExterior = "Gris",
            ColorInterior = "Negro",
            Anio = 2021,
            Precio = 900000,
            Kilometraje = 30000,
            Transmision = "Automática",
            Combustible = "Diésel",
            Accesorios = new List<string>(),
            Ubicacion = "Santo Domingo Oeste",
            Descripcion = "Vehículo en buenas condiciones"
        };

        var usuarioValido = CrearUsuarioSimulado(11, esDealer: false);
        _mockUsuarioRepo.Setup(r => r.ObtenerDealerConPerfilPorIdAsync(11)).ReturnsAsync(usuarioValido);

        // 2. ACT
        await _servicio.CrearAnuncioAsync(dto);

        // 3. ASSERT: se guardan sin acentos para que el ILike de la búsqueda coincida
        _mockRepo.Verify(r => r.AgregarAsync(It.Is<Anuncio>(a =>
            a.Modelo == "Clio" &&
            a.TipoVehiculo == "Sedan" &&
            a.Transmision == "Automatica" &&
            a.Combustible == "Diesel"
        )), Times.Once);
    }

    // =========================================================================
    // PRUEBA 12: Actualizar - Fallo por no encontrado
    // =========================================================================
    [Fact]
    public async Task ActualizarAsync_AnuncioNoExiste_DebeRetornarNull()
    {
        // 1. ARRANGE
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(999)).ReturnsAsync((Anuncio?)null);

        // 2. ACT
        var resultado = await _servicio.ActualizarAsync(999, 1, new AnuncioUpdateDto());

        // 3. ASSERT
        Assert.Null(resultado);
    }

    // =========================================================================
    // PRUEBA 13: Actualizar - Fallo por seguridad (Intento de Hackeo 🕵️)
    // =========================================================================
    [Fact]
    public async Task ActualizarAsync_NoEsElDueno_DebeLanzarExcepcion()
    {
        // 1. ARRANGE
        var idAnuncio = 5;
        var idDueñoReal = 1;
        var idHacker = 99;

        var anuncioEnBD = new Anuncio(idDueñoReal, "Honda", "Civic", "", "Sedan", "1.8L", "Delantera", "Rojo", "Gris", 2022, 1200000, "DOP", 15000, "Automática", "Gasolina", new List<string>(), "Santiago", "Casi nuevo");

        _mockRepo.Setup(r => r.ObtenerPorIdAsync(idAnuncio)).ReturnsAsync(anuncioEnBD);

        // 2 & 3. ACT & ASSERT
        var excepcion = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _servicio.ActualizarAsync(idAnuncio, idHacker, new AnuncioUpdateDto())
        );

        Assert.Equal("Acceso denegado: No tienes permiso para modificar un anuncio que no te pertenece.", excepcion.Message);
        _mockRepo.Verify(r => r.ActualizarAsync(It.IsAny<Anuncio>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 14: Actualizar - Éxito al modificar (Es el dueño real)
    // =========================================================================
    [Fact]
    public async Task ActualizarAsync_EsElDueno_DebeActualizarYRetornarDto()
    {
        // 1. ARRANGE
        var idAnuncio = 5;
        var idDueño = 1;

        var anuncioEnBD = new Anuncio(idDueño, "Honda", "Civic", "", "Sedan", "1.8L", "Delantera", "Rojo", "Gris", 2022, 1200000, "DOP", 15000, "Automática", "Gasolina", new List<string>(), "Santiago", "Casi nuevo");

        var updateDto = new AnuncioUpdateDto
        {
            Marca = "Honda",
            Modelo = "Civic",
            TipoVehiculo = "Sedan",
            Motor = "1.5L",
            Traccion = "Delantera",
            ColorExterior = "Azul", // 👈 Cambió a Azul
            ColorInterior = "Gris",
            Anio = 2022,
            Precio = 1100000, // 👈 Le bajó el precio
            Kilometraje = 16000,
            Transmision = "Automática",
            Combustible = "Gasolina",
            Accesorios = new List<string>(),
            Ubicacion = "Santiago",
            Descripcion = "Actualizado"
        };

        _mockRepo.Setup(r => r.ObtenerPorIdAsync(idAnuncio)).ReturnsAsync(anuncioEnBD);

        // 2. ACT
        var resultado = await _servicio.ActualizarAsync(idAnuncio, idDueño, updateDto);

        // 3. ASSERT
        Assert.NotNull(resultado);
        _mockRepo.Verify(r => r.ActualizarAsync(It.Is<Anuncio>(a => a.ColorExterior == "Azul" && a.Precio == 1100000)), Times.Once);
    }

    // =========================================================================
    // PRUEBA 15: Publicar - Fallo por no encontrado
    // =========================================================================
    [Fact]
    public async Task PublicarAnuncioAsync_AnuncioNoExiste_DebeRetornarFalso()
    {
        // 1. ARRANGE
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(999)).ReturnsAsync((Anuncio?)null);

        // 2. ACT
        var resultado = await _servicio.PublicarAnuncioAsync(999, 1);

        // 3. ASSERT
        Assert.False(resultado);
    }

    // =========================================================================
    // PRUEBA 16: Publicar - Fallo por seguridad (No es el dueño)
    // =========================================================================
    [Fact]
    public async Task PublicarAnuncioAsync_NoEsElDueno_DebeLanzarExcepcion()
    {
        // 1. ARRANGE
        var idAnuncio = 5;
        var idDueñoReal = 1;
        var idHacker = 99;

        var anuncioEnBD = new Anuncio(idDueñoReal, "Honda", "Civic", "", "Sedan", "1.8L", "Delantera", "Rojo", "Gris", 2022, 1200000, "DOP", 15000, "Automática", "Gasolina", new List<string>(), "Santiago", "Casi nuevo");

        _mockRepo.Setup(r => r.ObtenerPorIdAsync(idAnuncio)).ReturnsAsync(anuncioEnBD);

        // 2 & 3. ACT & ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _servicio.PublicarAnuncioAsync(idAnuncio, idHacker)
        );

        _mockRepo.Verify(r => r.ActualizarAsync(It.IsAny<Anuncio>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 17: Publicar - Éxito al cambiar estado a publicado
    // =========================================================================
    [Fact]
    public async Task PublicarAnuncioAsync_EsElDueno_DebePublicarYRetornarTrue()
    {
        // 1. ARRANGE
        var idAnuncio = 5;
        var idDueño = 1;

        var anuncioEnBD = new Anuncio(idDueño, "Honda", "Civic", "", "Sedan", "1.8L", "Delantera", "Rojo", "Gris", 2022, 1200000, "DOP", 15000, "Automática", "Gasolina", new List<string>(), "Santiago", "Casi nuevo");

        // Regla de negocio: agregamos 5 fotos
        anuncioEnBD.AgregarFotos(new List<string>
        {
            "url1.jpg", "url2.jpg", "url3.jpg", "url4.jpg", "url5.jpg"
        });

        _mockRepo.Setup(r => r.ObtenerPorIdAsync(idAnuncio)).ReturnsAsync(anuncioEnBD);

        _mockUsuarioRepo
            .Setup(r => r.ObtenerDealerConPerfilPorIdAsync(idDueño))
            .ReturnsAsync(CrearUsuarioDealerConSuscripcion(idDueño, PlanNivel.Pro, CicloFacturacion.Mensual, EstadoSuscripcion.Activa));

        // 2. ACT
        var resultado = await _servicio.PublicarAnuncioAsync(idAnuncio, idDueño);

        // 3. ASSERT
        Assert.True(resultado);
        _mockRepo.Verify(r => r.ActualizarAsync(It.Is<Anuncio>(a => a.Estado == "Publicado")), Times.Once);
    }

    // =========================================================================
    // PRUEBA 17b: Publicar - Fallo por límite del plan alcanzado
    // =========================================================================
    [Fact]
    public async Task PublicarAnuncioAsync_SinCupoEnElPlan_DebeLanzarBusinessRuleException()
    {
        // 1. ARRANGE
        var idAnuncio = 5;
        var idDueño = 1;

        var anuncioEnBD = new Anuncio(idDueño, "Honda", "Civic", "", "Sedan", "1.8L", "Delantera", "Rojo", "Gris", 2022, 1200000, "DOP", 15000, "Automática", "Gasolina", new List<string>(), "Santiago", "Casi nuevo");
        anuncioEnBD.AgregarFotos(new List<string> { "url1.jpg", "url2.jpg", "url3.jpg", "url4.jpg", "url5.jpg" });

        var usuario = CrearUsuarioDealerConSuscripcion(idDueño, PlanNivel.Gratis, CicloFacturacion.Mensual, EstadoSuscripcion.Activa);

        // Un anuncio ya publicado ocupa el único cupo del plan Gratis
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(idAnuncio)).ReturnsAsync(anuncioEnBD);
        _mockRepo.Setup(r => r.ContarAnunciosPorUsuarioAsync(idDueño)).ReturnsAsync(1);
        _mockUsuarioRepo.Setup(r => r.ObtenerDealerConPerfilPorIdAsync(idDueño)).ReturnsAsync(usuario);

        // 2 & 3. ACT & ASSERT
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _servicio.PublicarAnuncioAsync(idAnuncio, idDueño)
        );

        _mockRepo.Verify(r => r.ActualizarAsync(It.IsAny<Anuncio>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 18: Subir Imágenes - Fallo por no encontrado
    // =========================================================================
    [Fact]
    public async Task SubirImagenesAsync_AnuncioNoExiste_DebeLanzarKeyNotFoundException()
    {
        // 1. ARRANGE
        var dto = new AnuncioImagenUploadDto { AnuncioId = 999, UsuarioId = 1, Imagenes = new List<IFormFile>() };

        _mockRepo.Setup(r => r.ObtenerPorIdAsync(dto.AnuncioId)).ReturnsAsync((Anuncio?)null);

        // 2 & 3. ACT & ASSERT
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _servicio.SubirImagenesAsync(dto));
    }

    // =========================================================================
    // PRUEBA 19: Subir Imágenes - Fallo por seguridad (No es el dueño)
    // =========================================================================
    [Fact]
    public async Task SubirImagenesAsync_NoEsElDueno_DebeLanzarUnauthorizedAccessException()
    {
        // 1. ARRANGE
        var idDueñoReal = 1;
        var dto = new AnuncioImagenUploadDto { AnuncioId = 5, UsuarioId = 99, Imagenes = new List<IFormFile>() };

        var anuncioEnBD = new Anuncio(idDueñoReal, "Honda", "Civic", "", "Sedan", "1.8L", "Delantera", "Rojo", "Gris", 2022, 1200000, "DOP", 15000, "Automática", "Gasolina", new List<string>(), "Santiago", "Casi nuevo");

        _mockRepo.Setup(r => r.ObtenerPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncioEnBD);

        // 2 & 3. ACT & ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _servicio.SubirImagenesAsync(dto));
    }

    // =========================================================================
    // PRUEBA 20: Subir Imágenes - Fallo por tamaño (> 5MB)
    // =========================================================================
    [Fact]
    public async Task SubirImagenesAsync_ImagenMuyGrande_DebeLanzarArgumentException()
    {
        // 1. ARRANGE
        var idDueño = 1;
        var anuncioEnBD = new Anuncio(idDueño, "Honda", "Civic", "", "Sedan", "1.8L", "Delantera", "Rojo", "Gris", 2022, 1200000, "DOP", 15000, "Automática", "Gasolina", new List<string>(), "Santiago", "Casi nuevo");
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(5)).ReturnsAsync(anuncioEnBD);

        // Simulamos un archivo de 6 Megabytes
        var mockArchivoPesado = new Mock<IFormFile>();
        mockArchivoPesado.Setup(f => f.Length).Returns(6 * 1024 * 1024);

        var dto = new AnuncioImagenUploadDto { AnuncioId = 5, UsuarioId = idDueño, Imagenes = new List<IFormFile> { mockArchivoPesado.Object } };

        // 2 & 3. ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentException>(() => _servicio.SubirImagenesAsync(dto));
    }

    // =========================================================================
    // PRUEBA 21: Crear Anuncio - Éxito para particular con 0 anuncios
    // =========================================================================
    [Fact]
    public async Task CrearAnuncioAsync_UsuarioParticularConCeroAnuncios_DebeCrearAnuncio()
    {
        // Arrange
        // 🌟 FIX: Agregamos Anio = 2022 para pasar la validación
        var dto = new AnuncioCreateDto { UsuarioId = 1, Marca = "Toyota", Modelo = "Civic", TipoVehiculo = "Sedan", Motor = "1.8L", Traccion = "Delantera", ColorExterior = "Blanco", ColorInterior = "Negro", Precio = 500000, Anio = 2022, Kilometraje = 0, Transmision = "Automática", Combustible = "Gasolina", Accesorios = new List<string>(), Ubicacion = "Santo Domingo", Descripcion = "Vehículo de prueba" };

        var usuarioParticular = CrearUsuarioSimulado(id: 1, esDealer: false);

        _mockUsuarioRepo.Setup(repo => repo.ObtenerDealerConPerfilPorIdAsync(1))
            .ReturnsAsync(usuarioParticular);

        _mockRepo.Setup(repo => repo.ContarAnunciosPorUsuarioAsync(1))
            .ReturnsAsync(0);

        // Act
        await _servicio.CrearAnuncioAsync(dto);

        // Assert
        _mockRepo.Verify(repo => repo.AgregarAsync(It.IsAny<Anuncio>()), Times.Once);
    }

    // =========================================================================
    // PRUEBA 22: Crear Anuncio - Fallo por límite alcanzado (Particular con 1 anuncio)
    // =========================================================================
    [Fact]
    public async Task CrearAnuncioAsync_UsuarioParticularConUnAnuncio_DebeLanzarBusinessRuleException()
    {
        // Arrange
        var dto = new AnuncioCreateDto { UsuarioId = 1, Marca = "Honda", Modelo = "Accord" };

        var usuarioParticular = CrearUsuarioSimulado(id: 1, esDealer: false);

        _mockUsuarioRepo.Setup(repo => repo.ObtenerDealerConPerfilPorIdAsync(1))
            .ReturnsAsync(usuarioParticular);

        _mockRepo.Setup(repo => repo.ContarAnunciosPorUsuarioAsync(1))
            .ReturnsAsync(1); // Ya tiene el límite consumido

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _servicio.CrearAnuncioAsync(dto));

        Assert.Equal("Has alcanzado el límite de 1 anuncio gratuito. Mejora tu cuenta a Dealer para publicar más inventario.", excepcion.Message);
        _mockRepo.Verify(repo => repo.AgregarAsync(It.IsAny<Anuncio>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 23: Crear Anuncio - Fallo para Dealer sin suscripción
    // =========================================================================
    [Fact]
    public async Task CrearAnuncioAsync_DealerSinSuscripcion_DebeLanzarBusinessRuleException()
    {
        var dto = new AnuncioCreateDto
        {
            UsuarioId = 2,
            Marca = "Ford",
            Modelo = "Explorer",
            TipoVehiculo = "SUV",
            ColorExterior = "Negro",
            ColorInterior = "Negro",
            Anio = 2023,
            Precio = 900000,
            Kilometraje = 40000,
            Transmision = "Automática",
            Combustible = "Gasolina",
            Accesorios = new List<string>(),
            Ubicacion = "Santo Domingo",
            Descripcion = "Dealer sin plan"
        };

        var usuarioDealer = CrearUsuarioDealerSinSuscripcion(2);

        _mockUsuarioRepo.Setup(repo => repo.ObtenerDealerConPerfilPorIdAsync(2))
            .ReturnsAsync(usuarioDealer);

        _mockRepo.Setup(repo => repo.ContarAnunciosPorUsuarioAsync(2))
            .ReturnsAsync(0);

        var excepcion = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _servicio.CrearAnuncioAsync(dto));

        Assert.Contains("suscripción activa", excepcion.Message);
        _mockRepo.Verify(repo => repo.AgregarAsync(It.IsAny<Anuncio>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 24: Crear Anuncio - Dealer con suscripción cancelada pero días vigentes puede publicar
    // =========================================================================
    [Fact]
    public async Task CrearAnuncioAsync_DealerConSuscripcionCanceladaConVigencia_DebePermitir()
    {
        var dto = new AnuncioCreateDto
        {
            UsuarioId = 3,
            Marca = "Honda",
            Modelo = "CRV",
            TipoVehiculo = "SUV",
            Motor = "2.4L",
            Traccion = "En las 4 ruedas",
            ColorExterior = "Blanco",
            ColorInterior = "Negro",
            Anio = 2024,
            Precio = 1100000,
            Kilometraje = 25000,
            Transmision = "Automática",
            Combustible = "Gasolina",
            Accesorios = new List<string>(),
            Ubicacion = "Santiago",
            Descripcion = "Dealer cancelado"
        };

        var usuarioDealer = CrearUsuarioDealerConSuscripcion(
            id: 3,
            nivel: PlanNivel.Pro,
            ciclo: CicloFacturacion.Mensual,
            estado: EstadoSuscripcion.Cancelada);

        _mockUsuarioRepo.Setup(repo => repo.ObtenerDealerConPerfilPorIdAsync(3))
            .ReturnsAsync(usuarioDealer);

        _mockRepo.Setup(repo => repo.ContarAnunciosPorUsuarioAsync(3))
            .ReturnsAsync(0);

        // Act
        await _servicio.CrearAnuncioAsync(dto);

        // Assert: la cancelación no retira los días ya pagados
        _mockRepo.Verify(repo => repo.AgregarAsync(It.IsAny<Anuncio>()), Times.Once);
    }

    // =========================================================================
    // PRUEBA 25: Crear Anuncio - Fallo para Dealer con límite alcanzado
    // =========================================================================
    [Fact]
    public async Task CrearAnuncioAsync_DealerConLimiteAlcanzado_DebeLanzarBusinessRuleException()
    {
        var dto = new AnuncioCreateDto
        {
            UsuarioId = 4,
            Marca = "BMW",
            Modelo = "X5",
            TipoVehiculo = "SUV",
            ColorExterior = "Azul",
            ColorInterior = "Beige",
            Anio = 2022,
            Precio = 1800000,
            Kilometraje = 30000,
            Transmision = "Automática",
            Combustible = "Gasolina",
            Accesorios = new List<string>(),
            Ubicacion = "Santo Domingo",
            Descripcion = "Límite alcanzado"
        };

        var usuarioDealer = CrearUsuarioDealerConSuscripcion(
            id: 4,
            nivel: PlanNivel.Basico,
            ciclo: CicloFacturacion.Mensual,
            estado: EstadoSuscripcion.Activa);

        _mockUsuarioRepo.Setup(repo => repo.ObtenerDealerConPerfilPorIdAsync(4))
            .ReturnsAsync(usuarioDealer);

        _mockRepo.Setup(repo => repo.ContarAnunciosPorUsuarioAsync(4))
            .ReturnsAsync((int)PlanNivel.Basico);

        var excepcion = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _servicio.CrearAnuncioAsync(dto));

        Assert.Contains("límite de anuncios permitidos", excepcion.Message);
        _mockRepo.Verify(repo => repo.AgregarAsync(It.IsAny<Anuncio>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 26: Crear Anuncio - Éxito para Dealer con suscripción activa y cupo
    // =========================================================================
    [Fact]
    public async Task CrearAnuncioAsync_DealerConSuscripcionActivaYCupo_DebeCrearAnuncio()
    {
        var dto = new AnuncioCreateDto
        {
            UsuarioId = 5,
            Marca = "Kia",
            Modelo = "Sorento",
            TipoVehiculo = "SUV",
            Motor = "1.8L",
            Traccion = "Delantera",
            ColorExterior = "Gris",
            ColorInterior = "Negro",
            Anio = 2023,
            Precio = 1250000,
            Kilometraje = 15000,
            Transmision = "Automática",
            Combustible = "Gasolina",
            Accesorios = new List<string>(),
            Ubicacion = "La Vega",
            Descripcion = "Dealer con cupo"
        };

        var usuarioDealer = CrearUsuarioDealerConSuscripcion(
            id: 5,
            nivel: PlanNivel.Pro,
            ciclo: CicloFacturacion.Mensual,
            estado: EstadoSuscripcion.Activa);

        _mockUsuarioRepo.Setup(repo => repo.ObtenerDealerConPerfilPorIdAsync(5))
            .ReturnsAsync(usuarioDealer);

        _mockRepo.Setup(repo => repo.ContarAnunciosPorUsuarioAsync(5))
            .ReturnsAsync(1);

        await _servicio.CrearAnuncioAsync(dto);

        _mockRepo.Verify(repo => repo.AgregarAsync(It.IsAny<Anuncio>()), Times.Once);
        _mockRepo.Verify(repo => repo.GuardarCambiosAsync(), Times.Once);
    }

    // =========================================================================
    // HELPER: Crear Entidades Encapsuladas para Tests
    // =========================================================================
    private Usuario CrearUsuarioSimulado(int id, bool esDealer)
    {
        if (esDealer)
            return CrearUsuarioDealerSinSuscripcion(id);

        return new Usuario(
            nombre: "Erick",
            apellido: "Lopez",
            email: $"user{id}@test.com",
            passwordHash: "hash",
            telefonoPersonal: "8090000000",
            rol: "Vendedor",
            emailConfirmado: true
        );
    }

    // =========================================================================
    // BuscarAnunciosAsync: el filtro VendedorId se pasa al repositorio tal cual
    // (filtro público, no se nulifica).
    // =========================================================================
    [Fact]
    public async Task BuscarAnunciosAsync_VendedorId_DebePasarseAlRepositorio()
    {
        // ARRANGE
        var anuncios = new List<Anuncio>
        {
            new Anuncio(1, "Toyota", "Corolla", "", "Sedan", "1.8L", "Delantera", "Blanco", "Negro", 2015, 600000, "DOP", 80000, "Automática", "Gasolina", new List<string> { "Ninguno" }, "Santo Domingo", "Excelente estado")
        };

        var dto = new AnuncioSearchDto
        {
            VendedorId = 42,
            PaginaActual = 1,
            CantidadAnuncios = 12
        };

        _mockRepo
            .Setup(r => r.BuscarPaginadoAsync(It.IsAny<AnuncioQueryFilter>()))
            .ReturnsAsync((anuncios, 1));

        // ACT
        await _servicio.BuscarAnunciosAsync(dto);

        // ASSERT
        _mockRepo.Verify(
            r => r.BuscarPaginadoAsync(It.Is<AnuncioQueryFilter>(f =>
                f.VendedorId == 42)),
            Times.Once);
    }

    private Usuario CrearUsuarioDealerSinSuscripcion(int id)
    {
        var usuario = new Usuario(
            nombre: "Dealer",
            apellido: "SinPlan",
            email: $"dealer{id}@test.com",
            passwordHash: "hash",
            telefonoPersonal: "8091111111",
            rol: "Dealer",
            emailConfirmado: true
        );

        var perfil = new PerfilDealer(
            usuario: usuario,
            nombreAgencia: "AutoMarket Dealer",
            agenciaRNC: "123456789",
            ubicacion: "Santo Domingo",
            telefonoAgencia: "8092222222",
            descripcion: "Dealer de prueba"
        );

        usuario.AsignarPerfilDealer(perfil);

        SetPrivateProperty(usuario, "UsuarioId", id);
        SetPrivateProperty(perfil, "UsuarioId", id);

        return usuario;
    }

    private Usuario CrearUsuarioDealerConSuscripcion(
        int id,
        PlanNivel nivel,
        CicloFacturacion ciclo,
        EstadoSuscripcion estado)
    {
        var usuario = CrearUsuarioDealerSinSuscripcion(id);
        var perfil = usuario.PerfilDealer!;

        var suscripcion = new SuscripcionDealer(
            perfilDealerId: id,
            nivel: nivel,
            ciclo: ciclo
        );

        if (estado != EstadoSuscripcion.Activa)
        {
            SetPrivateProperty(suscripcion, "Estado", estado);
        }

        SetPrivateProperty(perfil, "Suscripcion", suscripcion);
        SetPrivateProperty(suscripcion, "PerfilDealer", perfil);

        return usuario;
    }

    private static void SetPrivateProperty(object obj, string propertyName, object? value)
    {
        var prop = obj.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (prop == null)
            throw new InvalidOperationException(
                $"No se encontró la propiedad '{propertyName}' en {obj.GetType().Name}.");

        prop.SetValue(obj, value);
    }

}