namespace CivilConstruction.Domain.Entities;

public enum StatusObra
{
    Planejada,
    EmExecucao,
    Pausada,
    Concluida,
    Cancelada
}

public sealed class Obra
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public decimal Orcamento { get; set; }
    public DateTimeOffset DataInicioPrevista { get; set; }
    public DateTimeOffset? DataFimPrevista { get; set; }
    public StatusObra Status { get; set; } = StatusObra.Planejada;
}
