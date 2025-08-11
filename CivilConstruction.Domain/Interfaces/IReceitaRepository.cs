using CivilConstruction.Domain.Entities;

namespace CivilConstruction.Domain.Interfaces
{
    public interface IReceitaRepository
    {
        Task<IEnumerable<Receita>> ObterTodasAsync();
        Task<Receita?> ObterPorIdAsync(Guid id);
        Task AdicionarAsync(Receita receita);
    }
}
