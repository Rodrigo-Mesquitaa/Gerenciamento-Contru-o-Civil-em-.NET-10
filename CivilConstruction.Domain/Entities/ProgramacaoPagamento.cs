namespace CivilConstruction.Domain.Entities;

public enum PeriodicidadePagamento
{
    Semanal,
    Quinzenal,
    Mensal
}

public sealed class ProgramacaoPagamento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FuncionarioId { get; set; }
    public decimal ValorBruto { get; set; }
    public PeriodicidadePagamento Periodicidade { get; set; }
    public DateTimeOffset ProximoPagamentoEm { get; set; }
    public bool Ativa { get; set; } = true;
}

public sealed class PagamentoFuncionario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProgramacaoPagamentoId { get; set; }
    public Guid FuncionarioId { get; set; }
    public decimal ValorBruto { get; set; }
    public DateTimeOffset Competencia { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public bool EventoPublicado { get; set; }
}
