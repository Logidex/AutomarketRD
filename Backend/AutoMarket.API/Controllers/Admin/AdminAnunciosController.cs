using AutoMarket.API.Constants;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/admin/anuncios")]
[ApiController]
[Authorize(Roles = Roles.Admin)]
public class AdminAnunciosController : ControllerBase
{
    private readonly IAdminAnuncioService _adminAnuncioService;

    public AdminAnunciosController(IAdminAnuncioService adminAnuncioService)
    {
        _adminAnuncioService = adminAnuncioService;
    }

    [HttpGet]
    public async Task<IActionResult> ListarAnuncios([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 20)
    {
        var (items, total) = await _adminAnuncioService.ListarAnunciosPaginadosAsync(pagina, tamanoPagina);
        return Ok(new
        {
            items,
            totalRegistros = total,
            paginaActual = pagina,
            cantidadPorPagina = tamanoPagina,
            totalPaginas = (int)Math.Ceiling(total / (double)tamanoPagina)
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarAnuncioForzoso(int id)
    {
        var eliminado = await _adminAnuncioService.EliminarAnuncioForzosoAsync(id);
        if (!eliminado)
            return NotFound(new { mensaje = "Anuncio no encontrado." });

        return Ok(new { exito = true, mensaje = "Proceso terminado." });
    }
}
