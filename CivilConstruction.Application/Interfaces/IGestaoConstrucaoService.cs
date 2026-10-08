using CivilConstruction.Application.DTOs;
using CivilConstruction.Domain.Entities;

namespace CivilConstruction.Application.Interfaces;

public interface IGestaoConstrucaoService
{
    Task<IReadOnlyList<ObraResponse>> ListarObrasAsync(CancellationToken ct);
    Task<ObraResponse?> ObterObraAsync(Guid id, CancellationToken ct);
    Task<ObraResponse> CriarObraAsync(CriarObraRequest request, CancellationToken ct);
    Task<ObraResponse?> AlterarStatusObraAsync(Guid id, StatusObra status, CancellationToken ct);
    Task<IReadOnlyList<FornecedorResponse>> ListarFornecedoresAsync(CancellationToken ct);
    Task<FornecedorResponse?> ObterFornecedorAsync(Guid id, CancellationToken ct);
    Task<FornecedorResponse> CriarFornecedorAsync(CriarFornecedorRequest request, CancellationToken ct);
    Task<IReadOnlyList<OrdemCompraResponse>> ListarOrdensCompraAsync(CancellationToken ct);
    Task<OrdemCompraResponse?> ObterOrdemCompraPorIdAsync(Guid id, CancellationToken ct);
    Task<OrdemCompraResponse> CriarOrdemCompraAsync(CriarOrdemCompraRequest request, CancellationToken ct);
    Task<OrdemCompraResponse?> AlterarStatusOrdemCompraAsync(Guid id, StatusOrdemCompra status, CancellationToken ct);
    Task<IReadOnlyList<ItemEstoqueResponse>> ListarEstoqueAsync(CancellationToken ct);
    Task<IReadOnlyList<EquipamentoResponse>> ListarEquipamentosAsync(CancellationToken ct);
    Task<EquipamentoResponse?> ObterEquipamentoAsync(Guid id, CancellationToken ct);
    Task<EquipamentoResponse> CriarEquipamentoAsync(CriarEquipamentoRequest request, CancellationToken ct);
    Task<EquipamentoResponse?> AlterarStatusEquipamentoAsync(
        Guid id, StatusEquipamento status, Guid? obraId, CancellationToken ct);
    Task<IReadOnlyList<ProgramacaoPagamentoResponse>> ListarProgramacoesPagamentoAsync(CancellationToken ct);
    Task<ProgramacaoPagamentoResponse?> ObterProgramacaoPagamentoAsync(Guid id, CancellationToken ct);
    Task<ProgramacaoPagamentoResponse> CriarProgramacaoPagamentoAsync(CriarProgramacaoPagamentoRequest request, CancellationToken ct);
    Task<ProgramacaoPagamentoResponse?> AlterarAtividadeProgramacaoPagamentoAsync(
        Guid id, bool ativa, CancellationToken ct);
    Task<IReadOnlyList<PagamentoFuncionarioResponse>> GerarPagamentosVencidosAsync(DateTimeOffset agora, CancellationToken ct);
    Task<IReadOnlyList<PagamentoFuncionarioResponse>> ListarPagamentosPendentesAsync(CancellationToken ct);
    Task EnfileirarPagamentoAsync(Guid pagamentoId, CancellationToken ct);
}

public interface IGestaoConstrucaoRepository
{
    Task<List<Obra>> ListarObrasAsync(CancellationToken ct);
    Task<Obra?> ObterObraAsync(Guid id, CancellationToken ct);
    Task<Obra?> ObterObraTrackedAsync(Guid id, CancellationToken ct);
    Task<bool> ObraExisteAsync(Guid id, CancellationToken ct);
    Task AdicionarObraAsync(Obra obra, CancellationToken ct);
    Task<List<Fornecedor>> ListarFornecedoresAsync(CancellationToken ct);
    Task<Fornecedor?> ObterFornecedorAsync(Guid id, CancellationToken ct);
    Task<bool> FornecedorExisteAsync(Guid id, CancellationToken ct);
    Task<bool> DocumentoFornecedorExisteAsync(string documento, CancellationToken ct);
    Task AdicionarFornecedorAsync(Fornecedor fornecedor, CancellationToken ct);
    Task<List<OrdemCompra>> ListarOrdensCompraAsync(CancellationToken ct);
    Task<OrdemCompra?> ObterOrdemCompraPorIdAsync(Guid id, CancellationToken ct);
    Task<OrdemCompra?> ObterOrdemCompraAsync(Guid id, CancellationToken ct);
    Task AdicionarOrdemCompraAsync(OrdemCompra ordem, CancellationToken ct);
    Task AdicionarOuAtualizarEstoqueAsync(ItemOrdemCompra item, CancellationToken ct);
    Task<List<ItemEstoque>> ListarEstoqueAsync(CancellationToken ct);
    Task<List<Equipamento>> ListarEquipamentosAsync(CancellationToken ct);
    Task<Equipamento?> ObterEquipamentoAsync(Guid id, CancellationToken ct);
    Task<Equipamento?> ObterEquipamentoTrackedAsync(Guid id, CancellationToken ct);
    Task<bool> EquipamentoExisteAsync(string numeroPatrimonio, CancellationToken ct);
    Task<bool> ObraExisteAsync(Guid? id, CancellationToken ct);
    Task AdicionarEquipamentoAsync(Equipamento equipamento, CancellationToken ct);
    Task<List<ProgramacaoPagamento>> ListarProgramacoesPagamentoAsync(CancellationToken ct);
    Task<ProgramacaoPagamento?> ObterProgramacaoPagamentoAsync(Guid id, CancellationToken ct);
    Task<ProgramacaoPagamento?> ObterProgramacaoPagamentoTrackedAsync(Guid id, CancellationToken ct);
    Task<bool> FuncionarioExisteAsync(Guid id, CancellationToken ct);
    Task AdicionarProgramacaoPagamentoAsync(ProgramacaoPagamento programacao, CancellationToken ct);
    Task<List<ProgramacaoPagamento>> ObterProgramacoesVencidasAsync(DateTimeOffset agora, CancellationToken ct);
    Task<bool> PagamentoExisteAsync(Guid programacaoId, DateTimeOffset competencia, CancellationToken ct);
    Task AdicionarPagamentoAsync(PagamentoFuncionario pagamento, CancellationToken ct);
    Task<List<PagamentoFuncionario>> ListarPagamentosPendentesAsync(CancellationToken ct);
    Task<PagamentoFuncionario?> ObterPagamentoAsync(Guid id, CancellationToken ct);
}
