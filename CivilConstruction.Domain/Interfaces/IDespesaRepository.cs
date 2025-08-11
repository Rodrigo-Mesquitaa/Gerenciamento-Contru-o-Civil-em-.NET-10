using CivilConstruction.Domain.Entities;

namespace CivilConstruction.Domain.Interfaces
{
    public interface IDespesaRepository
    {
        Task<IEnumerable<Despesa>> ObterTodasAsync();
        Task<Despesa?> ObterPorIdAsync(Guid id);
        Task AdicionarAsync(Despesa despesa);
    }
}
