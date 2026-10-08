namespace CivilConstruction.Domain.Entities;

public enum TipoLancamentoFinanceiro
{
    ContaPagar,
    ContaReceber
}

public enum StatusLancamentoFinanceiro
{
    Aberto,
    Pago,
    Recebido,
    Cancelado,
    Atrasado
}

public sealed class LancamentoFinanceiro
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ObraId { get; set; }
    public Guid? FornecedorId { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? MedicaoContratoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public TipoLancamentoFinanceiro Tipo { get; set; }
    public StatusLancamentoFinanceiro Status { get; set; } = StatusLancamentoFinanceiro.Aberto;
    public decimal Valor { get; set; }
    public DateTimeOffset Vencimento { get; set; }
    public DateTimeOffset? LiquidadoEm { get; set; }
    public string? ReferenciaExterna { get; set; }
}
