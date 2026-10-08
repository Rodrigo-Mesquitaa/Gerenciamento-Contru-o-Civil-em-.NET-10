using CivilConstruction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CivilConstruction.Infrastructure.Context.Configurations;

internal sealed class FuncionarioConfiguration : IEntityTypeConfiguration<Funcionario>
{
    public void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ValorDiaria).HasPrecision(18, 2).IsRequired();
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_Funcionarios_ValorDiaria_Positive", "[ValorDiaria] > 0"));
    }
}

internal sealed class ObraConfiguration : IEntityTypeConfiguration<Obra>
{
    public void Configure(EntityTypeBuilder<Obra> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Endereco).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Cliente).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Orcamento).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_Obras_Orcamento_NonNegative", "[Orcamento] >= 0"));
    }
}

internal sealed class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Documento).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(254);
        builder.Property(x => x.Telefone).HasMaxLength(30);
        builder.HasIndex(x => x.Documento).IsUnique();
    }
}

internal sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Documento).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(254);
        builder.Property(x => x.Telefone).HasMaxLength(30);
        builder.HasIndex(x => x.Documento).IsUnique();
    }
}

internal sealed class ContratoConfiguration : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Numero).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ValorContratado).HasPrecision(18, 2);
        builder.Property(x => x.RetencaoPercentual).HasPrecision(5, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => x.Numero).IsUnique();
        builder.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Obra>().WithMany().HasForeignKey(x => x.ObraId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Contratos_ValorContratado_Positive", "[ValorContratado] > 0");
            table.HasCheckConstraint(
                "CK_Contratos_RetencaoPercentual_Range", "[RetencaoPercentual] BETWEEN 0 AND 100");
        });
    }
}

internal sealed class MedicaoContratoConfiguration : IEntityTypeConfiguration<MedicaoContrato>
{
    public void Configure(EntityTypeBuilder<MedicaoContrato> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Referencia).HasMaxLength(30).IsRequired();
        builder.Property(x => x.PercentualExecutado).HasPrecision(5, 3);
        builder.Property(x => x.ValorMedido).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => new { x.ContratoId, x.Referencia }).IsUnique();
        builder.HasOne<Contrato>().WithMany().HasForeignKey(x => x.ContratoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_MedicoesContrato_PercentualExecutado_Range",
                "[PercentualExecutado] > 0 AND [PercentualExecutado] <= 100");
            table.HasCheckConstraint("CK_MedicoesContrato_ValorMedido_Positive", "[ValorMedido] > 0");
        });
    }
}

internal sealed class ApontamentoObraConfiguration : IEntityTypeConfiguration<ApontamentoObra>
{
    public void Configure(EntityTypeBuilder<ApontamentoObra> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Clima).HasMaxLength(30);
        builder.Property(x => x.Atividades).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.HorasTrabalhadas).HasPrecision(5, 2);
        builder.Property(x => x.Observacoes).HasMaxLength(4000);
        builder.HasIndex(x => new { x.ObraId, x.Data }).IsUnique();
        builder.HasOne<Obra>().WithMany().HasForeignKey(x => x.ObraId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_ApontamentosObra_Indicadores_NonNegative",
            "[TrabalhadoresPresentes] >= 0 AND [HorasTrabalhadas] >= 0 AND [HorasTrabalhadas] <= 24"));
    }
}

internal sealed class AlocacaoFuncionarioObraConfiguration : IEntityTypeConfiguration<AlocacaoFuncionarioObra>
{
    public void Configure(EntityTypeBuilder<AlocacaoFuncionarioObra> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Funcao).HasMaxLength(100);
        builder.HasIndex(x => x.FuncionarioId).IsUnique().HasFilter("[Fim] IS NULL");
        builder.HasOne<Obra>().WithMany().HasForeignKey(x => x.ObraId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Funcionario>().WithMany().HasForeignKey(x => x.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(table => table.HasCheckConstraint(
            "CK_AlocacoesFuncionarios_Periodo_Valid", "[Fim] IS NULL OR [Fim] >= [Inicio]"));
    }
}

internal sealed class IncidenteSegurancaConfiguration : IEntityTypeConfiguration<IncidenteSeguranca>
{
    public void Configure(EntityTypeBuilder<IncidenteSeguranca> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Gravidade).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Descricao).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.AcoesCorretivas).HasMaxLength(4000);
        builder.HasIndex(x => new { x.ObraId, x.Encerrado, x.OcorreuEm });
        builder.HasOne<Obra>().WithMany().HasForeignKey(x => x.ObraId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Funcionario>().WithMany().HasForeignKey(x => x.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class InspecaoQualidadeConfiguration : IEntityTypeConfiguration<InspecaoQualidade>
{
    public void Configure(EntityTypeBuilder<InspecaoQualidade> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Referencia).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Categoria).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Resultado).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Observacoes).HasMaxLength(4000);
        builder.Property(x => x.AcaoCorretiva).HasMaxLength(4000);
        builder.HasIndex(x => new { x.ObraId, x.Referencia }).IsUnique();
        builder.HasIndex(x => new { x.ObraId, x.Encerrada, x.InspecionadaEm });
        builder.HasOne<Obra>().WithMany().HasForeignKey(x => x.ObraId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
