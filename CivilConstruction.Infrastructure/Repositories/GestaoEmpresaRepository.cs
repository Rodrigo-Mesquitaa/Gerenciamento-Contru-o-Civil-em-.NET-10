using CivilConstruction.Application.Interfaces;
using CivilConstruction.Domain.Entities;
using CivilConstruction.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CivilConstruction.Infrastructure.Repositories;

public sealed class GestaoEmpresaRepository(AppDbContext db) : IGestaoEmpresaRepository
{
    public Task<List<Cliente>> ListarClientesAsync(CancellationToken ct) =>
        db.Clientes.AsNoTracking().OrderBy(x => x.Nome).ToListAsync(ct);
    public Task<Cliente?> ObterClienteAsync(Guid id, CancellationToken ct) =>
        db.Clientes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<Cliente?> ObterClienteTrackedAsync(Guid id, CancellationToken ct) =>
        db.Clientes.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> DocumentoClienteExisteAsync(string documento, CancellationToken ct) =>
        db.Clientes.AnyAsync(x => x.Documento == documento, ct);
    public Task AdicionarClienteAsync(Cliente cliente, CancellationToken ct) =>
        db.Clientes.AddAsync(cliente, ct).AsTask();

    public Task<List<Contrato>> ListarContratosAsync(CancellationToken ct) =>
        db.Contratos.AsNoTracking().OrderByDescending(x => x.DataAssinatura).ToListAsync(ct);
    public Task<Contrato?> ObterContratoAsync(Guid id, CancellationToken ct) =>
        db.Contratos.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<Contrato?> ObterContratoTrackedAsync(Guid id, CancellationToken ct) =>
        db.Contratos.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> ContratoNumeroExisteAsync(string numero, CancellationToken ct) =>
        db.Contratos.AnyAsync(x => x.Numero == numero, ct);
    public async Task<decimal> ObterTotalMedidoAsync(Guid contratoId, CancellationToken ct) =>
        await db.MedicoesContrato.Where(x => x.ContratoId == contratoId &&
            x.Status != StatusMedicao.Rejeitada).SumAsync(x => x.PercentualExecutado, ct);
    public Task AdicionarContratoAsync(Contrato contrato, CancellationToken ct) =>
        db.Contratos.AddAsync(contrato, ct).AsTask();

    public Task<List<MedicaoContrato>> ListarMedicoesAsync(Guid contratoId, CancellationToken ct) =>
        db.MedicoesContrato.AsNoTracking().Where(x => x.ContratoId == contratoId)
            .OrderByDescending(x => x.DataMedicao).ToListAsync(ct);
    public Task<MedicaoContrato?> ObterMedicaoTrackedAsync(Guid id, CancellationToken ct) =>
        db.MedicoesContrato.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> ReferenciaMedicaoExisteAsync(Guid contratoId, string referencia, CancellationToken ct) =>
        db.MedicoesContrato.AnyAsync(x => x.ContratoId == contratoId && x.Referencia == referencia, ct);
    public Task AdicionarMedicaoAsync(MedicaoContrato medicao, CancellationToken ct) =>
        db.MedicoesContrato.AddAsync(medicao, ct).AsTask();
    public Task<bool> LancamentoMedicaoExisteAsync(Guid medicaoId, CancellationToken ct) =>
        db.LancamentosFinanceiros.AnyAsync(x => x.MedicaoContratoId == medicaoId, ct);

    public Task AdicionarLancamentoAsync(LancamentoFinanceiro lancamento, CancellationToken ct) =>
        db.LancamentosFinanceiros.AddAsync(lancamento, ct).AsTask();
    public Task<List<LancamentoFinanceiro>> ListarLancamentosAsync(
        TipoLancamentoFinanceiro? tipo, CancellationToken ct)
    {
        var query = db.LancamentosFinanceiros.AsNoTracking();
        if (tipo is not null) query = query.Where(x => x.Tipo == tipo);
        return query.OrderBy(x => x.Vencimento).ToListAsync(ct);
    }
    public Task<LancamentoFinanceiro?> ObterLancamentoTrackedAsync(Guid id, CancellationToken ct) =>
        db.LancamentosFinanceiros.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<LancamentoFinanceiro?> ObterLancamentoAsync(Guid id, CancellationToken ct) =>
        db.LancamentosFinanceiros.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> ObraExisteAsync(Guid id, CancellationToken ct) =>
        db.Obras.AnyAsync(x => x.Id == id, ct);
    public Task<bool> ClienteExisteAsync(Guid id, CancellationToken ct) =>
        db.Clientes.AnyAsync(x => x.Id == id, ct);
    public Task<bool> FornecedorExisteAsync(Guid id, CancellationToken ct) =>
        db.Fornecedores.AnyAsync(x => x.Id == id, ct);
    public Task<bool> MedicaoExisteAsync(Guid id, CancellationToken ct) =>
        db.MedicoesContrato.AnyAsync(x => x.Id == id, ct);

    public Task<List<ApontamentoObra>> ListarApontamentosAsync(Guid obraId, CancellationToken ct) =>
        db.ApontamentosObra.AsNoTracking().Where(x => x.ObraId == obraId)
            .OrderByDescending(x => x.Data).ToListAsync(ct);
    public Task<bool> ApontamentoDataExisteAsync(Guid obraId, DateTimeOffset data, CancellationToken ct) =>
        db.ApontamentosObra.AnyAsync(x => x.ObraId == obraId && x.Data == data, ct);
    public Task AdicionarApontamentoAsync(ApontamentoObra apontamento, CancellationToken ct) =>
        db.ApontamentosObra.AddAsync(apontamento, ct).AsTask();

    public Task<List<AlocacaoFuncionarioObra>> ListarAlocacoesAsync(Guid obraId, CancellationToken ct) =>
        db.AlocacoesFuncionarios.AsNoTracking().Where(x => x.ObraId == obraId)
            .OrderByDescending(x => x.Inicio).ToListAsync(ct);
    public Task<bool> FuncionarioAtivoExisteAsync(Guid id, CancellationToken ct) =>
        db.Funcionarios.AnyAsync(x => x.Id == id && x.Ativo, ct);
    public Task<bool> AlocacaoAtivaExisteAsync(Guid funcionarioId, CancellationToken ct) =>
        db.AlocacoesFuncionarios.AnyAsync(x => x.FuncionarioId == funcionarioId && x.Fim == null, ct);
    public Task AdicionarAlocacaoAsync(AlocacaoFuncionarioObra alocacao, CancellationToken ct) =>
        db.AlocacoesFuncionarios.AddAsync(alocacao, ct).AsTask();
    public Task<AlocacaoFuncionarioObra?> ObterAlocacaoTrackedAsync(Guid id, CancellationToken ct) =>
        db.AlocacoesFuncionarios.SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<IncidenteSeguranca>> ListarIncidentesAsync(Guid obraId, CancellationToken ct) =>
        db.IncidentesSeguranca.AsNoTracking().Where(x => x.ObraId == obraId)
            .OrderByDescending(x => x.OcorreuEm).ToListAsync(ct);
    public Task AdicionarIncidenteAsync(IncidenteSeguranca incidente, CancellationToken ct) =>
        db.IncidentesSeguranca.AddAsync(incidente, ct).AsTask();
    public Task<IncidenteSeguranca?> ObterIncidenteTrackedAsync(Guid id, CancellationToken ct) =>
        db.IncidentesSeguranca.SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<InspecaoQualidade>> ListarInspecoesAsync(Guid obraId, CancellationToken ct) =>
        db.InspecoesQualidade.AsNoTracking().Where(x => x.ObraId == obraId)
            .OrderByDescending(x => x.InspecionadaEm).ToListAsync(ct);
    public Task<bool> ReferenciaInspecaoExisteAsync(Guid obraId, string referencia, CancellationToken ct) =>
        db.InspecoesQualidade.AnyAsync(x => x.ObraId == obraId && x.Referencia == referencia, ct);
    public Task AdicionarInspecaoAsync(InspecaoQualidade inspecao, CancellationToken ct) =>
        db.InspecoesQualidade.AddAsync(inspecao, ct).AsTask();
    public Task<InspecaoQualidade?> ObterInspecaoTrackedAsync(Guid id, CancellationToken ct) =>
        db.InspecoesQualidade.SingleOrDefaultAsync(x => x.Id == id, ct);
}
