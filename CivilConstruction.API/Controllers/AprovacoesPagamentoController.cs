using System.Security.Claims;
using CivilConstruction.Application.Features.AprovacoesPagamento;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivilConstruction.API.Controllers;

[ApiController]
[Route("api/finance/aprovacoes-pagamento")]
[Produces("application/json")]
public sealed class AprovacoesPagamentoController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "finance.approval.request")]
    [ProducesResponseType<AprovacaoPagamentoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AprovacaoPagamentoResponse>> Solicitar(
        SolicitarAprovacaoPagamentoRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
    {
        if (!Guid.TryParse(idempotencyKey, out var approvalId) || approvalId == Guid.Empty)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Idempotency-Key must be a non-empty GUID."
            });
        if (!TryGetEmployeeId(out var actorId))
            return Unauthorized();

        var approval = await sender.Send(new SolicitarAprovacaoPagamentoCommand(
            approvalId, request.PagamentoId, actorId, DateTimeOffset.UtcNow), ct);
        return CreatedAtAction(nameof(Obter), new { id = approval.Id }, approval);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "finance.approval.read")]
    [ProducesResponseType<AprovacaoPagamentoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AprovacaoPagamentoResponse>> Obter(Guid id, CancellationToken ct)
    {
        var approval = await sender.Send(new ObterAprovacaoPagamentoQuery(id), ct);
        return approval is null ? NotFound() : Ok(approval);
    }

    [HttpPost("{id:guid}/aprovar")]
    [Authorize(Policy = "finance.approval.approve")]
    [ProducesResponseType<AprovacaoPagamentoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AprovacaoPagamentoResponse>> Aprovar(Guid id, CancellationToken ct)
    {
        if (!TryGetEmployeeId(out var actorId))
            return Unauthorized();
        return Ok(await sender.Send(
            new AprovarPagamentoCommand(id, actorId, DateTimeOffset.UtcNow), ct));
    }

    [HttpPost("{id:guid}/rejeitar")]
    [Authorize(Policy = "finance.approval.approve")]
    [ProducesResponseType<AprovacaoPagamentoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AprovacaoPagamentoResponse>> Rejeitar(
        Guid id,
        RejeitarAprovacaoPagamentoRequest request,
        CancellationToken ct)
    {
        if (!TryGetEmployeeId(out var actorId))
            return Unauthorized();
        return Ok(await sender.Send(
            new RejeitarPagamentoCommand(id, actorId, request.Motivo, DateTimeOffset.UtcNow), ct));
    }

    [HttpPost("{id:guid}/liberar")]
    [Authorize(Policy = "finance.approval.release")]
    [ProducesResponseType<AprovacaoPagamentoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AprovacaoPagamentoResponse>> Liberar(Guid id, CancellationToken ct)
    {
        if (!TryGetEmployeeId(out var actorId))
            return Unauthorized();
        return Ok(await sender.Send(
            new LiberarPagamentoCommand(id, actorId, DateTimeOffset.UtcNow), ct));
    }

    private bool TryGetEmployeeId(out Guid employeeId)
    {
        var claim = User.FindFirstValue("employee_id")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out employeeId) && employeeId != Guid.Empty;
    }
}
