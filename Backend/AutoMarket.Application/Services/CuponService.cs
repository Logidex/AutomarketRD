using AutoMarket.Application.DTOs.Cupones;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

public class CuponService : ICuponService
{
    private readonly ICuponRepository _cuponRepository;
    private readonly ISuscripcionRepository _suscripcionRepository;
    private readonly IPlanCatalogoRepository _planCatalogoRepository;

    public CuponService(
        ICuponRepository cuponRepository,
        ISuscripcionRepository suscripcionRepository,
        IPlanCatalogoRepository planCatalogoRepository)
    {
        _cuponRepository = cuponRepository;
        _suscripcionRepository = suscripcionRepository;
        _planCatalogoRepository = planCatalogoRepository;
    }

    public async Task<CuponAplicadoDto> AplicarCuponAsync(int perfilDealerId, string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new BusinessRuleException("Escribe el código del cupón.");

        // Normalizar antes de consultar: el código se guarda en mayúsculas.
        codigo = codigo.Trim().ToUpperInvariant();

        var cupon = await _cuponRepository.ObtenerPorCodigoAsync(codigo);

        if (cupon is null || !cupon.Activo)
            throw new BusinessRuleException("El cupón no es válido.");

        if (await _cuponRepository.ExisteRedencionAsync(cupon.Id, perfilDealerId))
            throw new BusinessRuleException("Ya canjeaste este cupón con tu cuenta.");

        if (!cupon.TieneCupoDisponible)
            throw new BusinessRuleException(
                $"El cupón alcanzó su límite de {cupon.MaximoUsos} usos.");

        var suscripcion = await _suscripcionRepository.ObtenerPorDealerIdAsync(perfilDealerId);

        if (suscripcion is null)
            throw new BusinessRuleException(
                "Tu cuenta no tiene una suscripción inicial. Contacta a soporte.");

        if (suscripcion.Nivel == cupon.Nivel
            && suscripcion.FechaVencimientoUtc > DateTime.UtcNow)
        {
            throw new BusinessRuleException(
                "Ya tienes una suscripción Pro activa. El cupón no aplica.");
        }

        var plan = await _planCatalogoRepository.ObtenerPorNivelAsync(cupon.Nivel);

        // ActivarConPlan reactiva (incluso canceladas) y RenovarManualmente fija
        // la vigencia exacta del cupón: ahora + Dias.
        suscripcion.ActivarConPlan(cupon.Nivel, CicloFacturacion.Mensual);
        suscripcion.RenovarManualmente(DateTime.UtcNow.AddDays(cupon.Dias));
        suscripcion.VincularPlanCatalogo(plan);

        cupon.RegistrarUso();

        var redencion = new CuponRedencion(cupon.Id, perfilDealerId);

        // Un solo SaveChanges: incremento de usos + redención + suscripción.
        // El índice único (CuponId, PerfilDealerId) y el token de concurrencia
        // de UsosActuales cierran las carreras de canjes simultáneos; el
        // repositorio traduce esas violaciones a BusinessRuleException.
        await _cuponRepository.GuardarCanjeAsync(cupon, redencion);

        return new CuponAplicadoDto
        {
            Nivel = cupon.Nivel,
            Dias = cupon.Dias,
            FechaVencimientoUtc = suscripcion.FechaVencimientoUtc,
            Mensaje = $"¡Cupón aplicado! Tienes {cupon.Dias} días del plan {cupon.Nivel}."
        };
    }
}
