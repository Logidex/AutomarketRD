using AutoMarket.Application.DTOs;
using AutoMarket.Application.DTOs.Lead;
using AutoMarket.Application.Helpers;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoMarket.Application.Services;

/// <summary>
/// Servicio para manejar Lead.
/// </summary>
public class LeadService : ILeadService
{
    private readonly ILeadRepository _leadRepository;
    private readonly IAnuncioRepository _anuncioRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEmailSenderService _emailSender;
    private readonly ILogger<LeadService> _logger;
    private readonly IConfiguration _configuration;

/// <summary>
/// Inicializa una nueva instancia de la clase LeadService.
/// </summary>
    public LeadService(
        ILeadRepository leadRepository,
        IAnuncioRepository anuncioRepository,
        IUsuarioRepository usuarioRepository,
        IEmailSenderService emailSender,
        ILogger<LeadService> logger,
        IConfiguration configuration)
    {
        _leadRepository = leadRepository;
        _anuncioRepository = anuncioRepository;
        _usuarioRepository = usuarioRepository;
        _emailSender = emailSender;
        _logger = logger;
        _configuration = configuration;
    }

/// <summary>
/// CrearLeadAsync Crear lead async. Parámetros: Parámetro dto (LeadCreateDto), Parámetro null (int? usuarioIdRemitente =). Retorna: Task.
/// </summary>
    public async Task CrearLeadAsync(LeadCreateDto dto, int? usuarioIdRemitente = null)
    {
        var anuncio = await _anuncioRepository.ObtenerPorIdAsync(dto.AnuncioId);

        if (anuncio == null)
            throw new KeyNotFoundException("El vehículo al que intentas contactar no existe o ya fue vendido.");

        // Solo se pueden enviar leads a vehículos visibles en la vitrina.
        if (anuncio.Estado != "Publicado")
            throw new BusinessRuleException("Este vehículo ya no está disponible para contactos.");

        var vendedor = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(anuncio.UsuarioId);

        if (vendedor == null)
            throw new BusinessRuleException("No se encontró el propietario de este anuncio.");

        ValidarAutocontacto(dto, anuncio, vendedor, usuarioIdRemitente);

        var lead = new Lead(
            anuncioId: dto.AnuncioId,
            nombreContacto: dto.NombreContacto,
            emailContacto: dto.EmailContacto ?? string.Empty,
            telefonoContacto: dto.TelefonoContacto ?? string.Empty,
            mensaje: dto.Mensaje,
            canal: dto.Canal,
            usuarioIdRemitente: usuarioIdRemitente
        );

        await _leadRepository.AgregarAsync(lead);

        try
        {
            string asunto = $"Nuevo Lead de AutoMarket RD: {anuncio.Marca} {anuncio.Modelo}";

            // Plantilla básica en HTML para que luzca profesional
            string cuerpoHtml = $@"
                <h2>¡Tienes un nuevo interesado en tu vehículo!</h2>
                <p><strong>Vehículo:</strong> {anuncio.Marca} {anuncio.Modelo} ({anuncio.Anio})</p>
                <p><strong>Nombre del cliente:</strong> {lead.NombreContacto}</p>
                <p><strong>Teléfono:</strong> {lead.TelefonoContacto}</p>
                <p><strong>Email:</strong> {lead.EmailContacto}</p>
                <p><strong>Canal de origen:</strong> {lead.Canal}</p>
                <hr/>
                <p><strong>Mensaje:</strong></p>
                <p><i>{lead.Mensaje}</i></p>";

            // Asumiendo que tu entidad Usuario tiene la propiedad Email/Correo
            await _emailSender.EnviarCorreoAsync(
                vendedor.Email,
                asunto,
                PlantillaCorreoHelper.Envolver(
                    _configuration["App:FrontendUrl"],
                    "¡Tienes un nuevo interesado!",
                    cuerpoHtml));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando correo SMTP al dealer {DealerId}", vendedor.UsuarioId);
        }
    }

    private void ValidarAutocontacto(LeadCreateDto dto, Anuncio anuncio, Usuario vendedor, int? usuarioIdRemitente)
    {
        _logger.LogDebug(
            "Validando autocontacto: anuncio {AnuncioId}, remitente {UsuarioIdRemitente}",
            anuncio.Id,
            usuarioIdRemitente);
        // Regla 1: remitente autenticado dueño del anuncio
        if (usuarioIdRemitente is int usuarioId && usuarioId == anuncio.UsuarioId)
            throw new BusinessRuleException("No puedes crear un contacto sobre tu propio vehículo.");

        var emailPropietario = (vendedor.Email ?? string.Empty).Trim().ToLowerInvariant();
        var emailIngresado = (dto.EmailContacto ?? string.Empty).Trim().ToLowerInvariant();

        var telefonosPropietario = new[] { vendedor.TelefonoPersonal, vendedor.PerfilDealer?.TelefonoAgencia, vendedor.PerfilDealer?.WhatsApp }
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(SoloDigitos)
            .ToHashSet();

        var telefonoIngresado = SoloDigitos(dto.TelefonoContacto);

        bool esAutoContacto =
            (emailIngresado.Length > 0 && emailIngresado == emailPropietario) ||
            (!string.IsNullOrWhiteSpace(telefonoIngresado) && telefonosPropietario.Contains(telefonoIngresado));

        if (esAutoContacto)
            throw new BusinessRuleException("No puedes crear un contacto sobre tu propio vehículo.");
    }

    private static string SoloDigitos(string? valor)
        => string.Concat((valor ?? string.Empty).Where(char.IsDigit));

    public async Task<IReadOnlyCollection<Lead>> ObtenerLeadsPorAnuncioAsync(int anuncioId, int usuarioId)
    {
        var anuncio = await _anuncioRepository.ObtenerPorIdAsync(anuncioId);

        if (anuncio is null)
            throw new KeyNotFoundException("El anuncio no existe o ya fue eliminado.");

        if (anuncio.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException("Acceso denegado: Este anuncio no pertenece a tu inventario.");

        return await _leadRepository.ObtenerPorAnuncioIdAsync(anuncioId);
    }

    public async Task<IReadOnlyCollection<LeadDealerDto>> ObtenerLeadsPorDealerAsync(int dealerId)
    {
        var leads = await _leadRepository.ObtenerPorUsuarioIdAsync(dealerId);

        return leads
            .Select(l => new LeadDealerDto
            {
                Id = l.Id,
                AnuncioId = l.AnuncioId,
                Anuncio = l.Anuncio == null
                    ? null
                    : new LeadAnuncioResumenDto
                    {
                        Id = l.Anuncio.Id,
                        NombreAnuncio = l.Anuncio.NombreAnuncio,
                        Marca = l.Anuncio.Marca,
                        Modelo = l.Anuncio.Modelo,
                        Anio = l.Anuncio.Anio
                    },
                NombreContacto = l.NombreContacto,
                EmailContacto = l.EmailContacto,
                TelefonoContacto = l.TelefonoContacto,
                Mensaje = l.Mensaje,
                Canal = l.Canal.ToString(),
                FechaCreacionUtc = l.FechaCreacionUtc,
                Leido = l.Leido
            })
            .ToList();
    }

    public async Task<LeadNoLeidosResumenDto> ObtenerResumenNoLeidosAsync(int usuarioId)
    {
        var cantidad = await _leadRepository.ContarNoLeidosPorUsuarioAsync(usuarioId);
        var recientes = await _leadRepository.ObtenerRecientesNoLeidosPorUsuarioAsync(usuarioId, 5);

        return new LeadNoLeidosResumenDto
        {
            CantidadNoLeidos = cantidad,
            Recientes = recientes
                .Select(l => new LeadDealerDto
                {
                    Id = l.Id,
                    AnuncioId = l.AnuncioId,
                    Anuncio = l.Anuncio == null
                        ? null
                        : new LeadAnuncioResumenDto
                        {
                            Id = l.Anuncio.Id,
                            NombreAnuncio = l.Anuncio.NombreAnuncio,
                            Marca = l.Anuncio.Marca,
                            Modelo = l.Anuncio.Modelo,
                            Anio = l.Anuncio.Anio
                        },
                    NombreContacto = l.NombreContacto,
                    Mensaje = l.Mensaje,
                    Canal = l.Canal.ToString(),
                    FechaCreacionUtc = l.FechaCreacionUtc,
                    Leido = false
                })
                .ToList()
        };
    }

    public async Task<IReadOnlyCollection<LeadContactoUsuarioDto>> ObtenerMisContactosAsync(int usuarioId)
    {
        var leads = await _leadRepository.ObtenerPorRemitenteIdAsync(usuarioId);

        return leads
            .Select(l =>
            {
                var vendedor = l.Anuncio?.Usuario;
                string? nombreVendedor = null;
                bool esParticular = false;

                if (vendedor != null)
                {
                    nombreVendedor =
                        vendedor.PerfilDealer?.NombreAgencia ??
                        $"{vendedor.Nombre} {vendedor.Apellido}".Trim();
                    esParticular =
                        string.Equals(vendedor.Rol, "Vendedor", StringComparison.OrdinalIgnoreCase);
                }

                return new LeadContactoUsuarioDto
                {
                    Id = l.Id,
                    AnuncioId = l.AnuncioId,
                    Marca = l.Anuncio?.Marca ?? string.Empty,
                    Modelo = l.Anuncio?.Modelo ?? string.Empty,
                    Anio = l.Anuncio?.Anio ?? 0,
                    FotoPrincipal = l.Anuncio?.Fotos.FirstOrDefault(),
                    NombreVendedor = nombreVendedor,
                    EsVendedorParticular = esParticular,
                    Mensaje = l.Mensaje,
                    Canal = l.Canal.ToString(),
                    FechaCreacionUtc = l.FechaCreacionUtc
                };
            })
            .ToList();
    }

    public async Task<bool> MarcarLeidoAsync(int leadId, int usuarioId)
    {
        var lead = await _leadRepository.ObtenerPorIdAsync(leadId);

        if (lead is null) return false;

        // Solo el dueño del anuncio puede atender sus leads
        if (lead.Anuncio.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException("Acceso denegado: Este lead no pertenece a tu inventario.");

        lead.MarcarComoLeido();
        await _leadRepository.GuardarCambiosAsync();

        return true;
    }

    public async Task<int> MarcarTodosLeidosAsync(int usuarioId)
    {
        return await _leadRepository.MarcarTodosLeidosAsync(usuarioId);
    }
}
