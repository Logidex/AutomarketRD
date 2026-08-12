using AutoMarket.Application.DTOs;

namespace AutoMarket.Application.Interfaces;

public interface IContactoService
{
    /// <summary>
    /// Procesa un mensaje de contacto enviado desde el formulario público.
    /// Devuelve true si el mensaje se envió correctamente al correo de soporte.
    /// </summary>
    Task<bool> ProcesarMensajeContactoAsync(ContactoCreateDto dto);
}
