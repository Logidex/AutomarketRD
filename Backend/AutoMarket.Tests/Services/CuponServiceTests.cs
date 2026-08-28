using Moq;
using Xunit;
using AutoMarket.Application.Services;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Tests.Services;

public class CuponServiceTests
{
    private const int DealerId = 15;

    private readonly Mock<ICuponRepository> _mockCuponRepo;
    private readonly Mock<ISuscripcionRepository> _mockSuscripcionRepo;
    private readonly Mock<IPlanCatalogoRepository> _mockPlanCatalogoRepo;
    private readonly CuponService _servicio;

    public CuponServiceTests()
    {
        _mockCuponRepo = new Mock<ICuponRepository>();
        _mockSuscripcionRepo = new Mock<ISuscripcionRepository>();
        _mockPlanCatalogoRepo = new Mock<IPlanCatalogoRepository>();

        _mockPlanCatalogoRepo
            .Setup(r => r.ObtenerPorNivelAsync(PlanNivel.Pro))
            .ReturnsAsync(new PlanCatalogo { Nivel = PlanNivel.Pro, Nombre = "Plan Pro" });

        _servicio = new CuponService(
            _mockCuponRepo.Object,
            _mockSuscripcionRepo.Object,
            _mockPlanCatalogoRepo.Object);
    }

    private static Cupon CrearCupon(int usosActuales = 0, int maximoUsos = 15, bool activo = true)
    {
        var cupon = new Cupon("PRO15BIENVENIDA", PlanNivel.Pro, 15, maximoUsos);

        if (!activo)
        {
            cupon.Desactivar();
        }

        for (var i = 0; i < usosActuales; i++)
        {
            cupon.RegistrarUso();
        }

        return cupon;
    }

    private void ConfigurarEscenarioFeliz(Cupon cupon, SuscripcionDealer suscripcion)
    {
        _mockCuponRepo.Setup(r => r.ObtenerPorCodigoAsync("PRO15BIENVENIDA")).ReturnsAsync(cupon);
        _mockCuponRepo.Setup(r => r.ExisteRedencionAsync(cupon.Id, DealerId)).ReturnsAsync(false);
        _mockSuscripcionRepo.Setup(r => r.ObtenerPorDealerIdAsync(DealerId)).ReturnsAsync(suscripcion);
    }

    [Fact]
    public async Task AplicarCupon_DealerConPlanGratis_DebeActivarProPor15Dias()
    {
        // ARRANGE
        var cupon = CrearCupon();
        var suscripcion = new SuscripcionDealer(DealerId, PlanNivel.Gratis, CicloFacturacion.Mensual);
        ConfigurarEscenarioFeliz(cupon, suscripcion);

        // ACT
        var resultado = await _servicio.AplicarCuponAsync(DealerId, "  pro15bienvenida  ");

        // ASSERT: normaliza el código, activa Pro y fija la vigencia en ~15 días
        Assert.Equal(PlanNivel.Pro, resultado.Nivel);
        Assert.Equal(15, resultado.Dias);
        Assert.Equal(PlanNivel.Pro, suscripcion.Nivel);
        Assert.Equal(EstadoSuscripcion.Activa, suscripcion.Estado);

        var diasRestantes = (suscripcion.FechaVencimientoUtc - DateTime.UtcNow).TotalDays;
        Assert.InRange(diasRestantes, 14.9, 15.1);

        Assert.Equal(1, cupon.UsosActuales);
        _mockCuponRepo.Verify(
            r => r.GuardarCanjeAsync(cupon, It.Is<CuponRedencion>(r => r.PerfilDealerId == DealerId)),
            Times.Once);
    }

    [Fact]
    public async Task AplicarCupon_SuscripcionCancelada_DebeReactivarConPro()
    {
        // ARRANGE
        var cupon = CrearCupon();
        var suscripcion = new SuscripcionDealer(DealerId, PlanNivel.Gratis, CicloFacturacion.Mensual);
        suscripcion.Cancelar();
        ConfigurarEscenarioFeliz(cupon, suscripcion);

        // ACT
        var resultado = await _servicio.AplicarCuponAsync(DealerId, "PRO15BIENVENIDA");

        // ASSERT
        Assert.Equal(PlanNivel.Pro, resultado.Nivel);
        Assert.Equal(EstadoSuscripcion.Activa, suscripcion.Estado);
    }

    [Fact]
    public async Task AplicarCupon_CodigoInexistente_DebeLanzarBusinessRule()
    {
        // ARRANGE
        _mockCuponRepo
            .Setup(r => r.ObtenerPorCodigoAsync("FANTASMA"))
            .ReturnsAsync((Cupon?)null);

        // ACT + ASSERT
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _servicio.AplicarCuponAsync(DealerId, "FANTASMA"));
        Assert.Contains("no es válido", ex.Message);
    }

    [Fact]
    public async Task AplicarCupon_CodigoVacio_DebeLanzarBusinessRule()
    {
        // ACT + ASSERT
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _servicio.AplicarCuponAsync(DealerId, "   "));
        Assert.Contains("código", ex.Message);
    }

    [Fact]
    public async Task AplicarCupon_YaCanjeadoPorElDealer_DebeLanzarBusinessRule()
    {
        // ARRANGE
        var cupon = CrearCupon();
        var suscripcion = new SuscripcionDealer(DealerId, PlanNivel.Gratis, CicloFacturacion.Mensual);
        _mockCuponRepo.Setup(r => r.ObtenerPorCodigoAsync("PRO15BIENVENIDA")).ReturnsAsync(cupon);
        _mockCuponRepo.Setup(r => r.ExisteRedencionAsync(cupon.Id, DealerId)).ReturnsAsync(true);
        _mockSuscripcionRepo.Setup(r => r.ObtenerPorDealerIdAsync(DealerId)).ReturnsAsync(suscripcion);

        // ACT + ASSERT
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _servicio.AplicarCuponAsync(DealerId, "PRO15BIENVENIDA"));
        Assert.Contains("Ya canjeaste", ex.Message);
    }

    [Fact]
    public async Task AplicarCupon_CupoAgotado_DebeLanzarBusinessRule()
    {
        // ARRANGE: 15 de 15 usos consumidos
        var cupon = CrearCupon(usosActuales: 15);
        var suscripcion = new SuscripcionDealer(DealerId, PlanNivel.Gratis, CicloFacturacion.Mensual);
        ConfigurarEscenarioFeliz(cupon, suscripcion);

        // ACT + ASSERT
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _servicio.AplicarCuponAsync(DealerId, "PRO15BIENVENIDA"));
        Assert.Contains("límite", ex.Message);
        Assert.Equal(15, cupon.UsosActuales);
    }

    [Fact]
    public async Task AplicarCupon_DealerConProVigente_DebeLanzarBusinessRule()
    {
        // ARRANGE: el dealer ya tiene Pro vigente (no debería ocurrir en
        // staging, pero protege el flujo de producción)
        var cupon = CrearCupon();
        var suscripcion = new SuscripcionDealer(DealerId, PlanNivel.Pro, CicloFacturacion.Mensual);
        ConfigurarEscenarioFeliz(cupon, suscripcion);

        // ACT + ASSERT
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _servicio.AplicarCuponAsync(DealerId, "PRO15BIENVENIDA"));
        Assert.Contains("Pro activa", ex.Message);
    }

    [Fact]
    public async Task AplicarCupon_SinSuscripcionInicial_DebeLanzarBusinessRule()
    {
        // ARRANGE: dealer sin suscripción (no debería ocurrir: el registro crea Gratis)
        var cupon = CrearCupon();
        _mockCuponRepo.Setup(r => r.ObtenerPorCodigoAsync("PRO15BIENVENIDA")).ReturnsAsync(cupon);
        _mockCuponRepo.Setup(r => r.ExisteRedencionAsync(cupon.Id, DealerId)).ReturnsAsync(false);
        _mockSuscripcionRepo.Setup(r => r.ObtenerPorDealerIdAsync(DealerId)).ReturnsAsync((SuscripcionDealer?)null);

        // ACT + ASSERT
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _servicio.AplicarCuponAsync(DealerId, "PRO15BIENVENIDA"));
        Assert.Contains("suscripción inicial", ex.Message);
    }
}
