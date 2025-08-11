using CivilConstruction.Application.DTOs;

namespace CivilConstruction.Application.Interfaces
{
    public interface IServicoFuncionario
    {
        Task<IEnumerable<FuncionarioDto>> ObterTodosAsync();
        Task<FuncionarioDto?> ObterPorIdAsync(Guid id);
        Task CriarAsync(FuncionarioDto dto);
        Task AtualizarAsync(FuncionarioDto dto);
        Task RemoverAsync(Guid id);
    }
}
