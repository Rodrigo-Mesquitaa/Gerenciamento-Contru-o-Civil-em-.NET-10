using CivilConstruction.Application.DTOs;

namespace CivilConstruction.Application.Interfaces
{
    public interface IServicoFuncionario
    {
        Task<IEnumerable<FuncionarioDto>> ObterTodosAsync();
        Task<FuncionarioDto?> ObterPorIdAsync(Guid id);
        Task CriarAsync(FuncionarioDto dto, CancellationToken cancellationToken = default);
        Task<bool> AtualizarAsync(FuncionarioDto dto);
        Task<bool> RemoverAsync(Guid id);
    }
}
