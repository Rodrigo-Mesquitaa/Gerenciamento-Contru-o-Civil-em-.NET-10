namespace CivilConstruction.Domain.Entities;

public sealed class ItemEstoque
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Unidade { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal EstoqueMinimo { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
