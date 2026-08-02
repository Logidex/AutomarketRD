using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AutoMarket.Application.DTOs;
using AutoMarket.Core.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.API.Controllers;

namespace AutoMarket.Tests.Controllers;

public class LeadsControllerTests
{
    private readonly Mock<ILeadService> _mockLeadService;
    private readonly LeadsController _controller;

    public LeadsControllerTests()
    {
        _mockLeadService = new Mock<ILeadService>();
        _controller = new LeadsController(_mockLeadService.Object);
    }

    // =========================================================================
    // HELPER: Configurar Usuario Autenticado (Simular el Token JWT)
    // =========================================================================
    private void SimularUsuarioAutenticado(string usuarioId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    // =========================================================================
    // PRUEBA 01: POST CrearLead - Éxito
    // =========================================================================
    [Fact]
    public async Task CrearLead_DatosValidos_DebeRetornarOkConMensaje()
    {
        // Arrange
        var dto = new LeadCreateDto 
        { 
            AnuncioId = 1, 
            NombreContacto = "Carlos", 
            Mensaje = "Me interesa", 
            Canal = CanalContacto.Formulario 
        };

        // Act
        var resultado = await _controller.CrearLead(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        
        // Verificamos que el servicio se haya llamado una vez
        _mockLeadService.Verify(s => s.CrearLeadAsync(dto), Times.Once);
        
        // C# extrae las propiedades anónimas retornadas en el Ok(new { mensaje = "..." })
        var value = okResult.Value;
        var propMensaje = value?.GetType().GetProperty("mensaje")?.GetValue(value, null);
        Assert.Equal("Tu mensaje ha sido enviado exitosamente al vendedor.", propMensaje);
    }

    // =========================================================================
    // PRUEBA 02: POST CrearLead - Falla por validaciones (ModelState)
    // =========================================================================
    [Fact]
    public async Task CrearLead_DatosInvalidos_DebeRetornarBadRequest()
    {
        // Arrange
        var dto = new LeadCreateDto { AnuncioId = 1 }; // Falta Nombre y Mensaje
        
        // Simulamos que el framework ASP.NET detectó errores en los DataAnnotations del DTO
        _controller.ModelState.AddModelError("NombreContacto", "El nombre es obligatorio.");

        // Act
        var resultado = await _controller.CrearLead(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
        _mockLeadService.Verify(s => s.CrearLeadAsync(It.IsAny<LeadCreateDto>()), Times.Never);
    }

    // =========================================================================
    // PRUEBA 03: GET ObtenerPorAnuncio - Éxito
    // =========================================================================
    [Fact]
    public async Task ObtenerPorAnuncio_DebeRetornarOkConLista()
    {
        // Arrange
        int anuncioId = 10;
        var listaSimulada = new List<Lead>(); // Lista vacía para fines de estructura
        
        _mockLeadService.Setup(s => s.ObtenerLeadsPorAnuncioAsync(anuncioId))
            .ReturnsAsync(listaSimulada);

        // Act
        var resultado = await _controller.ObtenerPorAnuncio(anuncioId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(listaSimulada, okResult.Value);
    }

    // =========================================================================
    // PRUEBA 04: GET ObtenerMisLeads - Falla (Sin Token / Token Inválido)
    // =========================================================================
    [Fact]
    public async Task ObtenerMisLeads_SinIdentidadEnToken_DebeRetornarUnauthorized()
    {
        // Arrange
        // NO llamamos al helper SimularUsuarioAutenticado, por lo que User será nulo/vacío
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext() 
        };

        // Act
        var resultado = await _controller.ObtenerMisLeads();

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(resultado);
        
        var value = unauthorizedResult.Value;
        var propMensaje = value?.GetType().GetProperty("mensaje")?.GetValue(value, null);
        Assert.Equal("Usuario no válido o sesión expirada.", propMensaje);
    }

    // =========================================================================
    // PRUEBA 05: GET ObtenerMisLeads - Éxito (Extrae ID del JWT)
    // =========================================================================
    [Fact]
    public async Task ObtenerMisLeads_ConTokenValido_DebeExtraerIdYRetornarOk()
    {
        // Arrange
        int dealerIdLogueado = 5;
        SimularUsuarioAutenticado(dealerIdLogueado.ToString());

        var listaSimulada = new List<Lead>();
        _mockLeadService.Setup(s => s.ObtenerLeadsPorDealerAsync(dealerIdLogueado))
            .ReturnsAsync(listaSimulada);

        // Act
        var resultado = await _controller.ObtenerMisLeads();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(listaSimulada, okResult.Value);
        
        // Confirmamos que le pasó el ID extraído del token (5) al servicio, y no cualquier otro número
        _mockLeadService.Verify(s => s.ObtenerLeadsPorDealerAsync(dealerIdLogueado), Times.Once);
    }
}