using System.Security.Cryptography;
using System.Text;

namespace AutoMarket.Application.Helpers;

public static class CodigoUtil
{
    // Código numérico de 6 dígitos (000000 - 999999)
    public static string GenerarCodigoNumerico()
    {
        var numero = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return numero.ToString("D6");
    }

    // Guardamos solo el hash del código, nunca el código en sí
    public static string HashCodigo(string codigo)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(codigo.Trim()));
        return Convert.ToHexString(bytes);
    }
}