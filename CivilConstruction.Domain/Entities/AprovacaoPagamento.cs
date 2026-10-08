using CivilConstruction.Domain.Events;

namespace CivilConstruction.Domain.Entities;

public enum StatusAprovacaoPagamento
{
    Pendente,
    Aprovado,
    Rejeitado,
    Liberado
}

public sealed record AprovacaoPagamentoSolicitada(
    Guid AprovacaoId,
    Guid PagamentoId,
    Guid SolicitanteId,
    DateTimeOffset SolicitadaEm) : IDomainEvent;

public sealed record AprovacaoPagamentoAprovada(
    Guid AprovacaoId,
    Guid AprovadorId,
    DateTimeOffset AprovadaEm) : IDomainEvent;

public sealed record AprovacaoPagamentoRejeitada(
    Guid AprovacaoId,
    Guid RejeitadorId,
    string Motivo,
    DateTimeOffset RejeitadaEm) : IDomainEvent;

public sealed record PagamentoLiberado(
    Guid AprovacaoId,
    Guid LiberadorId,
    DateTimeOffset LiberadoEm) : IDomainEvent;

public sealed class AprovacaoPagamento : EventSourcedAggregate
{
    public Guid PagamentoId { get; private set; }
    public Guid SolicitanteId { get; private set; }
    public Guid? AprovadorId { get; private set; }
    public Guid? LiberadorId { get; private set; }
    public StatusAprovacaoPagamento Status { get; private set; }
    public string? MotivoRejeicao { get; private set; }

    private AprovacaoPagamento() { }

    public static AprovacaoPagamento Rehydrate(IEnumerable<IDomainEvent> history)
    {
        ArgumentNullException.ThrowIfNull(history);
        var aggregate = new AprovacaoPagamento();
        aggregate.LoadFromHistory(history);
        if (aggregate.Id == Guid.Empty)
            throw new InvalidOperationException("Aprovação de pagamento sem evento de solicitação.");
        return aggregate;
    }

    public static AprovacaoPagamento Solicitar(
        Guid aprovacaoId,
        Guid pagamentoId,
        Guid solicitanteId,
        DateTimeOffset solicitadaEm)
    {
        if (aprovacaoId == Guid.Empty || pagamentoId == Guid.Empty || solicitanteId == Guid.Empty)
            throw new ArgumentException("Os identificadores de aprovação, pagamento e solicitante são obrigatórios.");

        var aggregate = new AprovacaoPagamento();
        aggregate.Raise(new AprovacaoPagamentoSolicitada(
            aprovacaoId, pagamentoId, solicitanteId, solicitadaEm));
        return aggregate;
    }

    public void Aprovar(Guid aprovadorId, DateTimeOffset aprovadaEm)
    {
        if (Status != StatusAprovacaoPagamento.Pendente)
            throw new InvalidOperationException("Somente pagamentos pendentes podem ser aprovados.");
        if (aprovadorId == Guid.Empty || aprovadorId == SolicitanteId)
            throw new InvalidOperationException("O solicitante não pode aprovar a própria solicitação.");

        Raise(new AprovacaoPagamentoAprovada(Id, aprovadorId, aprovadaEm));
    }

    public void Rejeitar(Guid rejeitadorId, string motivo, DateTimeOffset rejeitadaEm)
    {
        if (Status != StatusAprovacaoPagamento.Pendente)
            throw new InvalidOperationException("Somente pagamentos pendentes podem ser rejeitados.");
        if (rejeitadorId == Guid.Empty || rejeitadorId == SolicitanteId)
            throw new InvalidOperationException("O solicitante não pode rejeitar a própria solicitação.");
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("O motivo da rejeição é obrigatório.", nameof(motivo));

        Raise(new AprovacaoPagamentoRejeitada(Id, rejeitadorId, motivo.Trim(), rejeitadaEm));
    }

    public void Liberar(Guid liberadorId, DateTimeOffset liberadoEm)
    {
        if (Status != StatusAprovacaoPagamento.Aprovado)
            throw new InvalidOperationException("Somente pagamentos aprovados podem ser liberados.");
        if (liberadorId == Guid.Empty || liberadorId == SolicitanteId || liberadorId == AprovadorId)
            throw new InvalidOperationException("A liberação exige um operador diferente do solicitante e aprovador.");

        Raise(new PagamentoLiberado(Id, liberadorId, liberadoEm));
    }

    protected override void When(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case AprovacaoPagamentoSolicitada requested:
                Id = requested.AprovacaoId;
                PagamentoId = requested.PagamentoId;
                SolicitanteId = requested.SolicitanteId;
                Status = StatusAprovacaoPagamento.Pendente;
                break;
            case AprovacaoPagamentoAprovada approved:
                AprovadorId = approved.AprovadorId;
                Status = StatusAprovacaoPagamento.Aprovado;
                break;
            case AprovacaoPagamentoRejeitada rejected:
                MotivoRejeicao = rejected.Motivo;
                Status = StatusAprovacaoPagamento.Rejeitado;
                break;
            case PagamentoLiberado released:
                LiberadorId = released.LiberadorId;
                Status = StatusAprovacaoPagamento.Liberado;
                break;
            default:
                throw new InvalidOperationException($"Evento de aprovação não suportado: {domainEvent.GetType().Name}.");
        }
    }
}
