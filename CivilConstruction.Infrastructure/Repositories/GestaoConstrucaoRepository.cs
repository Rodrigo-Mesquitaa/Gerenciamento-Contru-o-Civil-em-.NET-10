using CivilConstruction.Application.Interfaces;
using CivilConstruction.Domain.Entities;
using CivilConstruction.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CivilConstruction.Infrastructure.Repositories;

public sealed class GestaoConstrucaoRepository(AppDbContext db) : IGestaoConstrucaoRepository
{
    public Task<List<Obra>> ListarObrasAsync(CancellationToken ct) =>
        db.Obras.AsNoTracking().OrderByDescending(x => x.DataInicioPrevista).ToListAsync(ct);
    public Task<Obra?> ObterObraAsync(Guid id, CancellationToken ct) =>
        db.Obras.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<Obra?> ObterObraTrackedAsync(Guid id, CancellationToken ct) =>
        db.Obras.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> ObraExisteAsync(Guid id, CancellationToken ct) =>
        db.Obras.AnyAsync(x => x.Id == id, ct);
    public Task<bool> ObraExisteAsync(Guid? id, CancellationToken ct) =>
        id is null ? Task.FromResult(true) : db.Obras.AnyAsync(x => x.Id == id, ct);
    public Task AdicionarObraAsync(Obra obra, CancellationToken ct) => db.Obras.AddAsync(obra, ct).AsTask();

    public Task<List<Fornecedor>> ListarFornecedoresAsync(CancellationToken ct) =>
        db.Fornecedores.AsNoTracking().OrderBy(x => x.Nome).ToListAsync(ct);
    public Task<Fornecedor?> ObterFornecedorAsync(Guid id, CancellationToken ct) =>
        db.Fornecedores.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> FornecedorExisteAsync(Guid id, CancellationToken ct) =>
        db.Fornecedores.AnyAsync(x => x.Id == id, ct);
    public Task<bool> DocumentoFornecedorExisteAsync(string documento, CancellationToken ct) =>
        db.Fornecedores.AnyAsync(x => x.Documento == documento, ct);
    public Task AdicionarFornecedorAsync(Fornecedor fornecedor, CancellationToken ct) =>
        db.Fornecedores.AddAsync(fornecedor, ct).AsTask();

    public Task<List<OrdemCompra>> ListarOrdensCompraAsync(CancellationToken ct) =>
        db.OrdensCompra.AsNoTracking().Include(x => x.Itens)
            .OrderByDescending(x => x.CriadaEm).ToListAsync(ct);
    public Task<OrdemCompra?> ObterOrdemCompraPorIdAsync(Guid id, CancellationToken ct) =>
        db.OrdensCompra.AsNoTracking().Include(x => x.Itens).SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<OrdemCompra?> ObterOrdemCompraAsync(Guid id, CancellationToken ct) =>
        db.OrdensCompra.Include(x => x.Itens).SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task AdicionarOrdemCompraAsync(OrdemCompra ordem, CancellationToken ct) =>
        db.OrdensCompra.AddAsync(ordem, ct).AsTask();

    public async Task AdicionarOuAtualizarEstoqueAsync(ItemOrdemCompra item, CancellationToken ct)
    {
        var estoque = db.ItensEstoque.Local.FirstOrDefault(
            x => string.Equals(x.Codigo, item.Codigo, StringComparison.OrdinalIgnoreCase));
        estoque ??= await db.ItensEstoque.SingleOrDefaultAsync(x => x.Codigo == item.Codigo, ct);
        if (estoque is null)
        {
            db.ItensEstoque.Add(new ItemEstoque
            {
                Codigo = item.Codigo,
                Descricao = item.Descricao,
                Unidade = item.Unidade,
                Quantidade = item.Quantidade
            });
            return;
        }

        if (!string.Equals(estoque.Unidade, item.Unidade, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"A unidade do item {item.Codigo} não corresponde ao cadastro do estoque.");

        estoque.Quantidade += item.Quantidade;
        estoque.Descricao = item.Descricao;
    }

    public Task<List<ItemEstoque>> ListarEstoqueAsync(CancellationToken ct) =>
        db.ItensEstoque.AsNoTracking().OrderBy(x => x.Descricao).ToListAsync(ct);
    public Task<List<Equipamento>> ListarEquipamentosAsync(CancellationToken ct) =>
        db.Equipamentos.AsNoTracking().OrderBy(x => x.Nome).ToListAsync(ct);
    public Task<Equipamento?> ObterEquipamentoAsync(Guid id, CancellationToken ct) =>
        db.Equipamentos.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<Equipamento?> ObterEquipamentoTrackedAsync(Guid id, CancellationToken ct) =>
        db.Equipamentos.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> EquipamentoExisteAsync(string numeroPatrimonio, CancellationToken ct) =>
        db.Equipamentos.AnyAsync(x => x.NumeroPatrimonio == numeroPatrimonio, ct);
    public Task AdicionarEquipamentoAsync(Equipamento equipamento, CancellationToken ct) =>
        db.Equipamentos.AddAsync(equipamento, ct).AsTask();

    public Task<List<ProgramacaoPagamento>> ListarProgramacoesPagamentoAsync(CancellationToken ct) =>
        db.ProgramacoesPagamento.AsNoTracking().OrderBy(x => x.ProximoPagamentoEm).ToListAsync(ct);
    public Task<ProgramacaoPagamento?> ObterProgramacaoPagamentoAsync(Guid id, CancellationToken ct) =>
        db.ProgramacoesPagamento.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<ProgramacaoPagamento?> ObterProgramacaoPagamentoTrackedAsync(Guid id, CancellationToken ct) =>
        db.ProgramacoesPagamento.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> FuncionarioExisteAsync(Guid id, CancellationToken ct) =>
        db.Funcionarios.AnyAsync(x => x.Id == id && x.Ativo, ct);
    public Task AdicionarProgramacaoPagamentoAsync(ProgramacaoPagamento programacao, CancellationToken ct) =>
        db.ProgramacoesPagamento.AddAsync(programacao, ct).AsTask();
    public Task<List<ProgramacaoPagamento>> ObterProgramacoesVencidasAsync(DateTimeOffset agora, CancellationToken ct) =>
        db.ProgramacoesPagamento.Where(x => x.Ativa && x.ProximoPagamentoEm <= agora).ToListAsync(ct);
    public Task<bool> PagamentoExisteAsync(Guid programacaoId, DateTimeOffset competencia, CancellationToken ct) =>
        db.PagamentosFuncionarios.AnyAsync(
            x => x.ProgramacaoPagamentoId == programacaoId && x.Competencia == competencia, ct);
    public Task AdicionarPagamentoAsync(PagamentoFuncionario pagamento, CancellationToken ct) =>
        db.PagamentosFuncionarios.AddAsync(pagamento, ct).AsTask();
    public Task<List<PagamentoFuncionario>> ListarPagamentosPendentesAsync(CancellationToken ct) =>
        db.PagamentosFuncionarios.Where(x => !x.EventoPublicado)
            .OrderBy(x => x.Competencia).ToListAsync(ct);
    public Task<PagamentoFuncionario?> ObterPagamentoAsync(Guid id, CancellationToken ct) =>
        db.PagamentosFuncionarios.SingleOrDefaultAsync(x => x.Id == id, ct);
}
