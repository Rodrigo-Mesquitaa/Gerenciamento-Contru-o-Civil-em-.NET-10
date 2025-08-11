using CivilConstruction.Application.DTOs;

namespace CivilConstruction.Application.Interfaces
{
    public interface IServicoFinanceiro
    {
        Task<IEnumerable<ReceitaDto>> ObterReceitasAsync();
        Task<IEnumerable<DespesaDto>> ObterDespesasAsync();
        Task AdicionarReceitaAsync(ReceitaDto dto);
        Task AdicionarDespesaAsync(DespesaDto dto);
        Task<decimal> CalcularLucroAsync();
        Task<CaixaDto> ObterCaixaAsync();
    }
}
