using CivilConstruction.Application.Interfaces;
using CivilConstruction.Domain.Entities;
using MediatR;

namespace CivilConstruction.Application.Features.AprovacoesPagamento;

internal sealed class SolicitarAprovacaoPagamentoHandler(IEventStore eventStore)
    : IRequestHandler<SolicitarAprovacaoPagamentoCommand, AprovacaoPagamentoResponse>
{
    public async Task<AprovacaoPagamentoResponse> Handle(
        SolicitarAprovacaoPagamentoCommand request,
        CancellationToken cancellationToken)
    {
        var existingEvents = await eventStore.ReadStreamAsync(request.AprovacaoId, cancellationToken);
        if (existingEvents.Count > 0)
        {
            var existing = AprovacaoPagamento.Rehydrate(existingEvents.Select(item => item.Payload));
            if (existing.PagamentoId != request.PagamentoId ||
                existing.SolicitanteId != request.SolicitanteId)
                throw new InvalidOperationException("A chave de idempotência já foi utilizada para outra solicitação.");
            return Map(existing);
        }

        var aggregate = AprovacaoPagamento.Solicitar(
            request.AprovacaoId, request.PagamentoId, request.SolicitanteId, request.SolicitadaEm);
        await eventStore.AppendAsync(
            aggregate.Id,
            aggregate.Version,
            aggregate.UncommittedEvents,
            cancellationToken);
        aggregate.MarkChangesCommitted();
        return Map(aggregate);
    }

    internal static AprovacaoPagamentoResponse Map(AprovacaoPagamento aggregate) =>
        new(aggregate.Id, aggregate.PagamentoId, aggregate.SolicitanteId,
            aggregate.AprovadorId, aggregate.LiberadorId, aggregate.Status,
            aggregate.MotivoRejeicao, aggregate.Version);
}

internal sealed class AprovarPagamentoHandler(IEventStore eventStore)
    : IRequestHandler<AprovarPagamentoCommand, AprovacaoPagamentoResponse>
{
    public async Task<AprovacaoPagamentoResponse> Handle(
        AprovarPagamentoCommand request,
        CancellationToken cancellationToken)
    {
        var aggregate = await LoadAsync(eventStore, request.AprovacaoId, cancellationToken);
        aggregate.Aprovar(request.AprovadorId, request.AprovadaEm);
        await AppendAsync(eventStore, aggregate, cancellationToken);
        return SolicitarAprovacaoPagamentoHandler.Map(aggregate);
    }

    internal static async Task<AprovacaoPagamento> LoadAsync(
        IEventStore eventStore,
        Guid id,
        CancellationToken cancellationToken)
    {
        var storedEvents = await eventStore.ReadStreamAsync(id, cancellationToken);
        if (storedEvents.Count == 0)
            throw new KeyNotFoundException("Solicitação de aprovação não encontrada.");

        return AprovacaoPagamento.Rehydrate(storedEvents.Select(item => item.Payload));
    }

    internal static async Task AppendAsync(
        IEventStore eventStore,
        AprovacaoPagamento aggregate,
        CancellationToken cancellationToken)
    {
        await eventStore.AppendAsync(
            aggregate.Id,
            aggregate.Version,
            aggregate.UncommittedEvents,
            cancellationToken);
        aggregate.MarkChangesCommitted();
    }
}

internal sealed class RejeitarPagamentoHandler(IEventStore eventStore)
    : IRequestHandler<RejeitarPagamentoCommand, AprovacaoPagamentoResponse>
{
    public async Task<AprovacaoPagamentoResponse> Handle(
        RejeitarPagamentoCommand request,
        CancellationToken cancellationToken)
    {
        var aggregate = await AprovarPagamentoHandler.LoadAsync(
            eventStore, request.AprovacaoId, cancellationToken);
        aggregate.Rejeitar(request.RejeitadorId, request.Motivo, request.RejeitadaEm);
        await AprovarPagamentoHandler.AppendAsync(eventStore, aggregate, cancellationToken);
        return SolicitarAprovacaoPagamentoHandler.Map(aggregate);
    }
}

internal sealed class LiberarPagamentoHandler(IEventStore eventStore)
    : IRequestHandler<LiberarPagamentoCommand, AprovacaoPagamentoResponse>
{
    public async Task<AprovacaoPagamentoResponse> Handle(
        LiberarPagamentoCommand request,
        CancellationToken cancellationToken)
    {
        var aggregate = await AprovarPagamentoHandler.LoadAsync(
            eventStore, request.AprovacaoId, cancellationToken);
        aggregate.Liberar(request.LiberadorId, request.LiberadoEm);
        await AprovarPagamentoHandler.AppendAsync(eventStore, aggregate, cancellationToken);
        return SolicitarAprovacaoPagamentoHandler.Map(aggregate);
    }
}

internal sealed class ObterAprovacaoPagamentoHandler(IEventStore eventStore)
    : IRequestHandler<ObterAprovacaoPagamentoQuery, AprovacaoPagamentoResponse?>
{
    public async Task<AprovacaoPagamentoResponse?> Handle(
        ObterAprovacaoPagamentoQuery request,
        CancellationToken cancellationToken)
    {
        var events = await eventStore.ReadStreamAsync(request.AprovacaoId, cancellationToken);
        if (events.Count == 0)
            return null;

        var aggregate = AprovacaoPagamento.Rehydrate(events.Select(item => item.Payload));
        return SolicitarAprovacaoPagamentoHandler.Map(aggregate);
    }
}
