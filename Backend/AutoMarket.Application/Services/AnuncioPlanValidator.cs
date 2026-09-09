using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

/// <summary>
/// Valida límites de planes de suscripción para anuncios.
/// Elimina la lógica duplicada entre CrearAnuncio, Publicar y CambiarEstado.
/// </summary>
public class AnuncioPlanValidator
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPlanCatalogoRepository _planCatalogoRepository;

    public AnuncioPlanValidator(
        IUsuarioRepository usuarioRepository,
        IPlanCatalogoRepository planCatalogoRepository)
    {
        _usuarioRepository = usuarioRepository;
        _planCatalogoRepository = planCatalogoRepository;
    }

    /// <summary>
    /// Resultado de la validación de cupo.
    /// </summary>
    public record PlanValidationResult(int DiasVigencia, bool EsGratis);

    /// <summary>
    /// Valida si el usuario puede crear/publicar un anuncio según su plan.
    /// Lanza BusinessRuleException si no cumple.
    /// </summary>
    public async Task<PlanValidationResult> ValidarCupoAsync(
        int usuarioId,
        int cantidadAnunciosActuales,
        string estadoActual)
    {
        var usuario = await _usuarioRepository
            .ObtenerDealerConPerfilPorIdAsync(usuarioId);

        if (usuario == null)
            throw new KeyNotFoundException("El usuario especificado no existe.");

        bool esVendedorParticular = string.Equals(
            usuario.Rol, "Vendedor", StringComparison.OrdinalIgnoreCase);

        bool esPlanGratis = esVendedorParticular ||
            (usuario.PerfilDealer?.Suscripcion?.EsGratis == true);

        // Vendedores particulares: máximo 1 anuncio gratuito
        if (esVendedorParticular || esPlanGratis)
        {
            if (cantidadAnunciosActuales >= 1)
            {
                throw new BusinessRuleException(
                    "Has alcanzado el límite de 1 anuncio gratuito. " +
                    "Mejora tu cuenta a Dealer para publicar más inventario.");
            }

            return new PlanValidationResult(
                PlanConfig.DiasVigencia(PlanNivel.Gratis),
                EsGratis: true);
        }

        // Dealers: validar suscripción activa y cupo del plan
        var suscripcion = usuario.PerfilDealer?.Suscripcion;

        if (suscripcion == null)
            throw new BusinessRuleException(
                "Tu cuenta Dealer no tiene una suscripción activa configurada.");

        if (suscripcion.FechaVencimientoUtc <= DateTime.UtcNow)
            throw new BusinessRuleException(
                "Tu suscripción Dealer está vencida. Renuevala para seguir publicando.");

        var plan = await _planCatalogoRepository
            .ObtenerPorNivelAsync(suscripcion.Nivel);

        if (!suscripcion.PermiteNuevosAnuncios(cantidadAnunciosActuales, plan))
            throw new BusinessRuleException(
                "Has alcanzado el límite de anuncios permitidos por tu plan.");

        return new PlanValidationResult(
            plan?.DiasVigenciaEfectivo ?? PlanConfig.DiasVigencia(suscripcion.Nivel),
            EsGratis: false);
    }

    /// <summary>
    /// Obtiene el máximo de fotos permitidas según el plan del usuario.
    /// </summary>
    public async Task<int> ObtenerMaxFotosAsync(int usuarioId)
    {
        var usuario = await _usuarioRepository
            .ObtenerDealerConPerfilPorIdAsync(usuarioId);

        bool esVendedorParticular = usuario != null &&
            string.Equals(usuario.Rol, "Vendedor", StringComparison.OrdinalIgnoreCase);

        if (esVendedorParticular || usuario?.PerfilDealer?.Suscripcion == null)
            return PlanConfig.MaxFotos(PlanNivel.Gratis);

        var plan = await _planCatalogoRepository
            .ObtenerPorNivelAsync(usuario.PerfilDealer.Suscripcion.Nivel);

        return plan?.MaxFotosEfectivo ??
            PlanConfig.MaxFotos(usuario.PerfilDealer.Suscripcion.Nivel);
    }

    /// <summary>
    /// Valida si el usuario puede destacar anuncios según su plan.
    /// </summary>
    public async Task ValidarDestacadosAsync(
        int usuarioId,
        int destacadosActuales,
        bool anuncioYaDestacado)
    {
        var usuario = await _usuarioRepository
            .ObtenerDealerConPerfilPorIdAsync(usuarioId);

        var suscripcion = usuario?.PerfilDealer?.Suscripcion;

        if (suscripcion == null)
            throw new BusinessRuleException(
                "Tu cuenta no tiene un plan de suscripción activo.");

        if (suscripcion.Estado != EstadoSuscripcion.Activa)
            throw new BusinessRuleException(
                "Tu suscripción no está activa.");

        if (suscripcion.FechaVencimientoUtc <= DateTime.UtcNow)
            throw new BusinessRuleException(
                "Tu suscripción ha vencido.");

        var plan = suscripcion.Plan
            ?? await _planCatalogoRepository.ObtenerPorNivelAsync(suscripcion.Nivel);

        if (plan == null)
            throw new BusinessRuleException(
                "Tu cuenta no tiene un plan de suscripción activo.");

        // Si el anuncio ya está destacado, no consume cupo adicional
        if (anuncioYaDestacado)
            destacadosActuales--;

        if (!suscripcion.PermiteDestacarMas(destacadosActuales, plan))
            throw new BusinessRuleException(
                $"Has alcanzado el límite de anuncios destacados de tu plan ({plan.CuotaDestacados}).");
    }

    /// <summary>
    /// Determina si el dealer está verificado (suscripción pagada + email confirmado).
    /// </summary>
    public static bool EsDealerVerificado(Usuario vendedor)
    {
        if (!vendedor.EmailConfirmado)
            return false;

        var nivel = vendedor.PerfilDealer?.Suscripcion?.Nivel;
        return nivel.HasValue && nivel.Value != PlanNivel.Gratis;
    }
}
