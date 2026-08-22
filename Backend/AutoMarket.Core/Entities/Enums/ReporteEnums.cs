namespace AutoMarket.Core.Entities.Enums;

public enum ReporteMotivo
{
    ContenidoInapropiado = 1,
    FraudeEstafa = 2,
    InformacionFalsa = 3,
    Duplicado = 4,
    Otro = 5
}

public enum ReporteEstado
{
    Pendiente = 1,
    Descartado = 2,
    Resuelto = 3
}
