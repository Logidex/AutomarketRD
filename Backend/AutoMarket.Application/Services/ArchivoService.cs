using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

public class ArchivoService : IArchivoService
{
    private readonly IAlmacenadorArchivos _almacenadorArchivos;
    private readonly IAnuncioRepository _anuncioRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISuscripcionRepository _suscripcionRepository;

    public ArchivoService(
        IAlmacenadorArchivos almacenadorArchivos,
        IAnuncioRepository anuncioRepository,
        IUsuarioRepository usuarioRepository,
        ISuscripcionRepository suscripcionRepository)
    {
        _almacenadorArchivos = almacenadorArchivos;
        _anuncioRepository = anuncioRepository;
        _usuarioRepository = usuarioRepository;
        _suscripcionRepository = suscripcionRepository;
    }

    public async Task<string?> ObtenerUrlFirmadaSiExisteAsync(string clave)
    {
        var esFotoDeAnuncio = await _anuncioRepository.ExisteFotoAsync(clave);
        var esLogoDeDealer = !esFotoDeAnuncio &&
                             await _usuarioRepository.ExisteLogoDealerAsync(clave);
        var esCapturaTransferencia = !esFotoDeAnuncio && !esLogoDeDealer &&
                                     await _suscripcionRepository.ExisteCapturaTransferenciaAsync(clave);

        if (!esFotoDeAnuncio && !esLogoDeDealer && !esCapturaTransferencia)
            return null;

        return await _almacenadorArchivos.GenerarUrlFirmadaAsync(clave);
    }
}
