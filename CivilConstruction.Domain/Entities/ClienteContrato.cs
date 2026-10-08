namespace CivilConstruction.Domain.Entities;

public sealed class Cliente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
}

public enum StatusContrato
{
    Rascunho,
    Ativo,
    Suspenso,
    Encerrado,
    Cancelado
}

public sealed class Contrato
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClienteId { get; set; }
    public Guid ObraId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTimeOffset DataAssinatura { get; set; }
    public DateTimeOffset DataInicio { get; set; }
    public DateTimeOffset DataFimPrevista { get; set; }
    public decimal ValorContratado { get; set; }
    public decimal RetencaoPercentual { get; set; }
    public StatusContrato Status { get; set; } = StatusContrato.Rascunho;
}

public enum StatusMedicao
{
    Rascunho,
    Enviada,
    Aprovada,
    Rejeitada,
    Faturada
}

public sealed class MedicaoContrato
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ContratoId { get; set; }
    public string Referencia { get; set; } = string.Empty;
    public DateTimeOffset DataMedicao { get; set; }
    public decimal PercentualExecutado { get; set; }
    public decimal ValorMedido { get; set; }
    public StatusMedicao Status { get; set; } = StatusMedicao.Rascunho;
}
