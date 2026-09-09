using AutoMarket.Application.DTOs;
using AutoMarket.Application.Helpers;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Core.Exceptions;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

/// <summary>
/// Servicio para manejar Anuncio. Delega validación de planes a
/// <see cref="AnuncioPlanValidator"/> y mapeo a <see cref="AnuncioMapper"/>.
/// </summary>
public class AnuncioService : IAnuncioService
{
    private readonly IAnuncioRepository _repository;
    private readonly IAlmacenadorArchivos _almacenadorArchivos;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly AnuncioPlanValidator _planValidator;

    private static readonly HashSet<string> EstadosValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "Publicado", "Borrador", "Pausado", "Vendido"
    };

    public AnuncioService(
        IAnuncioRepository repository,
        IAlmacenadorArchivos almacenadorArchivos,
        IUsuarioRepository usuarioRepository,
        IPlanCatalogoRepository planCatalogoRepository)
    {
        _repository = repository;
        _almacenadorArchivos = almacenadorArchivos;
        _usuarioRepository = usuarioRepository;
        _planValidator = new AnuncioPlanValidator(usuarioRepository, planCatalogoRepository);
    }

    // ==========================================
    // QUERIES (Lectura)
    // ==========================================

    public async Task<AnuncioDto?> ObtenerAnuncioPorIdAsync(int id, int? usuarioId = null)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(id);

        if (anuncio == null)
            return null;

        bool estaVencido = anuncio.EstaVencido || anuncio.EstaVencidoGratis;
        if (anuncio.Estado != "Publicado" || estaVencido)
        {
            if (usuarioId != anuncio.UsuarioId)
                return null;
        }

        var vendedor = await _usuarioRepository
            .ObtenerDealerConPerfilPorIdAsync(anuncio.UsuarioId);

        string? nombreVendedor = null;
        string? whatsAppContacto = null;
        bool esDealerVerificado = false;

        if (vendedor != null)
        {
            nombreVendedor = vendedor.PerfilDealer?.NombreAgencia ??
                $"{vendedor.Nombre} {vendedor.Apellido}".Trim();
            whatsAppContacto = vendedor.PerfilDealer?.WhatsApp ??
                vendedor.PerfilDealer?.TelefonoAgencia ??
                vendedor.TelefonoPersonal;
            esDealerVerificado = AnuncioPlanValidator.EsDealerVerificado(vendedor);
        }

        return AnuncioMapper.ToDto(anuncio, nombreVendedor, whatsAppContacto, esDealerVerificado, vendedor);
    }

    public async Task<IReadOnlyCollection<AnuncioListadoDto>> ObtenerTodosLosAnuncios()
    {
        var entidades = await _repository.ObtenerTodosLosAnuncios();
        return entidades
            .Select(e => AnuncioMapper.ToListadoDto(e, soloPrimeraFoto: true))
            .ToList();
    }

    public async Task<PagedResult<AnuncioListadoDto>> BuscarAnunciosAsync(AnuncioSearchDto dto)
    {
        var filtro = new AnuncioQueryFilter
        {
            UsuarioId = dto.UsuarioId,
            VendedorId = dto.VendedorId,
            Marca = dto.Marca,
            Modelo = dto.Modelo,
            Version = dto.Version,
            Busqueda = dto.Busqueda,
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

        var (anuncios, totalRegistros) = await _repository.BuscarPaginadoAsync(filtro);

        var anunciosDto = anuncios
            .Select(a => AnuncioMapper.ToListadoDto(a, soloPrimeraFoto: false))
            .ToList();

        return new PagedResult<AnuncioListadoDto>(
            anunciosDto, totalRegistros, dto.PaginaActual, dto.CantidadAnuncios);
    }

    public async Task<PagedResult<AnuncioListadoDto>> ObtenerDestacadosAsync(int pagina, int tamanoPagina)
    {
        if (pagina < 1) pagina = 1;
        if (tamanoPagina < 1 || tamanoPagina > 50) tamanoPagina = 20;

        var (anuncios, total) = await _repository.ObtenerDestacadosPaginadosAsync(pagina, tamanoPagina);
        var items = anuncios.Select(a => AnuncioMapper.ToListadoDto(a, soloPrimeraFoto: true)).ToList();

        return new PagedResult<AnuncioListadoDto>(items, total, pagina, tamanoPagina);
    }

    // ==========================================
    // COMMANDS (Escritura)
    // ==========================================

    public async Task<int> CrearAnuncioAsync(AnuncioCreateDto dto)
    {
        int cantidadAnuncios = await _repository.ContarAnunciosPorUsuarioAsync(dto.UsuarioId);
        await _planValidator.ValidarCupoAsync(dto.UsuarioId, cantidadAnuncios, "Borrador");

        var nuevoAnuncio = AnuncioMapper.ToEntity(dto);

        if (dto.PrecioAnterior.HasValue)
            nuevoAnuncio.FijarOferta(dto.PrecioAnterior);

        await _repository.AgregarAsync(nuevoAnuncio);
        await _repository.GuardarCambiosAsync();

        return nuevoAnuncio.Id;
    }

    public async Task<AnuncioUpdateDto?> ActualizarAsync(int id, int usuarioId, AnuncioUpdateDto updateAnuncio)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(id);

        if (anuncio == null)
            return null;

        if (anuncio.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException(
                "Acceso denegado: No tienes permiso para modificar un anuncio que no te pertenece.");

        anuncio.ActualizarInfo(
            marca: NormalizadorTexto.Normalizar(updateAnuncio.Marca),
            modelo: NormalizadorTexto.Normalizar(updateAnuncio.Modelo),
            version: NormalizadorTexto.Normalizar(updateAnuncio.Version),
            tipoVehiculo: NormalizadorTexto.Normalizar(updateAnuncio.TipoVehiculo),
            motor: NormalizadorTexto.Normalizar(updateAnuncio.Motor),
            traccion: NormalizadorTexto.Normalizar(updateAnuncio.Traccion),
            colorExterior: NormalizadorTexto.Normalizar(updateAnuncio.ColorExterior),
            colorInterior: NormalizadorTexto.Normalizar(updateAnuncio.ColorInterior),
            anio: updateAnuncio.Anio,
            precio: updateAnuncio.Precio,
            moneda: updateAnuncio.Moneda,
            kilometraje: updateAnuncio.Kilometraje,
            transmision: NormalizadorTexto.Normalizar(updateAnuncio.Transmision),
            combustible: NormalizadorTexto.Normalizar(updateAnuncio.Combustible),
            accesorios: updateAnuncio.Accesorios,
            ubicacion: NormalizadorTexto.Normalizar(updateAnuncio.Ubicacion),
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
            throw new UnauthorizedAccessException(
                "Acceso denegado: No tienes permiso para publicar un anuncio que no te pertenece.");

        if (anuncio.Estado == "Publicado" && !anuncio.EstaVencido && !anuncio.EstaVencidoGratis)
            return true;

        int cantidadActiva = await _repository.ContarAnunciosPorUsuarioAsync(usuarioId);
        if (anuncio.Estado == "Publicado" || anuncio.Estado == "Pausado")
            cantidadActiva--;

        var (diasVigencia, esGratis) = await _planValidator.ValidarCupoAsync(usuarioId, cantidadActiva, anuncio.Estado);

        if (esGratis)
            anuncio.PublicarGratis();
        else
            anuncio.Publicar(diasVigencia);

        await _repository.ActualizarAsync(anuncio);
        return true;
    }

    public async Task<bool> CambiarEstadoAsync(int id, int usuarioId, string estado)
    {
        if (string.IsNullOrWhiteSpace(estado) || !EstadosValidos.Contains(estado))
            throw new BusinessRuleException(
                $"El estado '{estado}' no es válido. Estados permitidos: Publicado, Borrador, Pausado, Vendido.");

        var anuncio = await _repository.ObtenerPorIdAsync(id);

        if (anuncio == null) return false;

        if (anuncio.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException(
                "Acceso denegado: No tienes permiso para cambiar el estado de este anuncio.");

        if (anuncio.Estado == estado)
            return true;

        if (string.Equals(estado, "Publicado", StringComparison.OrdinalIgnoreCase))
        {
            int cantidadActiva = await _repository.ContarAnunciosPorUsuarioAsync(usuarioId);
            if (anuncio.Estado == "Publicado" || anuncio.Estado == "Pausado")
                cantidadActiva--;

            var (diasVigencia, esGratis) = await _planValidator.ValidarCupoAsync(usuarioId, cantidadActiva, anuncio.Estado);
            if (esGratis)
                anuncio.PublicarGratis();
            else
                anuncio.Publicar(diasVigencia);
        }
        else
        {
            anuncio.CambiarEstado(estado);
        }

        await _repository.ActualizarAsync(anuncio);
        return true;
    }

    public async Task<bool> EliminarAnuncioAsync(int id, int usuarioId)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(id);

        if (anuncio == null) return false;

        if (anuncio.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException(
                "Acceso denegado: No tienes permiso para eliminar un anuncio que no te pertenece.");

        foreach (var foto in anuncio.Fotos)
        {
            try { await _almacenadorArchivos.EliminarArchivoAsync(foto); }
            catch { /* No bloqueamos el borrado si S3 falla */ }
        }

        _repository.Eliminar(anuncio);
        await _repository.GuardarCambiosAsync();

        return true;
    }

    public async Task<bool> RenovarAnuncioGratisAsync(int id, int usuarioId)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(id);

        if (anuncio == null)
            throw new KeyNotFoundException("Anuncio no encontrado.");

        if (anuncio.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException("No tienes permiso para renovar este anuncio.");

        if (!anuncio.EstaVencidoGratis)
            throw new BusinessRuleException("El anuncio no está vencido o no es un anuncio del plan gratis.");

        anuncio.RenovarVigenciaGratis();
        await _repository.ActualizarAsync(anuncio);
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

        int destacadosActuales = await _repository.ContarDestacadosPorUsuarioAsync(usuarioId);
        await _planValidator.ValidarDestacadosAsync(usuarioId, destacadosActuales, anuncio.EstaDestacadoVigente);

        var usuario = await _usuarioRepository.ObtenerDealerConPerfilPorIdAsync(usuarioId);
        var hasta = DateTime.UtcNow.AddDays(30);
        if (hasta > usuario!.PerfilDealer!.Suscripcion!.FechaVencimientoUtc)
            hasta = usuario.PerfilDealer.Suscripcion.FechaVencimientoUtc;

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

    // ==========================================
    // FOTOS (Commands)
    // ==========================================

    public async Task<List<string>> SubirImagenesAsync(AnuncioImagenUploadDto dto)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(dto.AnuncioId);
        if (anuncio == null) throw new KeyNotFoundException("El anuncio no existe");

        if (anuncio.UsuarioId != dto.UsuarioId)
            throw new UnauthorizedAccessException(
                "Acceso denegado: No tienes permiso para subir fotos a este anuncio.");

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
                var claveS3 = await _almacenadorArchivos.GuardarArchivoAsync(stream, nombreUnico, imagen.ContentType);
                rutasGuardadas.Add(claveS3);
            }
        }

        int maxFotos = await _planValidator.ObtenerMaxFotosAsync(dto.UsuarioId);
        anuncio.AgregarFotos(rutasGuardadas, maxFotos);

        await _repository.ActualizarAsync(anuncio);

        return rutasGuardadas;
    }

    public async Task<bool> EstablecerFotoPrincipalAsync(int id, int usuarioId, string urlImagen)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(id);

        if (anuncio == null) return false;

        if (anuncio.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException(
                "Acceso denegado: No tienes permiso para modificar este anuncio.");

        anuncio.MoverFotoAlInicio(urlImagen);
        await _repository.ActualizarAsync(anuncio);

        return true;
    }

    public async Task EliminarImagenAsync(int anuncioId, int usuarioId, string urlImagen)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(anuncioId);

        if (anuncio == null)
            throw new KeyNotFoundException("El anuncio no existe.");

        if (anuncio.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException(
                "Acceso denegado: No tienes permiso para modificar las fotos de este anuncio.");

        anuncio.EliminarFoto(urlImagen);
        await _repository.ActualizarAsync(anuncio);
        await _repository.GuardarCambiosAsync();

        await _almacenadorArchivos.EliminarArchivoAsync(urlImagen);
    }

    public async Task RegistrarVistaAsync(int anuncioId)
    {
        var anuncio = await _repository.ObtenerPorIdAsync(anuncioId);

        if (anuncio is null)
            throw new KeyNotFoundException("El anuncio no está disponible.");

        if (anuncio.Estado != "Publicado")
            return;

        anuncio.RegistrarVista();
        await _repository.GuardarCambiosAsync();
    }
}
