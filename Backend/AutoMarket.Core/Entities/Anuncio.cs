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
        string tipoVehiculo,
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
            throw new ArgumentException("El ID de usuario/dealer es inválido.");

        if (string.IsNullOrWhiteSpace(marca) || string.IsNullOrWhiteSpace(modelo))
            throw new ArgumentException("La marca y el modelo son obligatorios.");

        if (precio <= 0)
            throw new ArgumentException("El precio debe ser mayor a cero.");

        if (anio < 1900 || anio > DateTime.UtcNow.Year + 1)
            throw new ArgumentException("Año de fabricación inválido.");

        UsuarioId = usuarioId;
        Marca = marca;
        Modelo = modelo;
        TipoVehiculo = tipoVehiculo;
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
        string marca, string modelo, string tipoVehiculo, string colorExterior,
        string colorInterior, int anio, decimal precio, int kilometraje,
        string transmision, string combustible, List<string> accesorios,
        string ubicacion, string descripcion, bool publicarAlGuardar)
    {
        if (string.IsNullOrWhiteSpace(marca) || string.IsNullOrWhiteSpace(modelo))
            throw new ArgumentException("La marca y el modelo son obligatorios.");

        if (precio <= 0)
            throw new ArgumentException("El precio debe ser mayor a cero.");

        if (anio < 1900 || anio > DateTime.UtcNow.Year + 1)
            throw new ArgumentException("Año de fabricación inválido.");

        Marca = marca;
        Modelo = modelo;
        TipoVehiculo = tipoVehiculo;
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
        UpdatedAt = DateTime.UtcNow;

        if (publicarAlGuardar)
        {
            Publicar(); 
        }
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
}
