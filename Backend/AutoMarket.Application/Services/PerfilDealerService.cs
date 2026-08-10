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
        var usuario = await _usuarioRepository
            .ObtenerDealerConPerfilPorIdAsync(dealerId);

        if (usuario is null)
            return null;

        // Dealer: usa los datos de su PerfilDealer.
        if (usuario.PerfilDealer != null)
            return MapearPerfilPublico(usuario);

        // Vendedor (cuenta individual, sin PerfilDealer): página pública básica.
        if (string.Equals(usuario.Rol, "Vendedor", StringComparison.OrdinalIgnoreCase))
            return MapearVendedorPublico(usuario);

        // Otros roles (comprador/admin) no tienen página pública de vendedor.
        return null;
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

            var logoAnterior = perfil.LogoUrl;

            perfil.ActualizarLogo(rutaLogo);

            // Eliminamos el logo anterior de S3 para no acumular archivos huérfanos
            if (!string.IsNullOrWhiteSpace(logoAnterior))
            {
                try
                {
                    await _almacenadorArchivos.EliminarArchivoAsync(logoAnterior);
                }
                catch
                {
                    // No bloqueamos la actualización si falla la limpieza en S3
                }
            }
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

    private static PerfilDealerPublicoDto MapearVendedorPublico(
        AutoMarket.Core.Entities.Usuario vendedor)
    {
        var nombreCompleto =
            $"{vendedor.Nombre} {vendedor.Apellido}".Trim();

        return new PerfilDealerPublicoDto
        {
            Id = vendedor.UsuarioId,
            NombreAgencia = string.IsNullOrWhiteSpace(nombreCompleto)
                ? "Vendedor"
                : nombreCompleto,
            LogoUrl = null,
            Horarios = null,
            Ubicacion = string.Empty,
            TelefonoAgencia = vendedor.TelefonoPersonal ?? string.Empty,
            Descripcion = string.Empty,
            WhatsApp = null
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