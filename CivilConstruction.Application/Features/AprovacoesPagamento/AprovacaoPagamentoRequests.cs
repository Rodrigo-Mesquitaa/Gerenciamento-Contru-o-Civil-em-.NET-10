using CivilConstruction.Domain.Entities;
using MediatR;

namespace CivilConstruction.Application.Features.AprovacoesPagamento;

/// <summary>Comando interno que inicia um fluxo versionado de aprovação de pagamento.</summary>
public sealed record SolicitarAprovacaoPagamentoCommand(
    Guid AprovacaoId,
    Guid PagamentoId,
    Guid SolicitanteId,
    DateTimeOffset SolicitadaEm) : IRequest<AprovacaoPagamentoResponse>;

/// <summary>Aprova uma solicitação por um ator autorizado.</summary>
public sealed record AprovarPagamentoCommand(
    Guid AprovacaoId,
    Guid AprovadorId,
    DateTimeOffset AprovadaEm) : IRequest<AprovacaoPagamentoResponse>;

/// <summary>Rejeita uma solicitação pendente com justificativa.</summary>
public sealed record RejeitarPagamentoCommand(
    Guid AprovacaoId,
    Guid RejeitadorId,
    string Motivo,
    DateTimeOffset RejeitadaEm) : IRequest<AprovacaoPagamentoResponse>;

/// <summary>Libera um pagamento aprovado por um terceiro operador.</summary>
public sealed record LiberarPagamentoCommand(
    Guid AprovacaoId,
    Guid LiberadorId,
    DateTimeOffset LiberadoEm) : IRequest<AprovacaoPagamentoResponse>;

/// <summary>Consulta o estado atual reconstituído do stream de eventos.</summary>
public sealed record ObterAprovacaoPagamentoQuery(Guid AprovacaoId)
    : IRequest<AprovacaoPagamentoResponse?>;

/// <summary>Solicita a aprovação de um pagamento existente.</summary>
public sealed record SolicitarAprovacaoPagamentoRequest(Guid PagamentoId);

/// <summary>Motivo obrigatório para rejeitar uma solicitação de pagamento.</summary>
public sealed record RejeitarAprovacaoPagamentoRequest(string Motivo);

/// <summary>Estado reconstituído da aprovação de pagamento.</summary>
public sealed record AprovacaoPagamentoResponse(
    Guid Id,
    Guid PagamentoId,
    Guid SolicitanteId,
    Guid? AprovadorId,
    Guid? LiberadorId,
    StatusAprovacaoPagamento Status,
    string? MotivoRejeicao,
    long Version);
