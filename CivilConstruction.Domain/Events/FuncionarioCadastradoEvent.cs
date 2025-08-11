namespace CivilConstruction.Domain.Events
{
    public class FuncionarioCadastradoEvent
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal ValorDiaria { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
