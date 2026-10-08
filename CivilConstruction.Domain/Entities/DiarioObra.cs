namespace CivilConstruction.Domain.Entities;

public sealed class ApontamentoObra
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ObraId { get; set; }
    public DateTimeOffset Data { get; set; }
    public string Clima { get; set; } = string.Empty;
    public string Atividades { get; set; } = string.Empty;
    public int TrabalhadoresPresentes { get; set; }
    public decimal HorasTrabalhadas { get; set; }
    public string Observacoes { get; set; } = string.Empty;
}

public sealed class AlocacaoFuncionarioObra
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ObraId { get; set; }
    public Guid FuncionarioId { get; set; }
    public DateTimeOffset Inicio { get; set; }
    public DateTimeOffset? Fim { get; set; }
    public string Funcao { get; set; } = string.Empty;
}

public enum GravidadeIncidente
{
    QuaseAcidente,
    Leve,
    Moderado,
    Grave,
    Fatal
}

public sealed class IncidenteSeguranca
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ObraId { get; set; }
    public Guid? FuncionarioId { get; set; }
    public DateTimeOffset OcorreuEm { get; set; }
    public GravidadeIncidente Gravidade { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string AcoesCorretivas { get; set; } = string.Empty;
    public bool Encerrado { get; set; }
}
