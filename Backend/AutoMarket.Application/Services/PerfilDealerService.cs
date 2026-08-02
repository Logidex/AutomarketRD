using AutoMarket.Application.DTOs.Usuario;
using AutoMarket.Application.Interfaces;
using AutoMarket.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AutoMarket.Application.Services;

public class PerfilDealerService : IPerfilDealerService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAlmacenadorArchivos _almacenadorArchivos;

    public PerfilDealerService(
        IUsuarioRepository usuarioRepository,
        IAlmacenadorArchivos almacenadorArchivos)
    {
        _usuarioRepository = usuarioRepository;
        _almacenadorArchivos = almacenadorArchivos;
    }

    public async Task<PerfilDealerPublicoDto?> ObtenerPerfilPublicoAsync(int dealerId)
    {
        var dealer = await _usuarioRepository
            .ObtenerDealerConPerfilPorIdAsync(dealerId);

        if (dealer is null || dealer.PerfilDealer is null)
            return null;

        return MapearPerfilPublico(dealer);
    }

    public async Task<PerfilDealerPublicoDto?> ActualizarMiPerfilAsync(
        int dealerId,
        PerfilDealerUpdateDto dto)
    {
        var dealer = await _usuarioRepository
            .ObtenerDealerConPerfilPorIdAsync(dealerId);

        if (dealer is null || dealer.PerfilDealer is null)
            return null;

        var perfil = dealer.PerfilDealer;

        perfil.ActualizarPerfil(
            nombreAgencia: dto.NombreAgencia,
            ubicacion: dto.Ubicacion,
            telefonoAgencia: dto.TelefonoAgencia,
            horarios: dto.Horarios,
            descripcion: dto.Descripcion,
            whatsApp: dto.WhatsApp
        );

        if (dto.Logo is not null && dto.Logo.Length > 0)
        {
            ValidarLogo(dto.Logo);

            await using var stream = dto.Logo.OpenReadStream();

            var rutaLogo = await _almacenadorArchivos.GuardarArchivoAsync(
                stream,
                dto.Logo.FileName,
                dto.Logo.ContentType
            );

            perfil.ActualizarLogo(rutaLogo);
        }

        await _usuarioRepository.GuardarCambiosAsync();

        return MapearPerfilPublico(dealer);
    }

    private static PerfilDealerPublicoDto MapearPerfilPublico(
    AutoMarket.Core.Entities.Usuario dealer)
    {
        var perfil = dealer.PerfilDealer!;

        return new PerfilDealerPublicoDto
        {
            Id = dealer.UsuarioId,
            NombreAgencia = perfil.NombreAgencia,
            LogoUrl = perfil.LogoUrl,
            Horarios = perfil.Horarios,
            Ubicacion = perfil.Ubicacion,
            TelefonoAgencia = perfil.TelefonoAgencia,
            Descripcion = perfil.Descripcion ?? string.Empty,
            WhatsApp = perfil.WhatsApp
        };
    }

    private static void ValidarLogo(IFormFile logo)
    {
        const long tamanioMaximo = 5 * 1024 * 1024;

        var extensionesPermitidas = new[]
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        var extension = Path.GetExtension(logo.FileName).ToLowerInvariant();

        if (!extensionesPermitidas.Contains(extension))
        {
            throw new ArgumentException(
                "El logo debe ser una imagen JPG, JPEG, PNG o WEBP.");
        }

        if (logo.Length > tamanioMaximo)
        {
            throw new ArgumentException(
                "El logo no puede superar los 5 MB.");
        }
    }
}