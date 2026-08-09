using AutoMarket.Application.DTOs;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutoMarket.Application.Services;

public class LeadService : ILeadService
{
    private readonly ILeadRepository _leadRepository;
    private readonly IAnuncioRepository _anuncioRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEmailSenderService _emailSender;
    private readonly ILogger<LeadService> _logger;

    public LeadService(
        ILeadRepository leadRepository,
        IAnuncioRepository anuncioRepository,
        IUsuarioRepository usuarioRepository,
        IEmailSenderService emailSender,
        ILogger<LeadService> logger)
    {
        _leadRepository = leadRepository;
        _anuncioRepository = anuncioRepository;
        _usuarioRepository = usuarioRepository;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task CrearLeadAsync(LeadCreateDto dto)
    {
        var anuncio = await _anuncioRepository.ObtenerPorIdAsync(dto.AnuncioId);
        
        if (anuncio == null)
            throw new KeyNotFoundException("El vehículo al que intentas contactar no existe o ya fue vendido.");

        var vendedor = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(anuncio.UsuarioId);
        
        if (vendedor == null)
            throw new InvalidOperationException("No se encontró el propietario de este anuncio.");

        var lead = new Lead(
            anuncioId: dto.AnuncioId,
            nombreContacto: dto.NombreContacto,
            emailContacto: dto.EmailContacto ?? string.Empty,
            telefonoContacto: dto.TelefonoContacto ?? string.Empty,
            mensaje: dto.Mensaje,
            canal: dto.Canal
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
            await _emailSender.EnviarCorreoAsync(vendedor.Email, asunto, cuerpoHtml); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando correo SMTP al dealer {DealerId}", vendedor.UsuarioId);
        }
    }

    public async Task<IReadOnlyCollection<Lead>> ObtenerLeadsPorAnuncioAsync(int anuncioId, int usuarioId)
    {
        var anuncio = await _anuncioRepository.ObtenerPorIdAsync(anuncioId);

        if (anuncio is null)
            throw new KeyNotFoundException("El anuncio no existe o ya fue eliminado.");

        if (anuncio.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException("Acceso denegado: Este anuncio no pertenece a tu inventario.");

        return await _leadRepository.ObtenerPorAnuncioIdAsync(anuncioId);
    }

    public async Task<IReadOnlyCollection<Lead>> ObtenerLeadsPorDealerAsync(int dealerId)
    {
        return await _leadRepository.ObtenerPorUsuarioIdAsync(dealerId);
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
}