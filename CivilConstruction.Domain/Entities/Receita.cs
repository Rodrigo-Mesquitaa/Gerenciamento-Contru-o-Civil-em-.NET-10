namespace CivilConstruction.Domain.Entities
{
    public class Receita
    {
        public Guid Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime Data { get; set; } = DateTime.UtcNow;
    }
}
