namespace CivilConstruction.Application.DTOs
{
    public class DespesaDto
    {
        public Guid? Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
    }
}
