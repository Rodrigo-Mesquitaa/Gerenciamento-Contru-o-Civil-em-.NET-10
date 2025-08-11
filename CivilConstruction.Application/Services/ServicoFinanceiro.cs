using CivilConstruction.Application.DTOs;
using CivilConstruction.Application.Interfaces;
using CivilConstruction.Domain.Entities;
using CivilConstruction.Domain.Interfaces;

namespace CivilConstruction.Application.Services
{
    public class ServicoFinanceiro : IServicoFinanceiro
    {
        private readonly IReceitaRepository _receitaRepo;
        private readonly IDespesaRepository _despesaRepo;
        private readonly ICaixaRepository _caixaRepo;

        public ServicoFinanceiro(IReceitaRepository receitaRepo, IDespesaRepository despesaRepo, ICaixaRepository caixaRepo)
        {
            _receitaRepo = receitaRepo;
            _despesaRepo = despesaRepo;
            _caixaRepo = caixaRepo;
        }

        public async Task<IEnumerable<ReceitaDto>> ObterReceitasAsync()
        {
            var lista = await _receitaRepo.ObterTodasAsync();
            return lista.Select(r => new ReceitaDto { Id = r.Id, Descricao = r.Descricao, Valor = r.Valor });
        }

        public async Task<IEnumerable<DespesaDto>> ObterDespesasAsync()
        {
            var lista = await _despesaRepo.ObterTodasAsync();
            return lista.Select(d => new DespesaDto { Id = d.Id, Descricao = d.Descricao, Valor = d.Valor });
        }

        public async Task AdicionarReceitaAsync(ReceitaDto dto)
        {
            var entity = new Receita { Id = dto.Id ?? System.Guid.NewGuid(), Descricao = dto.Descricao, Valor = dto.Valor };
            await _receitaRepo.AdicionarAsync(entity);
            var caixa = await _caixaRepo.ObterAsync();
            if (caixa == null)
            {
                var novo = new Caixa { Id = System.Guid.NewGuid(), SaldoInicial = 20000m, SaldoAtual = 20000m + entity.Valor };
                await _caixaRepo.InicializarAsync(novo);
            }
            else
            {
                caixa.SaldoAtual += entity.Valor;
                await _caixaRepo.AtualizarAsync(caixa);
            }
        }

        public async Task AdicionarDespesaAsync(DespesaDto dto)
        {
            var entity = new Despesa { Id = dto.Id ?? System.Guid.NewGuid(), Descricao = dto.Descricao, Valor = dto.Valor };
            await _despesaRepo.AdicionarAsync(entity);
            var caixa = await _caixaRepo.ObterAsync();
            if (caixa == null)
            {
                var novo = new Caixa { Id = System.Guid.NewGuid(), SaldoInicial = 20000m, SaldoAtual = 20000m - entity.Valor };
                await _caixaRepo.InicializarAsync(novo);
            }
            else
            {
                caixa.SaldoAtual -= entity.Valor;
                await _caixaRepo.AtualizarAsync(caixa);
            }
        }

        public async Task<decimal> CalcularLucroAsync()
        {
            var receitas = await _receitaRepo.ObterTodasAsync();
            var despesas = await _despesaRepo.ObterTodasAsync();
            var totalReceitas = receitas.Sum(r => r.Valor);
            var totalDespesas = despesas.Sum(d => d.Valor);
            return totalReceitas - totalDespesas;
        }

        public async Task<CaixaDto> ObterCaixaAsync()
        {
            var caixa = await _caixaRepo.ObterAsync();
            if (caixa == null) return new CaixaDto { SaldoInicial = 20000m, SaldoAtual = 20000m };
            return new CaixaDto { SaldoInicial = caixa.SaldoInicial, SaldoAtual = caixa.SaldoAtual };
        }
    }
}
