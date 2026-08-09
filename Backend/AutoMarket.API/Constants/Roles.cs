namespace AutoMarket.API.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Dealer = "Dealer";
    public const string Vendedor = "Vendedor";
    public const string DealerVendedor = $"{Dealer},{Vendedor}";
}