namespace AutoMarket.Application.Helpers;

/// <summary>
/// Plantilla HTML compartida para los correos transaccionales de AutoMarket RD.
/// Envuelve el contenido con el logo principal y un pie de marca, de forma que
/// todos los correos (alta, recuperación, renovación, tickets, leads, contacto)
/// tengan una apariencia consistente.
/// </summary>
public static class PlantillaCorreoHelper
{
    public const string NombreArchivoLogo = "automarket-rdlogo-opt.png";

    /// <summary>
    /// Construye el cuerpo final del correo a partir del contenido interno.
    /// </summary>
    /// <param name="frontendUrl">Base del frontend (App:FrontendUrl); de ella se
    /// deriva la URL del logo. Si es nula/vacía, el logo se omite.</param>
    /// <param name="titulo">Título visible opcional en la cabecera.</param>
    /// <param name="cuerpoHtml">Contenido interno del correo (sin envoltura).</param>
    public static string Envolver(string? frontendUrl, string? titulo, string cuerpoHtml)
    {
        var logoUrl = ConstruirLogoUrl(frontendUrl);

        var bloqueLogo = string.IsNullOrWhiteSpace(logoUrl)
            ? string.Empty
            : $@"<div style='text-align:left;padding:16px 24px;background-color:#0c101b;'>
                    <img src='{logoUrl}' alt='AutoMarket RD' style='max-height:44px;width:auto;display:block;' />
                  </div>";

        var bloqueTitulo = string.IsNullOrWhiteSpace(titulo)
            ? string.Empty
            : $"<h1 style='color:#0c101b;font-size:20px;margin:20px 24px 8px;'>{titulo}</h1>";

        return $@"<!DOCTYPE html>
<html>
<body style='margin:0;padding:0;background-color:#f4f5f7;font-family:Arial,Helvetica,sans-serif;'>
  <div style='max-width:600px;margin:0 auto;background-color:#ffffff;'>
    {bloqueLogo}
    {bloqueTitulo}
    <div style='padding:0 24px 24px;color:#333;'>{cuerpoHtml}</div>
    <div style='padding:16px 24px;border-top:1px solid #eee;color:#6b7280;font-size:12px;'>
      AutoMarket RD &middot; Compra y venta de vehículos en República Dominicana.
    </div>
  </div>
</body>
</html>";
    }

    /// <summary>
    /// Escapa caracteres HTML para prevenir inyección de código en correos.
    /// Usar siempre que se interpolen datos de usuario en el cuerpo del correo.
    /// </summary>
    public static string EscaparHtml(string? input) =>
        System.Net.WebUtility.HtmlEncode(input ?? string.Empty);

    private static string? ConstruirLogoUrl(string? frontendUrl)
    {
        if (string.IsNullOrWhiteSpace(frontendUrl))
            return null;

        return $"{frontendUrl.TrimEnd('/')}/{NombreArchivoLogo}";
    }
}