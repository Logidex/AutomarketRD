using AutoMarket.Core.Entities.Constants;

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

        if (!email.Contains("@") || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
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
        CreatedAt = DateTime.UtcNow;
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

    public void CrearPerfilDealerVendedor()
    {
        if (Rol != "Vendedor")
            throw new InvalidOperationException("Solo los usuarios con rol 'Vendedor' pueden crear este perfil.");

        if (PerfilDealer != null)
            return; // Ya existe

        PerfilDealer = new PerfilDealer(
            usuario: this,
            nombreAgencia: Nombre + " " + (Apellido ?? ""),
            agenciaRNC: "VENDEDOR-" + UsuarioId,
            ubicacion: null,
            telefonoAgencia: TelefonoPersonal
        );
    }

    // ==========================================
    // ASCENSO DE ROL (Comprador → Vendedor/Dealer, Vendedor → Dealer)
    // El rol solo puede crecer, nunca degradarse.
    // ==========================================
    public void ConvertirAVendedor()
    {
        if (Rol == "Dealer")
            throw new InvalidOperationException("Una cuenta Dealer ya tiene privilegios mayores y no puede convertirse en Vendedor.");

        if (Rol == "Vendedor")
            throw new InvalidOperationException("Tu cuenta ya es de tipo Vendedor.");

        Rol = "Vendedor";
    }

    // Cambio de rol EXCLUSIVO del administrador: permite mover la cuenta a
    // cualquier rol básico sin re-crear el perfil comercial (que gestiona el servicio).
    public void FijarRolAdmin(string nuevoRol)
    {
        if (nuevoRol != "Vendedor" && nuevoRol != "Dealer" && nuevoRol != "Comprador")
            throw new ArgumentException($"El rol '{nuevoRol}' no es válido para esta operación.");

        if (nuevoRol == "Dealer")
            throw new InvalidOperationException("La promoción a Dealer requiere el perfil comercial; usa ConvertirADealer.");

        Rol = nuevoRol;
    }

    public void QuitarPerfilDealer()
    {
        PerfilDealer = null;
    }

    public void ConvertirADealer(string nombreAgencia, string agenciaRNC, string ubicacion, string telefonoAgencia)
    {
        if (Rol == "Dealer")
            throw new InvalidOperationException("Tu cuenta ya es de tipo Dealer.");

        Rol = "Dealer";
        CrearPerfilDealer(nombreAgencia, agenciaRNC, ubicacion, telefonoAgencia);
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
    // ACEPTACIÓN DE TÉRMINOS Y CONDICIONES
    // (fecha del alta; null en cuentas creadas antes del requerimiento)
    // ==========================================
    public DateTime? TerminosAceptadosUtc { get; private set; }

    public void AceptarTerminos(DateTime utc)
    {
        TerminosAceptadosUtc = utc;
    }

    // ==========================================
    // CAMBIO DE CORREO EN DOS PASOS (confirmación)
    // ==========================================
    public string? EmailPendiente { get; private set; }
    public string? CodigoConfirmacionHash { get; private set; }
    public DateTime? CodigoConfirmacionExpiracionUtc { get; private set; }

    // ==========================================
    // CONFIRMACIÓN DE CORREO EN EL ALTA DE CUENTA
    // (Dealer): enlace con token enviado al registrarse.
    // ==========================================
    public string? CodigoConfirmacionEmailHash { get; private set; }
    public DateTime? CodigoConfirmacionEmailExpiracionUtc { get; private set; }

    public void EstablecerConfirmacionEmail(string codigoHash, DateTime expiracionUtc)
    {
        CodigoConfirmacionEmailHash = codigoHash;
        CodigoConfirmacionEmailExpiracionUtc = expiracionUtc;
    }

    public bool ConfirmarEmailSiValido(string codigoHash, DateTime ahoraUtc)
    {
        if (string.IsNullOrEmpty(CodigoConfirmacionEmailHash) ||
            CodigoConfirmacionEmailExpiracionUtc is not DateTime expiracion)
            return false;

        if (ahoraUtc > expiracion)
            return false;

        if (!string.Equals(CodigoConfirmacionEmailHash, codigoHash, StringComparison.Ordinal))
            return false;

        EmailConfirmado = true;
        CodigoConfirmacionEmailHash = null;
        CodigoConfirmacionEmailExpiracionUtc = null;
        return true;
    }

    // ==========================================
    // CAMBIO DE CONTRASEÑA EN DOS PASOS (confirmación)
    // ==========================================
    public string? PasswordPendienteHash { get; private set; }
    public string? CodigoPasswordHash { get; private set; }
    public DateTime? CodigoPasswordExpiracionUtc { get; private set; }

    // ==========================================
    // RECUPERACIÓN DE CONTRASEÑA (olvidada)
    // ==========================================
    public string? CodigoRecuperacionHash { get; private set; }
    public DateTime? CodigoRecuperacionExpiracionUtc { get; private set; }

    public void EstablecerCodigoRecuperacion(string codigoHash, DateTime expiracionUtc)
    {
        CodigoRecuperacionHash = codigoHash;
        CodigoRecuperacionExpiracionUtc = expiracionUtc;
    }

    public bool AplicarCodigoRecuperacionSiValido(string codigoHash, DateTime ahoraUtc)
    {
        if (string.IsNullOrEmpty(CodigoRecuperacionHash) ||
            CodigoRecuperacionExpiracionUtc is not DateTime expiracion)
            return false;

        if (ahoraUtc > expiracion)
            return false;

        if (!string.Equals(CodigoRecuperacionHash, codigoHash, StringComparison.Ordinal))
            return false;

        CodigoRecuperacionHash = null;
        CodigoRecuperacionExpiracionUtc = null;
        return true;
    }

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

    public void EstablecerCambioPassword(string nuevoPasswordHash, string codigoHash, DateTime expiracionUtc)
    {
        if (string.IsNullOrWhiteSpace(nuevoPasswordHash))
            throw new ArgumentException("La contraseña es obligatoria.", nameof(nuevoPasswordHash));

        PasswordPendienteHash = nuevoPasswordHash;
        CodigoPasswordHash = codigoHash;
        CodigoPasswordExpiracionUtc = expiracionUtc;
    }

    public bool AplicarCambioPasswordSiValido(string codigoHash, DateTime ahoraUtc)
    {
        if (string.IsNullOrEmpty(PasswordPendienteHash) ||
            string.IsNullOrEmpty(CodigoPasswordHash) ||
            CodigoPasswordExpiracionUtc is not DateTime expiracion)
            return false;

        if (ahoraUtc > expiracion)
            return false;

        if (!string.Equals(CodigoPasswordHash, codigoHash, StringComparison.Ordinal))
            return false;

        PasswordHash = PasswordPendienteHash;
        PasswordPendienteHash = null;
        CodigoPasswordHash = null;
        CodigoPasswordExpiracionUtc = null;
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

    // ==========================================
    // BLOQUEO DE CUENTA POR INTENTOS FALLIDOS
    // Estrictamente por cuenta: los fallos de una cuenta
    // nunca afectan a otra ni dependen de la red/IP.
    // ==========================================
    public int IntentosFallidos { get; private set; }
    public DateTime? BloqueadoHastaUtc { get; private set; }

    public bool EstaBloqueado(DateTime ahoraUtc)
    {
        return BloqueadoHastaUtc is DateTime hasta && ahoraUtc < hasta;
    }

    /// <summary>
    /// Registra un intento fallido de contraseña para ESTA cuenta. Al alcanzar
    /// el máximo permitido la cuenta queda bloqueada durante la ventana indicada.
    /// Devuelve los minutos de bloqueo aplicados, o null si aún no se alcanza el máximo.
    /// </summary>
    public int? RegistrarIntentoFallido(int maxIntentos, TimeSpan ventanaBloqueo, DateTime ahoraUtc)
    {
        if (maxIntentos < 1)
            throw new ArgumentOutOfRangeException(nameof(maxIntentos), "El máximo de intentos debe ser al menos 1.");

        IntentosFallidos++;

        if (IntentosFallidos < maxIntentos)
            return null;

        var minutos = Math.Max(1, (int)Math.Round(ventanaBloqueo.TotalMinutes));
        BloqueadoHastaUtc = ahoraUtc.Add(TimeSpan.FromMinutes(minutos));
        return minutos;
    }

    /// <summary>
    /// Reinicia el contador y el bloqueo de esta cuenta. Se invoca cuando se
    /// demuestra la identidad: inicio de sesión correcto, restablecimiento de
    /// contraseña con código de correo o cambio de contraseña confirmado.
    /// </summary>
    public void ReiniciarIntentosFallidos()
    {
        IntentosFallidos = 0;
        BloqueadoHastaUtc = null;
    }

    // ==========================================
    // LÍMITE DE CORREOS ENVIADOS POR USUARIO
    // Cooldown por tipo de correo + tope diario,
    // para prevenir abuso del SMTP.
    // ==========================================
    public int EmailsEnviadosHoy { get; private set; }
    public DateTime? VentanaEmailsInicioUtc { get; private set; }
    public DateTime? UltimoEnvioRecuperacionUtc { get; private set; }
    public DateTime? UltimoEnvioConfirmacionCuentaUtc { get; private set; }
    public DateTime? UltimoEnvioCambioPasswordUtc { get; private set; }
    public DateTime? UltimoEnvioCambioEmailUtc { get; private set; }

    /// <summary>
    /// Intenta registrar el envío de un correo a este usuario aplicando el
    /// cooldown por tipo y el tope diario. Devuelve null si el envío queda
    /// autorizado; si no, los minutos que deben esperar antes del próximo envío.
    /// </summary>
    public int? TryRegistrarEnvioEmail(string tipoEmail, DateTime ahoraUtc, TimeSpan cooldown, int topeDiario)
    {
        if (topeDiario < 1)
            throw new ArgumentOutOfRangeException(nameof(topeDiario), "El tope diario debe ser al menos 1.");

        // La ventana diaria cubre 24 h desde el primer envío; luego se reinicia.
        if (VentanaEmailsInicioUtc is not DateTime inicio || ahoraUtc >= inicio.AddDays(1))
        {
            inicio = ahoraUtc;
            VentanaEmailsInicioUtc = inicio;
            EmailsEnviadosHoy = 0;
        }

        if (EmailsEnviadosHoy >= topeDiario)
            return MinutosRestantes(inicio.AddDays(1), ahoraUtc);

        if (ObtenerUltimoEnvio(tipoEmail) is DateTime cuando && ahoraUtc < cuando.Add(cooldown))
            return MinutosRestantes(cuando.Add(cooldown), ahoraUtc);

        EmailsEnviadosHoy++;
        FijarUltimoEnvio(tipoEmail, ahoraUtc);
        return null;
    }

    private static int MinutosRestantes(DateTime limite, DateTime ahoraUtc)
    {
        return Math.Max(1, (int)Math.Ceiling((limite - ahoraUtc).TotalMinutes));
    }

    private DateTime? ObtenerUltimoEnvio(string tipoEmail) => tipoEmail switch
    {
        TiposEmail.Recuperacion => UltimoEnvioRecuperacionUtc,
        TiposEmail.ConfirmacionCuenta => UltimoEnvioConfirmacionCuentaUtc,
        TiposEmail.CambioPassword => UltimoEnvioCambioPasswordUtc,
        TiposEmail.CambioEmail => UltimoEnvioCambioEmailUtc,
        _ => null
    };

    private void FijarUltimoEnvio(string tipoEmail, DateTime utc)
    {
        switch (tipoEmail)
        {
            case TiposEmail.Recuperacion:
                UltimoEnvioRecuperacionUtc = utc;
                break;
            case TiposEmail.ConfirmacionCuenta:
                UltimoEnvioConfirmacionCuentaUtc = utc;
                break;
            case TiposEmail.CambioPassword:
                UltimoEnvioCambioPasswordUtc = utc;
                break;
            case TiposEmail.CambioEmail:
                UltimoEnvioCambioEmailUtc = utc;
                break;
        }
    }

}