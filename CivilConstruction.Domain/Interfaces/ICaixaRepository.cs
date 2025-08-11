using CivilConstruction.Domain.Entities;

namespace CivilConstruction.Domain.Interfaces
{
    public interface ICaixaRepository
    {
        Task<Caixa?> ObterAsync();
        Task InicializarAsync(Caixa caixa);
        Task AtualizarAsync(Caixa caixa);
    }
}
