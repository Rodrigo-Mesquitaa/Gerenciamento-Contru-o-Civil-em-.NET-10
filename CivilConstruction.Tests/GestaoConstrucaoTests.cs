using CivilConstruction.Application.DTOs;
using CivilConstruction.Application.Interfaces;
using CivilConstruction.Application.Services;
using CivilConstruction.Domain.Entities;
using CivilConstruction.Domain.Events;
using CivilConstruction.Infrastructure.Context;
using CivilConstruction.Infrastructure.Messaging;
using CivilConstruction.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CivilConstruction.Tests;

public sealed class GestaoConstrucaoTests
{
    [Fact]
    public async Task ReceberOrdemAprovada_AtualizaEstoqueUmaUnicaVez()
    {
        await using var db = CriarDbContext();
        var service = CriarService(db);
        var obra = new Obra
        {
            Nome = "Residencial",
            Endereco = "Rua Central",
            Cliente = "Cliente",
            Orcamento = 100000m,
            DataInicioPrevista = DateTimeOffset.UtcNow
        };
        var fornecedor = new Fornecedor { Nome = "Fornecedor", Documento = "123" };
        db.AddRange(obra, fornecedor);
        await db.SaveChangesAsync();

        var ordem = await service.CriarOrdemCompraAsync(new CriarOrdemCompraRequest
        {
            ObraId = obra.Id,
            FornecedorId = fornecedor.Id,
            Itens =
            [
                new ItemOrdemCompraRequest
                {
                    Codigo = "CIMENTO-50",
                    Descricao = "Cimento 50kg",
                    Quantidade = 20,
                    Unidade = "saco",
                    ValorUnitario = 34.50m
                }
            ]
        }, CancellationToken.None);

        await service.AlterarStatusOrdemCompraAsync(ordem.Id, StatusOrdemCompra.Aprovada, CancellationToken.None);
        await service.AlterarStatusOrdemCompraAsync(ordem.Id, StatusOrdemCompra.Recebida, CancellationToken.None);

        var estoque = await service.ListarEstoqueAsync(CancellationToken.None);
        Assert.Single(estoque);
        Assert.Equal(20m, estoque[0].Quantidade);
        Assert.Equal(690m, ordem.ValorTotal);
    }

    [Fact]
    public async Task ReceberOrdemComCodigoRepetido_AgrupaNoMesmoItemDeEstoque()
    {
        await using var db = CriarDbContext();
        var service = CriarService(db);
        var obra = new Obra
        {
            Nome = "Obra",
            Endereco = "Rua Central",
            Cliente = "Cliente",
            Orcamento = 100000m,
            DataInicioPrevista = DateTimeOffset.UtcNow
        };
        var fornecedor = new Fornecedor { Nome = "Fornecedor", Documento = "789" };
        db.AddRange(obra, fornecedor);
        await db.SaveChangesAsync();

        var ordem = await service.CriarOrdemCompraAsync(new CriarOrdemCompraRequest
        {
            ObraId = obra.Id,
            FornecedorId = fornecedor.Id,
            Itens =
            [
                new ItemOrdemCompraRequest
                {
                    Codigo = "CIMENTO-50",
                    Descricao = "Cimento 50kg",
                    Quantidade = 20,
                    Unidade = "saco",
                    ValorUnitario = 34.50m
                },
                new ItemOrdemCompraRequest
                {
                    Codigo = "cimento-50",
                    Descricao = "Cimento 50kg",
                    Quantidade = 10,
                    Unidade = "saco",
                    ValorUnitario = 35m
                }
            ]
        }, CancellationToken.None);

        await service.AlterarStatusOrdemCompraAsync(ordem.Id, StatusOrdemCompra.Aprovada, CancellationToken.None);
        await service.AlterarStatusOrdemCompraAsync(ordem.Id, StatusOrdemCompra.Recebida, CancellationToken.None);

        var estoque = await service.ListarEstoqueAsync(CancellationToken.None);
        Assert.Single(estoque);
        Assert.Equal(30m, estoque[0].Quantidade);
        Assert.Equal(1040m, ordem.ValorTotal);
    }

    [Fact]
    public async Task GerarPagamentosVencidos_CriaCompetenciasEAproximaProgramacao()
    {
        await using var db = CriarDbContext();
        var service = CriarService(db);
        var funcionario = new Funcionario { Nome = "Ana", ValorDiaria = 250m };
        db.Funcionarios.Add(funcionario);
        await db.SaveChangesAsync();

        var inicio = new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);
        var programacao = await service.CriarProgramacaoPagamentoAsync(
            new CriarProgramacaoPagamentoRequest
            {
                FuncionarioId = funcionario.Id,
                ValorBruto = 3750m,
                Periodicidade = PeriodicidadePagamento.Quinzenal,
                ProximoPagamentoEm = inicio
            }, CancellationToken.None);

        var pagamentos = await service.GerarPagamentosVencidosAsync(
            inicio.AddDays(31), CancellationToken.None);
        var programacaoAtualizada = await service.ObterProgramacaoPagamentoAsync(
            programacao.Id, CancellationToken.None);

        Assert.Equal(3, pagamentos.Count);
        Assert.All(pagamentos, pagamento => Assert.Equal(3750m, pagamento.ValorBruto));
        Assert.Equal(inicio.AddDays(45), programacaoAtualizada!.ProximoPagamentoEm);
        Assert.Empty(await service.GerarPagamentosVencidosAsync(inicio.AddDays(31), CancellationToken.None));
    }

    [Fact]
    public async Task ReceberOrdemEmRascunho_RejeitaTransicaoSemAlterarEstoque()
    {
        await using var db = CriarDbContext();
        var service = CriarService(db);
        var obra = new Obra
        {
            Nome = "Obra",
            Endereco = "Endereço",
            Cliente = "Cliente",
            Orcamento = 10000m,
            DataInicioPrevista = DateTimeOffset.UtcNow
        };
        var fornecedor = new Fornecedor { Nome = "Fornecedor", Documento = "456" };
        db.AddRange(obra, fornecedor);
        await db.SaveChangesAsync();
        var ordem = await service.CriarOrdemCompraAsync(new CriarOrdemCompraRequest
        {
            ObraId = obra.Id,
            FornecedorId = fornecedor.Id,
            Itens =
            [
                new ItemOrdemCompraRequest
                {
                    Codigo = "ACO",
                    Descricao = "Aço",
                    Quantidade = 10,
                    Unidade = "kg",
                    ValorUnitario = 8m
                }
            ]
        }, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AlterarStatusOrdemCompraAsync(ordem.Id, StatusOrdemCompra.Recebida, CancellationToken.None));

        Assert.Empty(await service.ListarEstoqueAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ObraConcluida_NaoPodeSerReaberta()
    {
        await using var db = CriarDbContext();
        var service = CriarService(db);
        var obra = new Obra
        {
            Nome = "Obra",
            Endereco = "Endereço",
            Cliente = "Cliente",
            Orcamento = 10000m,
            DataInicioPrevista = DateTimeOffset.UtcNow
        };
        db.Obras.Add(obra);
        await db.SaveChangesAsync();

        await service.AlterarStatusObraAsync(obra.Id, StatusObra.EmExecucao, CancellationToken.None);
        await service.AlterarStatusObraAsync(obra.Id, StatusObra.Concluida, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AlterarStatusObraAsync(obra.Id, StatusObra.EmExecucao, CancellationToken.None));
    }

    [Fact]
    public async Task FaturarMedicaoAprovada_GeraContaReceberComRetencao()
    {
        await using var db = CriarDbContext();
        var service = CriarEmpresaService(db);
        var obra = new Obra
        {
            Nome = "Obra",
            Endereco = "Endereço",
            Cliente = "Cliente",
            Orcamento = 100000m,
            DataInicioPrevista = DateTimeOffset.UtcNow
        };
        var cliente = new Cliente { Nome = "Cliente", Documento = "123" };
        db.AddRange(obra, cliente);
        await db.SaveChangesAsync();
        var contrato = await service.CriarContratoAsync(new CriarContratoRequest
        {
            ClienteId = cliente.Id,
            ObraId = obra.Id,
            Numero = "CT-001",
            DataAssinatura = DateTimeOffset.UtcNow,
            DataInicio = DateTimeOffset.UtcNow,
            DataFimPrevista = DateTimeOffset.UtcNow.AddMonths(12),
            ValorContratado = 100000m,
            RetencaoPercentual = 5m
        }, CancellationToken.None);
        await service.AlterarStatusContratoAsync(contrato.Id, StatusContrato.Ativo, CancellationToken.None);
        var medicao = await service.CriarMedicaoAsync(contrato.Id, new CriarMedicaoRequest
        {
            Referencia = "MED-01",
            DataMedicao = DateTimeOffset.UtcNow,
            PercentualExecutado = 20m
        }, CancellationToken.None);

        await service.AlterarStatusMedicaoAsync(medicao.Id, StatusMedicao.Enviada, null, CancellationToken.None);
        await service.AlterarStatusMedicaoAsync(medicao.Id, StatusMedicao.Aprovada, null, CancellationToken.None);
        var vencimento = DateTimeOffset.UtcNow.AddDays(45);
        await service.AlterarStatusMedicaoAsync(
            medicao.Id, StatusMedicao.Faturada, vencimento, CancellationToken.None);

        var contas = await service.ListarLancamentosAsync(
            TipoLancamentoFinanceiro.ContaReceber, CancellationToken.None);
        Assert.Single(contas);
        Assert.Equal(19000m, contas[0].Valor);
        Assert.Equal(vencimento, contas[0].Vencimento);
        Assert.Equal(StatusLancamentoFinanceiro.Aberto, contas[0].Status);
    }

    [Fact]
    public async Task LiquidarContaPagar_AtualizaEstadoEImpedeLiquidacaoDuplicada()
    {
        await using var db = CriarDbContext();
        var service = CriarEmpresaService(db);
        var fornecedor = new Fornecedor { Nome = "Fornecedor", Documento = "456" };
        db.Fornecedores.Add(fornecedor);
        await db.SaveChangesAsync();
        var conta = await service.CriarLancamentoAsync(new CriarLancamentoFinanceiroRequest
        {
            Descricao = "Aluguel de equipamento",
            Tipo = TipoLancamentoFinanceiro.ContaPagar,
            FornecedorId = fornecedor.Id,
            Valor = 2500m,
            Vencimento = DateTimeOffset.UtcNow.AddDays(10)
        }, CancellationToken.None);

        var liquidadoEm = DateTimeOffset.UtcNow;
        var resultado = await service.LiquidarLancamentoAsync(conta.Id, liquidadoEm, CancellationToken.None);
        Assert.Equal(StatusLancamentoFinanceiro.Pago, resultado!.Status);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.LiquidarLancamentoAsync(conta.Id, liquidadoEm, CancellationToken.None));
    }

    [Fact]
    public async Task ApontamentoDuplicadoParaMesmoDiaEhRejeitado()
    {
        await using var db = CriarDbContext();
        var service = CriarEmpresaService(db);
        var obra = new Obra
        {
            Nome = "Obra",
            Endereco = "Endereço",
            Cliente = "Cliente",
            Orcamento = 10000m,
            DataInicioPrevista = DateTimeOffset.UtcNow
        };
        db.Obras.Add(obra);
        await db.SaveChangesAsync();
        var data = new DateTimeOffset(2026, 10, 8, 8, 0, 0, TimeSpan.Zero);
        var request = new CriarApontamentoObraRequest
        {
            ObraId = obra.Id,
            Data = data,
            Atividades = "Concretagem da laje",
            TrabalhadoresPresentes = 8,
            HorasTrabalhadas = 8m
        };
        await service.CriarApontamentoAsync(request, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CriarApontamentoAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task FuncionarioNaoPodeTerDuasAlocacoesAtivas()
    {
        await using var db = CriarDbContext();
        var service = CriarEmpresaService(db);
        var obra = new Obra
        {
            Nome = "Obra",
            Endereco = "Endereço",
            Cliente = "Cliente",
            Orcamento = 10000m,
            DataInicioPrevista = DateTimeOffset.UtcNow
        };
        var funcionario = new Funcionario { Nome = "João", ValorDiaria = 200m };
        db.AddRange(obra, funcionario);
        await db.SaveChangesAsync();
        var request = new CriarAlocacaoFuncionarioRequest
        {
            ObraId = obra.Id,
            FuncionarioId = funcionario.Id,
            Inicio = DateTimeOffset.UtcNow,
            Funcao = "Pedreiro"
        };
        await service.CriarAlocacaoAsync(request, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CriarAlocacaoAsync(request, CancellationToken.None));
    }

    [Fact]
    public void AlocacaoFuncionario_ModeloImpedeMaisDeUmaAlocacaoAtivaPorFuncionario()
    {
        using var db = CriarDbContext();

        var index = db.Model.FindEntityType(typeof(AlocacaoFuncionarioObra))!
            .GetIndexes()
            .Single(index => index.IsUnique && index.GetFilter() == "[Fim] IS NULL");

        Assert.Equal(nameof(AlocacaoFuncionarioObra.FuncionarioId), Assert.Single(index.Properties).Name);
    }

    [Fact]
    public async Task EnfileirarPagamento_GravaOutboxIdempotenteSemMarcarComoPublicado()
    {
        await using var db = CriarDbContext();
        var service = CriarService(db);
        var pagamento = new PagamentoFuncionario
        {
            ProgramacaoPagamentoId = Guid.NewGuid(),
            FuncionarioId = Guid.NewGuid(),
            ValorBruto = 1250m,
            Competencia = DateTimeOffset.UtcNow
        };
        db.PagamentosFuncionarios.Add(pagamento);
        await db.SaveChangesAsync();

        await service.EnfileirarPagamentoAsync(pagamento.Id, CancellationToken.None);
        await service.EnfileirarPagamentoAsync(pagamento.Id, CancellationToken.None);

        var outboxMessage = await db.OutboxMessages.SingleAsync();
        Assert.Equal(pagamento.Id, outboxMessage.Id);
        Assert.Equal("pagamento.realizado", outboxMessage.EventType);
        Assert.Equal(1, outboxMessage.SchemaVersion);
        Assert.Equal(OutboxMessageStatus.Pending, outboxMessage.Status);
        Assert.False(pagamento.EventoPublicado);
    }

    [Fact]
    public async Task CadastrarFuncionario_GravaFuncionarioEEventoOutboxNaMesmaConfirmacao()
    {
        await using var db = CriarDbContext();
        var service = new ServicoFuncionario(
            new FuncionarioRepository(db),
            new UnitOfWorkFake(db),
            new IntegrationEventOutbox(db));

        var id = Guid.NewGuid();
        await service.CriarAsync(
            new FuncionarioDto { Id = id, Nome = "Carpinteiro", ValorDiaria = 280m },
            CancellationToken.None);

        Assert.NotNull(await db.Funcionarios.SingleOrDefaultAsync(item => item.Id == id));
        var outboxMessage = await db.OutboxMessages.SingleAsync();
        Assert.Equal(id, outboxMessage.Id);
        Assert.Equal(IntegrationEventTypes.EmployeeRegistered, outboxMessage.EventType);
        Assert.Equal(OutboxMessageStatus.Pending, outboxMessage.Status);
    }

    [Fact]
    public void ModeloPersisteRestricoesDeIntegridadeParaDadosDeNegocio()
    {
        using var db = CriarDbContext();
        var checkConstraints = db.GetService<IDesignTimeModel>().Model.GetEntityTypes()
            .SelectMany(entity => entity.GetCheckConstraints())
            .Select(constraint => constraint.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("CK_Contratos_RetencaoPercentual_Range", checkConstraints);
        Assert.Contains("CK_ItensEstoque_Quantidades_NonNegative", checkConstraints);
        Assert.Contains("CK_LancamentosFinanceiros_Valor_Positive", checkConstraints);
        Assert.Contains("CK_ApontamentosObra_Indicadores_NonNegative", checkConstraints);
        Assert.Contains("CK_AlocacoesFuncionarios_Periodo_Valid", checkConstraints);
    }

    [Fact]
    public async Task InspecaoReprovada_ExigeAcaoCorretivaParaEncerrar()
    {
        await using var db = CriarDbContext();
        var service = CriarEmpresaService(db);
        var obra = new Obra
        {
            Nome = "Obra",
            Endereco = "Endereço",
            Cliente = "Cliente",
            Orcamento = 10000m,
            DataInicioPrevista = DateTimeOffset.UtcNow
        };
        db.Obras.Add(obra);
        await db.SaveChangesAsync();
        var inspecao = await service.CriarInspecaoAsync(new CriarInspecaoQualidadeRequest
        {
            ObraId = obra.Id,
            Referencia = "Q-001",
            Categoria = "Armadura",
            InspecionadaEm = DateTimeOffset.UtcNow,
            Resultado = ResultadoInspecao.Reprovada,
            Observacoes = "Espaçamento fora da especificação"
        }, CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentException>(() => service.AtualizarAcaoCorretivaAsync(
            inspecao.Id,
            new AtualizarAcaoCorretivaRequest { AcaoCorretiva = "", Encerrada = true },
            CancellationToken.None));

        var encerrada = await service.AtualizarAcaoCorretivaAsync(
            inspecao.Id,
            new AtualizarAcaoCorretivaRequest
            {
                AcaoCorretiva = "Reposicionar armadura conforme projeto",
                PrazoCorrecao = DateTimeOffset.UtcNow.AddDays(1),
                Encerrada = true
            },
            CancellationToken.None);
        Assert.True(encerrada!.Encerrada);
    }

    private static AppDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static GestaoConstrucaoService CriarService(AppDbContext db) =>
        new(new GestaoConstrucaoRepository(db), new UnitOfWorkFake(db), new IntegrationEventOutbox(db));

    private static GestaoEmpresaService CriarEmpresaService(AppDbContext db) =>
        new(new GestaoEmpresaRepository(db), new UnitOfWorkFake(db));

    private sealed class UnitOfWorkFake(AppDbContext db) : IUnitOfWork
    {
        public Task CommitAsync(CancellationToken cancellationToken = default) =>
            db.SaveChangesAsync(cancellationToken);
    }
}
