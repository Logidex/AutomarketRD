using AutoMarket.Application.DTOs;
using AutoMarket.Application.Helpers;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

/// <summary>
/// Servicio para manejar Anuncio.
/// </summary>
public class AnuncioService : IAnuncioService
{
    private readonly IAnuncioRepository _repository;
    private readonly IAlmacenadorArchivos _almacenadorArchivos;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPlanCatalogoRepository _planCatalogoRepository;

/// <summary>
/// Inicializa una nueva instancia de la clase AnuncioService.
/// </summary>
    public AnuncioService(
        IAnuncioRepository repository,
        IAlmacenadorArchivos almacenadorArchivos,
        IUsuarioRepository usuarioRepository,
        IPlanCatalogoRepository planCatalogoRepository)
    {
        _repository = repository;
        _almacenadorArchivos = almacenadorArchivos;
        _usuarioRepository = usuarioRepository;
        _planCatalogoRepository = planCatalogoRepository;
    }

    private static readonly HashSet<string> EstadosValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "Publicado", "Borrador", "Pausado", "Vendido"
    };

    // Los campos que participan en la búsqueda pública se guardan sin acentos
    // (NormalizadorTexto) para que el ILike del repositorio compare en igualdad.
    private static string NormalizarBusqueda(string? valor) =>
        NormalizadorTexto.Normalizar(valor);

    public async Task<int> CrearAnuncioAsync(
    AnuncioCreateDto dto)
    {
        var usuario =
            await _usuarioRepository
                .ObtenerDealerConPerfilPorIdAsync(
                    dto.UsuarioId
                );

        if (usuario == null)
        {
            throw new KeyNotFoundException(
                "El usuario especificado no existe."
            );
        }

        int cantidadAnuncios =
            await _repository.ContarAnunciosPorUsuarioAsync(
                dto.UsuarioId
            );

        bool esVendedorParticular =
            string.Equals(
                usuario.Rol,
                "Vendedor",
                StringComparison.OrdinalIgnoreCase
            );

        if (esVendedorParticular)
        {
            if (cantidadAnuncios >= 1)
            {
                throw new BusinessRuleException(
                    "Has alcanzado el límite de 1 anuncio gratuito. " +
                    "Mejora tu cuenta a Dealer para publicar más inventario."
                );
            }
        }
        else
        {
            var suscripcion =
                usuario.PerfilDealer?.Suscripcion;

            if (suscripcion == null)
            {
                throw new BusinessRuleException(
                    "Tu cuenta Dealer no tiene una suscripción activa configurada."
                );
            }

            if (
                suscripcion.FechaVencimientoUtc <=
                DateTime.UtcNow
            )
            {
                throw new BusinessRuleException(
                    "Tu suscripción Dealer está vencida. Renuevala para seguir publicando."
                );
            }

            if (
                !suscripcion.PermiteNuevosAnuncios(
                    cantidadAnuncios
                )
            )
            {
                throw new BusinessRuleException(
                    "Has alcanzado el límite de anuncios permitidos por tu plan."
                );
            }
        }

        var nuevoAnuncio = new Anuncio(
            usuarioId: dto.UsuarioId,
            marca: NormalizarBusqueda(dto.Marca),
            modelo: NormalizarBusqueda(dto.Modelo),
            version: NormalizarBusqueda(dto.Version),
            tipoVehiculo: NormalizarBusqueda(dto.TipoVehiculo),
            motor: NormalizarBusqueda(dto.Motor),
            traccion: NormalizarBusqueda(dto.Traccion),
            colorExterior: NormalizarBusqueda(dto.ColorExterior),
            colorInterior: NormalizarBusqueda(dto.ColorInterior),
            anio: dto.Anio,
            precio: dto.Precio,
            moneda: dto.Moneda,
            kilometraje: dto.Kilometraje,
            transmision: NormalizarBusqueda(dto.Transmision),
            combustible: NormalizarBusqueda(dto.Combustible),
            accesorios: dto.Accesorios,
            ubicacion: NormalizarBusqueda(dto.Ubicacion),
            descripcion: dto.Descripcion
        );

        if (dto.PrecioAnterior.HasValue)
        {
            nuevoAnuncio.FijarOferta(dto.PrecioAnterior);
        }

        await _repository.AgregarAsync(nuevoAnuncio);
        await _repository.GuardarCambiosAsync();

        return nuevoAnuncio.Id;
    }

    public async Task<AnuncioDto?> ObtenerAnuncioPorIdAsync(
    int id, int? usuarioId = null)
    {
        var anuncio =
            await _repository.ObtenerPorIdAsync(id);

        if (anuncio == null)
        {
            return null;
        }

        // Solo los anuncios publicados son visibles públicamente.
        // El resto (borradores, pausados, vendidos) solo los ve su dueño.
        if (anuncio.Estado != "Publicado")
        {
            if (usuarioId != anuncio.UsuarioId)
                return null;
        }

        var vendedor =
            await _usuarioRepository
                .ObtenerDealerConPerfilPorIdAsync(
                    anuncio.UsuarioId
                );

        string? nombreVendedor = null;
        string? whatsAppContacto = null;

        if (vendedor != null)
        {
            nombreVendedor =
                vendedor.PerfilDealer?.NombreAgencia ??
                $"{vendedor.Nombre} {vendedor.Apellido}".Trim();

            whatsAppContacto =
                vendedor.PerfilDealer?.WhatsApp ??
                vendedor.PerfilDealer?.TelefonoAgencia ??
                vendedor.TelefonoPersonal;
        }

        return new AnuncioDto
        {
            Id = anuncio.Id,
            UsuarioId = anuncio.UsuarioId,
            NombreAnuncio = anuncio.NombreAnuncio,

            Marca = anuncio.Marca,
            Modelo = anuncio.Modelo,
            Version = anuncio.Version,

            TipoVehiculo = anuncio.TipoVehiculo,
            Motor = anuncio.Motor,
            Traccion = anuncio.Traccion,

            ColorExterior = anuncio.ColorExterior,
            ColorInterior = anuncio.ColorInterior,

            Anio = anuncio.Anio,
            Precio = anuncio.Precio,
            Moneda = anuncio.Moneda,
            PrecioAnterior = anuncio.PrecioAnterior,
            Kilometraje = anuncio.Kilometraje,

            Condicion = anuncio.Condicion,
            EnOferta = anuncio.EnOferta,

            Transmision = anuncio.Transmision,
            Combustible = anuncio.Combustible,

            Accesorios = anuncio.Accesorios.ToList(),
            Ubicacion = anuncio.Ubicacion,
            Descripcion = anuncio.Descripcion,

            Estado = anuncio.Estado,
            Fotos = anuncio.Fotos.ToList(),

            EsDestacado = anuncio.EstaDestacadoVigente,
            FechaDestacadoHasta = anuncio.FechaDestacadoHasta,

            NombreVendedor = nombreVendedor,
            WhatsAppContacto = whatsAppContacto,

            EsVendedorParticular =
                vendedor != null &&
                string.Equals(vendedor.Rol, "Vendedor", StringComparison.OrdinalIgnoreCase)
        };
    }

    public async Task<
    IReadOnlyCollection<AnuncioListadoDto>
> ObtenerTodosLosAnuncios()
    {
        IEnumerable<Anuncio> entidades =
            await _repository.ObtenerTodosLosAnuncios();

        return entidades
            .Select(e => MapearListado(e, soloPrimeraFoto: true))
            .ToList();
    }

    public async Task<AnuncioUpdateDto?> ActualizarAsync(
    int id,
    int usuarioId,
    AnuncioUpdateDto updateAnuncio)
    {
        var anuncio =
            await _repository.ObtenerPorIdAsync(id);

        if (anuncio == null)
        {
            return null;
        }

        if (anuncio.UsuarioId != usuarioId)
        {
            throw new UnauthorizedAccessException(
                "Acceso denegado: No tienes permiso para modificar un anuncio que no te pertenece."
            );
        }

        anuncio.ActualizarInfo(
            marca: NormalizarBusqueda(updateAnuncio.Marca),
            modelo: NormalizarBusqueda(updateAnuncio.Modelo),
            version: NormalizarBusqueda(updateAnuncio.Version),
            tipoVehiculo: NormalizarBusqueda(updateAnuncio.TipoVehiculo),
            motor: NormalizarBusqueda(updateAnuncio.Motor),
            traccion: NormalizarBusqueda(updateAnuncio.Traccion),
            colorExterior: NormalizarBusqueda(updateAnuncio.ColorExterior),
            colorInterior: NormalizarBusqueda(updateAnuncio.ColorInterior),
            anio: updateAnuncio.Anio,
            precio: updateAnuncio.Precio,
            moneda: updateAnuncio.Moneda,
            kilometraje: updateAnuncio.Kilometraje,
            transmision: NormalizarBusqueda(updateAnuncio.Transmision),
            combustible: NormalizarBusqueda(updateAnuncio.Combustible),
            accesorios: updateAnuncio.Accesorios,
            ubicacion: NormalizarBusqueda(updateAnuncio.Ubicacion),
            descripcion: updateAnuncio.Descripcion,
            precioAnterior: updateAnuncio.PrecioAnterior
);

        await _repository.ActualizarAsync(anuncio);
        await _repository.GuardarCambiosAsync();

        return updateAnuncio;
    }

    public async Task<bool> PublicarAnuncioAsync(int id, int usuarioId)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(id);

        if (anuncio == null) return false;

        if (anuncio.UsuarioId != usuarioId)
        {
            throw new UnauthorizedAccessException("Acceso denegado: No tienes permiso para publicar un anuncio que no te pertenece.");
        }

        if (anuncio.Estado == "Publicado")
            return true;

        await ValidarCupoParaPublicarAsync(anuncio, usuarioId);

        anuncio.Publicar();

        await _repository.ActualizarAsync(anuncio);
        return true;
    }

    /// <summary>
    /// Valida cupo del plan, suscripción vigente y que el anuncio tenga el mínimo de
    /// fotos antes de publicarlo. El límite aplica a la vitrina activa (Publicado +
    /// Pausado); los borradores no ocupan cupo.
    /// </summary>
    private async Task ValidarCupoParaPublicarAsync(Anuncio anuncio, int usuarioId)
    {
        if (anuncio.Estado == "Publicado") return;

        // Solo cuentan los activos en vitrina; si este anuncio ya está publicado o
        // pausado (ya ocupa cupo), se descuenta para no ocupar doble cupo.
        int cantidadActiva = await _repository.ContarAnunciosPorUsuarioAsync(usuarioId);
        if (anuncio.Estado == "Publicado" || anuncio.Estado == "Pausado")
            cantidadActiva--;

        var usuario = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(usuarioId);

        bool esVendedorParticular =
            usuario != null &&
            string.Equals(usuario.Rol, "Vendedor", StringComparison.OrdinalIgnoreCase);

        if (esVendedorParticular)
        {
            if (cantidadActiva >= 1)
            {
                throw new BusinessRuleException(
                    "Has alcanzado el límite de 1 anuncio gratuito. " +
                    "Mejora tu cuenta a Dealer para publicar más inventario."
                );
            }

            return;
        }

        var suscripcion = usuario?.PerfilDealer?.Suscripcion;

        if (suscripcion == null)
        {
            throw new BusinessRuleException(
                "Tu cuenta Dealer no tiene una suscripción activa configurada."
            );
        }

        if (suscripcion.FechaVencimientoUtc <= DateTime.UtcNow)
        {
            throw new BusinessRuleException(
                "Tu suscripción Dealer está vencida. Renuevala para seguir publicando."
            );
        }

        if (!suscripcion.PermiteNuevosAnuncios(cantidadActiva))
        {
            throw new BusinessRuleException(
                "Has alcanzado el límite de anuncios permitidos por tu plan."
            );
        }
    }

    public async Task<List<string>> SubirImagenesAsync(AnuncioImagenUploadDto dto)
    {
        var _anuncio = await _repository.ObtenerPorIdAsync(dto.AnuncioId);
        if (_anuncio == null) throw new KeyNotFoundException("El anuncio no existe");

        if (_anuncio.UsuarioId != dto.UsuarioId)
        {
            throw new UnauthorizedAccessException("Acceso denegado: No tienes permiso para subir fotos a este anuncio.");
        }

        var rutasGuardadas = new List<string>();

         foreach (var imagen in dto.Imagenes)
         {
             // Valida que el archivo no exceda los 5 MB y que sea un tipo de imagen permitido (PNG o JPEG).
             // Lanza excepcion si la validacion falla para ser tratada en el controlador como BadRequest.
             if (imagen.Length > 5 * 1024 * 1024)
                 throw new ArgumentException("Imagen excede el tamaño máximo");

             if (imagen.ContentType != "image/png" && imagen.ContentType != "image/jpeg")
                 throw new ArgumentException("Formato no permitido");

            var extension = Path.GetExtension(imagen.FileName);
            var nombreUnico = $"{Guid.NewGuid()}{extension}";

            using (var stream = imagen.OpenReadStream())
            {
                var claveS3 = await _almacenadorArchivos.GuardarArchivoAsync(stream, nombreUnico, imagen.ContentType);
                rutasGuardadas.Add(claveS3);
            }
        }

        _anuncio.AgregarFotos(rutasGuardadas);

        await _repository.ActualizarAsync(_anuncio);

        return rutasGuardadas;
    }

    public async Task<bool> EstablecerFotoPrincipalAsync(int id, int usuarioId, string urlImagen)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(id);

        if (anuncio == null) return false;

        if (anuncio.UsuarioId != usuarioId)
        {
            throw new UnauthorizedAccessException("Acceso denegado: No tienes permiso para modificar este anuncio.");
        }

        anuncio.MoverFotoAlInicio(urlImagen);

        await _repository.ActualizarAsync(anuncio);

        return true;
    }

/// <summary>
/// EliminarImagenAsync Eliminar imagen async. Parámetros: Parámetro anuncioId (int), Parámetro usuarioId (int), Parámetro urlImagen (string). Retorna: Task.
/// </summary>
    public async Task EliminarImagenAsync(int anuncioId, int usuarioId, string urlImagen)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(anuncioId);

        if (anuncio == null)
            throw new KeyNotFoundException("El anuncio no existe.");

        if (anuncio.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException("Acceso denegado: No tienes permiso para modificar las fotos de este anuncio.");

        // 1. Eliminar la referencia en la base de datos
        // Asegúrate de tener este método RemoverFoto creado en tu entidad Anuncio (Core/Entities)
        anuncio.EliminarFoto(urlImagen);

        await _repository.ActualizarAsync(anuncio);
        await _repository.GuardarCambiosAsync();

        // 2. Destrucción física en AWS S3 usando el método que ya tenías
        await _almacenadorArchivos.EliminarArchivoAsync(urlImagen);
    }

    public async Task<
    PagedResult<AnuncioListadoDto>
> BuscarAnunciosAsync(
    AnuncioSearchDto dto)
    {
        var filtro = new AnuncioQueryFilter
        {
            UsuarioId = dto.UsuarioId,
            VendedorId = dto.VendedorId,

            Marca = dto.Marca,
            Modelo = dto.Modelo,
            Version = dto.Version,

            TipoVehiculo = dto.TipoVehiculo,
            Motor = dto.Motor,
            Traccion = dto.Traccion,

            ColorExterior = dto.ColorExterior,
            ColorInterior = dto.ColorInterior,

            Transmision = dto.Transmision,
            Combustible = dto.Combustible,
            Ubicacion = dto.Ubicacion,

            Condicion = dto.Condicion,
            EnOferta = dto.EnOferta,

            AnioDesde = dto.AnioDesde,
            AnioHasta = dto.AnioHasta,

            PrecioMinimo = dto.PrecioMinimo,
            PrecioMaximo = dto.PrecioMaximo,

            Moneda = dto.Moneda,

            KilometrajeMaximo = dto.KilometrajeMaximo,
            ExcluirDestacadosVigentes = dto.ExcluirDestacadosVigentes,

            PaginaActual = dto.PaginaActual,
            CantidadPorPagina = dto.CantidadAnuncios
        };

        var (
            anuncios,
            totalRegistros
        ) = await _repository.BuscarPaginadoAsync(filtro);

var anunciosDto = anuncios
            .Select(a => MapearListado(a, soloPrimeraFoto: false))
            .ToList();

        return new PagedResult<AnuncioListadoDto>(
            items: anunciosDto,
            totalRegistros: totalRegistros,
            paginaActual: dto.PaginaActual,
            cantidadPorPagina: dto.CantidadAnuncios
        );
    }

    public async Task<bool> CambiarEstadoAsync(int id, int usuarioId, string estado)
    {
        if (string.IsNullOrWhiteSpace(estado) || !EstadosValidos.Contains(estado))
        {
            throw new BusinessRuleException(
                $"El estado '{estado}' no es válido. Estados permitidos: Publicado, Borrador, Pausado, Vendido."
            );
        }

        var anuncio = await _repository.ObtenerPorIdAsync(id);

        if (anuncio == null) return false;

        if (anuncio.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException("Acceso denegado: No tienes permiso para cambiar el estado de este anuncio.");

        if (anuncio.Estado == estado)
            return true;

        // Pasar a Publicado aplica las mismas reglas que publicar: cupo del plan,
        // suscripción vigente y mínimo de 5 fotos.
        if (string.Equals(estado, "Publicado", StringComparison.OrdinalIgnoreCase))
        {
            await ValidarCupoParaPublicarAsync(anuncio, usuarioId);
            anuncio.Publicar();
        }
        else
        {
            anuncio.CambiarEstado(estado);
        }

        await _repository.ActualizarAsync(anuncio);
        return true;
    }

/// <summary>
/// RegistrarVistaAsync Registrar vista async. Parámetros: Parámetro anuncioId (int). Retorna: Task.
/// </summary>
    public async Task RegistrarVistaAsync(int anuncioId)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(anuncioId);

        if (anuncio is null)
        {
            throw new KeyNotFoundException("El anuncio no está disponible.");
        }

        // Solo los anuncios publicados cuentan vistas; los borradores, pausados o
        // vendidos no deben inflar estadísticas.
        if (anuncio.Estado != "Publicado")
            return;

        anuncio.RegistrarVista();
        await _repository.GuardarCambiosAsync();
    }

    public async Task<bool> EliminarAnuncioAsync(int id, int usuarioId)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(id);

        if (anuncio == null) return false;

        if (anuncio.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException("Acceso denegado: No tienes permiso para eliminar un anuncio que no te pertenece.");

        // Destrucción física de las fotos en AWS S3
        foreach (var foto in anuncio.Fotos)
        {
            try { await _almacenadorArchivos.EliminarArchivoAsync(foto); }
            catch { /* No bloqueamos el borrado si S3 falla */ }
        }

        _repository.Eliminar(anuncio);
        await _repository.GuardarCambiosAsync();

        return true;
    }

    public async Task<bool> MarcarComoDestacadoAsync(int id, int usuarioId)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(id);

        if (anuncio == null)
            throw new KeyNotFoundException("Anuncio no encontrado.");

        if (anuncio.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException("No tienes permiso para modificar este anuncio.");

        if (anuncio.Estado != "Publicado")
            throw new BusinessRuleException("Solo se pueden destacar anuncios publicados.");

        var usuario = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(usuarioId);
        var suscripcion = usuario?.PerfilDealer?.Suscripcion;
        var plan = usuario?.PerfilDealer?.Suscripcion?.Plan
                   ?? await _planCatalogoRepository.ObtenerPorNivelAsync(suscripcion?.Nivel ?? PlanNivel.Gratis);

        if (suscripcion == null || plan == null)
            throw new BusinessRuleException("Tu cuenta no tiene un plan de suscripción activo.");

        if (suscripcion.Estado != Core.Entities.Enums.EstadoSuscripcion.Activa)
            throw new BusinessRuleException("Tu suscripción no está activa.");

        if (suscripcion.FechaVencimientoUtc <= DateTime.UtcNow)
            throw new BusinessRuleException("Tu suscripción ha vencido.");

        // Contar anuncios destacados actuales del usuario
        int destacadosActuales = await _repository.ContarDestacadosPorUsuarioAsync(usuarioId);

        // Si este anuncio ya está destacado, no debe consumir cupo adicional al renovarlo
        if (anuncio.EstaDestacadoVigente)
            destacadosActuales--;

        if (!suscripcion.PermiteDestacarMas(destacadosActuales, plan))
            throw new BusinessRuleException($"Has alcanzado el límite de anuncios destacados de tu plan ({plan.CuotaDestacados}).");

        // Destacar por 30 días (o hasta fin de suscripción)
        var hasta = DateTime.UtcNow.AddDays(30);
        if (hasta > suscripcion.FechaVencimientoUtc)
            hasta = suscripcion.FechaVencimientoUtc;

        anuncio.MarcarComoDestacado(hasta);
        await _repository.ActualizarAsync(anuncio);
        await _repository.GuardarCambiosAsync();

        return true;
    }

    public async Task<bool> QuitarDestacadoAsync(int id, int usuarioId)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(id);

        if (anuncio == null)
            throw new KeyNotFoundException("Anuncio no encontrado.");

        if (anuncio.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException("No tienes permiso para modificar este anuncio.");

        if (!anuncio.EsDestacado)
            throw new BusinessRuleException("El anuncio no está destacado.");

        anuncio.QuitarDestacado();
        await _repository.ActualizarAsync(anuncio);
        await _repository.GuardarCambiosAsync();

        return true;
    }

    public async Task<PagedResult<AnuncioListadoDto>> ObtenerDestacadosAsync(int pagina, int tamanoPagina)
    {
        if (pagina < 1) pagina = 1;
        if (tamanoPagina < 1 || tamanoPagina > 50) tamanoPagina = 20;

        var (anuncios, total) = await _repository.ObtenerDestacadosPaginadosAsync(pagina, tamanoPagina);

        var items = anuncios.Select(a => MapearListado(a, soloPrimeraFoto: true)).ToList();

        return new PagedResult<AnuncioListadoDto>(items, total, pagina, tamanoPagina);
    }

    private static AnuncioListadoDto MapearListado(Anuncio anuncio, bool soloPrimeraFoto)
    {
        var fotos = soloPrimeraFoto
            ? anuncio.Fotos.Take(1).ToList()
            : anuncio.Fotos != null && anuncio.Fotos.Any()
                ? anuncio.Fotos.ToList()
                : new List<string> { "url_imagen_por_defecto.jpg" };

        return new AnuncioListadoDto
        {
            Id = anuncio.Id,
            UsuarioId = anuncio.UsuarioId,
            NombreAnuncio = anuncio.NombreAnuncio,
            Marca = anuncio.Marca,
            Modelo = anuncio.Modelo,
            Version = anuncio.Version,
            TipoVehiculo = anuncio.TipoVehiculo,
            Motor = anuncio.Motor,
            Traccion = anuncio.Traccion,
            ColorExterior = anuncio.ColorExterior,
            ColorInterior = anuncio.ColorInterior,
            Anio = anuncio.Anio,
            Precio = anuncio.Precio,
            Moneda = anuncio.Moneda,
            PrecioAnterior = anuncio.PrecioAnterior,
            Kilometraje = anuncio.Kilometraje,

            Condicion = anuncio.Condicion,
            EnOferta = anuncio.EnOferta,

            Transmision = anuncio.Transmision,
            Combustible = anuncio.Combustible,
            Ubicacion = anuncio.Ubicacion,
            Estado = anuncio.Estado,
            Vistas = anuncio.Vistas,
            Fotos = fotos,
            BadgeSuscripcion = anuncio.Usuario?.PerfilDealer?.Suscripcion?.Nivel.ToString() ?? "Gratis",
            EsDestacado = anuncio.EstaDestacadoVigente,
            FechaDestacadoHasta = anuncio.FechaDestacadoHasta,
            CreatedAt = anuncio.CreatedAt
        };
    }
}
