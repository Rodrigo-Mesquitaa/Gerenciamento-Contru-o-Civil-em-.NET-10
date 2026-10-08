using CivilConstruction.Domain.Entities;
using CivilConstruction.Infrastructure.EventSourcing;
using CivilConstruction.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

namespace CivilConstruction.Infrastructure.Context;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<Receita> Receitas => Set<Receita>();
    public DbSet<Despesa> Despesas => Set<Despesa>();
    public DbSet<Caixa> Caixas => Set<Caixa>();
    public DbSet<Obra> Obras => Set<Obra>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<OrdemCompra> OrdensCompra => Set<OrdemCompra>();
    public DbSet<ItemOrdemCompra> ItensOrdemCompra => Set<ItemOrdemCompra>();
    public DbSet<ItemEstoque> ItensEstoque => Set<ItemEstoque>();
    public DbSet<Equipamento> Equipamentos => Set<Equipamento>();
    public DbSet<ProgramacaoPagamento> ProgramacoesPagamento => Set<ProgramacaoPagamento>();
    public DbSet<PagamentoFuncionario> PagamentosFuncionarios => Set<PagamentoFuncionario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<MedicaoContrato> MedicoesContrato => Set<MedicaoContrato>();
    public DbSet<LancamentoFinanceiro> LancamentosFinanceiros => Set<LancamentoFinanceiro>();
    public DbSet<ApontamentoObra> ApontamentosObra => Set<ApontamentoObra>();
    public DbSet<AlocacaoFuncionarioObra> AlocacoesFuncionarios => Set<AlocacaoFuncionarioObra>();
    public DbSet<IncidenteSeguranca> IncidentesSeguranca => Set<IncidenteSeguranca>();
    public DbSet<InspecaoQualidade> InspecoesQualidade => Set<InspecaoQualidade>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<EventStream> EventStreams => Set<EventStream>();
    public DbSet<DomainEventRecord> DomainEventRecords => Set<DomainEventRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
