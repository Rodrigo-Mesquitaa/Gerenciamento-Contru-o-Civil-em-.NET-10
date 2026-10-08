namespace CivilConstruction.Domain.Events
{
    public class PagamentoRealizadoEvent
    {
        public Guid Id { get; set; }
        public decimal ValorTotal { get; set; }
        public int QuantidadeFuncionarios { get; set; }
        public Guid FuncionarioId { get; set; }
        public DateTimeOffset DataPagamento { get; set; }
        public string Observacao { get; set; } = string.Empty;
    }
}
