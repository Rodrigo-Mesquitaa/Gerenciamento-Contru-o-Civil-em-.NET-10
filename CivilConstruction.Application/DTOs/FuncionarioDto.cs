namespace CivilConstruction.Application.DTOs
{
    public class FuncionarioDto
    {
        public Guid? Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal ValorDiaria { get; set; }
    }
}
