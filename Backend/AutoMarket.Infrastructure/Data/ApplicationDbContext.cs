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
            b.HasKey(a => a.Id);

            b.Property(a => a.Marca)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(a => a.Modelo)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(a => a.Version)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(a => a.TipoVehiculo)
                .IsRequired()
                .HasMaxLength(50);

            b.Property(a => a.Motor)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(a => a.Traccion)
                .IsRequired()
                .HasMaxLength(50);

            b.Property(a => a.ColorExterior)
                .IsRequired()
                .HasMaxLength(50);

            b.Property(a => a.ColorInterior)
                .IsRequired()
                .HasMaxLength(50);

            b.Property(a => a.Transmision)
                .IsRequired()
                .HasMaxLength(50);

            b.Property(a => a.Combustible)
                .IsRequired()
                .HasMaxLength(50);

            b.Property(a => a.Ubicacion)
                .IsRequired()
                .HasMaxLength(150);

            b.Property(a => a.Descripcion)
                .IsRequired()
                .HasColumnType("text");

            b.Property(a => a.Estado)
                .IsRequired()
                .HasMaxLength(30);

            b.Property(a => a.Precio)
                .HasPrecision(18, 2);

            b.Property(a => a.Kilometraje)
                .IsRequired();

            b.Property(a => a.Anio)
                .IsRequired();

            b.Property(a => a.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            b.Property(a => a.UpdatedAt)
                .HasColumnType("timestamp with time zone");

            b.Property(a => a.Vistas)
                .IsRequired();

            b.HasIndex(a => a.Vistas);

            /*
             * Configuración de la colección de fotos privada.
             */
            var fotosComparer = new ValueComparer<List<string>>(
                (c1, c2) =>
                    c1 != null &&
                    c2 != null &&
                    c1.SequenceEqual(c2),

                c =>
                    c.Aggregate(
                        0,
                        (a, v) => HashCode.Combine(
                            a,
                            v.GetHashCode()
                        )
                    ),

                c => c.ToList()
            );

            b.Property<List<string>>("_fotos")
                .HasColumnName("Fotos")
                .HasConversion(
                    v => string.Join(',', v),

                    v =>
                        !string.IsNullOrEmpty(v)
                            ? v.Split(
                                ',',
                                StringSplitOptions
                                    .RemoveEmptyEntries
                            ).ToList()
                            : new List<string>()
                )
                .Metadata.SetValueComparer(fotosComparer);

            /*
             * Accesorios como arreglo de texto de PostgreSQL.
             *
             * Si esta propiedad ya existe en tu base de datos
             * con otra configuración, revisaremos la migración
             * antes de aplicarla.
             */
            b.Property(a => a.Accesorios)
                .HasColumnType("text[]");

            /*
             * Relación Anuncio -> Usuario.
             */
            b.HasOne(a => a.Usuario)
                .WithMany(u => u.Anuncios)
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            /*
             * Índices para búsquedas frecuentes.
             */
            b.HasIndex(a => a.Marca);
            b.HasIndex(a => a.Modelo);
            b.HasIndex(a => a.TipoVehiculo);
            b.HasIndex(a => a.Estado);
            b.HasIndex(a => a.UsuarioId);
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
