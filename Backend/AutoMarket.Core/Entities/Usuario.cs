namespace AutoMarket.Core.Entities;

public class Usuario
{
    public int UsuarioId { get; private set; }
    public string Nombre { get; private set; } = null!;
    public string Apellido { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string? TelefonoPersonal { get; private set; }
    public string Rol { get; private set; } = null!;
    public bool EmailConfirmado { get; private set; }
    public PerfilDealer? PerfilDealer { get; private set; }
    private readonly List<Anuncio> _anuncios = new();
    public IReadOnlyCollection<Anuncio> Anuncios => _anuncios.AsReadOnly();

    public DateTime CreatedAt { get; private set; }

    // ==========================================
    // 1. CONSTRUCTOR PARA EF CORE
    // ==========================================
    private Usuario() { }

    // ==========================================
    // 2. CONSTRUCTOR DE DOMINIO MODIFICADO
    // ==========================================

    public Usuario(
        string nombre,
        string apellido,
        string email,
        string passwordHash,
        string? telefonoPersonal,
        string rol,
        bool emailConfirmado = false)
    {

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));

        if (string.IsNullOrWhiteSpace(apellido))
            throw new ArgumentException("El apellido es obligatorio.", nameof(apellido));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El correo electrónico es obligatorio.", nameof(email));

        if (!email.Contains("@")) // Validación básica de estructura de correo
            throw new ArgumentException("El formato del correo electrónico es inválido.", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("La contraseña es obligatoria.", nameof(passwordHash));

        if (string.IsNullOrWhiteSpace(rol))
            throw new ArgumentException("El rol es obligatorio.", nameof(rol));

        if (rol == "Admin")
            throw new InvalidOperationException("No está permitido registrar un administrador por la vía pública.");

        if (rol != "Vendedor" && rol != "Dealer" && rol != "Comprador")
            throw new ArgumentException($"El rol '{rol}' no es válido para un registro de usuario.");

        Nombre = nombre;
        Apellido = apellido;
        Email = email.ToLowerInvariant().Trim();
        PasswordHash = passwordHash;
        TelefonoPersonal = telefonoPersonal;
        Rol = rol;
        EmailConfirmado = emailConfirmado;
    }

    // MÉTODO DE FÁBRICA ESTÁTICO: Solo accesible internamente por el sistema
    public static Usuario CrearAdministradorInterno(string nombre, string apellido, string email, string passwordHash)
    {
        var usuario = new Usuario
        {
            Nombre = nombre,
            Apellido = apellido,
            Email = email,
            EmailConfirmado = true,
            PasswordHash = passwordHash,
            Rol = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        return usuario;
    }

    public void AsignarPerfilDealer(PerfilDealer perfil)
    {
        if (Rol != "Dealer") throw new InvalidOperationException("Solo los dealers pueden tener un perfil comercial.");
        PerfilDealer = perfil;
    }

    public void CrearPerfilDealer(string nombreAgencia, string agenciaRNC, string ubicacion, string telefonoAgencia, string? descripcion = null, string? whatsApp = null)
    {
        if (Rol != "Dealer")
            throw new InvalidOperationException("Solo los usuarios con rol 'Dealer' pueden tener un perfil comercial.");

        // Instanciamos el perfil pasándole el objeto 'Usuario' completo (this) 
        // en lugar de un ID numérico que aún no existe
        PerfilDealer = new PerfilDealer(
            usuario: this,
            nombreAgencia: nombreAgencia,
            agenciaRNC: agenciaRNC,
            ubicacion: ubicacion,
            telefonoAgencia: telefonoAgencia,
            descripcion: descripcion,
            whatsApp: whatsApp
        );
    }

    public bool IsActivo { get; private set; } = true;

    // ==========================================
    // CAMBIO DE CORREO EN DOS PASOS (confirmación)
    // ==========================================
    public string? EmailPendiente { get; private set; }
    public string? CodigoConfirmacionHash { get; private set; }
    public DateTime? CodigoConfirmacionExpiracionUtc { get; private set; }

    public void ActualizarDatos(string nombre, string apellido, string? telefonoPersonal)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));

        if (string.IsNullOrWhiteSpace(apellido))
            throw new ArgumentException("El apellido es obligatorio.", nameof(apellido));

        Nombre = nombre.Trim();
        Apellido = apellido.Trim();
        TelefonoPersonal = string.IsNullOrWhiteSpace(telefonoPersonal)
            ? null
            : telefonoPersonal.Trim();
    }

    public void EstablecerCambioEmail(string nuevoEmail, string codigoHash, DateTime expiracionUtc)
    {
        if (string.IsNullOrWhiteSpace(nuevoEmail))
            throw new ArgumentException("El correo electrónico es obligatorio.", nameof(nuevoEmail));

        EmailPendiente = nuevoEmail.ToLowerInvariant().Trim();
        CodigoConfirmacionHash = codigoHash;
        CodigoConfirmacionExpiracionUtc = expiracionUtc;
    }

    public bool AplicarCambioEmailSiValido(string codigoHash, DateTime ahoraUtc)
    {
        if (string.IsNullOrEmpty(EmailPendiente) ||
            string.IsNullOrEmpty(CodigoConfirmacionHash) ||
            CodigoConfirmacionExpiracionUtc is not DateTime expiracion)
            return false;

        if (ahoraUtc > expiracion)
            return false;

        if (!string.Equals(CodigoConfirmacionHash, codigoHash, StringComparison.Ordinal))
            return false;

        Email = EmailPendiente;
        EmailConfirmado = true;
        EmailPendiente = null;
        CodigoConfirmacionHash = null;
        CodigoConfirmacionExpiracionUtc = null;
        return true;
    }

    public void Suspender()
    {
        IsActivo = false;
    }

    public void Reactivar()
    {
        IsActivo = true;
    }

    public void CambiarPassword(string nuevoPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(nuevoPasswordHash))
            throw new ArgumentException("La contraseña es obligatoria.", nameof(nuevoPasswordHash));

        PasswordHash = nuevoPasswordHash;
    }

}