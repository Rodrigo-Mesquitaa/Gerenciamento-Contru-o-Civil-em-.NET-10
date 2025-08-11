using CivilConstruction.Domain.Entities;

namespace CivilConstruction.Domain.Interfaces
{
    public interface IFuncionarioRepository
    {
        Task<IEnumerable<Funcionario>> ObterTodosAsync();
        Task<Funcionario?> ObterPorIdAsync(Guid id);
        Task AdicionarAsync(Funcionario funcionario);
        Task AtualizarAsync(Funcionario funcionario);
        Task RemoverAsync(Guid id);
    }
}
