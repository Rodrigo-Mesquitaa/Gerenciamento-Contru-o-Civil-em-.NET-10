using System.ComponentModel.DataAnnotations;
using CivilConstruction.Domain.Entities;

namespace CivilConstruction.Application.DTOs;

/// <summary>Transição solicitada para uma ordem de compra.</summary>
public sealed record AlterarStatusOrdemCompraRequest
{
    [EnumDataType(typeof(StatusOrdemCompra))]
    public StatusOrdemCompra Status { get; init; }
}

/// <summary>Transição solicitada para uma obra.</summary>
public sealed record AlterarStatusObraRequest
{
    [EnumDataType(typeof(StatusObra))]
    public StatusObra Status { get; init; }
}

/// <summary>Atualiza a situação operacional de um equipamento.</summary>
public sealed record AlterarStatusEquipamentoRequest
{
    [EnumDataType(typeof(StatusEquipamento))]
    public StatusEquipamento Status { get; init; }
    public Guid? ObraId { get; init; }
}

/// <summary>Ativa ou suspende uma programação recorrente de pagamento.</summary>
public sealed record AlterarAtividadeProgramacaoPagamentoRequest
{
    public bool Ativa { get; init; }
}

/// <summary>Transição solicitada para um contrato.</summary>
public sealed record AlterarStatusContratoRequest
{
    [EnumDataType(typeof(StatusContrato))]
    public StatusContrato Status { get; init; }
}

/// <summary>Data de encerramento da alocação do funcionário.</summary>
public sealed record EncerrarAlocacaoRequest
{
    public DateTimeOffset Fim { get; init; }
}
