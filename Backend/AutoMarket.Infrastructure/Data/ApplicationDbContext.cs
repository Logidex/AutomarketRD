using AutoMarket.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AutoMarket.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Anuncio> Anuncios { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<PerfilDealer> PerfilesDealers { get; set; }
    public DbSet<SuscripcionDealer> SuscripcionDealers { get; set; }
    public DbSet<Lead> Leads { get; set; }
    public DbSet<UsuarioFavorito> Favoritos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================================
        // CONFIGURACIÓN: ANUNCIO
        // ==========================================
        modelBuilder.Entity<Anuncio>(b =>
        {
            // Definimos el comparador para que EF Core sepa rastrear los cambios de la lista
            var fotosComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!), // Compara si los elementos son iguales
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), // Genera el código Hash
                c => c.ToList() // Crea una copia limpia para la snapshot de EF Core
            );

            b.Property<List<string>>("_fotos")
                .HasColumnName("Fotos")
                .HasConversion(
                    v => string.Join(',', v),
                    v => !string.IsNullOrEmpty(v)
                        ? v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                        : new List<string>()
                )
                .Metadata.SetValueComparer(fotosComparer);
        });

        // ==========================================
        // CONFIGURACIÓN: USUARIO
        // ==========================================
        modelBuilder.Entity<Usuario>(b =>
        {
            b.Property(u => u.Nombre).HasMaxLength(100);
            b.Property(u => u.Apellido).HasMaxLength(100);
            b.Property(u => u.Email).HasMaxLength(150);

            // Email Unico
            b.HasIndex(u => u.Email).IsUnique();

            // -----------------------------------------------------
            // RELACIÓN 1 A MUCHOS: Usuario -> Anuncios
            // -----------------------------------------------------
            b.HasMany(u => u.Anuncios)
             .WithOne(a => a.Usuario)
             .HasForeignKey(a => a.UsuarioId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(u => u.PerfilDealer)
             .WithOne(u => u.Usuario)
             .HasForeignKey<PerfilDealer>(p => p.UsuarioId);
        });

        // ==========================================
        // CONFIGURACIÓN: PERFIL DEALER
        // ==========================================
        modelBuilder.Entity<PerfilDealer>(b =>
        {
            b.HasKey(p => p.UsuarioId);
            b.Property(p => p.NombreAgencia).HasMaxLength(150);

            // Relación 1 a 1 amarrada hacia SuscripcionDealer
            b.HasOne(p => p.Suscripcion)
                .WithOne(s => s.PerfilDealer)
                .HasForeignKey<SuscripcionDealer>(s => s.PerfilDealerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ==========================================
        // CONFIGURACIÓN: SUSCRIPCION DEALER
        // ==========================================
        modelBuilder.Entity<SuscripcionDealer>(b =>
        {
            // Clave primaria explícita de la tabla
            b.HasKey(s => s.Id);

            // El Contrato (Enums configurados como integers de PostgreSQL)
            b.Property(s => s.Nivel)
                .IsRequired()
                .HasColumnType("integer");

            b.Property(s => s.Ciclo)
                .IsRequired()
                .HasColumnType("integer");

            b.Property(s => s.Estado)
                .IsRequired()
                .HasColumnType("integer");

            // El Reloj (Fechas configuradas explícitamente con Zona Horaria para PostgreSQL)
            b.Property(s => s.FechaInicioUtc)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            b.Property(s => s.FechaVencimientoUtc)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            // Índice Compuesto de alto rendimiento para el BackgroundService
            b.HasIndex(s => new { s.FechaVencimientoUtc, s.Estado })
                .HasDatabaseName("IX_SuscripcionDealer_Vencimiento_Estado");
        });

        // =========================================================================
        // CONFIGURACIÓN DE LA ENTIDAD: Lead
        // =========================================================================
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.ToTable("Leads");

            entity.HasKey(l => l.Id);

            // Restricciones de longitud para optimizar la base de datos
            entity.Property(l => l.NombreContacto)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(l => l.EmailContacto)
                .HasMaxLength(150);

            entity.Property(l => l.TelefonoContacto)
                .HasMaxLength(20);

            entity.Property(l => l.Mensaje)
                .IsRequired()
                .HasMaxLength(1000); // Límite razonable para un mensaje de contacto

            // Conversión del Enum a entero en la base de datos (PostgreSQL lo maneja eficientemente)
            entity.Property(l => l.Canal)
                .IsRequired();

            // Relación 1 a Muchos: 1 Anuncio -> N Leads
            entity.HasOne(l => l.Anuncio)
                .WithMany(a => a.Leads)
                .HasForeignKey(l => l.AnuncioId)
                .OnDelete(DeleteBehavior.Cascade); // Si se elimina un anuncio, se borran sus leads asociados
        });
        
        // ==========================================
        // CONFIGURACIÓN DE FAVORITOS (Muchos a Muchos)
        // ==========================================
        modelBuilder.Entity<UsuarioFavorito>()
            .HasKey(f => new { f.UsuarioId, f.AnuncioId }); // Llave compuesta para evitar duplicados

        modelBuilder.Entity<UsuarioFavorito>()
            .HasOne(f => f.Usuario)
            .WithMany() 
            .HasForeignKey(f => f.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade); // Si borran al usuario, se borran sus favoritos

        modelBuilder.Entity<UsuarioFavorito>()
            .HasOne(f => f.Anuncio)
            .WithMany()
            .HasForeignKey(f => f.AnuncioId)
            .OnDelete(DeleteBehavior.Cascade); // Si el dealer borra el anuncio, desaparece de los favoritos
    }
}
