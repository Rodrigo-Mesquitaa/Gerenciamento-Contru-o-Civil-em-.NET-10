using CivilConstruction.Domain.Entities;

namespace CivilConstruction.Domain.Interfaces
{
    public interface IFuncionarioRepository
    {
        Task<IEnumerable<Funcionario>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task<Funcionario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AdicionarAsync(Funcionario funcionario, CancellationToken cancellationToken = default);
        Task AtualizarAsync(Funcionario funcionario, CancellationToken cancellationToken = default);
        Task<bool> RemoverAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
