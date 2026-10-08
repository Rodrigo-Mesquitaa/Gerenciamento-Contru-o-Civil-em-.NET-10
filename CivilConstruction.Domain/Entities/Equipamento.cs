namespace CivilConstruction.Domain.Entities;

public enum StatusEquipamento
{
    Disponivel,
    EmUso,
    EmManutencao,
    Baixado
}

public sealed class Equipamento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public string NumeroPatrimonio { get; set; } = string.Empty;
    public string? NumeroSerie { get; set; }
    public Guid? ObraId { get; set; }
    public DateTimeOffset DataAquisicao { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProximaManutencao { get; set; }
    public decimal ValorAquisicao { get; set; }
    public StatusEquipamento Status { get; set; } = StatusEquipamento.Disponivel;
}
