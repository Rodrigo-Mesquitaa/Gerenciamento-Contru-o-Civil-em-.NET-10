using CivilConstruction.Application.DTOs;
using CivilConstruction.Application.Interfaces;
using CivilConstruction.Application.Services;
using CivilConstruction.Domain.Entities;
using CivilConstruction.Domain.Interfaces;

namespace CivilConstruction.Tests
{
    public class ServicoFinanceiroTests
    {
        [Fact]
        public async Task AdicionarReceita_AtualizaCaixaEConfirmaUmaUnicaVez()
        {
            var receitas = new ReceitaRepositoryFake();
            var caixa = new CaixaRepositoryFake
            {
                Caixa = new Caixa { Id = Guid.NewGuid(), SaldoInicial = 20000m, SaldoAtual = 20000m }
            };
            var unitOfWork = new UnitOfWorkFake();
            var service = new ServicoFinanceiro(
                receitas,
                new DespesaRepositoryFake(),
                caixa,
                unitOfWork);

            await service.AdicionarReceitaAsync(new ReceitaDto { Descricao = "Serviço", Valor = 250m });

            Assert.Equal(20250m, caixa.Caixa.SaldoAtual);
            Assert.Single(receitas.Receitas);
            Assert.Equal(1, unitOfWork.CommitCount);
        }

        [Fact]
        public async Task AdicionarDespesa_AtualizaCaixaEConfirmaUmaUnicaVez()
        {
            var despesas = new DespesaRepositoryFake();
            var caixa = new CaixaRepositoryFake
            {
                Caixa = new Caixa { Id = Guid.NewGuid(), SaldoInicial = 20000m, SaldoAtual = 20000m }
            };
            var unitOfWork = new UnitOfWorkFake();
            var service = new ServicoFinanceiro(
                new ReceitaRepositoryFake(),
                despesas,
                caixa,
                unitOfWork);

            await service.AdicionarDespesaAsync(new DespesaDto { Descricao = "Material", Valor = 100m });

            Assert.Equal(19900m, caixa.Caixa.SaldoAtual);
            Assert.Single(despesas.Despesas);
            Assert.Equal(1, unitOfWork.CommitCount);
        }

        private sealed class ReceitaRepositoryFake : IReceitaRepository
        {
            public List<Receita> Receitas { get; } = [];

            public Task AdicionarAsync(Receita receita)
            {
                Receitas.Add(receita);
                return Task.CompletedTask;
            }

            public Task<IEnumerable<Receita>> ObterTodasAsync() =>
                Task.FromResult<IEnumerable<Receita>>(Receitas);

            public Task<Receita?> ObterPorIdAsync(Guid id) =>
                Task.FromResult(Receitas.FirstOrDefault(receita => receita.Id == id));
        }

        private sealed class DespesaRepositoryFake : IDespesaRepository
        {
            public List<Despesa> Despesas { get; } = [];

            public Task AdicionarAsync(Despesa despesa)
            {
                Despesas.Add(despesa);
                return Task.CompletedTask;
            }

            public Task<IEnumerable<Despesa>> ObterTodasAsync() =>
                Task.FromResult<IEnumerable<Despesa>>(Despesas);

            public Task<Despesa?> ObterPorIdAsync(Guid id) =>
                Task.FromResult(Despesas.FirstOrDefault(despesa => despesa.Id == id));
        }

        private sealed class CaixaRepositoryFake : ICaixaRepository
        {
            public Caixa? Caixa { get; set; }

            public Task<Caixa?> ObterAsync() => Task.FromResult(Caixa);

            public Task InicializarAsync(Caixa caixa)
            {
                Caixa = caixa;
                return Task.CompletedTask;
            }

            public Task AtualizarAsync(Caixa caixa)
            {
                Caixa = caixa;
                return Task.CompletedTask;
            }
        }

        private sealed class UnitOfWorkFake : IUnitOfWork
        {
            public int CommitCount { get; private set; }

            public Task CommitAsync(CancellationToken cancellationToken = default)
            {
                CommitCount++;
                return Task.CompletedTask;
            }
        }
    }
}
