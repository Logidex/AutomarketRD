namespace AutoMarket.Application.Interfaces;

public interface IArchivoService
{
    Task<string?> ObtenerUrlFirmadaSiExisteAsync(string clave);
}
