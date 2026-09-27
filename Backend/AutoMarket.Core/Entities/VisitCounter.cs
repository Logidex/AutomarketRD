namespace AutoMarket.Core.Entities;

/// <summary>
/// Contador global de visitas al sitio.
/// Se mantiene como una única fila (Id = 1) para simplificar el conteo.
/// </summary>
public class VisitCounter
{
    public int Id { get; set; }
    public long TotalVisitas { get; set; }
    public DateTime UltimaVisita { get; set; }
}
