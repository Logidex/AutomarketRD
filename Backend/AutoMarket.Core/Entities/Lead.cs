using AutoMarket.Core.Entities.Enums;

namespace AutoMarket.Core.Entities;

public class Lead
{
    public int Id { get; private set; }
    
    // Relación con el anuncio por el que están preguntando
    public int AnuncioId { get; private set; }
    public Anuncio Anuncio { get; private set; } = null!;

    // Datos del prospecto
    public string NombreContacto { get; private set; } = null!;
    public string EmailContacto { get; private set; } = null!;
    public string TelefonoContacto { get; private set; } = null!;
    public string Mensaje { get; private set; } = null!;
    
    // Metadatos para analítica
    public CanalContacto Canal { get; private set; }
    public DateTime FechaCreacionUtc { get; private set; }

    private Lead() { }

    // Constructor de dominio para instanciación segura
    public Lead(int anuncioId, string nombreContacto, string emailContacto, string telefonoContacto, string mensaje, CanalContacto canal)
    {
        if (anuncioId <= 0) 
            throw new ArgumentException("El ID del anuncio es inválido.");
            
        if (string.IsNullOrWhiteSpace(nombreContacto)) 
            throw new ArgumentException("El nombre de contacto es obligatorio.");

        AnuncioId = anuncioId;
        NombreContacto = nombreContacto;
        EmailContacto = emailContacto;
        TelefonoContacto = telefonoContacto;
        Mensaje = mensaje;
        Canal = canal;
        FechaCreacionUtc = DateTime.UtcNow;
    }
}