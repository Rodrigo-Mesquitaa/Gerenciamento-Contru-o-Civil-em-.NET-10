using CivilConstruction.Application.DTOs;
using CivilConstruction.Application.Interfaces;
using CivilConstruction.Domain.Entities;
using CivilConstruction.Domain.Events;
using System.Text.Json;

namespace CivilConstruction.Application.Services;

public sealed class GestaoConstrucaoService(
    IGestaoConstrucaoRepository repository,
    IUnitOfWork unitOfWork,
    IIntegrationEventOutbox outbox) : IGestaoConstrucaoService
{
    public async Task<IReadOnlyList<ObraResponse>> ListarObrasAsync(CancellationToken ct) =>
        (await repository.ListarObrasAsync(ct)).Select(Map).ToArray();
    public async Task<ObraResponse?> ObterObraAsync(Guid id, CancellationToken ct) =>
        (await repository.ObterObraAsync(id, ct)) is { } obra ? Map(obra) : null;

    public async Task<ObraResponse> CriarObraAsync(CriarObraRequest request, CancellationToken ct)
    {
        if (request.DataInicioPrevista == default)
            throw new ArgumentException("A data prevista de início é obrigatória.");
        if (request.DataFimPrevista < request.DataInicioPrevista)
            throw new ArgumentException("A previsão de término não pode anteceder o início.");

        var obra = new Obra
        {
            Nome = request.Nome.Trim(),
            Endereco = request.Endereco.Trim(),
            Cliente = request.Cliente.Trim(),
            Orcamento = request.Orcamento,
            DataInicioPrevista = request.DataInicioPrevista,
            DataFimPrevista = request.DataFimPrevista
        };
        await repository.AdicionarObraAsync(obra, ct);
        await unitOfWork.CommitAsync(ct);
        return Map(obra);
    }

    public async Task<ObraResponse?> AlterarStatusObraAsync(Guid id, StatusObra status, CancellationToken ct)
    {
        var obra = await repository.ObterObraTrackedAsync(id, ct);
        if (obra is null) return null;
        if (!Enum.IsDefined(status)) throw new ArgumentException("Situação de obra inválida.");

        var transicaoValida = (obra.Status, status) switch
        {
            (StatusObra.Planejada, StatusObra.EmExecucao or StatusObra.Cancelada) => true,
            (StatusObra.EmExecucao, StatusObra.Pausada or StatusObra.Concluida or StatusObra.Cancelada) => true,
            (StatusObra.Pausada, StatusObra.EmExecucao or StatusObra.Cancelada) => true,
            _ => false
        };
        if (!transicaoValida)
            throw new InvalidOperationException($"Transição de obra de {obra.Status} para {status} não permitida.");

        obra.Status = status;
        await unitOfWork.CommitAsync(ct);
        return Map(obra);
    }

    public async Task<IReadOnlyList<FornecedorResponse>> ListarFornecedoresAsync(CancellationToken ct) =>
        (await repository.ListarFornecedoresAsync(ct)).Select(Map).ToArray();
    public async Task<FornecedorResponse?> ObterFornecedorAsync(Guid id, CancellationToken ct) =>
        (await repository.ObterFornecedorAsync(id, ct)) is { } fornecedor ? Map(fornecedor) : null;

    public async Task<FornecedorResponse> CriarFornecedorAsync(CriarFornecedorRequest request, CancellationToken ct)
    {
        var documento = NormalizarDocumento(request.Documento);
        if (await repository.DocumentoFornecedorExisteAsync(documento, ct))
            throw new InvalidOperationException("Já existe um fornecedor cadastrado com esse documento.");

        var fornecedor = new Fornecedor
        {
            Nome = request.Nome.Trim(),
            Documento = documento,
            Email = request.Email.Trim(),
            Telefone = request.Telefone.Trim()
        };
        await repository.AdicionarFornecedorAsync(fornecedor, ct);
        await unitOfWork.CommitAsync(ct);
        return Map(fornecedor);
    }

    public async Task<IReadOnlyList<OrdemCompraResponse>> ListarOrdensCompraAsync(CancellationToken ct) =>
        (await repository.ListarOrdensCompraAsync(ct)).Select(Map).ToArray();
    public async Task<OrdemCompraResponse?> ObterOrdemCompraPorIdAsync(Guid id, CancellationToken ct) =>
        (await repository.ObterOrdemCompraPorIdAsync(id, ct)) is { } ordem ? Map(ordem) : null;

    public async Task<OrdemCompraResponse> CriarOrdemCompraAsync(
        CriarOrdemCompraRequest request,
        CancellationToken ct)
    {
        if (!await repository.ObraExisteAsync(request.ObraId, ct))
            throw new KeyNotFoundException("Obra não encontrada.");
        if (!await repository.FornecedorExisteAsync(request.FornecedorId, ct))
            throw new KeyNotFoundException("Fornecedor não encontrado.");

        var ordem = new OrdemCompra
        {
            ObraId = request.ObraId,
            FornecedorId = request.FornecedorId,
            Itens = request.Itens.Select(item => new ItemOrdemCompra
            {
                Codigo = item.Codigo.Trim(),
                Descricao = item.Descricao.Trim(),
                Quantidade = item.Quantidade,
                Unidade = item.Unidade.Trim(),
                ValorUnitario = item.ValorUnitario
            }).ToList()
        };
        await repository.AdicionarOrdemCompraAsync(ordem, ct);
        await unitOfWork.CommitAsync(ct);
        return Map(ordem);
    }

    public async Task<OrdemCompraResponse?> AlterarStatusOrdemCompraAsync(
        Guid id,
        StatusOrdemCompra status,
        CancellationToken ct)
    {
        var ordem = await repository.ObterOrdemCompraAsync(id, ct);
        if (ordem is null) return null;

        var transicaoValida = (ordem.Status, status) switch
        {
            (StatusOrdemCompra.Rascunho, StatusOrdemCompra.Aprovada) => true,
            (StatusOrdemCompra.Rascunho, StatusOrdemCompra.Cancelada) => true,
            (StatusOrdemCompra.Aprovada, StatusOrdemCompra.Recebida) => true,
            (StatusOrdemCompra.Aprovada, StatusOrdemCompra.Cancelada) => true,
            _ => false
        };
        if (!transicaoValida)
            throw new InvalidOperationException($"Transição de {ordem.Status} para {status} não permitida.");

        if (status == StatusOrdemCompra.Recebida)
        {
            foreach (var item in ordem.Itens)
                await repository.AdicionarOuAtualizarEstoqueAsync(item, ct);
        }

        ordem.Status = status;
        await unitOfWork.CommitAsync(ct);
        return Map(ordem);
    }

    public async Task<IReadOnlyList<ItemEstoqueResponse>> ListarEstoqueAsync(CancellationToken ct) =>
        (await repository.ListarEstoqueAsync(ct)).Select(item =>
            new ItemEstoqueResponse(item.Id, item.Codigo, item.Descricao, item.Unidade, item.Quantidade, item.EstoqueMinimo))
            .ToArray();

    public async Task<IReadOnlyList<EquipamentoResponse>> ListarEquipamentosAsync(CancellationToken ct) =>
        (await repository.ListarEquipamentosAsync(ct)).Select(Map).ToArray();
    public async Task<EquipamentoResponse?> ObterEquipamentoAsync(Guid id, CancellationToken ct) =>
        (await repository.ObterEquipamentoAsync(id, ct)) is { } equipamento ? Map(equipamento) : null;

    public async Task<EquipamentoResponse> CriarEquipamentoAsync(
        CriarEquipamentoRequest request,
        CancellationToken ct)
    {
        if (request.DataAquisicao == default)
            throw new ArgumentException("A data de aquisição é obrigatória.");
        if (request.ObraId is not null && !await repository.ObraExisteAsync(request.ObraId, ct))
            throw new KeyNotFoundException("Obra não encontrada.");
        if (await repository.EquipamentoExisteAsync(request.NumeroPatrimonio.Trim(), ct))
            throw new InvalidOperationException("Já existe um equipamento com esse número de patrimônio.");

        var equipamento = new Equipamento
        {
            Nome = request.Nome.Trim(),
            NumeroPatrimonio = request.NumeroPatrimonio.Trim(),
            NumeroSerie = request.NumeroSerie?.Trim(),
            ObraId = request.ObraId,
            DataAquisicao = request.DataAquisicao,
            ProximaManutencao = request.ProximaManutencao,
            ValorAquisicao = request.ValorAquisicao
        };
        await repository.AdicionarEquipamentoAsync(equipamento, ct);
        await unitOfWork.CommitAsync(ct);
        return Map(equipamento);
    }

    public async Task<EquipamentoResponse?> AlterarStatusEquipamentoAsync(
        Guid id, StatusEquipamento status, Guid? obraId, CancellationToken ct)
    {
        var equipamento = await repository.ObterEquipamentoTrackedAsync(id, ct);
        if (equipamento is null) return null;
        if (!Enum.IsDefined(status)) throw new ArgumentException("Situação de equipamento inválida.");
        if (status == StatusEquipamento.EmUso && obraId is null)
            throw new ArgumentException("Informe a obra para a qual o equipamento será alocado.");
        if (obraId is not null && !await repository.ObraExisteAsync(obraId, ct))
            throw new KeyNotFoundException("Obra não encontrada.");
        if (equipamento.Status == StatusEquipamento.Baixado && status != StatusEquipamento.Baixado)
            throw new InvalidOperationException("Equipamento baixado não pode voltar à operação.");

        equipamento.Status = status;
        equipamento.ObraId = status == StatusEquipamento.EmUso ? obraId : null;
        await unitOfWork.CommitAsync(ct);
        return Map(equipamento);
    }

    public async Task<IReadOnlyList<ProgramacaoPagamentoResponse>> ListarProgramacoesPagamentoAsync(CancellationToken ct) =>
        (await repository.ListarProgramacoesPagamentoAsync(ct)).Select(Map).ToArray();
    public async Task<ProgramacaoPagamentoResponse?> ObterProgramacaoPagamentoAsync(Guid id, CancellationToken ct) =>
        (await repository.ObterProgramacaoPagamentoAsync(id, ct)) is { } programacao ? Map(programacao) : null;

    public async Task<ProgramacaoPagamentoResponse> CriarProgramacaoPagamentoAsync(
        CriarProgramacaoPagamentoRequest request,
        CancellationToken ct)
    {
        if (request.ProximoPagamentoEm == default)
            throw new ArgumentException("A data do próximo pagamento é obrigatória.");
        if (!await repository.FuncionarioExisteAsync(request.FuncionarioId, ct))
            throw new KeyNotFoundException("Funcionário não encontrado.");
        if (!Enum.IsDefined(request.Periodicidade))
            throw new ArgumentException("Periodicidade inválida.");

        var programacao = new ProgramacaoPagamento
        {
            FuncionarioId = request.FuncionarioId,
            ValorBruto = request.ValorBruto,
            Periodicidade = request.Periodicidade,
            ProximoPagamentoEm = request.ProximoPagamentoEm
        };
        await repository.AdicionarProgramacaoPagamentoAsync(programacao, ct);
        await unitOfWork.CommitAsync(ct);
        return Map(programacao);
    }

    public async Task<ProgramacaoPagamentoResponse?> AlterarAtividadeProgramacaoPagamentoAsync(
        Guid id, bool ativa, CancellationToken ct)
    {
        var programacao = await repository.ObterProgramacaoPagamentoTrackedAsync(id, ct);
        if (programacao is null) return null;
        programacao.Ativa = ativa;
        await unitOfWork.CommitAsync(ct);
        return Map(programacao);
    }

    public async Task<IReadOnlyList<PagamentoFuncionarioResponse>> GerarPagamentosVencidosAsync(
        DateTimeOffset agora,
        CancellationToken ct)
    {
        var programacoes = await repository.ObterProgramacoesVencidasAsync(agora, ct);
        var pagamentos = new List<PagamentoFuncionarioResponse>();
        foreach (var programacao in programacoes)
        {
            var vencimentosProcessados = 0;
            while (programacao.ProximoPagamentoEm <= agora)
            {
                if (++vencimentosProcessados > 120)
                    throw new InvalidOperationException("A programação contém mais de 120 competências vencidas; regularize-a antes de processar.");

                var competencia = programacao.ProximoPagamentoEm;
                if (!await repository.PagamentoExisteAsync(programacao.Id, competencia, ct))
                {
                    var pagamento = new PagamentoFuncionario
                    {
                        ProgramacaoPagamentoId = programacao.Id,
                        FuncionarioId = programacao.FuncionarioId,
                        ValorBruto = programacao.ValorBruto,
                        Competencia = competencia
                    };
                    await repository.AdicionarPagamentoAsync(pagamento, ct);
                    pagamentos.Add(Map(pagamento));
                }

                programacao.ProximoPagamentoEm = Avancar(competencia, programacao.Periodicidade);
            }
        }

        if (programacoes.Count > 0)
            await unitOfWork.CommitAsync(ct);
        return pagamentos;
    }

    public async Task<IReadOnlyList<PagamentoFuncionarioResponse>> ListarPagamentosPendentesAsync(CancellationToken ct) =>
        (await repository.ListarPagamentosPendentesAsync(ct)).Select(Map).ToArray();

    public async Task EnfileirarPagamentoAsync(Guid pagamentoId, CancellationToken ct)
    {
        var pagamento = await repository.ObterPagamentoAsync(pagamentoId, ct)
            ?? throw new KeyNotFoundException("Pagamento não encontrado.");

        if (pagamento.EventoPublicado || await outbox.ContainsAsync(pagamento.Id, ct))
            return;

        var evento = new PagamentoRealizadoEvent
        {
            Id = pagamento.Id,
            FuncionarioId = pagamento.FuncionarioId,
            ValorTotal = pagamento.ValorBruto,
            QuantidadeFuncionarios = 1,
            DataPagamento = pagamento.Competencia,
            Observacao = $"Pagamento programado {pagamento.ProgramacaoPagamentoId}"
        };
        outbox.Add(
            evento.Id,
            IntegrationEventTypes.ScheduledPaymentCreated,
            1,
            JsonSerializer.Serialize(evento),
            DateTimeOffset.UtcNow);
        await unitOfWork.CommitAsync(ct);
    }

    private static DateTimeOffset Avancar(DateTimeOffset vencimento, PeriodicidadePagamento periodicidade) =>
        periodicidade switch
        {
            PeriodicidadePagamento.Semanal => vencimento.AddDays(7),
            PeriodicidadePagamento.Quinzenal => vencimento.AddDays(15),
            PeriodicidadePagamento.Mensal => vencimento.AddMonths(1),
            _ => throw new ArgumentOutOfRangeException(nameof(periodicidade))
        };

    private static string NormalizarDocumento(string documento) =>
        new(documento.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    private static ObraResponse Map(Obra obra) =>
        new(obra.Id, obra.Nome, obra.Endereco, obra.Cliente, obra.Orcamento,
            obra.DataInicioPrevista, obra.DataFimPrevista, obra.Status);
    private static FornecedorResponse Map(Fornecedor fornecedor) =>
        new(fornecedor.Id, fornecedor.Nome, fornecedor.Documento, fornecedor.Email, fornecedor.Telefone);
    private static OrdemCompraResponse Map(OrdemCompra ordem) =>
        new(ordem.Id, ordem.ObraId, ordem.FornecedorId, ordem.CriadaEm, ordem.Status, ordem.ValorTotal,
            ordem.Itens.Select(item => new ItemOrdemCompraRequest
            {
                Codigo = item.Codigo,
                Descricao = item.Descricao,
                Quantidade = item.Quantidade,
                Unidade = item.Unidade,
                ValorUnitario = item.ValorUnitario
            }).ToArray());
    private static EquipamentoResponse Map(Equipamento equipamento) =>
        new(equipamento.Id, equipamento.Nome, equipamento.NumeroPatrimonio, equipamento.NumeroSerie,
            equipamento.ObraId, equipamento.DataAquisicao, equipamento.ProximaManutencao,
            equipamento.ValorAquisicao, equipamento.Status);
    private static ProgramacaoPagamentoResponse Map(ProgramacaoPagamento programacao) =>
        new(programacao.Id, programacao.FuncionarioId, programacao.ValorBruto, programacao.Periodicidade,
            programacao.ProximoPagamentoEm, programacao.Ativa);
    private static PagamentoFuncionarioResponse Map(PagamentoFuncionario pagamento) =>
        new(pagamento.Id, pagamento.ProgramacaoPagamentoId, pagamento.FuncionarioId, pagamento.ValorBruto,
            pagamento.Competencia, pagamento.CriadoEm, pagamento.EventoPublicado);
}
