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

    public async Task CrearAnuncioAsync(AnuncioCreateDto dto)
    {
        var usuario = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(dto.UsuarioId);

        if (usuario == null)
            throw new KeyNotFoundException("El usuario especificado no existe.");

        int cantidadAnuncios = await _repository.ContarAnunciosPorUsuarioAsync(dto.UsuarioId);

        bool esVendedorParticular = usuario.PerfilDealer == null;

        if (esVendedorParticular)
        {
            if (cantidadAnuncios >= 1)
            {
                throw new BusinessRuleException(
                    "Has alcanzado el límite de 1 anuncio gratuito. Mejora tu cuenta a Dealer para publicar más inventario.");
            }
        }
        else
        {
            var suscripcion = usuario.PerfilDealer?.Suscripcion;

            if (suscripcion == null)
            {
                throw new BusinessRuleException(
                    "Tu cuenta Dealer no tiene una suscripción activa configurada. Debes activar un plan para publicar más inventario.");
            }

            if (suscripcion.Estado != EstadoSuscripcion.Activa)
            {
                throw new BusinessRuleException(
                    "Tu suscripción Dealer no está activa. Debes renovarla para publicar anuncios.");
            }

            if (!suscripcion.PermiteNuevosAnuncios(cantidadAnuncios))
            {
                throw new BusinessRuleException(
                    "Has alcanzado el límite de anuncios permitidos por tu plan actual. Cambia de plan o renueva tu suscripción.");
            }
        }

        var nuevoAnuncio = new Anuncio(
            usuarioId: dto.UsuarioId,
            marca: dto.Marca,
            modelo: dto.Modelo,
            tipoVehiculo: dto.TipoVehiculo,
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
    }

    public async Task<AnuncioDto?> ObtenerAnuncioPorIdAsync(int id)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(id);

        if (anuncio == null) return null;

        return new AnuncioDto
        {
            Id = anuncio.Id,
            UsuarioId = anuncio.UsuarioId,
            NombreAnuncio = anuncio.NombreAnuncio,
            Marca = anuncio.Marca,
            Modelo = anuncio.Modelo,
            TipoVehiculo = anuncio.TipoVehiculo,
            ColorExterior = anuncio.ColorExterior,
            ColorInterior = anuncio.ColorInterior,
            Anio = anuncio.Anio,
            Precio = anuncio.Precio,
            Kilometraje = anuncio.Kilometraje,
            Transmision = anuncio.Transmision,
            Combustible = anuncio.Combustible,
            Accesorios = anuncio.Accesorios,
            Ubicacion = anuncio.Ubicacion,
            Descripcion = anuncio.Descripcion,
            Estado = anuncio.Estado,
            Fotos = anuncio.Fotos.ToList()
        };
    }

    public async Task<IReadOnlyCollection<AnuncioListadoDto>> ObtenerTodosLosAnuncios()
    {
        IEnumerable<Anuncio> entidades = await _repository.ObtenerTodosLosAnuncios();

        return entidades.Select(e => new AnuncioListadoDto
        {
            Id = e.Id,
            NombreAnuncio = e.NombreAnuncio,
            Precio = e.Precio,
            Ubicacion = e.Ubicacion,
            Estado = e.Estado,
            // Enviamos solo la primera foto para la miniatura de la tarjeta
            Fotos = e.Fotos.Take(1).ToList(),
            BadgeSuscripcion = e.Usuario?.PerfilDealer?.Suscripcion?.Nivel.ToString() ?? "Gratis"
        }).ToList();

    }

    public async Task<AnuncioUpdateDto?> ActualizarAsync(int id, int usuarioId, AnuncioUpdateDto updateAnuncio)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(id);

        if (anuncio == null) return null;

        if (anuncio.UsuarioId != usuarioId)
        {
            throw new UnauthorizedAccessException("Acceso denegado: No tienes permiso para modificar un anuncio que no te pertenece.");
        }

        anuncio.ActualizarInfo(
            updateAnuncio.Marca,
            updateAnuncio.Modelo,
            updateAnuncio.TipoVehiculo,
            updateAnuncio.ColorExterior,
            updateAnuncio.ColorInterior,
            updateAnuncio.Anio,
            updateAnuncio.Precio,
            updateAnuncio.Kilometraje,
            updateAnuncio.Transmision,
            updateAnuncio.Combustible,
            updateAnuncio.Accesorios,
            updateAnuncio.Ubicacion,
            updateAnuncio.Descripcion,
            updateAnuncio.PublicarAlGuardar
        );

        await _repository.ActualizarAsync(anuncio);
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

        anuncio.Publicar();

        await _repository.ActualizarAsync(anuncio);
        return true;
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

    public async Task<PagedResult<AnuncioListadoDto>> BuscarAnunciosAsync(AnuncioSearchDto dto)
    {

        var filtro = new AnuncioQueryFilter
        {
            UsuarioId = dto.UsuarioId,
            Marca = dto.Marca,
            Modelo = dto.Modelo,
            TipoVehiculo = dto.TipoVehiculo,
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

        var (anuncios, totalRegistros) = await _repository.BuscarPaginadoAsync(filtro);

        var anunciosDto = anuncios.Select(a => new AnuncioListadoDto
        {
            Id = a.Id,
            Precio = a.Precio,
            Ubicacion = a.Ubicacion,
            // Tomamos solo la primera foto para la miniatura de la tarjeta, si hay alguna
            Fotos = a.Fotos != null && a.Fotos.Any() ? a.Fotos.ToList() : new List<string> { "url_imagen_por_defecto.jpg" },
            BadgeSuscripcion = a.Usuario?.PerfilDealer?.Suscripcion?.Nivel.ToString() ?? "Gratis"
        }).ToList();

        return new PagedResult<AnuncioListadoDto>(
            items: anunciosDto,
            totalRegistros: totalRegistros,
            paginaActual: dto.PaginaActual,
            cantidadPorPagina: dto.CantidadAnuncios
        );
    }
}