using Microsoft.EntityFrameworkCore;
using ObligatorioApiario.Models;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ObligatorioApiario.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Apicultor> Apicultores { get; set; }
        public DbSet<ApicultorTelefono> ApicultorTelefonos { get; set; }
        public DbSet<Barril> Barriles { get; set; }
        public DbSet<Herramienta> Herramientas { get; set; }
        public DbSet<Visita> Visitas { get; set; }
        public DbSet<Planifica> Planifica { get; set; }
        public DbSet<Cosecha> Cosechas { get; set; }
        public DbSet<Genera> Genera { get; set; }
        public DbSet<Realiza> Realiza { get; set; }
        public DbSet<Revisa> Revisa { get; set; }
        public DbSet<Apiario> Apiarios { get; set; }
        public DbSet<Tiene> Tiene { get; set; }
        public DbSet<Colmena> Colmenas { get; set; }
        public DbSet<Asigna> Asigna { get; set; }
        public DbSet<Reina> Reinas { get; set; }
        public DbSet<Aplica> Aplica { get; set; }
        public DbSet<Tratamiento> Tratamientos { get; set; }
        public DbSet<Aporta> Aporta { get; set; }
        public DbSet<TareaVisita> TareasVisita { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones de claves compuestas
            modelBuilder.Entity<ApicultorTelefono>().HasKey(at => new { at.CIApicultor, at.Telefono });
            modelBuilder.Entity<Planifica>().HasKey(p => new { p.ID_Visita, p.ID_Herramienta });
            modelBuilder.Entity<Genera>().HasKey(g => new { g.ID_Barril, g.ID_Cosecha });
            modelBuilder.Entity<Realiza>().HasKey(r => new { r.ID_Cosecha, r.ID_Apiario });
            modelBuilder.Entity<Revisa>().HasKey(r => new { r.ID_Visita, r.ID_Apiario });
            modelBuilder.Entity<Tiene>().HasKey(t => new { t.ID_Apiario, t.ID_Colmena });
            modelBuilder.Entity<Asigna>().HasKey(a => new { a.ID_Colmena, a.ID_Reina });
            modelBuilder.Entity<Aplica>().HasKey(a => new { a.ID_Colmena, a.ID_Tratamiento });

            // Configuraciones de decimales
            modelBuilder.Entity<Barril>().Property(b => b.Cantidad_Miel).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Barril>().Property(b => b.Precio).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Apiario>().Property(a => a.Longitud).HasColumnType("decimal(9,6)");
            modelBuilder.Entity<Apiario>().Property(a => a.Latitud).HasColumnType("decimal(9,6)");

            // Constraints
            modelBuilder.Entity<Colmena>()
                .ToTable(t => t.HasCheckConstraint("CHK_Fortaleza_Abejas", "\"Fortaleza_Abejas\" IN ('Fuerte', 'Media', 'Débil')"));
            modelBuilder.Entity<Apicultor>()
                .ToTable(t => t.HasCheckConstraint("CHK_Apicultor_Cargo", "\"Cargo\" IN ('Administrador de Apiario', 'Apicultor de Campo')"));

            // POSTGRESQL FIX: Convert all DateTime properties to UTC before saving, 
            // and back from UTC when reading, to avoid Npgsql timestamp strictness errors.
            var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.ToUniversalTime() : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.IsKeyless) continue;

                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                        property.SetValueConverter(dateTimeConverter);
                    else if (property.ClrType == typeof(DateTime?))
                        property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }
    }
}
