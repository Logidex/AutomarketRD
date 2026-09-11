using AutoMarket.Application.DTOs.AdSlots;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

public class AdSlotService : IAdSlotService
{
    private readonly IAdSlotRepository _repository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPlanCatalogoRepository _planRepository;
    private readonly ISuscripcionRepository _suscripcionRepository;

    public AdSlotService(
        IAdSlotRepository repository,
        IUsuarioRepository usuarioRepository,
        IPlanCatalogoRepository planRepository,
        ISuscripcionRepository suscripcionRepository)
    {
        _repository = repository;
        _usuarioRepository = usuarioRepository;
        _planRepository = planRepository;
        _suscripcionRepository = suscripcionRepository;
    }

    // ============================
    // PÚBLICOS
    // ============================

    public async Task<List<AdSlotPublicoDto>> ObtenerSlotsPublicosPorUbicacionAsync(UbicacionAdSlot ubicacion)
    {
        var slots = await _repository.ObtenerSlotsPorUbicacionAsync(ubicacion);
        var resultado = new List<AdSlotPublicoDto>();

        foreach (var slot in slots)
        {
            var anuncios = await _repository.ObtenerAnunciosActivosPorSlotAsync(slot.Id);
            var anunciosVigentes = anuncios
                .Where(a => a.EstaVigente)
                .OrderByDescending(a => a.Prioridad)
                .ToList();

            if (anunciosVigentes.Count == 0) continue;

            resultado.Add(new AdSlotPublicoDto
            {
                Id = slot.Id,
                Titulo = slot.Titulo,
                Ubicacion = slot.Ubicacion,
                AnchoPx = slot.AnchoPx,
                AltoPx = slot.AltoPx,
                IntervaloRotacionSeg = slot.IntervaloRotacionSeg,
                Anuncios = anunciosVigentes.Select(a => new AdSlotAnuncioPublicoDto
                {
                    Id = a.Id,
                    ImagenUrl = !string.IsNullOrEmpty(a.ImagenRedimensionadaUrl)
                        ? a.ImagenRedimensionadaUrl
                        : a.ImagenOriginalUrl,
                    Enlace = a.Enlace,
                    Titulo = a.Titulo,
                    NombreDealer = a.PerfilDealer?.NombreAgencia ?? "Dealer",
                    Prioridad = a.Prioridad
                }).ToList()
            });
        }

        return resultado;
    }

    public async Task RegistrarImpresionAsync(int anuncioId)
    {
        var anuncio = await _repository.ObtenerAnuncioPorIdAsync(anuncioId);
        if (anuncio == null || !anuncio.EstaVigente) return;

        anuncio.RegistrarImpresion();
        await _repository.ActualizarAnuncioAsync(anuncio);
    }

    public async Task RegistrarClickAsync(int anuncioId)
    {
        var anuncio = await _repository.ObtenerAnuncioPorIdAsync(anuncioId);
        if (anuncio == null || !anuncio.EstaVigente) return;

        anuncio.RegistrarClick();
        await _repository.ActualizarAnuncioAsync(anuncio);
    }

    // ============================
    // DEALER
    // ============================

    public async Task<List<AdSlotPublicoDto>> ObtenerSlotsDisponiblesAsync()
    {
        var slots = await _repository.ObtenerSlotsActivosAsync();
        return slots.Select(s => new AdSlotPublicoDto
        {
            Id = s.Id,
            Titulo = s.Titulo,
            Ubicacion = s.Ubicacion,
            AnchoPx = s.AnchoPx,
            AltoPx = s.AltoPx,
            IntervaloRotacionSeg = s.IntervaloRotacionSeg,
            Precios = s.Precios.Where(p => p.Activo).Select(p => new AdSlotPrecioDto
            {
                Id = p.Id,
                DuracionDias = p.DuracionDias,
                Precio = p.Precio,
                DescuentoProElitePorcentaje = p.DescuentoProElitePorcentaje,
                Activo = p.Activo
            }).ToList()
        }).ToList();
    }

    public async Task<List<AdSlotAnuncioPublicoDto>> ObtenerMisAnunciosAsync(int perfilDealerId)
    {
        var anuncios = await _repository.ObtenerAnunciosPorDealerAsync(perfilDealerId);
        return anuncios.Select(a => new AdSlotAnuncioPublicoDto
        {
            Id = a.Id,
            ImagenUrl = !string.IsNullOrEmpty(a.ImagenRedimensionadaUrl)
                ? a.ImagenRedimensionadaUrl
                : a.ImagenOriginalUrl,
            Enlace = a.Enlace,
            Titulo = a.Titulo,
            NombreDealer = a.PerfilDealer?.NombreAgencia ?? "Dealer",
            Prioridad = a.Prioridad,
            Ubicacion = a.AdSlot.Ubicacion,
            FechaInicioUtc = a.FechaInicioUtc,
            FechaFinUtc = a.FechaFinUtc,
            Estado = a.Estado,
            Impresiones = a.Impresiones,
            Clicks = a.Clicks
        }).ToList();
    }

    public async Task<AdSlotAnuncioPublicoDto> CrearAnuncioAsync(int perfilDealerId, CrearAdSlotAnuncioDto dto)
    {
        var slot = await _repository.ObtenerSlotPorIdAsync(dto.AdSlotId)
            ?? throw new KeyNotFoundException("El slot publicitario no existe.");

        if (!slot.Activo)
            throw new InvalidOperationException("El slot publicitario no está activo.");

        var precio = slot.Precios.FirstOrDefault(p => p.DuracionDias == dto.DuracionDias && p.Activo)
            ?? throw new KeyNotFoundException($"No existe precio para {dto.DuracionDias} días en este slot.");

        // Calcular monto con descuento
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(perfilDealerId)
            ?? throw new KeyNotFoundException("El usuario no existe.");

        var perfil = usuario.PerfilDealer
            ?? throw new KeyNotFoundException("El perfil de dealer no existe.");

        var suscripcion = await _suscripcionRepository.ObtenerPorDealerIdAsync(perfilDealerId);
        var nivelPlan = suscripcion?.Nivel ?? PlanNivel.Gratis;

        var monto = CalcularPrecioConDescuento(precio, nivelPlan);

        var ahora = DateTime.UtcNow;
        var fechaFin = ahora.AddDays(dto.DuracionDias);

        // Calcular prioridad basada en el precio base del slot
        var precioBaseSlot = slot.Precios
            .Where(p => p.Activo)
            .OrderBy(p => p.Precio)
            .FirstOrDefault()?.Precio ?? 1m;

        var anuncio = new AdSlotAnuncio
        {
            AdSlotId = dto.AdSlotId,
            PerfilDealerId = perfilDealerId,
            ImagenOriginalUrl = dto.ImagenUrl,
            ImagenRedimensionadaUrl = dto.ImagenUrl,
            Enlace = dto.Enlace,
            Titulo = dto.Titulo,
            FechaInicioUtc = ahora,
            FechaFinUtc = fechaFin,
            Estado = EstadoAdSlot.Activo,
            MetodoPago = MetodoPago.PayPal,
            MontoPagado = monto,
            Moneda = "RD$",
            EstadoTransferencia = null,
            Impresiones = 0,
            Clicks = 0,
            FechaCreacionUtc = ahora
        };

        anuncio.CalcularPrioridad(precioBaseSlot);

        await _repository.AgregarAnuncioAsync(anuncio);

        return new AdSlotAnuncioPublicoDto
        {
            Id = anuncio.Id,
            ImagenUrl = anuncio.ImagenRedimensionadaUrl,
            Enlace = anuncio.Enlace,
            Titulo = anuncio.Titulo,
            NombreDealer = perfil.NombreAgencia ?? "Dealer",
            Prioridad = anuncio.Prioridad
        };
    }

    public async Task CancelarAnuncioAsync(int perfilDealerId, int anuncioId)
    {
        var anuncio = await _repository.ObtenerAnuncioPorIdAsync(anuncioId)
            ?? throw new KeyNotFoundException("El anuncio no existe.");

        if (anuncio.PerfilDealerId != perfilDealerId)
            throw new UnauthorizedAccessException("No tienes permiso para cancelar este anuncio.");

        if (anuncio.Estado != EstadoAdSlot.Activo)
            throw new InvalidOperationException("Solo se pueden cancelar anuncios activos.");

        anuncio.MarcarVencido();
        await _repository.ActualizarAnuncioAsync(anuncio);
    }

    public async Task<AdSlotStatsDto> ObtenerEstadisticasAsync(int perfilDealerId)
    {
        var anuncios = await _repository.ObtenerAnunciosPorDealerAsync(perfilDealerId);

        var totalImpresiones = anuncios.Sum(a => a.Impresiones);
        var totalClicks = anuncios.Sum(a => a.Clicks);
        var ctrPromedio = totalImpresiones > 0
            ? Math.Round((double)totalClicks / totalImpresiones * 100, 1)
            : 0;

        var topAnuncios = anuncios
            .OrderByDescending(a => a.Impresiones)
            .Take(5)
            .Select(a => new AdSlotAnuncioStatsDto
            {
                Id = a.Id,
                Ubicacion = a.AdSlot?.Ubicacion.ToString() ?? "",
                TituloSlot = a.AdSlot?.Titulo ?? "",
                Impresiones = a.Impresiones,
                Clicks = a.Clicks,
                Ctr = a.Impresiones > 0
                    ? Math.Round((double)a.Clicks / a.Impresiones * 100, 1)
                    : 0,
                FechaFinUtc = a.FechaFinUtc,
                EstaVigente = a.EstaVigente
            })
            .ToList();

        return new AdSlotStatsDto
        {
            TotalImpresiones = totalImpresiones,
            TotalClicks = totalClicks,
            CtrPromedio = ctrPromedio,
            TopAnuncios = topAnuncios
        };
    }

    // ============================
    // ADMIN
    // ============================

    public async Task<List<AdSlotDto>> ObtenerTodosLosSlotsAsync()
    {
        var slots = await _repository.ObtenerTodosLosSlotsAsync();
        return slots.Select(MapearSlotADto).ToList();
    }

    public async Task<AdSlotDto> ObtenerSlotPorIdAsync(int id)
    {
        var slot = await _repository.ObtenerSlotPorIdAsync(id)
            ?? throw new KeyNotFoundException("El slot no existe.");
        return MapearSlotADto(slot);
    }

    public async Task<AdSlotDto> CrearSlotAsync(CrearAdSlotAdminDto dto)
    {
        var slot = new AdSlot
        {
            Titulo = dto.Titulo,
            Ubicacion = dto.Ubicacion,
            AnchoPx = dto.AnchoPx,
            AltoPx = dto.AltoPx,
            IntervaloRotacionSeg = dto.IntervaloRotacionSeg,
            MaxAnunciosSimultaneos = dto.MaxAnunciosSimultaneos,
            Activo = dto.Activo,
            Orden = dto.Orden,
            FechaCreacionUtc = DateTime.UtcNow
        };

        foreach (var precioDto in dto.Precios)
        {
            slot.Precios.Add(new AdSlotPrecio
            {
                DuracionDias = precioDto.DuracionDias,
                Precio = precioDto.Precio,
                DescuentoProElitePorcentaje = precioDto.DescuentoProElitePorcentaje,
                Activo = true
            });
        }

        await _repository.AgregarSlotAsync(slot);
        return MapearSlotADto(slot);
    }

    public async Task<AdSlotDto> ActualizarSlotAsync(int id, ActualizarAdSlotAdminDto dto)
    {
        var slot = await _repository.ObtenerSlotPorIdAsync(id)
            ?? throw new KeyNotFoundException("El slot no existe.");

        if (dto.Titulo != null) slot.Titulo = dto.Titulo;
        if (dto.Ubicacion.HasValue) slot.Ubicacion = dto.Ubicacion.Value;
        if (dto.AnchoPx.HasValue) slot.AnchoPx = dto.AnchoPx.Value;
        if (dto.AltoPx.HasValue) slot.AltoPx = dto.AltoPx.Value;
        if (dto.IntervaloRotacionSeg.HasValue) slot.IntervaloRotacionSeg = dto.IntervaloRotacionSeg.Value;
        if (dto.MaxAnunciosSimultaneos.HasValue) slot.MaxAnunciosSimultaneos = dto.MaxAnunciosSimultaneos.Value;
        if (dto.Activo.HasValue) slot.Activo = dto.Activo.Value;
        if (dto.Orden.HasValue) slot.Orden = dto.Orden.Value;

        await _repository.ActualizarSlotAsync(slot);
        return MapearSlotADto(slot);
    }

    public async Task EliminarSlotAsync(int id)
    {
        var slot = await _repository.ObtenerSlotPorIdAsync(id)
            ?? throw new KeyNotFoundException("El slot no existe.");

        slot.Activo = false;
        await _repository.ActualizarSlotAsync(slot);
    }

    public async Task<List<AdSlotAnuncioAdminDto>> ObtenerTodosLosAnunciosAsync()
    {
        var anuncios = await _repository.ObtenerAnunciosActivosPorUbicacionAsync(UbicacionAdSlot.HomepageLateral);
        var resultado = new List<AdSlotAnuncioAdminDto>();

        // Obtener todos los slots para mapear
        var slots = await _repository.ObtenerTodosLosSlotsAsync();
        var slotsDict = slots.ToDictionary(s => s.Id);

        foreach (var slot in slots)
        {
            var anunciosSlot = await _repository.ObtenerAnunciosActivosPorSlotAsync(slot.Id);
            foreach (var a in anunciosSlot)
            {
                resultado.Add(MapearAnuncioAAdminDto(a, slot));
            }
        }

        return resultado.OrderByDescending(a => a.FechaCreacionUtc).ToList();
    }

    public async Task<AdSlotAnuncioAdminDto> ObtenerAnuncioAdminPorIdAsync(int id)
    {
        var anuncio = await _repository.ObtenerAnuncioPorIdAsync(id)
            ?? throw new KeyNotFoundException("El anuncio no existe.");
        return MapearAnuncioAAdminDto(anuncio, anuncio.AdSlot);
    }

    public async Task RechazarAnuncioAsync(int anuncioId)
    {
        var anuncio = await _repository.ObtenerAnuncioPorIdAsync(anuncioId)
            ?? throw new KeyNotFoundException("El anuncio no existe.");

        anuncio.Rechazar();
        await _repository.ActualizarAnuncioAsync(anuncio);
    }

    public async Task EliminarAnuncioAsync(int anuncioId)
    {
        var anuncio = await _repository.ObtenerAnuncioPorIdAsync(anuncioId)
            ?? throw new KeyNotFoundException("El anuncio no existe.");

        await _repository.ActualizarAnuncioAsync(anuncio);
    }

    public async Task<List<AdSlotAnuncioAdminDto>> ObtenerAnunciosPorVencerAsync(int diasAntes)
    {
        var anuncios = await _repository.ObtenerAnunciosPorVencerAsync(diasAntes);
        return anuncios.Select(a => MapearAnuncioAAdminDto(a, a.AdSlot)).ToList();
    }

    // ============================
    // HELPERS
    // ============================

    private static decimal CalcularPrecioConDescuento(AdSlotPrecio precio, PlanNivel nivelPlan)
    {
        var base_ = precio.Precio;
        if (nivelPlan == PlanNivel.Pro || nivelPlan == PlanNivel.Elite)
        {
            var descuento = precio.DescuentoProElitePorcentaje / 100m;
            base_ = base_ * (1 - descuento);
        }
        return Math.Round(base_, 2);
    }

    private static AdSlotDto MapearSlotADto(AdSlot slot)
    {
        return new AdSlotDto
        {
            Id = slot.Id,
            Titulo = slot.Titulo,
            Ubicacion = slot.Ubicacion,
            AnchoPx = slot.AnchoPx,
            AltoPx = slot.AltoPx,
            IntervaloRotacionSeg = slot.IntervaloRotacionSeg,
            MaxAnunciosSimultaneos = slot.MaxAnunciosSimultaneos,
            Activo = slot.Activo,
            Orden = slot.Orden,
            Precios = slot.Precios.Select(p => new AdSlotPrecioDto
            {
                Id = p.Id,
                DuracionDias = p.DuracionDias,
                Precio = p.Precio,
                DescuentoProElitePorcentaje = p.DescuentoProElitePorcentaje,
                Activo = p.Activo
            }).ToList(),
            AnunciosActivosCount = slot.Anuncios.Count(a => a.Estado == EstadoAdSlot.Activo)
        };
    }

    private static AdSlotAnuncioAdminDto MapearAnuncioAAdminDto(AdSlotAnuncio anuncio, AdSlot? slot)
    {
        return new AdSlotAnuncioAdminDto
        {
            Id = anuncio.Id,
            AdSlotId = anuncio.AdSlotId,
            TituloSlot = slot?.Titulo ?? "",
            Ubicacion = slot?.Ubicacion ?? UbicacionAdSlot.HomepageLateral,
            PerfilDealerId = anuncio.PerfilDealerId,
            NombreDealer = anuncio.PerfilDealer?.NombreAgencia ?? "Dealer",
            ImagenUrl = anuncio.ImagenOriginalUrl,
            Enlace = anuncio.Enlace,
            Titulo = anuncio.Titulo,
            FechaInicioUtc = anuncio.FechaInicioUtc,
            FechaFinUtc = anuncio.FechaFinUtc,
            Estado = anuncio.Estado,
            MontoPagado = anuncio.MontoPagado,
            Impresiones = anuncio.Impresiones,
            Clicks = anuncio.Clicks,
            FechaCreacionUtc = anuncio.FechaCreacionUtc
        };
    }
}
