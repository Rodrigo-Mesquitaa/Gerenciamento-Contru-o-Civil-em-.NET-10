using CivilConstruction.Domain.Interfaces;

namespace CivilConstruction.Infrastructure.UnitOfWork
{
    public interface IUnitOfWork
    {
        IFuncionarioRepository Funcionarios { get; }
        IReceitaRepository Receitas { get; }
        IDespesaRepository Despesas { get; }
        ICaixaRepository Caixas { get; }
        Task CommitAsync();
    }
}
