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
    public DbSet<HistorialVista> HistorialVistas { get; set; }
    public DbSet<PlanCatalogo> PlanesCatalogo { get; set; }
    public DbSet<PagoSuscripcion> PagosSuscripcion { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketMensaje> TicketMensajes { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<ReporteAnuncio> ReportesAnuncios { get; set; }
    public DbSet<Cupon> Cupones { get; set; }
    public DbSet<CuponRedencion> RedencionesCupon { get; set; }
    public DbSet<Encuesta> Encuestas { get; set; }
    public DbSet<EncuestaPregunta> EncuestasPreguntas { get; set; }
    public DbSet<EncuestaRespuesta> EncuestasRespuestas { get; set; }

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

            b.Property(a => a.Moneda)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("DOP");

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
            b.Property(u => u.EmailPendiente).HasMaxLength(150);

            // Bloqueo por intentos fallidos y límite de correos:
            // contadores en 0 para las filas existentes.
            b.Property(u => u.IntentosFallidos).HasDefaultValue(0);
            b.Property(u => u.EmailsEnviadosHoy).HasDefaultValue(0);

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
             .HasForeignKey<PerfilDealer>(p => p.UsuarioId)
             .OnDelete(DeleteBehavior.Cascade);
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

            // Remitente autenticado (null si el contacto fue anónimo)
            entity.Property(l => l.UsuarioIdRemitente)
                .IsRequired(false);

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
        // CONFIGURACIÓN: REFRESH TOKENS (sesiones)
        // ==========================================
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");

            entity.HasKey(t => t.Id);

            entity.Property(t => t.TokenHash)
                .IsRequired()
                .HasMaxLength(64);

            entity.HasIndex(t => t.TokenHash)
                .IsUnique();

            entity.Property(t => t.ReplacedByTokenHash)
                .HasMaxLength(64);

            entity.HasOne(t => t.Usuario)
                .WithMany()
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ==========================================
        // CONFIGURACIÓN: REPORTES DE ANUNCIOS
        // ==========================================
        modelBuilder.Entity<ReporteAnuncio>(entity =>
        {
            entity.ToTable("ReportesAnuncios");

            entity.HasKey(r => r.Id);

            entity.Property(r => r.IpReportante)
                .IsRequired()
                .HasMaxLength(45); // IPv6 máx

            entity.Property(r => r.Detalle)
                .HasMaxLength(500);

            // Índice para el panel: pendientes primero, más recientes arriba
            entity.HasIndex(r => new { r.Estado, r.FechaCreacionUtc });

            entity.HasOne(r => r.Anuncio)
                .WithMany()
                .HasForeignKey(r => r.AnuncioId)
                .OnDelete(DeleteBehavior.Cascade);
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

        // ==========================================
        // CONFIGURACIÓN DE HISTORIAL DE VISTAS (Comprador)
        // ==========================================
        modelBuilder.Entity<HistorialVista>()
            .HasKey(h => new { h.UsuarioId, h.AnuncioId }); // Llave compuesta para evitar duplicados

        modelBuilder.Entity<HistorialVista>()
            .HasOne(h => h.Usuario)
            .WithMany()
            .HasForeignKey(h => h.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade); // Si borran al usuario, se borra su historial

        modelBuilder.Entity<HistorialVista>()
            .HasOne(h => h.Anuncio)
            .WithMany()
            .HasForeignKey(h => h.AnuncioId)
            .OnDelete(DeleteBehavior.Cascade); // Si el dealer borra el anuncio, desaparece del historial

        // ==========================================
        // CONFIGURACIÓN: CATÁLOGO DE PLANES
        // ==========================================
        modelBuilder.Entity<PlanCatalogo>(b =>
        {
            b.HasKey(p => p.Id);

            b.Property(p => p.Nivel)
                .IsRequired()
                .HasColumnType("integer");

            b.Property(p => p.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(p => p.Descripcion)
                .HasMaxLength(500);

            b.Property(p => p.PrecioMensual)
                .HasPrecision(18, 2);

            b.Property(p => p.DescuentoTrimestralPorcentaje)
                .HasPrecision(5, 2);

            b.Property(p => p.DescuentoAnualPorcentaje)
                .HasPrecision(5, 2);

            b.Property(p => p.Activo)
                .IsRequired();

            // Un solo plan por nivel (Gratis, Basico, Pro, Elite)
            b.HasIndex(p => p.Nivel)
                .IsUnique()
                .HasDatabaseName("IX_PlanesCatalogo_Nivel");
        });

        // ==========================================
        // CONFIGURACIÓN: PAGOS DE SUSCRIPCIÓN
        // ==========================================
        modelBuilder.Entity<PagoSuscripcion>(b =>
        {
            b.HasKey(p => p.Id);

            b.Property(p => p.Nivel)
                .IsRequired()
                .HasColumnType("integer");

            b.Property(p => p.Ciclo)
                .IsRequired()
                .HasColumnType("integer");

            b.Property(p => p.Estado)
                .IsRequired()
                .HasColumnType("integer");

            b.Property(p => p.Monto)
                .HasPrecision(18, 2);

            b.Property(p => p.Moneda)
                .IsRequired()
                .HasMaxLength(10);

            b.Property(p => p.OrderIdPayPal)
                .HasMaxLength(64);

            b.Property(p => p.EventoIdPayPal)
                .HasMaxLength(64);

            b.Property(p => p.CaptureIdPayPal)
                .HasMaxLength(64);

            b.Property(p => p.Referencia)
                .HasMaxLength(255);

            b.Property(p => p.FechaUtc)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            b.HasOne(p => p.PerfilDealer)
                .WithMany()
                .HasForeignKey(p => p.PerfilDealerId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(p => p.PerfilDealerId)
                .HasDatabaseName("IX_PagosSuscripcion_PerfilDealerId");

            b.HasIndex(p => p.FechaUtc);
        });

        // ==========================================
        // CONFIGURACIÓN: TICKETS DE SOPORTE
        // ==========================================
        modelBuilder.Entity<Ticket>(b =>
        {
            b.HasKey(t => t.Id);

            b.Property(t => t.Asunto)
                .IsRequired()
                .HasMaxLength(150);

            b.Property(t => t.Categoria)
                .IsRequired()
                .HasColumnType("integer");

            b.Property(t => t.Prioridad)
                .IsRequired()
                .HasColumnType("integer");

            b.Property(t => t.Estado)
                .IsRequired()
                .HasColumnType("integer");

            b.Property(t => t.FechaCreacionUtc)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            b.Property(t => t.FechaActualizacionUtc)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            b.HasOne(t => t.Usuario)
                .WithMany()
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(t => t.UsuarioId);
            b.HasIndex(t => t.Estado);
            b.HasIndex(t => new { t.Estado, t.Prioridad })
                .HasDatabaseName("IX_Tickets_Estado_Prioridad");
        });

        modelBuilder.Entity<TicketMensaje>(b =>
        {
            b.HasKey(m => m.Id);

            b.Property(m => m.Mensaje)
                .IsRequired()
                .HasMaxLength(2000);

            b.Property(m => m.EsAdmin)
                .IsRequired();

            b.Property(m => m.FechaCreacionUtc)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            b.HasOne(m => m.Ticket)
                .WithMany(t => t.Mensajes)
                .HasForeignKey(m => m.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(m => m.Autor)
                .WithMany()
                .HasForeignKey(m => m.AutorId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(m => m.TicketId);
            b.HasIndex(m => m.AutorId);
        });

        // ==========================================
        // CONFIGURACIÓN: CUPONES PROMOCIONALES
        // ==========================================
        modelBuilder.Entity<Cupon>(b =>
        {
            b.HasKey(c => c.Id);

            b.Property(c => c.Codigo)
                .IsRequired()
                .HasMaxLength(50);

            b.HasIndex(c => c.Codigo)
                .IsUnique()
                .HasDatabaseName("IX_Cupones_Codigo");

            b.Property(c => c.Nivel)
                .IsRequired()
                .HasColumnType("integer");

            b.Property(c => c.Dias)
                .IsRequired();

            b.Property(c => c.MaximoUsos)
                .IsRequired();

            // Token de concurrencia optimista para el tope de canjes.
            b.Property(c => c.UsosActuales)
                .IsRequired()
                .IsConcurrencyToken();

            b.Property(c => c.Activo)
                .IsRequired();

            b.Property(c => c.FechaCreacionUtc)
                .IsRequired()
                .HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<CuponRedencion>(b =>
        {
            b.HasKey(r => r.Id);

            b.HasOne(r => r.Cupon)
                .WithMany(c => c.Redenciones)
                .HasForeignKey(r => r.CuponId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(r => r.PerfilDealer)
                .WithMany()
                .HasForeignKey(r => r.PerfilDealerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Un solo canje por dealer a nivel de base de datos.
            b.HasIndex(r => new { r.CuponId, r.PerfilDealerId })
                .IsUnique()
                .HasDatabaseName("IX_CuponesRedencion_Cupon_PerfilDealer");

            b.Property(r => r.FechaUtc)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            b.HasIndex(r => r.PerfilDealerId);
        });

        // ==========================================
        // CONFIGURACIÓN: ENCUESTAS
        // ==========================================
        modelBuilder.Entity<Encuesta>(b =>
        {
            b.HasKey(e => e.Id);

            b.Property(e => e.Titulo)
                .IsRequired()
                .HasMaxLength(200);

            b.Property(e => e.Descripcion)
                .HasMaxLength(500);

            b.Property(e => e.Activa)
                .IsRequired();

            b.Property(e => e.FechaCreacionUtc)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            b.HasIndex(e => e.Activa);
        });

        modelBuilder.Entity<EncuestaPregunta>(b =>
        {
            b.HasKey(p => p.Id);

            b.Property(p => p.Texto)
                .IsRequired()
                .HasMaxLength(500);

            b.Property(p => p.Orden)
                .IsRequired();

            b.Property(p => p.Tipo)
                .IsRequired()
                .HasColumnType("integer");

            b.HasOne(p => p.Encuesta)
                .WithMany(e => e.Preguntas)
                .HasForeignKey(p => p.EncuestaId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(p => new { p.EncuestaId, p.Orden });
        });

        modelBuilder.Entity<EncuestaRespuesta>(b =>
        {
            b.HasKey(r => r.Id);

            b.Property(r => r.ValorEscala);

            b.Property(r => r.ValorTexto)
                .HasMaxLength(1000);

            b.Property(r => r.FechaUtc)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            b.HasOne(r => r.Encuesta)
                .WithMany()
                .HasForeignKey(r => r.EncuestaId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(r => r.Pregunta)
                .WithMany(p => p.Respuestas)
                .HasForeignKey(r => r.PreguntaId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Una respuesta por usuario y pregunta; el servicio rechaza de
            // antemano si el usuario ya respondió cualquier pregunta.
            b.HasIndex(r => new { r.EncuestaId, r.UsuarioId, r.PreguntaId })
                .IsUnique()
                .HasDatabaseName("IX_EncuestasRespuestas_Unicas");

            b.HasIndex(r => new { r.EncuestaId, r.UsuarioId });
        });
    }
}
