using System.Linq;

namespace AutoMarket.Application.Helpers;

public static class NormalizadorTexto
{
    public static string Normalizar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return string.Empty;

        string? normalizado = valor.Normalize(System.Text.NormalizationForm.FormD);

        var sinAcentos = new string(normalizado
            .Where(c =>
                System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) !=
                System.Globalization.UnicodeCategory.NonSpacingMark)
            .ToArray());

        return sinAcentos.Normalize(System.Text.NormalizationForm.FormC).Trim();
    }
}