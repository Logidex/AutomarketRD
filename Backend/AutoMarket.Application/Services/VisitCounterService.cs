using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Interfaces;

namespace AutoMarket.Application.Services;

public class VisitCounterService : IVisitCounterService
{
    private readonly IVisitCounterRepository _repository;

    public VisitCounterService(IVisitCounterRepository repository)
    {
        _repository = repository;
    }

    public async Task<long> ObtenerTotalAsync()
    {
        var counter = await _repository.ObtenerOCrearAsync();
        return counter.TotalVisitas;
    }

    public async Task<long> IncrementarAsync()
    {
        await _repository.IncrementarAsync();
        var counter = await _repository.ObtenerOCrearAsync();
        return counter.TotalVisitas;
    }
}
