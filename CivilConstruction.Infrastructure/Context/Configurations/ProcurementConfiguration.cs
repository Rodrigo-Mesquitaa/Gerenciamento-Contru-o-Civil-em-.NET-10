using CivilConstruction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivilConstruction.Infrastructure.Context.Configurations;

internal sealed class OrdemCompraConfiguration : IEntityTypeConfiguration<OrdemCompra>
{
    public void Configure(EntityTypeBuilder<OrdemCompra> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasMany(x => x.Itens).WithOne().HasForeignKey(x => x.OrdemCompraId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Obra>().WithMany().HasForeignKey(x => x.ObraId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Fornecedor>().WithMany().HasForeignKey(x => x.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ItemOrdemCompraConfiguration : IEntityTypeConfiguration<ItemOrdemCompra>
{
    public void Configure(EntityTypeBuilder<ItemOrdemCompra> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Unidade).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Quantidade).HasPrecision(18, 3);
        builder.Property(x => x.ValorUnitario).HasPrecision(18, 2);
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_ItensOrdemCompra_Quantidade_Positive", "[Quantidade] > 0");
            table.HasCheckConstraint("CK_ItensOrdemCompra_ValorUnitario_NonNegative", "[ValorUnitario] >= 0");
        });
    }
}

internal sealed class ItemEstoqueConfiguration : IEntityTypeConfiguration<ItemEstoque>
{
    public void Configure(EntityTypeBuilder<ItemEstoque> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Unidade).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Quantidade).HasPrecision(18, 3);
        builder.Property(x => x.EstoqueMinimo).HasPrecision(18, 3);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => x.Codigo).IsUnique();
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_ItensEstoque_Quantidades_NonNegative", "[Quantidade] >= 0 AND [EstoqueMinimo] >= 0"));
    }
}

internal sealed class EquipamentoConfiguration : IEntityTypeConfiguration<Equipamento>
{
    public void Configure(EntityTypeBuilder<Equipamento> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NumeroPatrimonio).HasMaxLength(50).IsRequired();
        builder.Property(x => x.NumeroSerie).HasMaxLength(100);
        builder.Property(x => x.ValorAquisicao).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => x.NumeroPatrimonio).IsUnique();
        builder.HasOne<Obra>().WithMany().HasForeignKey(x => x.ObraId).OnDelete(DeleteBehavior.SetNull);
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_Equipamentos_ValorAquisicao_NonNegative", "[ValorAquisicao] >= 0"));
    }
}
