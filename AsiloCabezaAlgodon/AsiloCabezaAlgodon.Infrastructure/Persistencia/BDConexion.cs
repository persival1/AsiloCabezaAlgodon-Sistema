using AsiloCabezaAlgodon.Domain.Entidad;
using Microsoft.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Infrastructure.Persistencia
{
    public class BDConexion : DbContext
    {
        public BDConexion(DbContextOptions<BDConexion> options) : base(options) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Rol> Roles => Set<Rol>();
        public DbSet<Interno> Internos => Set<Interno>();
        public DbSet<Familiar> Familiares => Set<Familiar>();
        public DbSet<Psicopatologia> Psicopatologias => Set<Psicopatologia>();
        public DbSet<MedicamentoCajon> MedicamentosCajon => Set<MedicamentoCajon>();
        public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
        public DbSet<VisitaMedica> VisitasMedicas => Set<VisitaMedica>();
        public DbSet<ExamenSolicitado> ExamenesSolicitados => Set<ExamenSolicitado>();
        public DbSet<Prescripcion> Prescripciones => Set<Prescripcion>();
        public DbSet<Donacion> Donaciones => Set<Donacion>();
        public DbSet<Gasto> Gastos => Set<Gasto>();
        public DbSet<Cobro> Cobros => Set<Cobro>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rol>()
                .HasKey(r => r.IdRol);

            modelBuilder.Entity<Usuario>()
                .HasKey(u => u.IdUsuario);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.NombreUsuario)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.IdRol);

            modelBuilder.Entity<Interno>()
                .HasKey(i => i.IdInterno);

            modelBuilder.Entity<Familiar>()
                .HasKey(f => f.IdFamiliar);

            modelBuilder.Entity<Familiar>()
                .HasOne(f => f.Interno)
                .WithOne(i => i.Familiar)
                .HasForeignKey<Familiar>(f => f.IdInterno);

            modelBuilder.Entity<Familiar>()
                .Property(f => f.CuotaMensual)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Familiar>()
                .Property(f => f.SaldoPendiente)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Psicopatologia>()
                .HasKey(p => p.IdPsicopatologia);

            modelBuilder.Entity<Psicopatologia>()
                .HasOne(p => p.Interno)
                .WithMany(i => i.Psicopatologias)
                .HasForeignKey(p => p.IdInterno);

            modelBuilder.Entity<MedicamentoCajon>()
                .HasKey(m => m.IdMedicamentoCajon);

            modelBuilder.Entity<MedicamentoCajon>()
                .HasOne(m => m.Interno)
                .WithMany(i => i.MedicamentosCajon)
                .HasForeignKey(m => m.IdInterno);

            modelBuilder.Entity<Solicitud>()
                .HasKey(s => s.IdSolicitud);

            modelBuilder.Entity<Solicitud>()
                .HasOne(s => s.Interno)
                .WithMany()
                .HasForeignKey(s => s.IdInterno);

            modelBuilder.Entity<Solicitud>()
                .HasOne(s => s.MedicoGeneral)
                .WithMany()
                .HasForeignKey(s => s.IdMedicoGeneral)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Solicitud>()
                .HasOne(s => s.Especialista)
                .WithMany()
                .HasForeignKey(s => s.IdEspecialista)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Solicitud>()
                .HasOne(s => s.Enfermero)
                .WithMany()
                .HasForeignKey(s => s.IdEnfermero)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VisitaMedica>()
                .HasKey(v => v.IdVisitaMedica);

            modelBuilder.Entity<VisitaMedica>()
                .HasOne(v => v.Solicitud)
                .WithMany()
                .HasForeignKey(v => v.IdSolicitud);

            modelBuilder.Entity<VisitaMedica>()
                .Property(v => v.CostoConsulta)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ExamenSolicitado>()
                .Property(e => e.Costo)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ExamenSolicitado>()
                .HasKey(e => e.IdExamenSolicitado);

            modelBuilder.Entity<ExamenSolicitado>()
                .HasOne(e => e.VisitaMedica)
                .WithMany(v => v.ExamenesSolicitados)
                .HasForeignKey(e => e.IdVisitaMedica);

            modelBuilder.Entity<Prescripcion>()
                .HasKey(p => p.IdPrescripcion);

            modelBuilder.Entity<Prescripcion>()
                .HasOne(p => p.VisitaMedica)
                .WithMany(v => v.Prescripciones)
                .HasForeignKey(p => p.IdVisitaMedica);

            modelBuilder.Entity<Prescripcion>()
                .Property(p => p.Costo)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Donacion>()
                .HasKey(d => d.IdDonacion);

            modelBuilder.Entity<Donacion>()
                .Property(d => d.Monto)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Gasto>()
                .HasKey(g => g.IdGasto);

            modelBuilder.Entity<Gasto>()
                .Property(g => g.Monto)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Cobro>()
                .HasKey(c => c.IdCobro);

            modelBuilder.Entity<Cobro>()
                .Property(c => c.Monto)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Cobro>()
                .HasOne(c => c.Familiar)
                .WithMany()
                .HasForeignKey(c => c.IdFamiliar);

            modelBuilder.Entity<Cobro>()
                .HasOne(c => c.VisitaMedica)
                .WithMany()
                .HasForeignKey(c => c.IdVisitaMedica)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}