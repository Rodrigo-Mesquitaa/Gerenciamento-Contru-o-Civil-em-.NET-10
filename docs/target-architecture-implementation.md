# Arquitetura-alvo e plano obrigatório de implementação

Este documento complementa a [avaliação atual e limitações](./architecture-validation-roadmap.md). Ele converte cada lacuna identificada em uma tarefa com solução, exemplo técnico, dependências e critério de aceite. **Os exemplos são padrões de implementação propostos, não código já integrado nem prova de operação.** Somente itens explicitamente marcados como implementados foram alterados/verificados.

## 1. Estado de execução deste plano

| ID | Lacuna | Situação nesta alteração |
|---|---|---|
| A-01 | CI reprodutível | **Configurado e validado localmente:** workflow executa restore, build Release, testes e publica artefatos API/Worker; execução remota no GitHub Actions ainda não verificada |
| A-02 | Deployment, promoção e rollback | **Desenhado; implantação pendente** da decisão de plataforma/ambientes |
| A-03 | Limites de microserviços e ownership de dados | **Desenhado; migração pendente**, permanecendo monólito modular |
| A-04 | CQRS formal e segregação de leitura/escrita | **Desenhado; não implementado** |
| A-05 | Event Sourcing e projeções | **Desenhado como piloto delimitado; não implementado** |
| A-06 | Outbox/inbox e consumidor confiável | **Outbox parcial implementada** para cadastro de funcionário e pagamento programado, com migration, relay, lease, retries e estado dead-letter; inbox/consumer e teste SQL/Rabbit ainda pendentes |
| A-07 | gRPC síncrono e contratos | **Desenhado; não implementado** |
| A-08 | Observabilidade distribuída | **Desenhado; não implementado** |
| A-09 | Segurança e isolamento organizacional | **Parcial:** credenciais padrão removidas do Compose e fallback `guest` removido do produtor; policies por recurso/tenant, auditoria e gestão de segredos de produção ainda não implementadas |
| A-10 | Persistência, migrations e testes de integração | **Desenhado; SQL Server real não validado nesta tarefa** |
| A-11 | Cobertura macro do ERP | **Roadmap funcional detalhado; não implica funcionalidades implementadas** |

Ordem sugerida: A-09 e A-10 como gates operacionais; A-06 antes de ampliar integrações assíncronas; A-04 em fatias por caso de uso; A-05 somente após seleção e validação do agregado piloto; A-03/A-07 quando ownership e contratos estiverem estáveis. Não se deve iniciar todos os serviços em paralelo sem equipe/operabilidade correspondentes.

## 2. A-03 — Evolução segura para microserviços autônomos

### Diagnóstico

API e Worker compartilham `AppDbContext`, repositórios e `ConnectionStrings:DefaultConnection`. Logo, processo separado não significa serviço autônomo. Não renomear projetos nem adicionar um endpoint gRPC muda esse fato.

### Topologia-alvo

Começar com **monólito modular com limites verificáveis**, depois separar contextos que tenham ownership, escalabilidade, cadência de mudança ou isolamento de falha próprios:

```text
API Gateway / Identity Provider externo
       | REST (clientes/web/mobile)     | gRPC (chamadas internas síncronas)
       +--------------------+------------+
                            |
        +-------------------+-------------------+
        |                   |                   |
  Construction          Procurement          Finance
  service               service              service
  own SQL DB             own SQL DB            own SQL DB
        |                   |                   |
        +------ RabbitMQ: versioned integration events ------+

Observability backend (OTel Collector -> traces/metrics/logs)
```

Boundaries candidatas, sujeitas a validação por domínio:

- **ObrasService:** identidade/ciclo de vida da obra, diário, avanço físico e medições por quantidade/composição, com ownership do seu banco.
- **PlanejamentoService:** EAP, cronograma, baseline, alocação de recursos e projeções. Recebe fatos de obra/recursos por contratos explícitos; não compartilha repositórios com ObrasService.
- **SuprimentosService:** fornecedores usados em compras, requisições, cotações, pedidos, recebimento e ledger de estoque. Financeiro não altera saldo de estoque diretamente.
- **ContratosService:** contratos de clientes e empreiteiros, aditivos, regras de medição, retenções, garantias e aprovações. Contratos de fornecimento podem ser referenciados por Suprimentos por identificadores e eventos, não por join ao banco.
- **FinanceiroService:** AP/AR, caixa, aprovações e integrações bancárias/fiscais; consome fatos versionados de Contratos/Suprimentos e não consulta tabelas desses serviços.
- **AuthService:** provedor OIDC/OAuth2 central. Se a decisão exigir Duende IdentityServer, validar licença/suporte operacional da versão selecionada; alternativa é um IdP compatível OIDC gerenciado. Cada API valida tokens localmente por issuer/audience e aplica autorização por recurso/tenant; AuthService não deve ser o banco de identidade consultado síncronamente em cada request.
- **Gateway/API:** gateway com YARP para roteamento e .NET rate limiting, autenticação de borda, correlation e política de timeout; agregação OpenAPI explícita e versionada. Gateway não contém lógica de domínio nem substitui autorização nos serviços.
- **Pessoas/Folha:** inicialmente pode permanecer módulo/contexto próprio; decidir se cadastro/alocação e cálculo/obrigações de folha são bounded contexts separados após descoberta de regras legais.

Estrutura-alvo de **cada** serviço:

```text
<Service>.Domain/          entidades, aggregates, value objects, domain events
<Service>.Application/     commands/queries, handlers, policies e ports
<Service>.Infrastructure/  DbContext próprio, migrations, Rabbit/outbox, adapters
<Service>.Api/             REST/OpenAPI e authz; endpoints/host Presentation
<Service>.Contracts/       protobuf e eventos públicos versionados (sem entidades EF)
<Service>.Tests/           unit, application, contract e integration tests
```

Dependências apontam para Domain; Infrastructure implementa ports de Application; API compõe Application + Infrastructure no composition root. Um serviço não referencia assembly de infraestrutura de outro. Cross-cutting deve ficar restrito a contratos técnicos sem esconder regras de domínio em uma biblioteca “shared”.

Cada serviço possui seu banco/schema sob ownership exclusivo, migrações próprias, API/contrato versionado, deployment, escala, health, SLO e runbook próprios. Não se compartilha tabela nem `DbContext`; consultas cross-context usam API, eventos e projeções locais com consistência eventual. IDs de referência externos são valores/IDs, não foreign keys cross-database. Saga/process manager coordena processos longos; não há transação distribuída SQL + RabbitMQ.

### Gateway ilustrativo com YARP

O exemplo configura rotas de borda; nomes de clusters, DNS, TLS, autenticação e health precisam ser fornecidos pelo ambiente de deployment. YARP não consolida sozinho documentos OpenAPI: publicar um índice de documentos de contrato por serviço ou gerar um documento agregado no pipeline, com detecção de colisão de paths/schemas e versionamento.

```json
{
  "ReverseProxy": {
    "Routes": {
      "obras": { "ClusterId": "obras", "Match": { "Path": "/api/obras/{**catch-all}" } },
      "suprimentos": { "ClusterId": "suprimentos", "Match": { "Path": "/api/suprimentos/{**catch-all}" } },
      "financeiro": { "ClusterId": "financeiro", "Match": { "Path": "/api/financeiro/{**catch-all}" } }
    },
    "Clusters": {
      "obras": { "Destinations": { "primary": { "Address": "https://obras.internal/" } } },
      "suprimentos": { "Destinations": { "primary": { "Address": "https://suprimentos.internal/" } } },
      "financeiro": { "Destinations": { "primary": { "Address": "https://financeiro.internal/" } } }
    }
  }
}
```

No gateway, configurar `AddRateLimiter` por cliente/tenant autenticado com partição bounded, limites diferenciados e resposta `429`/`Retry-After`; validar também limites no serviço para chamadas internas e proteger contra bypass do gateway. Aplicar limite de corpo, timeout e quotas por rota. Não usar IP como única identidade para rate limiting de clientes autenticados.

### Migração incremental

1. Mapear bounded contexts, invariantes, writes e consultas; declarar dono de cada tabela.
2. Extrair módulos para assemblies e APIs internas sem alterar dados compartilhados.
3. Introduzir contratos de integração versionados e outbox no monólito; nenhum consumidor consulta tabelas do produtor.
4. Criar serviço e banco próprios; publicar eventos a partir do banco dono; backfill idempotente e reconciliação de totais.
5. Redirecionar um fluxo/capacidade por vez usando strangler routing; manter compatibilidade e telemetria.
6. Comparar resultados, cortar leitura/escrita antigas, reter plano de rollback e só então remover o código legado.

### Aceite obrigatório

- Teste automatizado demonstra que serviço B não abre conexão nem referencia repositórios/tabelas do serviço A.
- Falha/indisponibilidade de um serviço tem comportamento e timeout/circuit breaker definidos, sem derrubar os demais.
- Deploy, migration, métricas, alertas, credenciais e rollback independentes foram ensaiados.
- Consistência eventual e reconciliação estão documentadas para cada fluxo.

## 3. A-04 — CQRS formal por caso de uso

CQRS significa modelos/caminhos separados de comando e consulta, não obrigatoriamente bancos diferentes nem Event Sourcing. Aplicar primeiro a uma fatia vertical, sem reescrever todos os serviços de uma vez. Commands validam autorização/invariantes e retornam resultado; queries não alteram estado e podem usar projeção otimizada.

### Contratos ilustrativos (Application, MediatR)

```csharp
using MediatR;

public sealed record CriarObraCommand(
    string Nome, string Endereco, string Cliente, decimal Orcamento,
    DateOnly InicioPrevisto, DateOnly FimPrevisto) : IRequest<Guid>;

public sealed record ObterObraQuery(Guid ObraId) : IRequest<ObraDetalheDto?>;
```

Handler de comando representa a transação da aplicação e invoca o agregado; o controller deve apenas traduzir HTTP e despachar o command/query. O contrato real precisa de Result/Problem Details padronizado para validação, conflito e not-found, sem usar exceções como fluxo normal.

```csharp
builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssembly(typeof(CriarObraHandler).Assembly));

public sealed class CriarObraHandler(
    IObraRepository obras,
    IUnitOfWork unitOfWork,
    IClock clock) : IRequestHandler<CriarObraCommand, Guid>
{
    public async Task<Guid> Handle(CriarObraCommand command, CancellationToken ct)
    {
        if (command.FimPrevisto < command.InicioPrevisto)
            throw new ValidationException("Término anterior ao início.");

        var obra = Obra.Criar(command.Nome, command.Endereco, command.Cliente,
            command.Orcamento, command.InicioPrevisto, command.FimPrevisto, clock.UtcNow);
        await obras.AddAsync(obra, ct);
        await unitOfWork.CommitAsync(ct);
        return obra.Id;
    }
}
```

Fixar MediatR numa versão compatível com .NET 10, verificar licença/termos vigentes e centralizar a dependência. Behaviors transversais podem compor validação, autorização, correlation, métricas e idempotency; uma query deve receber `AsNoTracking`, paginação obrigatória, projeção SQL específica e autorização tenant-aware. Não implementar um segundo dispatcher paralelo ao MediatR.

### Aceite

- Controllers não contêm regra de domínio nem chamam repositório diretamente.
- Cada operação mutável tem um command/handler testado; cada leitura uma query/handler.
- Teste prova que query não chama `SaveChanges` e que command persiste somente após regra aprovada.
- Concorrência otimista retorna conflito HTTP definido; paginação/ordenação e limites têm contrato explícito.

## 4. A-05 — Event Sourcing delimitado, não universal

Event Sourcing substitui o estado mutável do agregado por uma sequência append-only de eventos como fonte da verdade para aquele contexto. **Não** basta persistir logs ou mensagens RabbitMQ. Não migrar todo o ERP ao mesmo tempo.

### Piloto sugerido

Avaliar primeiro um **ledger financeiro imutável** ou o ciclo de aprovação de pagamentos, com validação de contabilidade e idempotência. Não escolher folha fiscal/transferência bancária até integrações e requisitos regulatórios serem definidos. Para o restante do ERP, CRUD relacional + audit trail/outbox pode ser mais simples e adequado.

Esquema conceitual:

```sql
CREATE TABLE dbo.DomainEvents (
    StreamId uniqueidentifier NOT NULL,
    Version bigint NOT NULL,
    EventId uniqueidentifier NOT NULL,
    EventType nvarchar(200) NOT NULL,
    SchemaVersion int NOT NULL,
    OccurredAt datetimeoffset NOT NULL,
    CorrelationId nvarchar(100) NULL,
    CausationId nvarchar(100) NULL,
    Payload nvarchar(max) NOT NULL,
    CONSTRAINT PK_DomainEvents PRIMARY KEY (StreamId, Version),
    CONSTRAINT UQ_DomainEvents_EventId UNIQUE (EventId)
);
```

Append usa optimistic concurrency: na mesma transação SQL verificar `expectedVersion`, inserir eventos sequenciais e atualizar projeção/outbox; conflito de versão deve gerar retry limitado somente quando command for reexecutável. Payloads exigem schema versioning, política de evolução/upcasting, retenção, backup, autorização de leitura e proteção de PII. Snapshot é cache reconstruível, nunca fonte da verdade.

```csharp
public interface IEventSourcedAggregate
{
    Guid Id { get; }
    long Version { get; }
    IReadOnlyCollection<IDomainEvent> PendingEvents { get; }
    void ClearPendingEvents();
}

public interface IEventStore
{
    Task AppendAsync(
        Guid streamId,
        long expectedVersion,
        IReadOnlyCollection<IDomainEvent> events,
        CancellationToken ct);
    Task<IReadOnlyList<StoredEvent>> LoadAsync(Guid streamId, CancellationToken ct);
}
```

O handler carrega stream, reconstitui o agregado aplicando eventos, executa command, e faz append com expected version. Projeções são consumidores idempotentes e reconstruíveis; armazenam posição/checkpoint e suportam replay. Eventos de domínio internos e eventos de integração públicos são contratos distintos: não publicar classes internas do domínio como wire contract.

### Aceite

- Teste reproduz estado do agregado somente pela sequência e testa replay determinístico.
- Testes de concorrência demonstram que duas escritas com a mesma versão não sobrescrevem uma à outra.
- Replay total e de projeção, snapshot, evolução de schema e reconciliação foram ensaiados.
- A equipe aceita custo de operação e retenção antes de habilitar o piloto em produção.

## 5. A-06 — Outbox/Inbox transacional e RabbitMQ

O fluxo anterior publicava diretamente e depois registrava estado no banco, permitindo falha e duplicação. Foi implementada uma fatia: cadastro de funcionário e enfileiramento de pagamento gravam a mensagem outbox com a mudança SQL usando o mesmo `AppDbContext`/`SaveChanges`; um relay no Worker reivindica mensagens por lease, publica persistent JSON com publisher confirms, inclui tipo/versão nos headers, aplica retry exponencial limitado e marca falhas esgotadas como `DeadLetter` no SQL. A migration `AddTransactionalOutbox` foi criada, mas **não aplicada**. Esse estado dead-letter é persistido na outbox; não há RabbitMQ DLX/queue nem operação administrativa de replay.

Publicação e registro de sucesso não formam uma transação distribuída: uma falha após o broker confirmar e antes do commit de `Published` pode causar reenvio. A entrega continua pelo menos uma vez; consumidor deve usar inbox/deduplicação transacional, ainda inexistente nesta solução.

### Entidades conceituais

```csharp
public sealed class OutboxMessage
{
    public Guid Id { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public string Type { get; init; } = "";
    public int SchemaVersion { get; init; }
    public string Payload { get; init; } = "";
    public string? CorrelationId { get; init; }
    public DateTimeOffset? PublishedAt { get; set; }
    public int Attempts { get; set; }
    public DateTimeOffset? NextAttemptAt { get; set; }
    public string? LastError { get; set; }
}
```

```csharp
// Dentro do mesmo DbContext e SaveChanges/SQL transaction da alteração de negócio:
db.OrdensCompra.Add(order);
db.OutboxMessages.Add(OutboxMessageFactory.From(
    new OrdemCompraCriadaV1(order.Id, order.ObraId, order.Total), correlationId));
await db.SaveChangesAsync(ct);
```

Dispatcher hosted service deve reivindicar lotes sem corrida entre réplicas (SQL Server: transação curta com `UPDLOCK`, `READPAST`, `ROWLOCK`, índices por `PublishedAt/NextAttemptAt`), confirmar claim/lease, publicar persistent JSON com `MessageId=Outbox.Id`, aguardar publisher confirm e registrar sucesso. Expiração de lease possibilita recuperação após queda; retries exponenciais com jitter, máximo configurado e DLQ, sem apagar mensagem falha. Publicar e marcar continuam não atômicos com Rabbit: duplicação é esperada e tratada pela Inbox.

```sql
CREATE TABLE dbo.InboxMessages (
    ConsumerName nvarchar(200) NOT NULL,
    MessageId nvarchar(100) NOT NULL,
    ProcessedAt datetimeoffset NOT NULL,
    CONSTRAINT PK_InboxMessages PRIMARY KEY (ConsumerName, MessageId)
);
```

Consumidor insere Inbox e aplica sua alteração local na mesma transação. Violação da chave composta significa duplicata já processada; confirmar ACK apenas depois do commit. Poison messages vão a DLQ com alerta e procedimento de replay autorizado.

### Aceite e operação

- Teste de aplicação confirma persistência idempotente na outbox e que enqueue não marca falsamente o pagamento como publicado; testes de crash antes/depois do publish/commit e consumidores idempotentes ainda precisam ser feitos.
- Teste de concorrência com múltiplos dispatchers/consumidores.
- Alarmes para idade/count da outbox, tentativas, DLQ, atraso de consumidores e indisponibilidade Rabbit.
- Contratos de eventos têm nome, versão, compatibilidade backward, tamanho máximo e política de PII.
- **Aplicado nesta alteração:** cadastro de funcionários e pagamentos programados publicam via outbox; o controller deixou de publicar diretamente. Consumidores idempotentes, replay administrativo, outbox para futuras operações de outros domínios, hardening do broker em produção e validação real ainda pendentes.

## 6. A-07 — gRPC síncrono e contratos

Usar gRPC para chamadas internas curtas com necessidade de contrato tipado/baixa latência; não substituir RabbitMQ para trabalhos duráveis. REST permanece interface pública quando apropriado. Definir deadlines, cancellation, autenticação service-to-service, retries apenas em operações idempotentes, circuit breaker e compatibilidade de protobuf.

Exemplo de contrato versionado:

```proto
syntax = "proto3";
package civil.procurement.v1;
option csharp_namespace = "CivilConstruction.Contracts.Procurement.V1";

service SupplierDirectory {
  rpc GetSupplier (GetSupplierRequest) returns (GetSupplierResponse);
}

message GetSupplierRequest { string supplier_id = 1; }
message GetSupplierResponse {
  string supplier_id = 1;
  string display_name = 2;
  bool active = 3;
}
```

`supplier_id` não implica acesso ao banco de Procurement por outro serviço. Quando indisponível, o chamador deve usar timeout/estado de falha definido ou uma projeção local; nunca fazer retry infinito nem retry automático de mutation não idempotente.

### Aceite

- Contratos são gerados/versionados em pacote independente; mudança incompatível cria versão major/package/service.
- Testes de contrato cliente-servidor e de autorização service-to-service.
- Telemetria inclui status gRPC, deadline excedido e dependência; timeouts têm orçamento ponta a ponta.

## 7. A-08 — Observabilidade distribuída

Meta: OpenTelemetry SDK em API, Worker e futuros serviços; export OTLP para Collector; Collector exporta ao backend aprovado (por exemplo, Grafana Tempo/Prometheus/Loki ou plataforma gerenciada). Backends e retenção devem ser definidos pela operação.

```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: "construction-api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter());
```

O exemplo requer pacotes OpenTelemetry compatíveis, configuração OTLP e instrumentação RabbitMQ validada; não está ativo apenas por estar documentado. Propagar W3C Trace Context em HTTP/gRPC e headers Rabbit (`traceparent`, `tracestate`, baggage permitido); não incluir segredos, tokens, CPF, dados bancários ou payload pessoal em labels/logs. Logs estruturados correlacionam `TraceId`, `SpanId`, `CorrelationId`, `TenantId` pseudonimizado e IDs de negócio.

Definir RED (rate/errors/duration), saturação, profundidade/idade de filas, retries/DLQ, falhas DB, pagamentos em atraso e SLOs. Health endpoints liveness/readiness separados; readiness checa dependências críticas com timeout e não expõe detalhes sensíveis. Dashboards e alertas precisam de owner, threshold, severidade e runbook.

### Aceite

- Trace ponta a ponta API -> banco/outbox -> Rabbit -> consumidor aparece em ambiente de teste.
- Métricas de alta cardinalidade não usam IDs individuais.
- Dashboard e alertas exercitados por falhas simuladas; logs são redigidos e retidos segundo política.

## 8. A-09 — Segurança, autorização e auditoria

1. Usar OIDC/OAuth2 com tokens curtos e validação de issuer/audience/HTTPS em todos ambientes expostos.
2. Adicionar policies por capability (ex.: `purchase.approve`, `finance.payment.release`) e resource authorization por empresa/obra/tenant; papel não é suficiente quando acesso é por recurso.
3. Aplicar tenant ID vindo de claim validada, filtros/constraints e chaves compostas onde necessário; teste negativo prova ausência de leitura/escrita cross-tenant.
4. Auditar actor, ação, entidade, timestamp UTC, correlation, motivo e before/after redigidos. Audit trail não deve permitir alteração comum do domínio.
5. Armazenar secrets em secret manager, rotacionar credenciais SQL/Rabbit/JWT signing keys, usar identidade gerenciada quando suportada e privilégio mínimo.
6. Rever TLS, encryption-at-rest/key ownership, backups cifrados, rate limiting, CORS, headers, antifraude e plano de incidente.
7. Modelar retenção/eliminação de dados pessoais e legais com jurídico/DPO; Event Sourcing complica apagamento e precisa estratégia de PII segregada/tombstone/chaves criptográficas avaliada formalmente.

```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("finance.payment.release", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("permission", "finance.payment.release"));
```

Essa policy é apenas exemplo; deve ser combinada a autorização baseada no recurso e contexto tenant. **Nenhuma configuração de exemplo elimina a necessidade de threat modeling, testes e revisão de segurança.**

## 9. A-10 — CI/CD, migrations e rollback

### CI implementado

`.github/workflows/ci.yml` executa restore, build Release, testes e publica artefatos API/Worker para cada push/PR em `main`/`master` e execução manual. Esse workflow é CI, não faz deploy; requer validação no GitHub Actions real.

### CD necessário antes de produção

O destino de implantação não está definido neste repositório. Após escolher (Kubernetes/Helm, Azure Container Apps, App Service ou outro), adicionar job protegido por GitHub Environment, aprovação, OIDC em vez de segredo de longa duração, promoção do mesmo artefato/digest, migrations compatíveis, smoke tests e rollback. Não inserir kubeconfig, senha ou token no YAML.

Fluxo recomendado:

```text
PR: restore -> build -> unit tests -> integration tests SQL/Rabbit -> security/SBOM
merge: build immutable image/artifact -> scan/sign -> staging deploy -> migrations expand
staging: smoke/contract tests -> approval
production: canary/rolling deploy -> health/SLO gate -> complete or rollback
rollback: route to prior image + compensate/forward-fix schema; never assume DB downgrade is safe
```

Migrations seguem expand/contract (adicionar compatível -> deploy código dual-compatible -> backfill verificado -> remover legado em release posterior). Backup/restore, downgrade, perda de dados e rollback devem ser testados antes de uso. O job não pode aplicar migrations destrutivas sem aprovação/backup verificável.

### Testes de integração

Adicionar projeto/fatia específica com SQL Server e RabbitMQ descartáveis (por exemplo Testcontainers se aprovados pelo repositório), testar migrations do zero e upgrade, constraints, transações, outbox/inbox, publisher confirm, redelivery, DLQ e concorrência. `EFCore.InMemory` permanece útil para alguns testes isolados, não como substituto de provedor relacional.

### Aceite

- CI executou no runner GitHub e artefatos são inspecionáveis/reprodutíveis.
- CD usa environment protection, identidade de curta duração, artefato imutável e smoke test.
- Rollback de app e compatibilidade de schema foram exercitados em staging com runbook e owner.

## 10. A-11 — Roadmap macro ERP de construção civil

Cada item exige discovery com responsáveis de obra, compras, financeiro, RH, jurídico/segurança, contabilidade e fiscalização. O roadmap funcional do relatório inicial é ampliado abaixo:

| Domínio | Capacidades a especificar e implementar | Dependências/aceite do domínio |
|---|---|---|
| Comercial | CRM, oportunidade, orçamento preliminar, composição, proposta, concorrência e contrato/aditivos | Versionamento de proposta, margem e aprovação; vínculo rastreável até obra |
| Planejamento | EAP/WBS, quantitativos, orçamento analítico, cronograma CPM, baseline, caminho crítico e curva S | Regras de calendário, unidade, produtividade e medição física auditáveis |
| Execução | Diário digital offline, clima, equipes, equipamentos, atividade/quantidade, evidências, RDO e avanço | Sincronização idempotente, histórico de autoria/data e reconciliação offline |
| Contratos/medição | Boletins por serviço/quantidade, glosas, retenções, aditivos, garantias e aceite | Separar avanço físico, faturamento e caixa; fluxo de aprovação e trilha |
| Compras | Requisição, orçamento, RFQ, mapa comparativo, alçada, pedido, entrega parcial, devolução e avaliação | Segregação requisitante/aprovador; vincular verba e contrato |
| Estoque/logística | Almoxarifado por obra, reserva, transferência, lote/série, inventário, perda e rastreabilidade | Ledger de movimentação, saldo reconciliável e prevenção de saldo inválido |
| Equipamentos | Cadastro, custo/hora, disponibilidade, locação, combustível, telemetria e manutenção preventiva/corretiva | Horímetro, inspeções e custo total; evidência de ordem de serviço |
| Pessoas/folha | Ponto, jornada, horas extras, alocação, treinamento, EPI, saúde ocupacional, benefícios e folha | Integrações homologadas; não calcular tributos sem motor/regra legal validada |
| Financeiro | AP/AR, orçamento de caixa, centro de custo, aprovação, banco, conciliação, cobrança, retenções e forecast | Dupla aprovação, idempotência, reconciliação e segregação de função |
| Fiscal/contábil | NF-e/NFS-e, impostos, retenções, plano de contas, razão, SPED e obrigações | Provedor homologado, período fiscal, trilha de correção e especialistas responsáveis |
| Segurança/qualidade | APR/PT, incidentes, investigação, treinamento, inspeção, não conformidade/CAPA e indicadores | Evidências imutáveis, prazos, ações corretivas e acesso controlado |
| Documentos | Contratos, ART/RRT, alvarás, licenças, projetos e versões/aprovações | Validade, assinatura, controle de acesso, retenção e relação com obra |
| Analytics | Custo previsto/real, prazo, produtividade, compras, risco e portfólio | Definições semânticas, origem/linhagem, latência e controle de acesso |

Não considerar domínio concluído apenas por CRUD. Critérios comuns: fluxos feliz/negativo, autorização, idempotência, concorrência, auditoria, migração, API/event contract, relatórios/reconciliação, observabilidade, operação e treinamento aprovados.

## 11. Portões de produção e limitações permanentes

- Não é possível garantir ausência absoluta de bugs.
- SQL Server e RabbitMQ reais **não foram validados** nesta alteração.
- Migrações **não foram aplicadas** a banco real.
- Build, testes e Swagger **não equivalem a certificação de funcionamento em produção**.
- A CI recém-criada não foi executada no GitHub Actions por esta sessão; seu sucesso local não certifica o runner remoto.
- Nenhum microserviço autônomo, CQRS formal, Event Sourcing, inbox/consumidor idempotente, gRPC, CD ou telemetria descritos aqui deve ser anunciado como implementado antes de código, deployment e testes/aceite correspondentes. A outbox está implementada apenas para os dois eventos descritos no A-06 e não foi validada com banco/broker reais.
- A autorização de destino de deployment (cloud/on-prem, cluster, DNS, secrets, estratégia de release) requer configuração do ambiente responsável; sem isso, job de deploy genérico não é seguro nem operacionalmente completo.
- Antes de tráfego financeiro real: threat model, revisão independente, reconciliação, autorização segregada, recuperação testada e homologação das integrações e requisitos fiscais aplicáveis.
