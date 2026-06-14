using Microsoft.EntityFrameworkCore;
using TuPenca.Domain.Entities;
using TuPenca.Infrastructure.Interfaces.Providers;

public class AppDbContext : DbContext
{
   
    private readonly ISitioProvider? _sitioProvider;

    // ISitioProvider es opcional (?) porque al correr migraciones
    // no hay HTTP context disponible
    public AppDbContext(DbContextOptions<AppDbContext> options,
        ISitioProvider? sitioProvider = null) : base(options)
    {
        _sitioProvider = sitioProvider;
    }

    // ─── DbSets ───────────────────────────────────────────────
    // Equivalente a los @Repository en Spring — cada uno representa una tabla
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Administrador> Administradores => Set<Administrador>();
    public DbSet<Sitio> Sitios => Set<Sitio>();
    public DbSet<Penca> Pencas => Set<Penca>();
    public DbSet<PlantillaPenca> PlantillasPenca => Set<PlantillaPenca>();
    public DbSet<ReglaPuntuacion> ReglasPuntuacion => Set<ReglaPuntuacion>();
    public DbSet<EventoDeportivo> EventosDeportivos => Set<EventoDeportivo>();
    public DbSet<Partido> Partidos => Set<Partido>();
    public DbSet<Equipo> Equipos => Set<Equipo>();
    public DbSet<Deporte> Deportes => Set<Deporte>();
    public DbSet<TipoCompetencia> TiposCompetencia => Set<TipoCompetencia>();
    public DbSet<Prediccion> Predicciones => Set<Prediccion>();
    public DbSet<PuntajeUsuario> PuntajesUsuario => Set<PuntajeUsuario>();
    public DbSet<Premio> Premios => Set<Premio>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<MensajeChat> MensajesChat => Set<MensajeChat>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<Invitacion> Invitaciones => Set<Invitacion>();
    public DbSet<ParametrosSitio> ParametrosSitio => Set<ParametrosSitio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ─── Partido: dos FKs al mismo Equipo ─────────────────
        // ⚠️ Esto es el caso especial que mencionamos antes
        // EF no sabe cuál FK es local y cuál visitante sin aclarárselo
        modelBuilder.Entity<Partido>()
            .HasOne(p => p.EquipoLocal)
            .WithMany(e => e.PartidosComoLocal)
            .HasForeignKey(p => p.EquipoLocalId)
            .OnDelete(DeleteBehavior.Restrict); // Restrict evita borrado en cascada

        modelBuilder.Entity<Partido>()
            .HasOne(p => p.EquipoVisitante)
            .WithMany(e => e.PartidosComoVisitante)
            .HasForeignKey(p => p.EquipoVisitanteId)
            .OnDelete(DeleteBehavior.Restrict);


          // ─── Filtros globales de multi-tenancy ────────────────
        // Solo aplican si NO es admin de plataforma
        modelBuilder.Entity<Usuario>()
            .HasQueryFilter(u => 
                _sitioProvider == null ||
                _sitioProvider.EsAdminPlataforma() ||
                u.SitioId == _sitioProvider.GetSitioId());

        modelBuilder.Entity<Penca>()
            .HasQueryFilter(p =>
                _sitioProvider == null ||
                _sitioProvider.EsAdminPlataforma() ||
                p.SitioId == _sitioProvider.GetSitioId());

        modelBuilder.Entity<Invitacion>()
            .HasQueryFilter(i =>
                _sitioProvider == null ||
                _sitioProvider.EsAdminPlataforma() ||
                i.SitioId == _sitioProvider.GetSitioId());

        modelBuilder.Entity<MensajeChat>()
            .HasQueryFilter(m =>
                _sitioProvider == null ||
                _sitioProvider.EsAdminPlataforma() ||
                m.Penca.SitioId == _sitioProvider.GetSitioId());

        modelBuilder.Entity<Notificacion>()
            .HasQueryFilter(n =>
                _sitioProvider == null ||
                _sitioProvider.EsAdminPlataforma() ||
                n.Penca.SitioId == _sitioProvider.GetSitioId());

        modelBuilder.Entity<Prediccion>()
            .HasQueryFilter(p =>
                _sitioProvider == null ||
                _sitioProvider.EsAdminPlataforma() ||
                p.Usuario.SitioId == _sitioProvider.GetSitioId());

        // ─── Enums como string en la BD ────────────────────────
        // Por defecto EF guarda enums como int (0,1,2)
        // Esto los guarda como "Pendiente", "Aprobado" — más legible
        modelBuilder.Entity<Usuario>()
            .Property(u => u.Estado)
            .HasConversion<string>();

        modelBuilder.Entity<Usuario>()
            .Property(u => u.ProveedorAuth)
            .HasConversion<string>();

        modelBuilder.Entity<Sitio>()
            .Property(s => s.Estado)
            .HasConversion<string>();

        modelBuilder.Entity<Sitio>()
            .Property(s => s.TipoRegistro)
            .HasConversion<string>();

        modelBuilder.Entity<Pago>()
            .Property(p => p.Estado)
            .HasConversion<string>();

        modelBuilder.Entity<Usuario>()
            .Property(u => u.Rol)
            .HasConversion<string>();

        modelBuilder.Entity<Penca>()
            .Property(p => p.Estado)
            .HasConversion<string>();

        // ─── ParametrosSitio: relación 1-1 con Sitio ──────────
        modelBuilder.Entity<ParametrosSitio>()
            .HasOne(p => p.Sitio)
            .WithOne(s => s.Parametros)
            .HasForeignKey<ParametrosSitio>(p => p.SitioId)
            .OnDelete(DeleteBehavior.Cascade); // si se borra el sitio, se borran sus parámetros

        modelBuilder.Entity<ParametrosSitio>()
            .Property(p => p.DiaResumenSemanal)
            .HasConversion<string>(); // guardar "Friday" en vez de 5

        //modelBuilder.Entity<Pago>()
        //    .Property(p => p.MetodoPago)
        //    .HasConversion<string>();

        // ─── Índices únicos ────────────────────────────────────
        // Equivalente a @Column(unique=true) en JPA
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => new { u.Email, u.SitioId })
            .IsUnique(); // El email es único por sitio, no globalmente

        modelBuilder.Entity<Sitio>()
            .HasIndex(s => s.UrlPropia)
            .IsUnique();

        modelBuilder.Entity<Invitacion>()
            .HasIndex(i => i.Codigo)
            .IsUnique();


        // experimental
        modelBuilder.Entity<Invitacion>()
    .HasOne(i => i.Sitio)
    .WithMany(s => s.Invitaciones)
    .HasForeignKey(i => i.SitioId)
    .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Invitacion>()
    .HasOne(i => i.Usuario)
    .WithMany()
    .HasForeignKey(i => i.UsuarioId)
    .OnDelete(DeleteBehavior.NoAction);


        modelBuilder.Entity<MensajeChat>()
    .HasOne(m => m.Usuario)
    .WithMany(u => u.Mensajes)
    .HasForeignKey(m => m.UsuarioId)
    .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MensajeChat>()
            .HasOne(m => m.Penca)
            .WithMany(p => p.Mensajes)
            .HasForeignKey(m => m.PencaId)
          .OnDelete(DeleteBehavior.NoAction);


        modelBuilder.Entity<Notificacion>()
    .HasOne(n => n.Penca)
    .WithMany(p => p.Notificaciones)
    .HasForeignKey(n => n.PencaId)
    .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Notificacion>()
    .HasOne(n => n.Usuario)
    .WithMany(u => u.Notificaciones)
    .HasForeignKey(n => n.UsuarioId)
    .OnDelete(DeleteBehavior.NoAction);


        modelBuilder.Entity<PuntajeUsuario>()
    .HasOne(p => p.Penca)
    .WithMany(p => p.Puntajes)
    .HasForeignKey(p => p.PencaId)
    .OnDelete(DeleteBehavior.NoAction);


        modelBuilder.Entity<PuntajeUsuario>()
    .HasOne(p => p.Usuario)
    .WithMany(u => u.Puntajes)
    .HasForeignKey(p => p.UsuarioId)
    .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Prediccion>()
    .HasOne(p => p.Penca)
    .WithMany()
    .HasForeignKey(p => p.PencaId)
    .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<PuntajeUsuario>()
     .HasOne(p => p.Partido)
     .WithMany()
     .HasForeignKey(p => p.PartidoId)
     .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Pago>()
     .HasOne(p => p.Penca)
     .WithMany()
     .HasForeignKey(p => p.PencaId)
     .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Premio>()
    .HasOne(p => p.UsuarioGanador)
    .WithMany()
    .HasForeignKey(p => p.UsuarioGanadorId)
    .OnDelete(DeleteBehavior.NoAction);

    }
}