using CivilConstruction.Application.Features.Funcionarios;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CivilConstruction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FuncionariosController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<FuncionarioResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FuncionarioResponse>>> Get(CancellationToken ct) =>
        Ok(await sender.Send(new ListarFuncionariosQuery(), ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<FuncionarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FuncionarioResponse>> GetById(Guid id, CancellationToken ct)
    {
        var funcionario = await sender.Send(new ObterFuncionarioQuery(id), ct);
        return funcionario is null ? NotFound() : Ok(funcionario);
    }

    [HttpPost]
    [ProducesResponseType<FuncionarioResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<FuncionarioResponse>> Post(
        [FromBody] CriarFuncionarioRequest request,
        CancellationToken ct)
    {
        var funcionario = await sender.Send(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = funcionario.Id }, funcionario);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(
        Guid id,
        [FromBody] AtualizarFuncionarioRequest request,
        CancellationToken ct)
    {
        var updated = await sender.Send(
            new AtualizarFuncionarioCommand(id, request.Nome, request.ValorDiaria),
            ct);
        if (!updated) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var removed = await sender.Send(new RemoverFuncionarioCommand(id), ct);
        if (!removed) return NotFound();
        return NoContent();
    }
}
