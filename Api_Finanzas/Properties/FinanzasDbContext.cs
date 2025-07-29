using Api_Finanzas.Models;
using Microsoft.EntityFrameworkCore;


namespace Api_Finanzas.Properties
{

    public class FinanzasDbContext : DbContext
    {
        public FinanzasDbContext(DbContextOptions<FinanzasDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<CuentaBancaria> CuentasBancarias { get; set; }
        public DbSet<CategoriaGasto> CategoriasGasto { get; set; }
        public DbSet<CategoriaIngreso> CategoriasIngreso { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }
        public DbSet<Presupuesto> Presupuestos { get; set; }
        public DbSet<MetaAhorro> MetasAhorro { get; set; }
        public DbSet<Alerta> Alertas { get; set; }
        public DbSet<TipoCambio> TiposCambio { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName().ToLower());

                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.Name.ToLower());
                }
            }
            modelBuilder.Entity<CategoriaGasto>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CategoriaIngreso>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Transaccion>()
                .HasOne(t => t.CategoriaGasto)
                .WithMany(c => c.Transacciones)
                .HasForeignKey(t => t.CategoriaGastoId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Transaccion>()
                .HasOne(t => t.CategoriaIngreso)
                .WithMany(c => c.Transacciones)
                .HasForeignKey(t => t.CategoriaIngresoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relación Usuario ↔ Presupuesto
            modelBuilder.Entity<Presupuesto>()
           .HasOne(p => p.Usuario)
           .WithMany(u => u.Presupuestos)
           .HasForeignKey(p => p.UsuarioId)
           .OnDelete(DeleteBehavior.Restrict);

            // CategoriaGasto ↔ Presupuesto
            modelBuilder.Entity<Presupuesto>()
                .HasOne(p => p.CategoriaGasto)
                .WithMany(cg => cg.Presupuestos)
                .HasForeignKey(p => p.CategoriaGastoId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Usuario>()
                  .HasMany(u => u.Cuentas)
                  .WithOne(c => c.Usuario)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
