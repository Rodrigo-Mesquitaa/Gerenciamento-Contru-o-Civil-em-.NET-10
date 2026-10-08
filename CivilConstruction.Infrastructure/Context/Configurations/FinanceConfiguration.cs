using CivilConstruction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivilConstruction.Infrastructure.Context.Configurations;

internal sealed class ReceitaConfiguration : IEntityTypeConfiguration<Receita>
{
    public void Configure(EntityTypeBuilder<Receita> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Descricao).HasMaxLength(500);
        builder.Property(x => x.Valor).HasPrecision(18, 2).IsRequired();
    }
}

internal sealed class DespesaConfiguration : IEntityTypeConfiguration<Despesa>
{
    public void Configure(EntityTypeBuilder<Despesa> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Descricao).HasMaxLength(500);
        builder.Property(x => x.Valor).HasPrecision(18, 2).IsRequired();
    }
}

internal sealed class CaixaConfiguration : IEntityTypeConfiguration<Caixa>
{
    public void Configure(EntityTypeBuilder<Caixa> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SaldoInicial).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.SaldoAtual).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

internal sealed class LancamentoFinanceiroConfiguration : IEntityTypeConfiguration<LancamentoFinanceiro>
{
    public void Configure(EntityTypeBuilder<LancamentoFinanceiro> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Descricao).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Valor).HasPrecision(18, 2);
        builder.Property(x => x.ReferenciaExterna).HasMaxLength(100);
        builder.HasIndex(x => new { x.Tipo, x.Status, x.Vencimento });
        builder.HasOne<Obra>().WithMany().HasForeignKey(x => x.ObraId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Fornecedor>().WithMany().HasForeignKey(x => x.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MedicaoContrato>().WithMany().HasForeignKey(x => x.MedicaoContratoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_LancamentosFinanceiros_Valor_Positive", "[Valor] > 0"));
    }
}
