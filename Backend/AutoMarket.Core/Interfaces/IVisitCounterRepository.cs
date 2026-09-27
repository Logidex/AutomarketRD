using AutoMarket.Core.Entities;

namespace AutoMarket.Core.Interfaces;

public interface IVisitCounterRepository
{
    Task<VisitCounter> ObtenerOCrearAsync();
    Task IncrementarAsync();
}
