namespace AutoMarket.Core.Entities;

public class Anuncio
{
    public int Id { get; private set; }
    public string NombreAnuncio => $"{Marca} {Modelo} {Anio}";
    public string Marca { get; private set; } = null!;
    public string Modelo { get; private set; } = null!;
    public string TipoVehiculo { get; private set; } = null!;
    public string ColorExterior { get; private set; } = null!;
    public string ColorInterior { get; private set; } = null!;
    public int Anio { get; private set; }
    public decimal Precio { get; private set; }
    public int Kilometraje { get; private set; }
    public string Transmision { get; private set; } = null!;
    public string Combustible { get; private set; } = null!;
    public string Ubicacion { get; private set; } = null!;
    public string Descripcion { get; private set; } = null!;
    public string Estado { get; private set; } = null!;
    public string Motor { get; private set; } = null!;
    public string Version { get; private set; } = null!;
    public string Traccion { get; private set; } = null!;
    public bool PublicarAlGuardar { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // El ID del creador (sea Dealer o Usuario Común)
    public int UsuarioId { get; private set; }

    // Propiedad de navegación hacia el padre
    public Usuario Usuario { get; private set; } = null!;

    // ==========================================
    // ENCAPSULAMIENTO DE COLECCIONES
    // ==========================================
    public List<string> Accesorios { get; private set; } = new();
    private readonly List<string> _fotos = new();
    public IReadOnlyCollection<string> Fotos => _fotos.AsReadOnly();
    private readonly List<Lead> _leads = new();
    public IReadOnlyCollection<Lead> Leads => _leads.AsReadOnly();

    // ==========================================
    // 1. CONSTRUCTOR PARA EF CORE
    // ==========================================
    private Anuncio() { }

    // ==========================================
    // 2. CONSTRUCTOR DE DOMINIO MODIFICADO
    // ==========================================
    public Anuncio(
    int usuarioId,
    string marca,
    string modelo,
    string version,
    string tipoVehiculo,
    string motor,
    string traccion,
    string colorExterior,
    string colorInterior,
    int anio,
    decimal precio,
    int kilometraje,
    string transmision,
    string combustible,
    List<string> accesorios,
    string ubicacion,
    string descripcion)
    {
        if (usuarioId <= 0)
        {
            throw new ArgumentException(
                "El ID de usuario/dealer es inválido."
            );
        }

        if (string.IsNullOrWhiteSpace(marca))
        {
            throw new ArgumentException(
                "La marca es obligatoria."
            );
        }

        if (string.IsNullOrWhiteSpace(modelo))
        {
            throw new ArgumentException(
                "El modelo es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(tipoVehiculo))
        {
            throw new ArgumentException(
                "El tipo de vehículo es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(motor))
        {
            throw new ArgumentException(
                "El motor es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(traccion))
        {
            throw new ArgumentException(
                "La tracción es obligatoria."
            );
        }

        if (precio <= 0)
        {
            throw new ArgumentException(
                "El precio debe ser mayor a cero."
            );
        }

        if (kilometraje < 0)
        {
            throw new ArgumentException(
                "El kilometraje no puede ser negativo."
            );
        }

        if (anio < 1900 || anio > DateTime.UtcNow.Year + 1)
        {
            throw new ArgumentException(
                "Año de fabricación inválido."
            );
        }
        
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            throw new ArgumentException(
                "La descripción es obligatoria."
            );
        }

        if (descripcion.Length > 5000)
        {
            throw new ArgumentException(
                "La descripción no puede superar los 5000 caracteres."
            );
        }

        UsuarioId = usuarioId;
        Marca = marca;
        Modelo = modelo;
        Version = version;
        TipoVehiculo = tipoVehiculo;
        Motor = motor;
        Traccion = traccion;
        ColorExterior = colorExterior;
        ColorInterior = colorInterior;
        Anio = anio;
        Precio = precio;
        Kilometraje = kilometraje;
        Transmision = transmision;
        Combustible = combustible;
        Accesorios = accesorios ?? new List<string>();
        Ubicacion = ubicacion;
        Descripcion = descripcion;

        Estado = "Borrador";
        CreatedAt = DateTime.UtcNow;
    }

    // ==========================================
    // 3. MÉTODO AGREGAR FOTOS OPTIMIZADO
    // ==========================================
    public void AgregarFotos(List<string> rutasFotos)
    {
        if (rutasFotos == null || !rutasFotos.Any())
            throw new ArgumentException("Debes proporcionar al menos una foto.");

        if (_fotos.Count + rutasFotos.Count > 10)
        {
            throw new InvalidOperationException($"Límite excedido. El anuncio ya tiene {_fotos.Count} fotos y el máximo total es 10.");
        }

        _fotos.AddRange(rutasFotos);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publicar()
    {
        if (Estado == "Publicado")
            throw new InvalidOperationException("El anuncio ya está publicado.");

        if (_fotos.Count < 5)
            throw new InvalidOperationException("Imposible publicar: Un anuncio requiere un mínimo de 5 fotos.");

        Estado = "Publicado";
        UpdatedAt = DateTime.UtcNow;
    }

    // ==========================================
    // 4. ACTUALIZAR INFO (Manteniendo consistencia)
    // ==========================================
    public void ActualizarInfo(
    string marca,
    string modelo,
    string version,
    string tipoVehiculo,
    string motor,
    string traccion,
    string colorExterior,
    string colorInterior,
    int anio,
    decimal precio,
    int kilometraje,
    string transmision,
    string combustible,
    List<string> accesorios,
    string ubicacion,
    string descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            throw new ArgumentException(
                "La descripción es obligatoria."
            );
        }

        if (descripcion.Length > 5000)
        {
            throw new ArgumentException(
                "La descripción no puede superar los 5000 caracteres."
            );
        }

        if (string.IsNullOrWhiteSpace(marca))
        {
            throw new ArgumentException(
                "La marca es obligatoria."
            );
        }

        if (string.IsNullOrWhiteSpace(modelo))
        {
            throw new ArgumentException(
                "El modelo es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(tipoVehiculo))
        {
            throw new ArgumentException(
                "El tipo de vehículo es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(motor))
        {
            throw new ArgumentException(
                "El motor es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(traccion))
        {
            throw new ArgumentException(
                "La tracción es obligatoria."
            );
        }

        if (string.IsNullOrWhiteSpace(colorExterior))
        {
            throw new ArgumentException(
                "El color exterior es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(colorInterior))
        {
            throw new ArgumentException(
                "El color interior es obligatorio."
            );
        }

        if (anio < 1900 || anio > DateTime.UtcNow.Year + 1)
        {
            throw new ArgumentException(
                "Año de fabricación inválido."
            );
        }

        if (precio <= 0)
        {
            throw new ArgumentException(
                "El precio debe ser mayor a cero."
            );
        }

        if (kilometraje < 0)
        {
            throw new ArgumentException(
                "El kilometraje no puede ser negativo."
            );
        }

        if (string.IsNullOrWhiteSpace(transmision))
        {
            throw new ArgumentException(
                "La transmisión es obligatoria."
            );
        }

        if (string.IsNullOrWhiteSpace(combustible))
        {
            throw new ArgumentException(
                "El combustible es obligatorio."
            );
        }

        if (string.IsNullOrWhiteSpace(ubicacion))
        {
            throw new ArgumentException(
                "La ubicación es obligatoria."
            );
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            throw new ArgumentException(
                "La descripción es obligatoria."
            );
        }

        Marca = marca.Trim();
        Modelo = modelo.Trim();
        Version = version?.Trim() ?? string.Empty;

        TipoVehiculo = tipoVehiculo.Trim();
        Motor = motor.Trim();
        Traccion = traccion.Trim();

        ColorExterior = colorExterior.Trim();
        ColorInterior = colorInterior.Trim();

        Anio = anio;
        Precio = precio;
        Kilometraje = kilometraje;

        Transmision = transmision.Trim();
        Combustible = combustible.Trim();

        Accesorios = accesorios ?? new List<string>();

        Ubicacion = ubicacion.Trim();
        Descripcion = descripcion.Trim();

        UpdatedAt = DateTime.UtcNow;
    }

    public void EliminarFoto(string urlFoto)
    {
        if (string.IsNullOrWhiteSpace(urlFoto))
            throw new ArgumentException("La URL de la foto no puede estar vacía.");

        if (!_fotos.Contains(urlFoto))
            throw new KeyNotFoundException("La foto especificada no pertenece a este anuncio.");

        _fotos.Remove(urlFoto);
        UpdatedAt = DateTime.UtcNow;
    }

    public void CambiarEstado(string nuevoEstado)
    {
        Estado = nuevoEstado;
    }
}
