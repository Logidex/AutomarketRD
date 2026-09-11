using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

public class ArchivoService : IArchivoService
{
    private readonly IAlmacenadorArchivos _almacenadorArchivos;
    private readonly IAnuncioRepository _anuncioRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISuscripcionRepository _suscripcionRepository;
    private readonly IAdSlotRepository _adSlotRepository;

    public ArchivoService(
        IAlmacenadorArchivos almacenadorArchivos,
        IAnuncioRepository anuncioRepository,
        IUsuarioRepository usuarioRepository,
        ISuscripcionRepository suscripcionRepository,
        IAdSlotRepository adSlotRepository)
    {
        _almacenadorArchivos = almacenadorArchivos;
        _anuncioRepository = anuncioRepository;
        _usuarioRepository = usuarioRepository;
        _suscripcionRepository = suscripcionRepository;
        _adSlotRepository = adSlotRepository;
    }

    public async Task<string?> ObtenerUrlFirmadaSiExisteAsync(string clave)
    {
        var esFotoDeAnuncio = await _anuncioRepository.ExisteFotoAsync(clave);
        var esLogoDeDealer = !esFotoDeAnuncio &&
                             await _usuarioRepository.ExisteLogoDealerAsync(clave);
        var esCapturaTransferencia = !esFotoDeAnuncio && !esLogoDeDealer &&
                                     await _suscripcionRepository.ExisteCapturaTransferenciaAsync(clave);
        var esImagenAdSlot = !esFotoDeAnuncio && !esLogoDeDealer && !esCapturaTransferencia &&
                             await _adSlotRepository.ExisteCapturaTransferenciaAdSlotAsync(clave);

        if (!esFotoDeAnuncio && !esLogoDeDealer && !esCapturaTransferencia && !esImagenAdSlot)
            return null;

        return await _almacenadorArchivos.GenerarUrlFirmadaAsync(clave);
    }
}
