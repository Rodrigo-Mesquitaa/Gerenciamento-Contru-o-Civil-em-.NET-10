using CivilConstruction.Application.DTOs;
using CivilConstruction.Application.Interfaces;
using CivilConstruction.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CivilConstruction.API.Controllers;

[ApiController]
[Route("api")]
[Produces("application/json")]
public sealed class GestaoConstrucaoController(IGestaoConstrucaoService service) : ControllerBase
{
    [HttpGet("obras")]
    [ProducesResponseType<IReadOnlyList<ObraResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ObraResponse>>> ListarObras(CancellationToken ct) =>
        Ok(await service.ListarObrasAsync(ct));

    [HttpGet("obras/{id:guid}")]
    [ProducesResponseType<ObraResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ObraResponse>> ObterObra(Guid id, CancellationToken ct) =>
        await service.ObterObraAsync(id, ct) is { } obra ? Ok(obra) : NotFound();

    [HttpPost("obras")]
    [ProducesResponseType<ObraResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ObraResponse>> CriarObra(CriarObraRequest request, CancellationToken ct)
    {
        var obra = await service.CriarObraAsync(request, ct);
        return CreatedAtAction(nameof(ObterObra), new { id = obra.Id }, obra);
    }

    [HttpPut("obras/{id:guid}/status")]
    [ProducesResponseType<ObraResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ObraResponse>> AlterarStatusObra(
        Guid id, AlterarStatusObraRequest request, CancellationToken ct)
    {
        var obra = await service.AlterarStatusObraAsync(id, request.Status, ct);
        return obra is null ? NotFound() : Ok(obra);
    }

    [HttpGet("fornecedores")]
    [ProducesResponseType<IReadOnlyList<FornecedorResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FornecedorResponse>>> ListarFornecedores(CancellationToken ct) =>
        Ok(await service.ListarFornecedoresAsync(ct));

    [HttpGet("fornecedores/{id:guid}")]
    [ProducesResponseType<FornecedorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FornecedorResponse>> ObterFornecedor(Guid id, CancellationToken ct) =>
        await service.ObterFornecedorAsync(id, ct) is { } fornecedor ? Ok(fornecedor) : NotFound();

    [HttpPost("fornecedores")]
    [ProducesResponseType<FornecedorResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FornecedorResponse>> CriarFornecedor(
        CriarFornecedorRequest request, CancellationToken ct)
    {
        var fornecedor = await service.CriarFornecedorAsync(request, ct);
        return CreatedAtAction(nameof(ObterFornecedor), new { id = fornecedor.Id }, fornecedor);
    }

    [HttpGet("compras")]
    [ProducesResponseType<IReadOnlyList<OrdemCompraResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrdemCompraResponse>>> ListarOrdens(CancellationToken ct) =>
        Ok(await service.ListarOrdensCompraAsync(ct));

    [HttpGet("compras/{id:guid}")]
    [ProducesResponseType<OrdemCompraResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdemCompraResponse>> ObterOrdem(Guid id, CancellationToken ct) =>
        await service.ObterOrdemCompraPorIdAsync(id, ct) is { } ordem ? Ok(ordem) : NotFound();

    [HttpPost("compras")]
    [ProducesResponseType<OrdemCompraResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdemCompraResponse>> CriarOrdem(
        CriarOrdemCompraRequest request, CancellationToken ct)
    {
        var ordem = await service.CriarOrdemCompraAsync(request, ct);
        return CreatedAtAction(nameof(ObterOrdem), new { id = ordem.Id }, ordem);
    }

    [HttpPut("compras/{id:guid}/status")]
    [ProducesResponseType<OrdemCompraResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrdemCompraResponse>> AlterarStatusOrdem(
        Guid id, AlterarStatusOrdemCompraRequest request, CancellationToken ct)
    {
        var ordem = await service.AlterarStatusOrdemCompraAsync(id, request.Status, ct);
        return ordem is null ? NotFound() : Ok(ordem);
    }

    [HttpGet("estoque")]
    [ProducesResponseType<IReadOnlyList<ItemEstoqueResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ItemEstoqueResponse>>> ListarEstoque(CancellationToken ct) =>
        Ok(await service.ListarEstoqueAsync(ct));

    [HttpGet("equipamentos")]
    [ProducesResponseType<IReadOnlyList<EquipamentoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EquipamentoResponse>>> ListarEquipamentos(CancellationToken ct) =>
        Ok(await service.ListarEquipamentosAsync(ct));

    [HttpGet("equipamentos/{id:guid}")]
    [ProducesResponseType<EquipamentoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipamentoResponse>> ObterEquipamento(Guid id, CancellationToken ct) =>
        await service.ObterEquipamentoAsync(id, ct) is { } equipamento ? Ok(equipamento) : NotFound();

    [HttpPost("equipamentos")]
    [ProducesResponseType<EquipamentoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EquipamentoResponse>> CriarEquipamento(
        CriarEquipamentoRequest request, CancellationToken ct)
    {
        var equipamento = await service.CriarEquipamentoAsync(request, ct);
        return CreatedAtAction(nameof(ObterEquipamento), new { id = equipamento.Id }, equipamento);
    }

    [HttpPut("equipamentos/{id:guid}/status")]
    [ProducesResponseType<EquipamentoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EquipamentoResponse>> AlterarStatusEquipamento(
        Guid id, AlterarStatusEquipamentoRequest request, CancellationToken ct)
    {
        var equipamento = await service.AlterarStatusEquipamentoAsync(
            id, request.Status, request.ObraId, ct);
        return equipamento is null ? NotFound() : Ok(equipamento);
    }

    [HttpGet("folha/programacoes")]
    [ProducesResponseType<IReadOnlyList<ProgramacaoPagamentoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProgramacaoPagamentoResponse>>> ListarProgramacoes(CancellationToken ct) =>
        Ok(await service.ListarProgramacoesPagamentoAsync(ct));

    [HttpGet("folha/programacoes/{id:guid}")]
    [ProducesResponseType<ProgramacaoPagamentoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProgramacaoPagamentoResponse>> ObterProgramacao(Guid id, CancellationToken ct) =>
        await service.ObterProgramacaoPagamentoAsync(id, ct) is { } programacao ? Ok(programacao) : NotFound();

    [HttpPost("folha/programacoes")]
    [ProducesResponseType<ProgramacaoPagamentoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProgramacaoPagamentoResponse>> CriarProgramacao(
        CriarProgramacaoPagamentoRequest request, CancellationToken ct)
    {
        var programacao = await service.CriarProgramacaoPagamentoAsync(request, ct);
        return CreatedAtAction(nameof(ObterProgramacao), new { id = programacao.Id }, programacao);
    }

    [HttpPut("folha/programacoes/{id:guid}/atividade")]
    [ProducesResponseType<ProgramacaoPagamentoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProgramacaoPagamentoResponse>> AlterarAtividadeProgramacao(
        Guid id, AlterarAtividadeProgramacaoPagamentoRequest request, CancellationToken ct)
    {
        var programacao = await service.AlterarAtividadeProgramacaoPagamentoAsync(
            id, request.Ativa, ct);
        return programacao is null ? NotFound() : Ok(programacao);
    }

    [HttpGet("folha/pagamentos")]
    [ProducesResponseType<IReadOnlyList<PagamentoFuncionarioResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PagamentoFuncionarioResponse>>> ListarPagamentosPendentes(
        CancellationToken ct) => Ok(await service.ListarPagamentosPendentesAsync(ct));
}
