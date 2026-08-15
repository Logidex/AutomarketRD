using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Infrastructure.Data;
using AutoMarket.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AutoMarket.Tests.Services;

public class DashboardServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly DashboardService _servicio;

    public DashboardServiceTests()
    {
        var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(opciones);
        _servicio = new DashboardService(_context);
    }

    // =========================================================================
    // HELPERS
    // =========================================================================

    private static Anuncio CrearAnuncio(
        int id,
        int usuarioId,
        string marca,
        string estado,
        int vistas = 0,
        int anio = 2022)
    {
        var anuncio = new Anuncio(
            usuarioId, marca, "Corolla", "1.8L", "Sedan", "1.8L", "Delantera",
            "Blanco", "Negro", anio, 600000, "DOP", 50000, "Automática", "Gasolina",
            new List<string>(), "Santo Domingo", "Vehículo de ejemplo");

        Set(anuncio, "Id", id);
        Set(anuncio, "Estado", estado);
        for (int i = 0; i < vistas; i++) anuncio.RegistrarVista();

        return anuncio;
    }

    private static Lead CrearLead(int anuncioId, int? id = null, bool leido = false)
    {
        var lead = new Lead(
            anuncioId,
            "Ana Pérez",
            "ana@test.com",
            "8091234567",
            "¿Está disponible?",
            CanalContacto.Formulario);

        if (id.HasValue) Set(lead, "Id", id.Value);
        if (leido) lead.MarcarComoLeido();
        return lead;
    }

    private static PerfilDealer CrearPerfilDealerConSuscripcion(
        int usuarioId,
        PlanNivel nivel,
        int diasVencimiento = 20)
    {
        var perfil = (PerfilDealer)Activator.CreateInstance(
            typeof(PerfilDealer),
            nonPublic: true)!;

        Set(perfil, "UsuarioId", usuarioId);
        Set(perfil, "NombreAgencia", "Agencia de Prueba");
        Set(perfil, "AgenciaRNC", "123456789");
        Set(perfil, "TelefonoAgencia", "8095554444");
        Set(perfil, "Ubicacion", "Santo Domingo");

        var suscripcion = new SuscripcionDealer(usuarioId, nivel, CicloFacturacion.Mensual);
        Set(suscripcion, "FechaVencimientoUtc", DateTime.UtcNow.AddDays(diasVencimiento));
        Set(perfil, "Suscripcion", suscripcion);
        Set(suscripcion, "PerfilDealer", perfil);

        return perfil;
    }

    private static void Set(object obj, string propiedad, object valor)
    {
        var prop = obj.GetType().GetProperty(
            propiedad,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Propiedad '{propiedad}' no encontrada.");

        prop.SetValue(obj, valor);
    }

    // =========================================================================
    // 1. RESUMEN GLOBAL (AdminController)
    // =========================================================================

    [Fact]
    public async Task ObtenerResumenAsync_Global_ContabilizaUsuariosAnunciosYLeads()
    {
        // Arrange
        _context.Usuarios.Add(new Usuario(
            nombre: "Juan",
            apellido: "DM",
            email: "juan@test.com",
            passwordHash: "hash",
            telefonoPersonal: "8090000000",
            rol: "Vendedor",
            emailConfirmado: true));

        _context.Anuncios.AddRange(
            CrearAnuncio(1, 1, "Toyota", "Publicado"),
            CrearAnuncio(2, 2, "Honda", "Vendido"),
            CrearAnuncio(3, 3, "Kia", "Pausado"));
        _context.Leads.AddRange(
            CrearLead(1, id: 1),
            CrearLead(2, id: 2, leido: true));

        await _context.SaveChangesAsync();

        // Act
        var resumen = await _servicio.ObtenerResumenAsync();

        // Assert
        Assert.Equal(1, resumen.TotalUsuarios);
        Assert.Equal(3, resumen.TotalAnuncios);
        Assert.Equal(2, resumen.TotalLeads);
        Assert.Equal(1, resumen.AnunciosActivos);
        Assert.Equal(1, resumen.AnunciosVendidos);
        Assert.Equal(1, resumen.AnunciosPausados);
        Assert.Equal(0, resumen.AnunciosBorrador);
    }

    // =========================================================================
    // 2. RESUMEN GLOBAL: anuncios más vistos (descendente)
    // =========================================================================

    [Fact]
    public async Task ObtenerResumenAsync_Global_OrdenaMasVistosDescendente()
    {
        // Arrange
        _context.Anuncios.AddRange(
            CrearAnuncio(1, 1, "Toyota", "Publicado", vistas: 1),
            CrearAnuncio(2, 2, "Honda", "Publicado", vistas: 5),
            CrearAnuncio(3, 3, "Kia", "Publicado", vistas: 3));

        await _context.SaveChangesAsync();

        // Act
        var resumen = await _servicio.ObtenerResumenAsync();

        // Assert
        Assert.Equal(new List<int> { 5, 3, 1 }, resumen.AnunciosMasVistos.Select(a => a.Vistas).ToList());
    }

    // =========================================================================
    // 3. RESUMEN DEL DEALER: solo cuenta sus propios anuncios y leads
    // =========================================================================

    [Fact]
    public async Task ObtenerResumenAsync_Dealer_SoloCuentaSusAnunciosYLeads()
    {
        // Arrange
        _context.Anuncios.AddRange(
            CrearAnuncio(1, 5, "Toyota", "Publicado"),
            CrearAnuncio(2, 5, "Honda", "Publicado"),
            CrearAnuncio(3, 5, "Kia", "Borrador"),
            CrearAnuncio(4, 99, "Otro Dealer", "Publicado"));
        _context.Leads.AddRange(
            CrearLead(1, id: 1),
            CrearLead(2, id: 2, leido: true),
            CrearLead(4, id: 3, leido: true));

        await _context.SaveChangesAsync();

        // Act
        var resumen = await _servicio.ObtenerResumenAsync(5);

        // Assert
        Assert.Equal(3, resumen.TotalAnuncios);
        Assert.Equal(2, resumen.AnunciosActivos);
        Assert.Equal(1, resumen.AnunciosBorrador);
        Assert.Equal(2, resumen.TotalLeads);
        Assert.Equal(1, resumen.LeadsNoLeidos);
    }

    // =========================================================================
    // 4. RESUMEN DEL DEALER: incluye plan, límite y días restantes
    // =========================================================================

    [Fact]
    public async Task ObtenerResumenAsync_DealerConSuscripcion_IncluyePlanLimiteYDias()
    {
        // Arrange
        var perfil = CrearPerfilDealerConSuscripcion(5, PlanNivel.Pro, 20);
        _context.PerfilesDealers.Add(perfil);
        _context.SuscripcionDealers.Add(perfil.Suscripcion!);
        _context.Anuncios.Add(CrearAnuncio(1, 5, "Toyota", "Publicado"));

        await _context.SaveChangesAsync();

        // Act
        var resumen = await _servicio.ObtenerResumenAsync(5);

        // Assert
        Assert.Equal("Pro", resumen.PlanActual);
        Assert.Equal(PlanConfig.LimiteAnuncios(PlanNivel.Pro), resumen.LimiteAnuncios);
        Assert.Equal(20, resumen.DiasRestantesSuscripcion);
        Assert.Equal(1, resumen.AnunciosActivos);
    }

    // =========================================================================
    // 5. RESUMEN DEL DEALER: sin suscripción → valores por defecto
    // =========================================================================

    [Fact]
    public async Task ObtenerResumenAsync_DealerSinSuscripcion_DatosPorDefecto()
    {
        // Arrange
        _context.Anuncios.Add(CrearAnuncio(1, 7, "Toyota", "Borrador"));
        await _context.SaveChangesAsync();

        // Act
        var resumen = await _servicio.ObtenerResumenAsync(7);

        // Assert
        Assert.Equal("N/A", resumen.PlanActual);
        Assert.Equal(0, resumen.LimiteAnuncios);
        Assert.Equal(0, resumen.DiasRestantesSuscripcion);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}