using AutoMarket.Application.DTOs;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

public class AnuncioService : IAnuncioService
{
    private readonly IAnuncioRepository _repository;
    private readonly IAlmacenadorArchivos _almacenadorArchivos;
    private readonly IUsuarioRepository _usuarioRepository;

    public AnuncioService(
        IAnuncioRepository repository,
        IAlmacenadorArchivos almacenadorArchivos,
        IUsuarioRepository usuarioRepository)
    {
        _repository = repository;
        _almacenadorArchivos = almacenadorArchivos;
        _usuarioRepository = usuarioRepository;
    }

    private static readonly HashSet<string> EstadosValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "Publicado", "Borrador", "Pausado", "Vendido"
    };

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
            marca: dto.Marca,
            modelo: dto.Modelo,
            version: dto.Version,
            tipoVehiculo: dto.TipoVehiculo,
            motor: dto.Motor,
            traccion: dto.Traccion,
            colorExterior: dto.ColorExterior,
            colorInterior: dto.ColorInterior,
            anio: dto.Anio,
            precio: dto.Precio,
            kilometraje: dto.Kilometraje,
            transmision: dto.Transmision,
            combustible: dto.Combustible,
            accesorios: dto.Accesorios,
            ubicacion: dto.Ubicacion,
            descripcion: dto.Descripcion
        );

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
            Kilometraje = anuncio.Kilometraje,

            Transmision = anuncio.Transmision,
            Combustible = anuncio.Combustible,

            Accesorios = anuncio.Accesorios.ToList(),
            Ubicacion = anuncio.Ubicacion,
            Descripcion = anuncio.Descripcion,

            Estado = anuncio.Estado,
            Fotos = anuncio.Fotos.ToList()
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
            marca: updateAnuncio.Marca,
            modelo: updateAnuncio.Modelo,
            version: updateAnuncio.Version,
            tipoVehiculo: updateAnuncio.TipoVehiculo,
            motor: updateAnuncio.Motor,
            traccion: updateAnuncio.Traccion,
            colorExterior: updateAnuncio.ColorExterior,
            colorInterior: updateAnuncio.ColorInterior,
            anio: updateAnuncio.Anio,
            precio: updateAnuncio.Precio,
            kilometraje: updateAnuncio.Kilometraje,
            transmision: updateAnuncio.Transmision,
            combustible: updateAnuncio.Combustible,
            accesorios: updateAnuncio.Accesorios,
            ubicacion: updateAnuncio.Ubicacion,
            descripcion: updateAnuncio.Descripcion
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

    public async Task SubirImagenesAsync(AnuncioImagenUploadDto dto)
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
            if (imagen.Length > 5 * 1024 * 1024)
                throw new ArgumentException("Imagen excede el tamaño máximo");

            if (imagen.ContentType != "image/png" && imagen.ContentType != "image/jpeg")
                throw new ArgumentException("Formato no permitido");

            var extension = Path.GetExtension(imagen.FileName);
            var nombreUnico = $"{Guid.NewGuid()}{extension}";

            using (var stream = imagen.OpenReadStream())
            {
                var urlPublicaAws = await _almacenadorArchivos.GuardarArchivoAsync(stream, nombreUnico, imagen.ContentType);
                rutasGuardadas.Add(urlPublicaAws);
            }
        }

        _anuncio.AgregarFotos(rutasGuardadas);

        await _repository.ActualizarAsync(_anuncio);
    }

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

            AnioDesde = dto.AnioDesde,
            AnioHasta = dto.AnioHasta,

            PrecioMinimo = dto.PrecioMinimo,
            PrecioMaximo = dto.PrecioMaximo,

            KilometrajeMaximo = dto.KilometrajeMaximo,

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
            Kilometraje = anuncio.Kilometraje,
            Transmision = anuncio.Transmision,
            Combustible = anuncio.Combustible,
            Ubicacion = anuncio.Ubicacion,
            Estado = anuncio.Estado,
            Vistas = anuncio.Vistas,
            Fotos = fotos,
            BadgeSuscripcion = anuncio.Usuario?.PerfilDealer?.Suscripcion?.Nivel.ToString() ?? "Gratis"
        };
    }
}