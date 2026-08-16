using AutoMarket.Application.DTOs.Ticket;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoMarket.Application.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEmailSenderService _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TicketService> _logger;

    public TicketService(
        ITicketRepository ticketRepository,
        IUsuarioRepository usuarioRepository,
        IEmailSenderService emailSender,
        IConfiguration configuration,
        ILogger<TicketService> logger)
    {
        _ticketRepository = ticketRepository;
        _usuarioRepository = usuarioRepository;
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
    }

    // =========================================================================
    // PANEL DEL DEALER / VENDEDOR
    // =========================================================================

    public async Task<int> CrearTicketAsync(TicketCreateDto dto, int usuarioId)
    {
        var autor = await _usuarioRepository.ObtenerPorIdAsync(usuarioId)
            ?? throw new KeyNotFoundException("No se encontró la cuenta del usuario.");

        var ticket = new Ticket(
            usuarioId: usuarioId,
            asunto: dto.Asunto,
            categoria: dto.Categoria,
            prioridad: dto.Prioridad,
            mensajeInicial: dto.Mensaje);

        await _ticketRepository.AgregarAsync(ticket);

        var primerMensaje = ticket.Mensajes.FirstOrDefault()?.Mensaje ?? dto.Mensaje;
        await NotificarAdminAsync(ticket, autor, primerMensaje, esRespuestaCliente: false);

        _logger.LogInformation("Ticket {TicketId} creado por usuario {UsuarioId}", ticket.Id, usuarioId);
        return ticket.Id;
    }

    public async Task<IReadOnlyCollection<TicketListadoDto>> ObtenerMisTicketsAsync(int usuarioId)
    {
        var tickets = await _ticketRepository.ObtenerPorUsuarioIdAsync(usuarioId);
        return tickets.Select(MapearListado).ToList();
    }

    public async Task<TicketDetalleDto> ObtenerTicketAsync(int ticketId, int usuarioId)
    {
        var ticket = await ObtenerYValidarDuenoAsync(ticketId, usuarioId);
        return MapearDetalle(ticket);
    }

    public async Task ResponderTicketAsync(int ticketId, TicketMensajeCreateDto dto, int usuarioId)
    {
        var ticket = await ObtenerYValidarDuenoAsync(ticketId, usuarioId);

        ticket.AgregarMensaje(autorId: usuarioId, esAdmin: false, mensaje: dto.Mensaje);
        await _ticketRepository.GuardarCambiosAsync();

        await NotificarAdminAsync(ticket, ticket.Usuario, dto.Mensaje, esRespuestaCliente: true);
    }

    public async Task CerrarTicketAsync(int ticketId, int usuarioId)
    {
        var ticket = await ObtenerYValidarDuenoAsync(ticketId, usuarioId);

        if (ticket.Estado == TicketEstado.Cerrado)
            throw new BusinessRuleException("Este ticket ya está cerrado.");

        ticket.Cerrar();
        await _ticketRepository.GuardarCambiosAsync();
    }

    // =========================================================================
    // PANEL DEL ADMINISTRADOR
    // =========================================================================

    public async Task<IReadOnlyCollection<TicketListadoDto>> ObtenerTicketsAdminAsync()
    {
        var tickets = await _ticketRepository.ObtenerTodosAdminAsync();
        return tickets.Select(MapearListado).ToList();
    }

    public async Task<TicketDetalleDto> ObtenerTicketAdminAsync(int ticketId)
    {
        var ticket = await _ticketRepository.ObtenerPorIdConMensajesAsync(ticketId)
            ?? throw new KeyNotFoundException("El ticket no existe.");
        return MapearDetalle(ticket);
    }

    public async Task ResponderTicketAdminAsync(int ticketId, TicketMensajeCreateDto dto, int adminId)
    {
        var ticket = await _ticketRepository.ObtenerPorIdConMensajesAsync(ticketId)
            ?? throw new KeyNotFoundException("El ticket no existe.");

        ticket.AgregarMensaje(autorId: adminId, esAdmin: true, mensaje: dto.Mensaje);

        // Al responder, el ticket pasa a "En proceso" para reflejar atención activa.
        if (ticket.Estado == TicketEstado.Abierto)
            ticket.CambiarEstado(TicketEstado.EnProceso);

        await _ticketRepository.GuardarCambiosAsync();

        await NotificarClienteAsync(ticket, dto.Mensaje);
    }

    public async Task CambiarEstadoAdminAsync(int ticketId, CambiarEstadoTicketDto dto)
    {
        var ticket = await _ticketRepository.ObtenerPorIdConMensajesAsync(ticketId)
            ?? throw new KeyNotFoundException("El ticket no existe.");

        ticket.CambiarEstado(dto.NuevoEstado);
        await _ticketRepository.GuardarCambiosAsync();

        if (dto.NuevoEstado is TicketEstado.Resuelto or TicketEstado.Cerrado or TicketEstado.Detenido)
            await NotificarCambioEstadoAsync(ticket, dto.NuevoEstado);
    }

    public async Task<TicketResumenAdminDto> ObtenerResumenAdminAsync()
    {
        return new TicketResumenAdminDto
        {
            CantidadAbiertos = await _ticketRepository.ContarAbiertosAsync()
        };
    }

    // =========================================================================
    // MAPEOS
    // =========================================================================

    private static TicketListadoDto MapearListado(Ticket t)
    {
        var ultimo = t.Mensajes
            .OrderByDescending(m => m.FechaCreacionUtc)
            .FirstOrDefault();

        return new TicketListadoDto
        {
            Id = t.Id,
            UsuarioId = t.UsuarioId,
            UsuarioNombre = NombreUsuario(t.Usuario),
            UsuarioEmail = t.Usuario.Email,
            Asunto = t.Asunto,
            Categoria = t.Categoria,
            Prioridad = t.Prioridad,
            Estado = t.Estado,
            FechaCreacionUtc = t.FechaCreacionUtc,
            FechaActualizacionUtc = t.FechaActualizacionUtc,
            UltimoMensaje = ultimo?.Mensaje ?? string.Empty,
            CantidadMensajes = t.Mensajes.Count,
            UltimoMensajeEsAdmin = ultimo?.EsAdmin ?? false
        };
    }

    private static TicketDetalleDto MapearDetalle(Ticket t)
    {
        return new TicketDetalleDto
        {
            Id = t.Id,
            UsuarioId = t.UsuarioId,
            UsuarioNombre = NombreUsuario(t.Usuario),
            UsuarioEmail = t.Usuario.Email,
            Asunto = t.Asunto,
            Categoria = t.Categoria,
            Prioridad = t.Prioridad,
            Estado = t.Estado,
            FechaCreacionUtc = t.FechaCreacionUtc,
            FechaActualizacionUtc = t.FechaActualizacionUtc,
            Mensajes = t.Mensajes
                .OrderBy(m => m.FechaCreacionUtc)
                .Select(m => new TicketMensajeDto
                {
                    Id = m.Id,
                    AutorId = m.AutorId,
                    AutorNombre = NombreUsuario(m.Autor),
                    EsAdmin = m.EsAdmin,
                    Mensaje = m.Mensaje,
                    FechaCreacionUtc = m.FechaCreacionUtc
                })
                .ToList()
        };
    }

    private static string NombreUsuario(Usuario? u)
    {
        if (u is null)
            return "Usuario";

        if (u.PerfilDealer is { } perfil && !string.IsNullOrWhiteSpace(perfil.NombreAgencia))
            return perfil.NombreAgencia;

        return $"{u.Nombre} {u.Apellido}".Trim();
    }

    // =========================================================================
    // NOTIFICACIONES POR CORREO (resilientes: un fallo SMTP no rompe el flujo)
    // =========================================================================

    private async Task<Ticket> ObtenerYValidarDuenoAsync(int ticketId, int usuarioId)
    {
        var ticket = await _ticketRepository.ObtenerPorIdConMensajesAsync(ticketId)
            ?? throw new KeyNotFoundException("El ticket no existe.");

        if (ticket.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException("Acceso denegado: Este ticket no pertenece a tu cuenta.");

        return ticket;
    }

    private async Task NotificarAdminAsync(Ticket ticket, Usuario? autor, string mensaje, bool esRespuestaCliente)
    {
        var adminEmail = _configuration["Admin:Email"];
        if (string.IsNullOrWhiteSpace(adminEmail))
            return;

        var cliente = ticket.Usuario ?? autor;
        var nombreCliente = NombreUsuario(cliente);
        var emailCliente = cliente?.Email ?? string.Empty;

        var titulo = esRespuestaCliente
            ? $"Nueva respuesta del cliente en el ticket #{ticket.Id}"
            : $"Nuevo ticket de soporte #{ticket.Id}";

        var cuerpoHtml = $@"
            <h2>{titulo}</h2>
            <p><strong>Cliente:</strong> {nombreCliente} ({emailCliente})</p>
            <p><strong>Asunto:</strong> {ticket.Asunto}</p>
            <p><strong>Categoría:</strong> {ticket.Categoria}</p>
            <p><strong>Prioridad:</strong> {ticket.Prioridad}</p>
            <hr/>
            <p><strong>Mensaje:</strong></p>
            <p><i>{mensaje}</i></p>
            <p>Gestiona este ticket desde el panel de administración de AutoMarket RD.</p>";

        await EnviarCorreoSeguroAsync(adminEmail, titulo, cuerpoHtml);
    }

    private async Task NotificarClienteAsync(Ticket ticket, string mensaje)
    {
        var destinatario = ticket.Usuario.Email;
        if (string.IsNullOrWhiteSpace(destinatario))
            return;

        var asunto = $"Tu ticket #{ticket.Id} fue respondido";

        var cuerpoHtml = $@"
            <h2>Respondimos a tu ticket #{ticket.Id}</h2>
            <p><strong>Asunto:</strong> {ticket.Asunto}</p>
            <hr/>
            <p><strong>Mensaje del equipo:</strong></p>
            <p><i>{mensaje}</i></p>
            <p>Puedes continuar la conversación desde el panel de soporte de AutoMarket RD.</p>";

        await EnviarCorreoSeguroAsync(destinatario, asunto, cuerpoHtml);
    }

    private async Task NotificarCambioEstadoAsync(Ticket ticket, TicketEstado estado)
    {
        var destinatario = ticket.Usuario.Email;
        if (string.IsNullOrWhiteSpace(destinatario))
            return;

        var asunto = $"Tu ticket #{ticket.Id} fue marcado como {estado}";

        var notaAdicional = estado == TicketEstado.Detenido
            ? "Nuestro equipo está revisando tu caso y no podrás enviar mensajes temporalmente. Te responderemos aquí mismo apenas resolvamos tu incidencia."
            : "Si necesitas más ayuda, abre un nuevo ticket desde el panel de soporte de AutoMarket RD.";

        var cuerpoHtml = $@"
            <h2>Actualización de tu ticket #{ticket.Id}</h2>
            <p><strong>Asunto:</strong> {ticket.Asunto}</p>
            <p><strong>Nuevo estado:</strong> {estado}</p>
            <p>{notaAdicional}</p>";

        await EnviarCorreoSeguroAsync(destinatario, asunto, cuerpoHtml);
    }

    private async Task EnviarCorreoSeguroAsync(string destinatario, string asunto, string cuerpoHtml)
    {
        try
        {
            await _emailSender.EnviarCorreoAsync(destinatario, asunto, cuerpoHtml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando correo SMTP para el ticket a {Destinatario}", destinatario);
        }
    }
}
