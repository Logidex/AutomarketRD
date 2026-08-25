namespace AutoMarket.Application.Constants;

/// <summary>
/// Reglas del límite de correos por usuario: cooldown mínimo entre envíos
/// del mismo tipo y tope de correos por día (ventana móvil de 24 h).
/// </summary>
public static class ReglasEmail
{
    public static readonly TimeSpan CooldownPorTipo = TimeSpan.FromMinutes(2);
    public const int TopeDiario = 5;
}
