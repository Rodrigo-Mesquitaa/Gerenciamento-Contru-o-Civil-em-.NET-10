using CivilConstruction.Domain.Interfaces;
using CivilConstruction.Infrastructure.Context;
using CivilConstruction.Infrastructure.Repositories;

namespace CivilConstruction.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Funcionarios = new FuncionarioRepository(_context);
            Receitas = new ReceitaRepository(_context);
            Despesas = new DespesaRepository(_context);
            Caixas = new CaixaRepository(_context);
        }

        public IFuncionarioRepository Funcionarios { get; }
        public IReceitaRepository Receitas { get; }
        public IDespesaRepository Despesas { get; }
        public ICaixaRepository Caixas { get; }

        public async Task CommitAsync() => await _context.SaveChangesAsync();
    }
}
