using AutoMarket.API.Constants;
using AutoMarket.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.API.Controllers;

[Route("api/admin/[controller]")]
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
    public async Task<IActionResult> ListarAnuncios()
    {
        var anuncios = await _adminAnuncioService.ListarAnunciosParaAdminAsync();
        return Ok(anuncios);
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
