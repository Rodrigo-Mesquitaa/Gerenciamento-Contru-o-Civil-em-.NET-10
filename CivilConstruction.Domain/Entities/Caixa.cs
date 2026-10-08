namespace CivilConstruction.Domain.Entities
{
    public class Caixa
    {
        public Guid Id { get; set; }
        public decimal SaldoInicial { get; set; } = 20000m;
        public decimal SaldoAtual { get; set; } = 20000m;
        public byte[] RowVersion { get; set; } = [];
    }
}
