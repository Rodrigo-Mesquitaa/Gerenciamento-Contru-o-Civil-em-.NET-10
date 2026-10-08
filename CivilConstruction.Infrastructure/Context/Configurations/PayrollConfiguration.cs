using CivilConstruction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivilConstruction.Infrastructure.Context.Configurations;

internal sealed class ProgramacaoPagamentoConfiguration : IEntityTypeConfiguration<ProgramacaoPagamento>
{
    public void Configure(EntityTypeBuilder<ProgramacaoPagamento> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ValorBruto).HasPrecision(18, 2);
        builder.Property(x => x.Periodicidade).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => new { x.Ativa, x.ProximoPagamentoEm });
        builder.HasOne<Funcionario>().WithMany().HasForeignKey(x => x.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_ProgramacoesPagamento_ValorBruto_Positive", "[ValorBruto] > 0"));
    }
}

internal sealed class PagamentoFuncionarioConfiguration : IEntityTypeConfiguration<PagamentoFuncionario>
{
    public void Configure(EntityTypeBuilder<PagamentoFuncionario> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ValorBruto).HasPrecision(18, 2);
        builder.HasIndex(x => new { x.ProgramacaoPagamentoId, x.Competencia }).IsUnique();
        builder.HasOne<ProgramacaoPagamento>().WithMany().HasForeignKey(x => x.ProgramacaoPagamentoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Funcionario>().WithMany().HasForeignKey(x => x.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_PagamentosFuncionarios_ValorBruto_Positive", "[ValorBruto] > 0"));
    }
}
