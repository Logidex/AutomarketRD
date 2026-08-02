namespace AutoMarket.Core.Entities;

public class PerfilDealer
{
    public int UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;
    public string NombreAgencia { get; private set; } = null!;
    public string AgenciaRNC { get; private set; } = null!;
    public string? LogoUrl { get; private set; }
    public string? Horarios { get; private set; }
    public string Ubicacion { get; private set; } = null!;
    public string TelefonoAgencia { get; private set; } = null!;
    public string? Descripcion { get; private set; }
    public string? WhatsApp { get; private set; }
    public virtual SuscripcionDealer? Suscripcion { get; private set; }

    // ==========================================
    // 1. CONSTRUCTOR PARA EF CORE
    // ==========================================
    private PerfilDealer() { }

    // ==========================================
    // 2. CONSTRUCTOR DE DOMINIO MODIFICADO
    // ==========================================
    public PerfilDealer(
    Usuario usuario,
    string nombreAgencia,
    string agenciaRNC,
    string ubicacion,
    string telefonoAgencia,
    string? logoUrl = null,
    string? horarios = null,
    string? descripcion = null,
    string? whatsApp = null)
    {

        Usuario = usuario ?? throw new ArgumentNullException(nameof(usuario));

        if (string.IsNullOrWhiteSpace(nombreAgencia))
            throw new ArgumentException("El nombre de la agencia es obligatorio.", nameof(nombreAgencia));

        if (string.IsNullOrWhiteSpace(agenciaRNC))
            throw new ArgumentException("El RNC de la agencia es obligatorio.", nameof(agenciaRNC));

        if (string.IsNullOrWhiteSpace(ubicacion))
            throw new ArgumentException("La ubicación es obligatoria.", nameof(ubicacion));

        if (string.IsNullOrWhiteSpace(telefonoAgencia))
            throw new ArgumentException("El teléfono es obligatorio.", nameof(telefonoAgencia));

        NombreAgencia = nombreAgencia;
        AgenciaRNC = agenciaRNC;
        Ubicacion = ubicacion;
        TelefonoAgencia = telefonoAgencia;
        LogoUrl = logoUrl;
        Horarios = horarios;
        Descripcion = descripcion;
        WhatsApp = whatsApp;
    }

    // ==========================================
    // 3. COMPORTAMIENTO DE DOMINIO
    // ==========================================
    public void ActualizarPerfil(string nombreAgencia, string ubicacion, string telefonoAgencia, string? horarios, string? descripcion, string? whatsApp)
    {
        if (string.IsNullOrWhiteSpace(nombreAgencia))
            throw new ArgumentException("El nombre de la agencia es obligatorio.", nameof(nombreAgencia));

        if (string.IsNullOrWhiteSpace(ubicacion))
            throw new ArgumentException("La ubicación es obligatoria.", nameof(ubicacion));

        if (string.IsNullOrWhiteSpace(telefonoAgencia))
            throw new ArgumentException("El teléfono es obligatorio.", nameof(telefonoAgencia));

        NombreAgencia = nombreAgencia;
        Ubicacion = ubicacion;
        TelefonoAgencia = telefonoAgencia;
        Horarios = horarios;
        Descripcion = descripcion;
        WhatsApp = whatsApp;
    }

    public void ActualizarLogo(string logoUrl)
    {
        if (string.IsNullOrWhiteSpace(logoUrl))
            throw new ArgumentException("La URL del logo no puede estar vacía.", nameof(logoUrl));
            
        LogoUrl = logoUrl;
    }

    
}