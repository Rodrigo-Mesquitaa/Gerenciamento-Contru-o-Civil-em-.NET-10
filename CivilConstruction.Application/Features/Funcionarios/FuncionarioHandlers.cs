using CivilConstruction.Application.Interfaces;
using CivilConstruction.Domain.Entities;
using CivilConstruction.Domain.Events;
using CivilConstruction.Domain.Interfaces;
using MediatR;
using System.Text.Json;

namespace CivilConstruction.Application.Features.Funcionarios;

internal sealed class CriarFuncionarioHandler(
    IFuncionarioRepository repository,
    IUnitOfWork unitOfWork,
    IIntegrationEventOutbox outbox) : IRequestHandler<CriarFuncionarioRequest, FuncionarioResponse>
{
    public async Task<FuncionarioResponse> Handle(
        CriarFuncionarioRequest request,
        CancellationToken cancellationToken)
    {
        var nome = request.Nome.Trim();
        if (nome.Length == 0)
            throw new ArgumentException("O nome do funcionário é obrigatório.");
        if (request.ValorDiaria <= 0)
            throw new ArgumentOutOfRangeException(nameof(request.ValorDiaria));

        var now = DateTime.UtcNow;
        var funcionario = new Funcionario
        {
            Id = request.Id ?? Guid.NewGuid(),
            Nome = nome,
            ValorDiaria = request.ValorDiaria,
            DataCadastro = now,
            Ativo = true
        };
        var evento = new FuncionarioCadastradoEvent
        {
            Id = funcionario.Id,
            Nome = funcionario.Nome,
            ValorDiaria = funcionario.ValorDiaria,
            DataCadastro = now
        };

        await repository.AdicionarAsync(funcionario, cancellationToken);
        outbox.Add(
            evento.Id,
            IntegrationEventTypes.EmployeeRegistered,
            1,
            JsonSerializer.Serialize(evento),
            new DateTimeOffset(now, TimeSpan.Zero));
        await unitOfWork.CommitAsync(cancellationToken);

        return new FuncionarioResponse(funcionario.Id, funcionario.Nome, funcionario.ValorDiaria);
    }
}

internal sealed class ListarFuncionariosHandler(IFuncionarioRepository repository)
    : IRequestHandler<ListarFuncionariosQuery, IReadOnlyList<FuncionarioResponse>>
{
    public async Task<IReadOnlyList<FuncionarioResponse>> Handle(
        ListarFuncionariosQuery request,
        CancellationToken cancellationToken)
    {
        var funcionarios = await repository.ObterTodosAsync(cancellationToken);
        return funcionarios
            .Select(funcionario => new FuncionarioResponse(
                funcionario.Id,
                funcionario.Nome,
                funcionario.ValorDiaria))
            .ToArray();
    }
}

internal sealed class ObterFuncionarioHandler(IFuncionarioRepository repository)
    : IRequestHandler<ObterFuncionarioQuery, FuncionarioResponse?>
{
    public async Task<FuncionarioResponse?> Handle(
        ObterFuncionarioQuery request,
        CancellationToken cancellationToken)
    {
        var funcionario = await repository.ObterPorIdAsync(request.Id, cancellationToken);
        return funcionario is null
            ? null
            : new FuncionarioResponse(funcionario.Id, funcionario.Nome, funcionario.ValorDiaria);
    }
}

internal sealed class AtualizarFuncionarioHandler(
    IFuncionarioRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<AtualizarFuncionarioCommand, bool>
{
    public async Task<bool> Handle(
        AtualizarFuncionarioCommand request,
        CancellationToken cancellationToken)
    {
        var funcionario = await repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (funcionario is null)
            return false;

        var nome = request.Nome.Trim();
        if (nome.Length == 0)
            throw new ArgumentException("O nome do funcionário é obrigatório.");
        if (request.ValorDiaria <= 0)
            throw new ArgumentOutOfRangeException(nameof(request.ValorDiaria));

        funcionario.Nome = nome;
        funcionario.ValorDiaria = request.ValorDiaria;
        await repository.AtualizarAsync(funcionario, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return true;
    }
}

internal sealed class RemoverFuncionarioHandler(
    IFuncionarioRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<RemoverFuncionarioCommand, bool>
{
    public async Task<bool> Handle(
        RemoverFuncionarioCommand request,
        CancellationToken cancellationToken)
    {
        var removed = await repository.RemoverAsync(request.Id, cancellationToken);
        if (!removed)
            return false;

        await unitOfWork.CommitAsync(cancellationToken);
        return true;
    }
}
