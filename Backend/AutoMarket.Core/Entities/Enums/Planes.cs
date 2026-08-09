namespace AutoMarket.Core.Entities.Enums;

public enum PlanNivel
{
    Gratis = 1,
    Basico = 50,
    Pro = 200,
    Elite = 500
}public enum CicloFacturacion
{
    Mensual = 1,
    Trimestral = 3,
    Anual = 12
}



public enum EstadoSuscripcion
{
    Activa = 1,
    Vencida = 2,
    Cancelada = 3
}

public enum EstadoPago
{
    Completado = 1,
    Fallido = 2,
    Reembolsado = 3
}
