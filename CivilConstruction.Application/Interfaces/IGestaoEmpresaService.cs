using CivilConstruction.Application.DTOs;
using CivilConstruction.Domain.Entities;

namespace CivilConstruction.Application.Interfaces;

public interface IGestaoEmpresaService
{
    Task<IReadOnlyList<ClienteResponse>> ListarClientesAsync(CancellationToken ct);
    Task<ClienteResponse?> ObterClienteAsync(Guid id, CancellationToken ct);
    Task<ClienteResponse> CriarClienteAsync(CriarClienteRequest request, CancellationToken ct);
    Task<IReadOnlyList<ContratoResponse>> ListarContratosAsync(CancellationToken ct);
    Task<ContratoResponse?> ObterContratoAsync(Guid id, CancellationToken ct);
    Task<ContratoResponse> CriarContratoAsync(CriarContratoRequest request, CancellationToken ct);
    Task<ContratoResponse?> AlterarStatusContratoAsync(Guid id, StatusContrato status, CancellationToken ct);
    Task<IReadOnlyList<MedicaoResponse>> ListarMedicoesAsync(Guid contratoId, CancellationToken ct);
    Task<MedicaoResponse> CriarMedicaoAsync(Guid contratoId, CriarMedicaoRequest request, CancellationToken ct);
    Task<MedicaoResponse?> AlterarStatusMedicaoAsync(
        Guid id, StatusMedicao status, DateTimeOffset? vencimento, CancellationToken ct);
    Task<IReadOnlyList<LancamentoFinanceiroResponse>> ListarLancamentosAsync(
        TipoLancamentoFinanceiro? tipo, CancellationToken ct);
    Task<LancamentoFinanceiroResponse> CriarLancamentoAsync(
        CriarLancamentoFinanceiroRequest request, CancellationToken ct);
    Task<LancamentoFinanceiroResponse?> ObterLancamentoAsync(Guid id, CancellationToken ct);
    Task<LancamentoFinanceiroResponse?> LiquidarLancamentoAsync(
        Guid id, DateTimeOffset liquidadoEm, CancellationToken ct);
    Task<IReadOnlyList<ApontamentoObraResponse>> ListarApontamentosAsync(Guid obraId, CancellationToken ct);
    Task<ApontamentoObraResponse> CriarApontamentoAsync(CriarApontamentoObraRequest request, CancellationToken ct);
    Task<IReadOnlyList<AlocacaoFuncionarioResponse>> ListarAlocacoesAsync(Guid obraId, CancellationToken ct);
    Task<AlocacaoFuncionarioResponse> CriarAlocacaoAsync(CriarAlocacaoFuncionarioRequest request, CancellationToken ct);
    Task<AlocacaoFuncionarioResponse?> EncerrarAlocacaoAsync(Guid id, DateTimeOffset fim, CancellationToken ct);
    Task<IReadOnlyList<IncidenteResponse>> ListarIncidentesAsync(Guid obraId, CancellationToken ct);
    Task<IncidenteResponse> CriarIncidenteAsync(CriarIncidenteRequest request, CancellationToken ct);
    Task<IncidenteResponse?> AtualizarIncidenteAsync(Guid id, AtualizarIncidenteRequest request, CancellationToken ct);
    Task<IReadOnlyList<InspecaoQualidadeResponse>> ListarInspecoesAsync(Guid obraId, CancellationToken ct);
    Task<InspecaoQualidadeResponse> CriarInspecaoAsync(CriarInspecaoQualidadeRequest request, CancellationToken ct);
    Task<InspecaoQualidadeResponse?> AtualizarAcaoCorretivaAsync(
        Guid id, AtualizarAcaoCorretivaRequest request, CancellationToken ct);
}

public interface IGestaoEmpresaRepository
{
    Task<List<Cliente>> ListarClientesAsync(CancellationToken ct);
    Task<Cliente?> ObterClienteAsync(Guid id, CancellationToken ct);
    Task<Cliente?> ObterClienteTrackedAsync(Guid id, CancellationToken ct);
    Task<bool> DocumentoClienteExisteAsync(string documento, CancellationToken ct);
    Task AdicionarClienteAsync(Cliente cliente, CancellationToken ct);
    Task<List<Contrato>> ListarContratosAsync(CancellationToken ct);
    Task<Contrato?> ObterContratoAsync(Guid id, CancellationToken ct);
    Task<Contrato?> ObterContratoTrackedAsync(Guid id, CancellationToken ct);
    Task<bool> ContratoNumeroExisteAsync(string numero, CancellationToken ct);
    Task<decimal> ObterTotalMedidoAsync(Guid contratoId, CancellationToken ct);
    Task AdicionarContratoAsync(Contrato contrato, CancellationToken ct);
    Task<List<MedicaoContrato>> ListarMedicoesAsync(Guid contratoId, CancellationToken ct);
    Task<MedicaoContrato?> ObterMedicaoTrackedAsync(Guid id, CancellationToken ct);
    Task<bool> ReferenciaMedicaoExisteAsync(Guid contratoId, string referencia, CancellationToken ct);
    Task AdicionarMedicaoAsync(MedicaoContrato medicao, CancellationToken ct);
    Task<bool> LancamentoMedicaoExisteAsync(Guid medicaoId, CancellationToken ct);
    Task AdicionarLancamentoAsync(LancamentoFinanceiro lancamento, CancellationToken ct);
    Task<List<LancamentoFinanceiro>> ListarLancamentosAsync(TipoLancamentoFinanceiro? tipo, CancellationToken ct);
    Task<LancamentoFinanceiro?> ObterLancamentoTrackedAsync(Guid id, CancellationToken ct);
    Task<LancamentoFinanceiro?> ObterLancamentoAsync(Guid id, CancellationToken ct);
    Task<bool> ObraExisteAsync(Guid id, CancellationToken ct);
    Task<bool> ClienteExisteAsync(Guid id, CancellationToken ct);
    Task<bool> FornecedorExisteAsync(Guid id, CancellationToken ct);
    Task<bool> MedicaoExisteAsync(Guid id, CancellationToken ct);
    Task<List<ApontamentoObra>> ListarApontamentosAsync(Guid obraId, CancellationToken ct);
    Task<bool> ApontamentoDataExisteAsync(Guid obraId, DateTimeOffset data, CancellationToken ct);
    Task AdicionarApontamentoAsync(ApontamentoObra apontamento, CancellationToken ct);
    Task<List<AlocacaoFuncionarioObra>> ListarAlocacoesAsync(Guid obraId, CancellationToken ct);
    Task<bool> FuncionarioAtivoExisteAsync(Guid id, CancellationToken ct);
    Task<bool> AlocacaoAtivaExisteAsync(Guid funcionarioId, CancellationToken ct);
    Task AdicionarAlocacaoAsync(AlocacaoFuncionarioObra alocacao, CancellationToken ct);
    Task<AlocacaoFuncionarioObra?> ObterAlocacaoTrackedAsync(Guid id, CancellationToken ct);
    Task<List<IncidenteSeguranca>> ListarIncidentesAsync(Guid obraId, CancellationToken ct);
    Task AdicionarIncidenteAsync(IncidenteSeguranca incidente, CancellationToken ct);
    Task<IncidenteSeguranca?> ObterIncidenteTrackedAsync(Guid id, CancellationToken ct);
    Task<List<InspecaoQualidade>> ListarInspecoesAsync(Guid obraId, CancellationToken ct);
    Task<bool> ReferenciaInspecaoExisteAsync(Guid obraId, string referencia, CancellationToken ct);
    Task AdicionarInspecaoAsync(InspecaoQualidade inspecao, CancellationToken ct);
    Task<InspecaoQualidade?> ObterInspecaoTrackedAsync(Guid id, CancellationToken ct);
}
