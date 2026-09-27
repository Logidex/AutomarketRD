namespace AutoMarket.Application.Interfaces;

public interface IVisitCounterService
{
    Task<long> ObtenerTotalAsync();
    Task<long> IncrementarAsync();
}
