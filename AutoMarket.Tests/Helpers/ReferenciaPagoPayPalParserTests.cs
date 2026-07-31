using AutoMarket.Application.DTOs.Paypal;
using AutoMarket.Application.Helpers;
using AutoMarket.Core.Entities.Enums;
using Xunit;

namespace AutoMarket.Tests.Helpers;

public class ReferenciaPagoPayPalParserTests
{
    // =========================================================================
    // PRUEBA 01: TryParse - Éxito con referencia válida
    // =========================================================================
    [Fact]
    public void TryParse_ReferenciaValida_DebeRetornarTrueYMapearCamposCorrectamente()
    {
        // Arrange
        var referenceId = "PERFIL_15_PLAN_Pro_CICLO_Mensual";

        // Act
        var resultado = ReferenciaPagoPayPalParser.TryParse(referenceId, out ReferenciaPagoPayPal? referencia);

        // Assert
        Assert.True(resultado);
        Assert.NotNull(referencia);
        Assert.Equal(15, referencia!.PerfilDealerId);
        Assert.Equal(PlanNivel.Pro, referencia.Plan);
        Assert.Equal(CicloFacturacion.Mensual, referencia.Ciclo);
    }

    // =========================================================================
    // PRUEBA 02: TryParse - Falla con referencia nula
    // =========================================================================
    [Fact]
    public void TryParse_ReferenciaNula_DebeRetornarFalseYReferenciaNula()
    {
        // Act
        var resultado = ReferenciaPagoPayPalParser.TryParse(null, out ReferenciaPagoPayPal? referencia);

        // Assert
        Assert.False(resultado);
        Assert.Null(referencia);
    }

    // =========================================================================
    // PRUEBA 03: TryParse - Falla con referencia vacía
    // =========================================================================
    [Fact]
    public void TryParse_ReferenciaVacia_DebeRetornarFalseYReferenciaNula()
    {
        // Arrange
        var referenceId = string.Empty;

        // Act
        var resultado = ReferenciaPagoPayPalParser.TryParse(referenceId, out ReferenciaPagoPayPal? referencia);

        // Assert
        Assert.False(resultado);
        Assert.Null(referencia);
    }

    // =========================================================================
    // PRUEBA 04: TryParse - Falla por cantidad incorrecta de partes
    // =========================================================================
    [Fact]
    public void TryParse_FormatoIncompleto_DebeRetornarFalse()
    {
        // Arrange
        var referenceId = "PERFIL_15_PLAN_Pro_CICLO";

        // Act
        var resultado = ReferenciaPagoPayPalParser.TryParse(referenceId, out ReferenciaPagoPayPal? referencia);

        // Assert
        Assert.False(resultado);
        Assert.Null(referencia);
    }

    // =========================================================================
    // PRUEBA 05: TryParse - Falla por prefijo inválido
    // =========================================================================
    [Fact]
    public void TryParse_PrefijoInvalido_DebeRetornarFalse()
    {
        // Arrange
        var referenceId = "DEALER_15_PLAN_Pro_CICLO_Mensual";

        // Act
        var resultado = ReferenciaPagoPayPalParser.TryParse(referenceId, out ReferenciaPagoPayPal? referencia);

        // Assert
        Assert.False(resultado);
        Assert.Null(referencia);
    }

    // =========================================================================
    // PRUEBA 06: TryParse - Falla por id inválido
    // =========================================================================
    [Fact]
    public void TryParse_PerfilDealerIdInvalido_DebeRetornarFalse()
    {
        // Arrange
        var referenceId = "PERFIL_ABC_PLAN_Pro_CICLO_Mensual";

        // Act
        var resultado = ReferenciaPagoPayPalParser.TryParse(referenceId, out ReferenciaPagoPayPal? referencia);

        // Assert
        Assert.False(resultado);
        Assert.Null(referencia);
    }

    // =========================================================================
    // PRUEBA 07: TryParse - Falla por plan inválido
    // =========================================================================
    [Fact]
    public void TryParse_PlanInvalido_DebeRetornarFalse()
    {
        // Arrange
        var referenceId = "PERFIL_15_PLAN_Premium_CICLO_Mensual";

        // Act
        var resultado = ReferenciaPagoPayPalParser.TryParse(referenceId, out ReferenciaPagoPayPal? referencia);

        // Assert
        Assert.False(resultado);
        Assert.Null(referencia);
    }

    // =========================================================================
    // PRUEBA 08: TryParse - Falla por ciclo inválido
    // =========================================================================
    [Fact]
    public void TryParse_CicloInvalido_DebeRetornarFalse()
    {
        // Arrange
        var referenceId = "PERFIL_15_PLAN_Pro_CICLO_Semanal";

        // Act
        var resultado = ReferenciaPagoPayPalParser.TryParse(referenceId, out ReferenciaPagoPayPal? referencia);

        // Assert
        Assert.False(resultado);
        Assert.Null(referencia);
    }

    // =========================================================================
    // PRUEBA 09: TryParse - Debe ignorar mayúsculas/minúsculas en tokens y enums
    // =========================================================================
    [Fact]
    public void TryParse_ValoresConDistintoCase_DebeRetornarTrue()
    {
        // Arrange
        var referenceId = "perfil_20_plan_basico_ciclo_trimestral";

        // Act
        var resultado = ReferenciaPagoPayPalParser.TryParse(referenceId, out ReferenciaPagoPayPal? referencia);

        // Assert
        Assert.True(resultado);
        Assert.NotNull(referencia);
        Assert.Equal(20, referencia!.PerfilDealerId);
        Assert.Equal(PlanNivel.Basico, referencia.Plan);
        Assert.Equal(CicloFacturacion.Trimestral, referencia.Ciclo);
    }
}