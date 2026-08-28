using AutoMarket.Application.DTOs.Cupones;
using AutoMarket.Application.Interfaces;
using AutoMarket.API.Constants;
using AutoMarket.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/[controller]")]
[ApiController]
/// <summary>
/// Controlador para el canje de cupones promocionales por dealers.
/// </summary>
public class CuponesController : ControllerBase
{
    private readonly ICuponService _cuponService;

    public CuponesController(ICuponService cuponService)
    {
        _cuponService = cuponService;
    }

    /// <summary>
    /// Canjea un cupón para el dealer autenticado. Las reglas (código válido,
    /// tope global de usos, un canje por dealer) se validan en el servicio y
    /// llegan aquí como BusinessRuleException (400 con mensaje para el usuario).
    /// </summary>
    [Authorize(Roles = Roles.Dealer)]
    [HttpPost("aplicar")]
    [ProducesResponseType(typeof(CuponAplicadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Aplicar([FromBody] AplicarCuponDto dto)
    {
        var usuarioId = User.ObtenerUsuarioId();
        var resultado = await _cuponService.AplicarCuponAsync(usuarioId, dto.Codigo);
        return Ok(resultado);
    }
}
