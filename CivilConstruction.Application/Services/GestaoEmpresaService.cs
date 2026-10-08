using CivilConstruction.Application.DTOs;
using CivilConstruction.Application.Interfaces;
using CivilConstruction.Domain.Entities;

namespace CivilConstruction.Application.Services;

public sealed class GestaoEmpresaService(
    IGestaoEmpresaRepository repository,
    IUnitOfWork unitOfWork) : IGestaoEmpresaService
{
    public async Task<IReadOnlyList<ClienteResponse>> ListarClientesAsync(CancellationToken ct) =>
        (await repository.ListarClientesAsync(ct)).Select(Map).ToArray();

    public async Task<ClienteResponse?> ObterClienteAsync(Guid id, CancellationToken ct) =>
        (await repository.ObterClienteAsync(id, ct)) is { } cliente ? Map(cliente) : null;

    public async Task<ClienteResponse> CriarClienteAsync(CriarClienteRequest request, CancellationToken ct)
    {
        var documento = NormalizarDocumento(request.Documento);
        if (await repository.DocumentoClienteExisteAsync(documento, ct))
            throw new InvalidOperationException("Já existe um cliente cadastrado com esse documento.");
        var cliente = new Cliente
        {
            Nome = request.Nome.Trim(),
            Documento = documento,
            Email = request.Email.Trim(),
            Telefone = request.Telefone.Trim()
        };
        await repository.AdicionarClienteAsync(cliente, ct);
        await unitOfWork.CommitAsync(ct);
        return Map(cliente);
    }

    public async Task<IReadOnlyList<ContratoResponse>> ListarContratosAsync(CancellationToken ct)
    {
        var contratos = await repository.ListarContratosAsync(ct);
        var responses = new List<ContratoResponse>(contratos.Count);
        foreach (var contrato in contratos)
            responses.Add(await MapContratoAsync(contrato, ct));
        return responses;
    }

    public async Task<ContratoResponse?> ObterContratoAsync(Guid id, CancellationToken ct)
    {
        var contrato = await repository.ObterContratoAsync(id, ct);
        return contrato is null ? null : await MapContratoAsync(contrato, ct);
    }

    public async Task<ContratoResponse> CriarContratoAsync(CriarContratoRequest request, CancellationToken ct)
    {
        if (request.DataInicio == default || request.DataAssinatura == default || request.DataFimPrevista == default)
            throw new ArgumentException("Datas de assinatura, início e término previsto são obrigatórias.");
        if (request.DataFimPrevista < request.DataInicio)
            throw new ArgumentException("A data final prevista não pode anteceder o início.");
        if (!await repository.ObraExisteAsync(request.ObraId, ct))
            throw new KeyNotFoundException("Obra não encontrada.");
        if (await repository.ObterClienteAsync(request.ClienteId, ct) is null)
            throw new KeyNotFoundException("Cliente não encontrado.");
        var numero = request.Numero.Trim();
        if (await repository.ContratoNumeroExisteAsync(numero, ct))
            throw new InvalidOperationException("Já existe um contrato com esse número.");

        var contrato = new Contrato
        {
            ClienteId = request.ClienteId,
            ObraId = request.ObraId,
            Numero = numero,
            DataAssinatura = request.DataAssinatura,
            DataInicio = request.DataInicio,
            DataFimPrevista = request.DataFimPrevista,
            ValorContratado = request.ValorContratado,
            RetencaoPercentual = request.RetencaoPercentual
        };
        await repository.AdicionarContratoAsync(contrato, ct);
        await unitOfWork.CommitAsync(ct);
        return await MapContratoAsync(contrato, ct);
    }

    public async Task<ContratoResponse?> AlterarStatusContratoAsync(Guid id, StatusContrato status, CancellationToken ct)
    {
        var contrato = await repository.ObterContratoTrackedAsync(id, ct);
        if (contrato is null) return null;
        if (!Enum.IsDefined(status)) throw new ArgumentException("Situação de contrato inválida.");
        var valid = (contrato.Status, status) switch
        {
            (StatusContrato.Rascunho, StatusContrato.Ativo or StatusContrato.Cancelado) => true,
            (StatusContrato.Ativo, StatusContrato.Suspenso or StatusContrato.Encerrado or StatusContrato.Cancelado) => true,
            (StatusContrato.Suspenso, StatusContrato.Ativo or StatusContrato.Encerrado) => true,
            _ => false
        };
        if (!valid) throw new InvalidOperationException($"Transição de contrato de {contrato.Status} para {status} não permitida.");
        contrato.Status = status;
        await unitOfWork.CommitAsync(ct);
        return await MapContratoAsync(contrato, ct);
    }

    public async Task<IReadOnlyList<MedicaoResponse>> ListarMedicoesAsync(Guid contratoId, CancellationToken ct) =>
        (await repository.ListarMedicoesAsync(contratoId, ct)).Select(Map).ToArray();

    public async Task<MedicaoResponse> CriarMedicaoAsync(Guid contratoId, CriarMedicaoRequest request, CancellationToken ct)
    {
        var contrato = await repository.ObterContratoAsync(contratoId, ct)
            ?? throw new KeyNotFoundException("Contrato não encontrado.");
        if (contrato.Status != StatusContrato.Ativo)
            throw new InvalidOperationException("Só é possível medir um contrato ativo.");
        if (request.DataMedicao == default)
            throw new ArgumentException("A data da medição é obrigatória.");
        var referencia = request.Referencia.Trim();
        if (await repository.ReferenciaMedicaoExisteAsync(contratoId, referencia, ct))
            throw new InvalidOperationException("A referência já foi medida neste contrato.");
        var totalPercentual = await repository.ObterTotalMedidoAsync(contratoId, ct);
        if (totalPercentual + request.PercentualExecutado > 100m)
            throw new InvalidOperationException("A medição ultrapassaria 100% do valor contratado.");

        var medicao = new MedicaoContrato
        {
            ContratoId = contratoId,
            Referencia = referencia,
            DataMedicao = request.DataMedicao,
            PercentualExecutado = request.PercentualExecutado,
            ValorMedido = decimal.Round(contrato.ValorContratado * request.PercentualExecutado / 100m, 2)
        };
        await repository.AdicionarMedicaoAsync(medicao, ct);
        await unitOfWork.CommitAsync(ct);
        return Map(medicao);
    }

    public async Task<MedicaoResponse?> AlterarStatusMedicaoAsync(
        Guid id, StatusMedicao status, DateTimeOffset? vencimento, CancellationToken ct)
    {
        var medicao = await repository.ObterMedicaoTrackedAsync(id, ct);
        if (medicao is null) return null;
        if (!Enum.IsDefined(status)) throw new ArgumentException("Situação de medição inválida.");
        var valid = (medicao.Status, status) switch
        {
            (StatusMedicao.Rascunho, StatusMedicao.Enviada or StatusMedicao.Rejeitada) => true,
            (StatusMedicao.Enviada, StatusMedicao.Aprovada or StatusMedicao.Rejeitada) => true,
            (StatusMedicao.Rejeitada, StatusMedicao.Rascunho) => true,
            (StatusMedicao.Aprovada, StatusMedicao.Faturada) => true,
            _ => false
        };
        if (!valid) throw new InvalidOperationException($"Transição de medição de {medicao.Status} para {status} não permitida.");

        if (status == StatusMedicao.Faturada)
        {
            if (vencimento is null || vencimento == default)
                throw new ArgumentException("Informe o vencimento para faturar a medição.");
            if (await repository.LancamentoMedicaoExisteAsync(medicao.Id, ct))
                throw new InvalidOperationException("Esta medição já possui lançamento financeiro.");
            var contrato = await repository.ObterContratoAsync(medicao.ContratoId, ct)
                ?? throw new KeyNotFoundException("Contrato não encontrado.");
            var liquido = decimal.Round(
                medicao.ValorMedido * (100m - contrato.RetencaoPercentual) / 100m, 2);
            await repository.AdicionarLancamentoAsync(new LancamentoFinanceiro
            {
                ObraId = contrato.ObraId,
                ClienteId = contrato.ClienteId,
                MedicaoContratoId = medicao.Id,
                Descricao = $"Faturamento da medição {medicao.Referencia} - contrato {contrato.Numero}",
                Tipo = TipoLancamentoFinanceiro.ContaReceber,
                Valor = liquido,
                Vencimento = vencimento.Value
            }, ct);
        }

        medicao.Status = status;
        await unitOfWork.CommitAsync(ct);
        return Map(medicao);
    }

    public async Task<IReadOnlyList<LancamentoFinanceiroResponse>> ListarLancamentosAsync(
        TipoLancamentoFinanceiro? tipo, CancellationToken ct) =>
        (await repository.ListarLancamentosAsync(tipo, ct))
        .Select(lancamento => Map(lancamento, DateTimeOffset.UtcNow)).ToArray();

    public async Task<LancamentoFinanceiroResponse> CriarLancamentoAsync(
        CriarLancamentoFinanceiroRequest request, CancellationToken ct)
    {
        if (request.Vencimento == default) throw new ArgumentException("O vencimento é obrigatório.");
        if (!Enum.IsDefined(request.Tipo)) throw new ArgumentException("Tipo de lançamento inválido.");
        if (request.ObraId is { } obraId && !await repository.ObraExisteAsync(obraId, ct))
            throw new KeyNotFoundException("Obra não encontrada.");
        if (request.Tipo == TipoLancamentoFinanceiro.ContaPagar)
        {
            if (request.FornecedorId is null) throw new ArgumentException("Conta a pagar exige um fornecedor.");
            if (!await repository.FornecedorExisteAsync(request.FornecedorId.Value, ct))
                throw new KeyNotFoundException("Fornecedor não encontrado.");
            if (request.ClienteId is not null || request.MedicaoContratoId is not null)
                throw new ArgumentException("Conta a pagar não aceita cliente nem medição.");
        }
        else
        {
            if (request.ClienteId is null) throw new ArgumentException("Conta a receber exige um cliente.");
            if (await repository.ObterClienteAsync(request.ClienteId.Value, ct) is null)
                throw new KeyNotFoundException("Cliente não encontrado.");
            if (request.FornecedorId is not null)
                throw new ArgumentException("Conta a receber não aceita fornecedor.");
            if (request.MedicaoContratoId is { } medicaoId && !await repository.MedicaoExisteAsync(medicaoId, ct))
                throw new KeyNotFoundException("Medição não encontrada.");
        }

        var lancamento = new LancamentoFinanceiro
        {
            Descricao = request.Descricao.Trim(),
            Tipo = request.Tipo,
            ObraId = request.ObraId,
            FornecedorId = request.FornecedorId,
            ClienteId = request.ClienteId,
            MedicaoContratoId = request.MedicaoContratoId,
            Valor = request.Valor,
            Vencimento = request.Vencimento,
            ReferenciaExterna = request.ReferenciaExterna?.Trim()
        };
        await repository.AdicionarLancamentoAsync(lancamento, ct);
        await unitOfWork.CommitAsync(ct);
        return Map(lancamento, DateTimeOffset.UtcNow);
    }

    public async Task<LancamentoFinanceiroResponse?> ObterLancamentoAsync(Guid id, CancellationToken ct)
    {
        var lancamento = await repository.ObterLancamentoAsync(id, ct);
        return lancamento is null ? null : Map(lancamento, DateTimeOffset.UtcNow);
    }

    public async Task<LancamentoFinanceiroResponse?> LiquidarLancamentoAsync(
        Guid id, DateTimeOffset liquidadoEm, CancellationToken ct)
    {
        var lancamento = await repository.ObterLancamentoTrackedAsync(id, ct);
        if (lancamento is null) return null;
        if (lancamento.Status != StatusLancamentoFinanceiro.Aberto &&
            lancamento.Status != StatusLancamentoFinanceiro.Atrasado)
            throw new InvalidOperationException("Somente lançamentos em aberto ou atrasados podem ser liquidados.");
        if (liquidadoEm == default) throw new ArgumentException("A data de liquidação é obrigatória.");
        lancamento.LiquidadoEm = liquidadoEm;
        lancamento.Status = lancamento.Tipo == TipoLancamentoFinanceiro.ContaPagar
            ? StatusLancamentoFinanceiro.Pago
            : StatusLancamentoFinanceiro.Recebido;
        await unitOfWork.CommitAsync(ct);
        return Map(lancamento, DateTimeOffset.UtcNow);
    }

    public async Task<IReadOnlyList<ApontamentoObraResponse>> ListarApontamentosAsync(Guid obraId, CancellationToken ct)
    {
        if (!await repository.ObraExisteAsync(obraId, ct)) throw new KeyNotFoundException("Obra não encontrada.");
        return (await repository.ListarApontamentosAsync(obraId, ct)).Select(Map).ToArray();
    }

    public async Task<ApontamentoObraResponse> CriarApontamentoAsync(
        CriarApontamentoObraRequest request, CancellationToken ct)
    {
        if (!await repository.ObraExisteAsync(request.ObraId, ct)) throw new KeyNotFoundException("Obra não encontrada.");
        if (request.Data == default) throw new ArgumentException("A data do apontamento é obrigatória.");
        if (await repository.ApontamentoDataExisteAsync(request.ObraId, request.Data, ct))
            throw new InvalidOperationException("Já existe um apontamento para esta obra e data.");
        var apontamento = new ApontamentoObra
        {
            ObraId = request.ObraId,
            Data = request.Data,
            Clima = request.Clima.Trim(),
            Atividades = request.Atividades.Trim(),
            TrabalhadoresPresentes = request.TrabalhadoresPresentes,
            HorasTrabalhadas = request.HorasTrabalhadas,
            Observacoes = request.Observacoes.Trim()
        };
        await repository.AdicionarApontamentoAsync(apontamento, ct);
        await unitOfWork.CommitAsync(ct);
        return Map(apontamento);
    }

    public async Task<IReadOnlyList<AlocacaoFuncionarioResponse>> ListarAlocacoesAsync(Guid obraId, CancellationToken ct)
    {
        if (!await repository.ObraExisteAsync(obraId, ct)) throw new KeyNotFoundException("Obra não encontrada.");
        return (await repository.ListarAlocacoesAsync(obraId, ct)).Select(Map).ToArray();
    }

    public async Task<AlocacaoFuncionarioResponse> CriarAlocacaoAsync(
        CriarAlocacaoFuncionarioRequest request, CancellationToken ct)
    {
        if (!await repository.ObraExisteAsync(request.ObraId, ct)) throw new KeyNotFoundException("Obra não encontrada.");
        if (!await repository.FuncionarioAtivoExisteAsync(request.FuncionarioId, ct))
            throw new KeyNotFoundException("Funcionário ativo não encontrado.");
        if (request.Inicio == default) throw new ArgumentException("A data de início é obrigatória.");
        if (await repository.AlocacaoAtivaExisteAsync(request.FuncionarioId, ct))
            throw new InvalidOperationException("O funcionário já possui uma alocação ativa.");
        var alocacao = new AlocacaoFuncionarioObra
        {
            ObraId = request.ObraId,
            FuncionarioId = request.FuncionarioId,
            Inicio = request.Inicio,
            Funcao = request.Funcao.Trim()
        };
        await repository.AdicionarAlocacaoAsync(alocacao, ct);
        await unitOfWork.CommitAsync(ct);
        return Map(alocacao);
    }

    public async Task<AlocacaoFuncionarioResponse?> EncerrarAlocacaoAsync(Guid id, DateTimeOffset fim, CancellationToken ct)
    {
        var alocacao = await repository.ObterAlocacaoTrackedAsync(id, ct);
        if (alocacao is null) return null;
        if (alocacao.Fim is not null) throw new InvalidOperationException("A alocação já foi encerrada.");
        if (fim < alocacao.Inicio) throw new ArgumentException("A data final não pode anteceder o início.");
        alocacao.Fim = fim;
        await unitOfWork.CommitAsync(ct);
        return Map(alocacao);
    }

    public async Task<IReadOnlyList<IncidenteResponse>> ListarIncidentesAsync(Guid obraId, CancellationToken ct)
    {
        if (!await repository.ObraExisteAsync(obraId, ct)) throw new KeyNotFoundException("Obra não encontrada.");
        return (await repository.ListarIncidentesAsync(obraId, ct)).Select(Map).ToArray();
    }

    public async Task<IncidenteResponse> CriarIncidenteAsync(CriarIncidenteRequest request, CancellationToken ct)
    {
        if (!await repository.ObraExisteAsync(request.ObraId, ct)) throw new KeyNotFoundException("Obra não encontrada.");
        if (request.FuncionarioId is { } employeeId &&
            !await repository.FuncionarioAtivoExisteAsync(employeeId, ct))
            throw new KeyNotFoundException("Funcionário ativo não encontrado.");
        if (request.OcorreuEm == default) throw new ArgumentException("A data do incidente é obrigatória.");
        if (!Enum.IsDefined(request.Gravidade)) throw new ArgumentException("Gravidade inválida.");
        var incidente = new IncidenteSeguranca
        {
            ObraId = request.ObraId,
            FuncionarioId = request.FuncionarioId,
            OcorreuEm = request.OcorreuEm,
            Gravidade = request.Gravidade,
            Descricao = request.Descricao.Trim()
        };
        await repository.AdicionarIncidenteAsync(incidente, ct);
        await unitOfWork.CommitAsync(ct);
        return Map(incidente);
    }

    public async Task<IncidenteResponse?> AtualizarIncidenteAsync(
        Guid id, AtualizarIncidenteRequest request, CancellationToken ct)
    {
        var incidente = await repository.ObterIncidenteTrackedAsync(id, ct);
        if (incidente is null) return null;
        incidente.AcoesCorretivas = request.AcoesCorretivas.Trim();
        incidente.Encerrado = request.Encerrado;
        await unitOfWork.CommitAsync(ct);
        return Map(incidente);
    }

    public async Task<IReadOnlyList<InspecaoQualidadeResponse>> ListarInspecoesAsync(
        Guid obraId, CancellationToken ct)
    {
        if (!await repository.ObraExisteAsync(obraId, ct)) throw new KeyNotFoundException("Obra não encontrada.");
        return (await repository.ListarInspecoesAsync(obraId, ct)).Select(Map).ToArray();
    }

    public async Task<InspecaoQualidadeResponse> CriarInspecaoAsync(
        CriarInspecaoQualidadeRequest request, CancellationToken ct)
    {
        if (!await repository.ObraExisteAsync(request.ObraId, ct)) throw new KeyNotFoundException("Obra não encontrada.");
        if (request.InspecionadaEm == default) throw new ArgumentException("A data da inspeção é obrigatória.");
        if (!Enum.IsDefined(request.Resultado)) throw new ArgumentException("Resultado de inspeção inválido.");
        var referencia = request.Referencia.Trim();
        if (await repository.ReferenciaInspecaoExisteAsync(request.ObraId, referencia, ct))
            throw new InvalidOperationException("A referência da inspeção já existe nesta obra.");
        var inspecao = new InspecaoQualidade
        {
            ObraId = request.ObraId,
            Referencia = referencia,
            Categoria = request.Categoria.Trim(),
            InspecionadaEm = request.InspecionadaEm,
            Resultado = request.Resultado,
            Observacoes = request.Observacoes.Trim(),
            Encerrada = request.Resultado == ResultadoInspecao.Aprovada
        };
        await repository.AdicionarInspecaoAsync(inspecao, ct);
        await unitOfWork.CommitAsync(ct);
        return Map(inspecao);
    }

    public async Task<InspecaoQualidadeResponse?> AtualizarAcaoCorretivaAsync(
        Guid id, AtualizarAcaoCorretivaRequest request, CancellationToken ct)
    {
        var inspecao = await repository.ObterInspecaoTrackedAsync(id, ct);
        if (inspecao is null) return null;
        if (inspecao.Resultado == ResultadoInspecao.Aprovada)
            throw new InvalidOperationException("Inspeção aprovada não precisa de ação corretiva.");
        if (request.Encerrada && string.IsNullOrWhiteSpace(request.AcaoCorretiva))
            throw new ArgumentException("Informe a ação corretiva antes de encerrar a inspeção.");
        inspecao.AcaoCorretiva = request.AcaoCorretiva.Trim();
        inspecao.PrazoCorrecao = request.PrazoCorrecao;
        inspecao.Encerrada = request.Encerrada;
        await unitOfWork.CommitAsync(ct);
        return Map(inspecao);
    }

    private async Task<ContratoResponse> MapContratoAsync(Contrato c, CancellationToken ct) =>
        new(c.Id, c.ClienteId, c.ObraId, c.Numero, c.DataAssinatura, c.DataInicio,
            c.DataFimPrevista, c.ValorContratado, await repository.ObterTotalMedidoAsync(c.Id, ct),
            c.RetencaoPercentual, c.Status);
    private static string NormalizarDocumento(string documento) =>
        new(documento.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
    private static ClienteResponse Map(Cliente c) => new(c.Id, c.Nome, c.Documento, c.Email, c.Telefone);
    private static MedicaoResponse Map(MedicaoContrato m) =>
        new(m.Id, m.ContratoId, m.Referencia, m.DataMedicao, m.PercentualExecutado, m.ValorMedido, m.Status);
    private static LancamentoFinanceiroResponse Map(LancamentoFinanceiro l, DateTimeOffset now) =>
        new(l.Id, l.ObraId, l.FornecedorId, l.ClienteId, l.MedicaoContratoId, l.Descricao,
            l.Tipo, l.Status == StatusLancamentoFinanceiro.Aberto && l.Vencimento < now
                ? StatusLancamentoFinanceiro.Atrasado : l.Status,
            l.Valor, l.Vencimento, l.LiquidadoEm, l.ReferenciaExterna);
    private static ApontamentoObraResponse Map(ApontamentoObra a) =>
        new(a.Id, a.ObraId, a.Data, a.Clima, a.Atividades, a.TrabalhadoresPresentes, a.HorasTrabalhadas, a.Observacoes);
    private static AlocacaoFuncionarioResponse Map(AlocacaoFuncionarioObra a) =>
        new(a.Id, a.ObraId, a.FuncionarioId, a.Inicio, a.Fim, a.Funcao);
    private static IncidenteResponse Map(IncidenteSeguranca i) =>
        new(i.Id, i.ObraId, i.FuncionarioId, i.OcorreuEm, i.Gravidade,
            i.Descricao, i.AcoesCorretivas, i.Encerrado);
    private static InspecaoQualidadeResponse Map(InspecaoQualidade i) =>
        new(i.Id, i.ObraId, i.Referencia, i.Categoria, i.InspecionadaEm, i.Resultado,
            i.Observacoes, i.AcaoCorretiva, i.PrazoCorrecao, i.Encerrada);
}
