using AutoMarket.Core.Entities;
using AutoMarket.Core.Entities.Enums;
using AutoMarket.Infrastructure.Data;
using AutoMarket.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AutoMarket.Tests.Repositories;

public class LeadRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly LeadRepository _repositorio;

    public LeadRepositoryTests()
    {
        var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(opciones);
        _repositorio = new LeadRepository(_context);
    }

    // =========================================================================
    // HELPERS
    // =========================================================================

    private static Anuncio CrearAnuncio(int id, int usuarioId, string marca = "Toyota")
    {
        var anuncio = new Anuncio(
            usuarioId, marca, "Corolla", "1.8L", "Sedan", "1.8L", "Delantera",
            "Blanco", "Negro", 2022, 600000, "DOP", 50000, "Automática", "Gasolina",
            new List<string>(), "Santo Domingo", "Vehículo de ejemplo");

        Set(anuncio, "Id", id);
        Set(anuncio, "Estado", "Publicado");

        return anuncio;
    }

    private static Lead CrearLead(int anuncioId, int? id = null, int? usuarioIdRemitente = null, bool leido = false)
    {
        var lead = new Lead(
            anuncioId,
            "Ana Pérez",
            "ana@test.com",
            "8091234567",
            "¿Está disponible?",
            CanalContacto.Formulario,
            usuarioIdRemitente);

        if (id.HasValue) Set(lead, "Id", id.Value);
        if (leido) lead.MarcarComoLeido();

        return lead;
    }

    private static void Set(object obj, string propiedad, object valor)
    {
        var prop = obj.GetType().GetProperty(
            propiedad,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Propiedad '{propiedad}' no encontrada.");

        prop.SetValue(obj, valor);
    }

    private async Task SembrarEscenario()
    {
        // Usuario 1 (Dealer) es dueño del anuncio 1.
        // Usuario 2 (Vendedor) es dueño del anuncio 2 y además envía un lead al anuncio 1.
        // Usuario 3 envía un lead al anuncio 2 (que sí debe llegar al inbox de 2).
        _context.Anuncios.AddRange(
            CrearAnuncio(1, usuarioId: 1, marca: "Toyota"),
            CrearAnuncio(2, usuarioId: 2, marca: "Honda"));

        _context.Leads.AddRange(
            CrearLead(anuncioId: 1, id: 1, usuarioIdRemitente: 2),
            CrearLead(anuncioId: 2, id: 2, usuarioIdRemitente: 3));

        await _context.SaveChangesAsync();
    }

    // =========================================================================
    // 1. INBOX: un lead enviado por el usuario NO aparece en su propio inbox
    // =========================================================================

    [Fact]
    public async Task ObtenerPorUsuarioIdAsync_ExcluyeLeadsQueElUsuarioEnvio()
    {
        await SembrarEscenario();

        // Act
        var inboxUsuario1 = await _repositorio.ObtenerPorUsuarioIdAsync(1);
        var inboxUsuario2 = await _repositorio.ObtenerPorUsuarioIdAsync(2);

        // Assert
        // Usuario 1 recibe el lead que le envió el usuario 2.
        Assert.Single(inboxUsuario1);
        Assert.Equal(1, inboxUsuario1.Single().Id);

        // Usuario 2 NO ve el lead que él mismo envió (id 1); solo ve el que recibió (id 2).
        Assert.Single(inboxUsuario2);
        Assert.Equal(2, inboxUsuario2.Single().Id);
    }

    // =========================================================================
    // 2. CONTADOR DE LEADS del dealer: excluye los enviados por el propio usuario
    // =========================================================================

    [Fact]
    public async Task ContarLeadsPorUsuarioAsync_ExcluyeLeadsQueElUsuarioEnvio()
    {
        await SembrarEscenario();

        var totalUsuario2 = await _repositorio.ContarLeadsPorUsuarioAsync(2);

        Assert.Equal(1, totalUsuario2);
    }

    // =========================================================================
    // 3. CAMPANA (no leídos): los leads enviados por el usuario no generan badge
    // =========================================================================

    [Fact]
    public async Task ContarNoLeidosPorUsuarioAsync_NoCuentaLosLeadsEnviadosPorElUsuario()
    {
        await SembrarEscenario();

        var noLeidosUsuario2 = await _repositorio.ContarNoLeidosPorUsuarioAsync(2);

        // Solo cuenta el lead entrante (id 2); no el que él envió (id 1).
        Assert.Equal(1, noLeidosUsuario2);
    }

    [Fact]
    public async Task ObtenerRecientesNoLeidosPorUsuarioAsync_NoIncluyeLeadsEnviadosPorElUsuario()
    {
        await SembrarEscenario();

        var recientes = await _repositorio.ObtenerRecientesNoLeidosPorUsuarioAsync(2, 10);

        Assert.Single(recientes);
        Assert.Equal(2, recientes.Single().Id);
    }

    // =========================================================================
    // 4. MARCAR TODOS LEÍDOS: solo afecta a los leads entrantes del usuario
    // =========================================================================

    [Fact]
    public async Task MarcarTodosLeidosAsync_SoloMarcaLosLeadsEntrantes()
    {
        await SembrarEscenario();

        var marcados = await _repositorio.MarcarTodosLeidosAsync(2);

        Assert.Equal(1, marcados);
        Assert.False((await _repositorio.ObtenerPorIdAsync(1))!.Leido);
        Assert.True((await _repositorio.ObtenerPorIdAsync(2))!.Leido);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
