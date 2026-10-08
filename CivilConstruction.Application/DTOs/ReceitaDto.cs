using System.ComponentModel.DataAnnotations;

namespace CivilConstruction.Application.DTOs
{
    public class ReceitaDto
    {
        public Guid? Id { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Descricao { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        public decimal Valor { get; set; }
    }
}
