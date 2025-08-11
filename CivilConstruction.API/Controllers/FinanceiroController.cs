using CivilConstruction.Application.DTOs;
using CivilConstruction.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CivilConstruction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinanceiroController : ControllerBase
    {
        private readonly IServicoFinanceiro _servico;

        public FinanceiroController(IServicoFinanceiro servico)
        {
            _servico = servico;
        }

        [HttpGet("receitas")] public async Task<IActionResult> GetReceitas() => Ok(await _servico.ObterReceitasAsync());
        [HttpPost("receitas")] public async Task<IActionResult> PostReceita([FromBody] ReceitaDto dto) { await _servico.AdicionarReceitaAsync(dto); return Created("", dto); }

        [HttpGet("despesas")] public async Task<IActionResult> GetDespesas() => Ok(await _servico.ObterDespesasAsync());
        [HttpPost("despesas")] public async Task<IActionResult> PostDespesa([FromBody] DespesaDto dto) { await _servico.AdicionarDespesaAsync(dto); return Created("", dto); }

        [HttpGet("caixa")] public async Task<IActionResult> GetCaixa() => Ok(await _servico.ObterCaixaAsync());
        [HttpGet("lucro")] public async Task<IActionResult> GetLucro() => Ok(new { Lucro = await _servico.CalcularLucroAsync() });
    }
}