using System.ComponentModel.DataAnnotations;

namespace CivilConstruction.Application.DTOs
{
    public class FuncionarioDto
    {
        public Guid? Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Nome { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        public decimal ValorDiaria { get; set; }
    }
}
