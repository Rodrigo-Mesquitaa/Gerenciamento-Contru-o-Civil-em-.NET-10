namespace CivilConstruction.Domain.Entities;

public enum ResultadoInspecao
{
    Aprovada,
    Reprovada,
    Pendente
}

public sealed class InspecaoQualidade
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ObraId { get; set; }
    public string Referencia { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public DateTimeOffset InspecionadaEm { get; set; }
    public ResultadoInspecao Resultado { get; set; } = ResultadoInspecao.Pendente;
    public string Observacoes { get; set; } = string.Empty;
    public string AcaoCorretiva { get; set; } = string.Empty;
    public DateTimeOffset? PrazoCorrecao { get; set; }
    public bool Encerrada { get; set; }
}
