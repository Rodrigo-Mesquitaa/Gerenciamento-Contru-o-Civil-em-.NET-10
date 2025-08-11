using CivilConstruction.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivilConstruction.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Funcionario> Funcionarios { get; set; } = null!;
        public DbSet<Receita> Receitas { get; set; } = null!;
        public DbSet<Despesa> Despesas { get; set; } = null!;
        public DbSet<Caixa> Caixas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Funcionario>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Nome).IsRequired().HasMaxLength(200);
                b.Property(x => x.ValorDiaria).IsRequired();
            });

            modelBuilder.Entity<Receita>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Descricao).HasMaxLength(500);
                b.Property(x => x.Valor).IsRequired();
            });

            modelBuilder.Entity<Despesa>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Descricao).HasMaxLength(500);
                b.Property(x => x.Valor).IsRequired();
            });

            modelBuilder.Entity<Caixa>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.SaldoInicial).IsRequired();
                b.Property(x => x.SaldoAtual).IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
