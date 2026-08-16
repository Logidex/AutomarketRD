namespace AutoMarket.Core.Entities.Enums;

public enum TicketEstado
{
    Abierto = 1,
    EnProceso = 2,
    Resuelto = 3,
    Cerrado = 4,
    Detenido = 5
}

public enum TicketPrioridad
{
    Baja = 1,
    Normal = 2,
    Alta = 3,
    Urgente = 4
}

public enum TicketCategoria
{
    General = 1,
    Facturacion = 2,
    Anuncios = 3,
    SoporteTecnico = 4,
    Cuenta = 5
}
