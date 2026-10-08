using CivilConstruction.Application.DTOs;
using CivilConstruction.Application.Interfaces;
using CivilConstruction.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CivilConstruction.API.Controllers;

[ApiController]
[Route("api")]
[Produces("application/json")]
public sealed class GestaoEmpresaController(IGestaoEmpresaService service) : ControllerBase
{
    [HttpGet("clientes")]
    [ProducesResponseType<IReadOnlyList<ClienteResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClienteResponse>>> ListarClientes(CancellationToken ct) =>
        Ok(await service.ListarClientesAsync(ct));

    [HttpGet("clientes/{id:guid}")]
    [ProducesResponseType<ClienteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteResponse>> ObterCliente(Guid id, CancellationToken ct) =>
        await service.ObterClienteAsync(id, ct) is { } cliente ? Ok(cliente) : NotFound();

    [HttpPost("clientes")]
    [ProducesResponseType<ClienteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteResponse>> CriarCliente(CriarClienteRequest request, CancellationToken ct)
    {
        var cliente = await service.CriarClienteAsync(request, ct);
        return CreatedAtAction(nameof(ObterCliente), new { id = cliente.Id }, cliente);
    }

    [HttpGet("contratos")]
    [ProducesResponseType<IReadOnlyList<ContratoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContratoResponse>>> ListarContratos(CancellationToken ct) =>
        Ok(await service.ListarContratosAsync(ct));

    [HttpGet("contratos/{id:guid}")]
    [ProducesResponseType<ContratoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContratoResponse>> ObterContrato(Guid id, CancellationToken ct) =>
        await service.ObterContratoAsync(id, ct) is { } contrato ? Ok(contrato) : NotFound();

    [HttpPost("contratos")]
    [ProducesResponseType<ContratoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContratoResponse>> CriarContrato(CriarContratoRequest request, CancellationToken ct)
    {
        var contrato = await service.CriarContratoAsync(request, ct);
        return CreatedAtAction(nameof(ObterContrato), new { id = contrato.Id }, contrato);
    }

    [HttpPut("contratos/{id:guid}/status")]
    [ProducesResponseType<ContratoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContratoResponse>> AlterarStatusContrato(
        Guid id, AlterarStatusContratoRequest request, CancellationToken ct)
    {
        var contrato = await service.AlterarStatusContratoAsync(id, request.Status, ct);
        return contrato is null ? NotFound() : Ok(contrato);
    }

    [HttpGet("contratos/{contratoId:guid}/medicoes")]
    [ProducesResponseType<IReadOnlyList<MedicaoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MedicaoResponse>>> ListarMedicoes(
        Guid contratoId, CancellationToken ct) =>
        Ok(await service.ListarMedicoesAsync(contratoId, ct));

    [HttpPost("contratos/{contratoId:guid}/medicoes")]
    [ProducesResponseType<MedicaoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MedicaoResponse>> CriarMedicao(
        Guid contratoId, CriarMedicaoRequest request, CancellationToken ct)
    {
        var medicao = await service.CriarMedicaoAsync(contratoId, request, ct);
        return CreatedAtAction(nameof(ListarMedicoes), new { contratoId }, medicao);
    }

    [HttpPut("medicoes/{id:guid}/status")]
    [ProducesResponseType<MedicaoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MedicaoResponse>> AlterarStatusMedicao(
        Guid id, AlterarStatusMedicaoRequest request, CancellationToken ct)
    {
        var medicao = await service.AlterarStatusMedicaoAsync(id, request.Status, request.Vencimento, ct);
        return medicao is null ? NotFound() : Ok(medicao);
    }

    [HttpGet("financeiro/lancamentos")]
    [ProducesResponseType<IReadOnlyList<LancamentoFinanceiroResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LancamentoFinanceiroResponse>>> ListarLancamentos(
        [FromQuery] TipoLancamentoFinanceiro? tipo, CancellationToken ct) =>
        Ok(await service.ListarLancamentosAsync(tipo, ct));

    [HttpPost("financeiro/lancamentos")]
    [ProducesResponseType<LancamentoFinanceiroResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LancamentoFinanceiroResponse>> CriarLancamento(
        CriarLancamentoFinanceiroRequest request, CancellationToken ct)
    {
        var lancamento = await service.CriarLancamentoAsync(request, ct);
        return CreatedAtAction(nameof(ObterLancamento), new { id = lancamento.Id }, lancamento);
    }

    [HttpGet("financeiro/lancamentos/{id:guid}")]
    [ProducesResponseType<LancamentoFinanceiroResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LancamentoFinanceiroResponse>> ObterLancamento(Guid id, CancellationToken ct) =>
        await service.ObterLancamentoAsync(id, ct) is { } lancamento ? Ok(lancamento) : NotFound();

    [HttpPut("financeiro/lancamentos/{id:guid}/liquidacao")]
    [ProducesResponseType<LancamentoFinanceiroResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LancamentoFinanceiroResponse>> LiquidarLancamento(
        Guid id, LiquidarLancamentoRequest request, CancellationToken ct)
    {
        var lancamento = await service.LiquidarLancamentoAsync(id, request.LiquidadoEm, ct);
        return lancamento is null ? NotFound() : Ok(lancamento);
    }

    [HttpGet("obras/{obraId:guid}/apontamentos")]
    [ProducesResponseType<IReadOnlyList<ApontamentoObraResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ApontamentoObraResponse>>> ListarApontamentos(
        Guid obraId, CancellationToken ct) =>
        Ok(await service.ListarApontamentosAsync(obraId, ct));

    [HttpPost("apontamentos")]
    [ProducesResponseType<ApontamentoObraResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApontamentoObraResponse>> CriarApontamento(
        CriarApontamentoObraRequest request, CancellationToken ct)
    {
        var apontamento = await service.CriarApontamentoAsync(request, ct);
        return CreatedAtAction(nameof(ListarApontamentos), new { obraId = apontamento.ObraId }, apontamento);
    }

    [HttpGet("obras/{obraId:guid}/alocacoes")]
    [ProducesResponseType<IReadOnlyList<AlocacaoFuncionarioResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AlocacaoFuncionarioResponse>>> ListarAlocacoes(
        Guid obraId, CancellationToken ct) =>
        Ok(await service.ListarAlocacoesAsync(obraId, ct));

    [HttpPost("alocacoes")]
    [ProducesResponseType<AlocacaoFuncionarioResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AlocacaoFuncionarioResponse>> CriarAlocacao(
        CriarAlocacaoFuncionarioRequest request, CancellationToken ct)
    {
        var alocacao = await service.CriarAlocacaoAsync(request, ct);
        return CreatedAtAction(nameof(ListarAlocacoes), new { obraId = alocacao.ObraId }, alocacao);
    }

    [HttpPut("alocacoes/{id:guid}/encerramento")]
    [ProducesResponseType<AlocacaoFuncionarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AlocacaoFuncionarioResponse>> EncerrarAlocacao(
        Guid id, EncerrarAlocacaoRequest request, CancellationToken ct)
    {
        var alocacao = await service.EncerrarAlocacaoAsync(id, request.Fim, ct);
        return alocacao is null ? NotFound() : Ok(alocacao);
    }

    [HttpGet("obras/{obraId:guid}/seguranca/incidentes")]
    [ProducesResponseType<IReadOnlyList<IncidenteResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<IncidenteResponse>>> ListarIncidentes(
        Guid obraId, CancellationToken ct) =>
        Ok(await service.ListarIncidentesAsync(obraId, ct));

    [HttpPost("seguranca/incidentes")]
    [ProducesResponseType<IncidenteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidenteResponse>> CriarIncidente(
        CriarIncidenteRequest request, CancellationToken ct)
    {
        var incidente = await service.CriarIncidenteAsync(request, ct);
        return CreatedAtAction(nameof(ListarIncidentes), new { obraId = incidente.ObraId }, incidente);
    }

    [HttpPut("seguranca/incidentes/{id:guid}")]
    [ProducesResponseType<IncidenteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidenteResponse>> AtualizarIncidente(
        Guid id, AtualizarIncidenteRequest request, CancellationToken ct)
    {
        var incidente = await service.AtualizarIncidenteAsync(id, request, ct);
        return incidente is null ? NotFound() : Ok(incidente);
    }

    [HttpGet("obras/{obraId:guid}/qualidade/inspecoes")]
    [ProducesResponseType<IReadOnlyList<InspecaoQualidadeResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InspecaoQualidadeResponse>>> ListarInspecoes(
        Guid obraId, CancellationToken ct) =>
        Ok(await service.ListarInspecoesAsync(obraId, ct));

    [HttpPost("qualidade/inspecoes")]
    [ProducesResponseType<InspecaoQualidadeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InspecaoQualidadeResponse>> CriarInspecao(
        CriarInspecaoQualidadeRequest request, CancellationToken ct)
    {
        var inspecao = await service.CriarInspecaoAsync(request, ct);
        return CreatedAtAction(nameof(ListarInspecoes), new { obraId = inspecao.ObraId }, inspecao);
    }

    [HttpPut("qualidade/inspecoes/{id:guid}/acao-corretiva")]
    [ProducesResponseType<InspecaoQualidadeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InspecaoQualidadeResponse>> AtualizarAcaoCorretiva(
        Guid id, AtualizarAcaoCorretivaRequest request, CancellationToken ct)
    {
        var inspecao = await service.AtualizarAcaoCorretivaAsync(id, request, ct);
        return inspecao is null ? NotFound() : Ok(inspecao);
    }
}
