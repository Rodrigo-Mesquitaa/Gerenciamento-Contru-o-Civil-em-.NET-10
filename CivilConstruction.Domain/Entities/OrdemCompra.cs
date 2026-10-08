namespace CivilConstruction.Domain.Entities;

public enum StatusOrdemCompra
{
    Rascunho,
    Aprovada,
    Recebida,
    Cancelada
}

public sealed class OrdemCompra
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ObraId { get; set; }
    public Guid FornecedorId { get; set; }
    public DateTimeOffset CriadaEm { get; set; } = DateTimeOffset.UtcNow;
    public StatusOrdemCompra Status { get; set; } = StatusOrdemCompra.Rascunho;
    public byte[] RowVersion { get; set; } = [];
    public List<ItemOrdemCompra> Itens { get; set; } = [];
    public decimal ValorTotal => Itens.Sum(item => item.Quantidade * item.ValorUnitario);
}

public sealed class ItemOrdemCompra
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrdemCompraId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public string Unidade { get; set; } = string.Empty;
    public decimal ValorUnitario { get; set; }
}
