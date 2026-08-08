using Moq;
using Xunit;
using AutoMarket.Application.Services;
using AutoMarket.Application.DTOs.Planes;
using AutoMarket.Core.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;

namespace AutoMarket.Tests.Services;

public class PlanCatalogoServiceTests
{
    private readonly Mock<IPlanCatalogoRepository> _mockRepo;
    private readonly PlanCatalogoService _servicio;

    public PlanCatalogoServiceTests()
    {
        _mockRepo = new Mock<IPlanCatalogoRepository>();
        _servicio = new PlanCatalogoService(_mockRepo.Object);
    }

    private PlanCatalogo CrearPlan(
        int id,
        PlanNivel nivel,
        string nombre,
        decimal precioMensual,
        bool activo = true,
        decimal descTrim = 7m,
        decimal descAnual = 15m)
    {
        return new PlanCatalogo
        {
            Id = id,
            Nivel = nivel,
            Nombre = nombre,
            Descripcion = null,
            PrecioMensual = precioMensual,
            DescuentoTrimestralPorcentaje = descTrim,
            DescuentoAnualPorcentaje = descAnual,
            Activo = activo
        };
    }

    // =========================================================================
    // CATÁLOGO PÚBLICO
    // =========================================================================

    [Fact]
    public async Task ObtenerCatalogoPublicoAsync_SoloDevuelvePlanesActivos()
    {
        // Arrange
        _mockRepo.Setup(r => r.ObtenerTodosAsync(true))
            .ReturnsAsync(new List<PlanCatalogo>
            {
                CrearPlan(1, PlanNivel.Gratis, "Plan Gratis", 0m, activo: true),
                CrearPlan(2, PlanNivel.Pro, "Plan Pro", 3000m, activo: true)
                // Elite está inactivo y no debe aparecer
            });

        // Act
        var resultado = await _servicio.ObtenerCatalogoPublicoAsync();

        // Assert
        Assert.Equal(2, resultado.Count);
        Assert.All(resultado, p => Assert.True(p.Nivel != PlanNivel.Elite));
    }

    [Fact]
    public async Task ObtenerCatalogoPublicoAsync_CalculaPreciosPorCiclo()
    {
        // Arrange
        _mockRepo.Setup(r => r.ObtenerTodosAsync(true))
            .ReturnsAsync(new List<PlanCatalogo>
            {
                CrearPlan(1, PlanNivel.Pro, "Plan Pro", 3000m)
            });

        // Act
        var resultado = await _servicio.ObtenerCatalogoPublicoAsync();

        // Assert
        var plan = Assert.Single(resultado);
        Assert.Equal(3000m, plan.PrecioMensual);
        // Trimestral: 3000 * 3 * (1 - 0.07) = 8,370.00
        Assert.Equal(8370m, plan.PrecioTrimestral);
        // Anual: 3000 * 12 * (1 - 0.15) = 30,600.00
        Assert.Equal(30600m, plan.PrecioAnual);
    }

    [Fact]
    public async Task ObtenerCatalogoPublicoAsync_PlanGratisSiempreSaleEnCero()
    {
        // Arrange
        _mockRepo.Setup(r => r.ObtenerTodosAsync(true))
            .ReturnsAsync(new List<PlanCatalogo>
            {
                CrearPlan(1, PlanNivel.Gratis, "Plan Gratis", 0m)
            });

        // Act
        var resultado = await _servicio.ObtenerCatalogoPublicoAsync();

        // Assert
        var plan = Assert.Single(resultado);
        Assert.Equal(0m, plan.PrecioMensual);
        Assert.Equal(0m, plan.PrecioTrimestral);
        Assert.Equal(0m, plan.PrecioAnual);
    }

    // =========================================================================
    // CRUD ADMIN
    // =========================================================================

    [Fact]
    public async Task CrearPlanAsync_NivelDuplicado_DebeLanzarBusinessRuleException()
    {
        // Arrange
        _mockRepo.Setup(r => r.ObtenerPorNivelAsync(PlanNivel.Pro))
            .ReturnsAsync(CrearPlan(1, PlanNivel.Pro, "Plan Pro", 3000m));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _servicio.CrearPlanAsync(new PlanCatalogoCreateDto { Nivel = PlanNivel.Pro }));
    }

    [Fact]
    public async Task CrearPlanAsync_PrecioNegativo_DebeLanzarBusinessRuleException()
    {
        // Arrange
        _mockRepo.Setup(r => r.ObtenerPorNivelAsync(PlanNivel.Basico))
            .ReturnsAsync((PlanCatalogo?)null);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _servicio.CrearPlanAsync(new PlanCatalogoCreateDto
            {
                Nivel = PlanNivel.Basico,
                PrecioMensual = -100m
            }));
    }

    [Fact]
    public async Task CrearPlanAsync_DatosValidos_DebeAgregarYRetornar()
    {
        // Arrange
        _mockRepo.Setup(r => r.ObtenerPorNivelAsync(PlanNivel.Basico))
            .ReturnsAsync((PlanCatalogo?)null);

        // Act
        var resultado = await _servicio.CrearPlanAsync(new PlanCatalogoCreateDto
        {
            Nivel = PlanNivel.Basico,
            Nombre = "Plan Básico",
            PrecioMensual = 1500m,
            DescuentoTrimestralPorcentaje = 7m,
            DescuentoAnualPorcentaje = 15m
        });

        // Assert
        Assert.Equal(PlanNivel.Basico, resultado.Nivel);
        Assert.Equal(1500m, resultado.PrecioMensual);
        _mockRepo.Verify(r => r.AgregarAsync(It.IsAny<PlanCatalogo>()), Times.Once);
    }

    [Fact]
    public async Task ActualizarPlanAsync_PlanNoExiste_DebeLanzarKeyNotFoundException()
    {
        // Arrange
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(999))
            .ReturnsAsync((PlanCatalogo?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _servicio.ActualizarPlanAsync(999, new PlanCatalogoUpdateDto()));
    }

    [Fact]
    public async Task ActualizarPlanAsync_DatosValidos_DebeActualizarYGuardar()
    {
        // Arrange
        var plan = CrearPlan(1, PlanNivel.Elite, "Plan Elite", 5500m);

        _mockRepo.Setup(r => r.ObtenerPorIdAsync(1))
            .ReturnsAsync(plan);

        // Act
        var resultado = await _servicio.ActualizarPlanAsync(1, new PlanCatalogoUpdateDto
        {
            Nombre = "Plan Elite Plus",
            PrecioMensual = 6000m,
            Activo = false
        });

        // Assert
        Assert.Equal("Plan Elite Plus", resultado.Nombre);
        Assert.Equal(6000m, resultado.PrecioMensual);
        Assert.False(resultado.Activo);
        _mockRepo.Verify(r => r.ActualizarAsync(plan), Times.Once);
    }

    [Fact]
    public async Task EliminarPlanAsync_PlanGratis_DebeLanzarBusinessRuleException()
    {
        // Arrange
        _mockRepo.Setup(r => r.ObtenerPorIdAsync(1))
            .ReturnsAsync(CrearPlan(1, PlanNivel.Gratis, "Plan Gratis", 0m));

        // Act & Assert
        var excepcion = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            _servicio.EliminarPlanAsync(1));

        Assert.Contains("Gratis", excepcion.Message);
        _mockRepo.Verify(r => r.ActualizarAsync(It.IsAny<PlanCatalogo>()), Times.Never);
    }

    [Fact]
    public async Task EliminarPlanAsync_PlanPagado_DesactivaLogicoNoBorra()
    {
        // Arrange
        var plan = CrearPlan(2, PlanNivel.Basico, "Plan Básico", 1500m);

        _mockRepo.Setup(r => r.ObtenerPorIdAsync(2))
            .ReturnsAsync(plan);

        // Act
        await _servicio.EliminarPlanAsync(2);

        // Assert
        Assert.False(plan.Activo);
        _mockRepo.Verify(r => r.ActualizarAsync(plan), Times.Once);
    }
}