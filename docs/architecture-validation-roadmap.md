# Avaliação arquitetural, validação e roadmap

## 1. Objetivo e limites desta avaliação

Este documento registra o estado observável da solução, os resultados das verificações executadas e uma evolução recomendada para um ERP de construção civil. A distinção entre **implementado**, **parcial** e **proposto** é intencional: uma integração documentada ou planejada não é evidência de que foi implementada ou validada.

A avaliação do código não substitui análise de ameaças, validação por especialistas de negócio/contabilidade, testes de carga, homologação fiscal, auditoria independente ou certificação para produção.

## 2. Resumo executivo

| Área | Estado constatado | Consequência |
|---|---|---|
| Plataforma | Projetos direcionados a .NET 10, com versões NuGet centralizadas | Base tecnológica consistente; manter SDK e dependências corrigidos |
| Arquitetura | Domain, Application, Infrastructure, API e Worker | Separação por camadas existe, mas o produto é um monólito modular com processo Worker, não uma arquitetura de microserviços autônomos |
| Persistência | EF Core com SQL Server, `AppDbContext` compartilhado e migrações versionadas | A API e o Worker compartilham banco e modelo de dados; não há isolamento de dados por serviço |
| Mensageria | RabbitMQ com filas duráveis, mensagens persistentes, publisher confirms e `MessageId` estável; outbox/relay agora cobre cadastro de funcionário e pagamentos programados | Há entrega pelo menos uma vez; consumidor/inbox idempotente não foi encontrado; migration da outbox ainda não foi aplicada |
| API | Controllers REST e Swagger/OpenAPI | Swagger UI e documento OpenAPI responderam com HTTP 200 durante esta verificação |
| Segurança | JWT e política global de autenticação fora de `Development`; credenciais RabbitMQ agora são explícitas e não aceitam usuário `guest` | Em `Development` a API não exige autenticação; autorização fina, tenant e auditoria de negócio ainda precisam de implementação |
| Testes | 19 testes xUnit passaram; persistência de teste usa EF Core InMemory | Não comprovam comportamento de SQL Server, RabbitMQ, concorrência relacional ou integrações externas |
| Operação | `ILogger` e Worker com repetição periódica; workflow CI adicionado nesta revisão | CI remoto não foi executado; CD, OpenTelemetry, health checks e gRPC não estão configurados |

**Conclusão:** a solução é uma base funcional de gestão operacional e uma arquitetura em camadas em evolução. Não é, no estado verificado, um ERP abrangente de ponta a ponta certificado para produção, nem um conjunto de microserviços independentes. O roadmap abaixo trata os principais riscos e lacunas sem atribuir capacidades inexistentes ao sistema.

## 3. Arquitetura e princípios

### 3.1 Estrutura observada

```text
CivilConstruction.Domain
    Entidades, interfaces de repositório e eventos de domínio
        ↑
CivilConstruction.Application
    DTOs, interfaces de aplicação e serviços/casos de uso
        ↑
CivilConstruction.Infrastructure
    EF Core, SQL Server, repositórios, Unit of Work e RabbitMQ

CivilConstruction.API
    Controllers REST, autenticação, Swagger e composição da aplicação

CivilConstruction.Worker
    Serviço hospedado para competências de pagamento programado
```

O host da API faz a composição das camadas e depende diretamente de Application e Infrastructure. Essa dependência no ponto de composição é aceitável, mas os módulos de aplicação permanecem agrupados por funcionalidade e usam serviços de aplicação; não foi encontrado um pipeline CQRS explícito com handlers separados de comando e consulta.

### 3.2 Clean Architecture e DDD

**Implementado ou parcialmente implementado**

- Separação física entre domínio, aplicação, infraestrutura e apresentação.
- Interfaces de repositório e `IUnitOfWork` na camada de aplicação/domínio, com implementações na infraestrutura.
- Entidades e regras/transições de negócio em áreas como contratos, compras, medições, estoque, alocação e pagamentos programados.
- Configurações EF Core separadas por entidade, em vez de concentrar todo o mapeamento no contexto.
- Restrições de integridade e índices também aplicados no modelo/migrações.

**Não constatado**

- Limites completos de Aggregate Root, invariantes encapsuladas e regras formais de consistência por agregado para todos os módulos.
- CQRS com modelos de leitura e escrita independentes, handlers, pipeline behaviors ou segregação de armazenamento.
- Event Sourcing. Os eventos existentes são mensagens/eventos de integração; não há evidência de event store, replay de estado ou trilha imutável de eventos de domínio.
- Outbox parcial: o cadastro de funcionário e o enfileiramento de pagamentos programados persistem mensagens na mesma unidade SQL, e o Worker entrega com retry; inbox/consumidores idempotentes e outbox para os demais casos de uso ainda não foram implementados.

Recomenda-se consolidar primeiro os limites de domínio e os casos de uso transacionais. CQRS deve ser aplicado onde houver necessidade mensurável de escalabilidade ou modelos de leitura distintos. Event Sourcing não deve ser adotado como padrão universal: só faz sentido para domínios que exijam histórico imutável/reconstrução e cuja equipe possa operar essa complexidade.

### 3.3 Monólito modular versus microserviços

API e Worker usam `ConnectionStrings:DefaultConnection`, compartilham o `AppDbContext` e os repositórios da mesma infraestrutura. Portanto, Worker é um processo separado, mas não representa autonomia de microserviço. A extração de serviços só deve ocorrer depois de definir propriedade exclusiva dos dados, contratos versionados, consistência distribuída, observabilidade, implantação e responsabilidade operacional por serviço.

## 4. Integrações e fluxo de dados

### 4.1 SQL Server e Entity Framework Core

- A infraestrutura configura EF Core com SQL Server e valida a presença de `ConnectionStrings:DefaultConnection`.
- `AppDbContext` contém entidades dos módulos de pessoal, obras, compras, estoque, equipamentos e finanças.
- Há migrações versionadas. **Nenhuma migração foi aplicada ou validada contra uma instância real de SQL Server nesta revisão.**
- A configuração de teste usa EF Core InMemory, que não reproduz completamente constraints, SQL gerado, transações, bloqueios, isolamento, índices filtrados ou comportamento do provedor SQL Server.

**Evolução recomendada:** executar migrations em banco descartável de CI via SQL Server container/Testcontainers; validar upgrade a partir de uma versão anterior; testar rollback/backup e estratégia expand-contract; não executar migrations automaticamente no startup de produção sem uma decisão operacional explícita.

### 4.2 RabbitMQ

O publicador declara filas duráveis, publica mensagens persistentes em JSON e habilita publisher confirmations. O `MessageId` é estável, derivado do identificador do evento. O Worker cria competências vencidas e publica pagamentos programados pendentes.

**Limitações e riscos verificados**

- Não foi localizado consumidor RabbitMQ de negócio na solução.
- A outbox grava alterações e eventos de funcionário/pagamento na mesma unidade SQL. Publicar e marcar como entregue no relay não é atômico com RabbitMQ; uma falha entre essas operações pode produzir reenvio, portanto a entrega é pelo menos uma vez.
- A idempotência pelo `MessageId` é responsabilidade do consumidor e ainda não há implementação de consumidor/inbox/deduplicação constatada.
- A configuração exigia anteriormente fallback `guest`/`guest`; essa lacuna foi corrigida nesta alteração. Usuário RabbitMQ não vazio e diferente de `guest`, e senha explícita agora são validados; o Compose requer credenciais via ambiente. Isso não substitui ACLs/vhost com privilégio mínimo, TLS, secret manager ou rotação.
- A mensagem representa instrução/evento de pagamento; não executa transferência bancária.

**Evolução recomendada:** outbox transacional no banco da origem, consumidor idempotente com inbox, política de retry com backoff e jitter, dead-letter exchange, limites de tentativas, métricas de fila e gestão de credenciais/TLS. Não declarar exatamente-uma-vez: a meta realista é entrega pelo menos uma vez com efeitos idempotentes.

### 4.3 REST, OpenAPI e gRPC

- A camada de apresentação expõe controllers REST.
- O Swagger/OpenAPI é ativado somente em `Development`.
- O documento retornou OpenAPI 3.0.4, esquema de segurança Bearer e **43 caminhos** (`paths`) na execução desta revisão.
- Nenhuma configuração ou serviço gRPC foi localizado. gRPC é uma possibilidade de integração futura entre serviços, não uma capacidade atual.
- Não foi localizada configuração de versionamento de API. Para alterações incompatíveis, planejar versionamento explícito e política de descontinuação.

## 5. Funcionalidades ERP observadas

O modelo e a API cobrem parcialmente as seguintes áreas:

| Área | Capacidades presentes | Lacunas relevantes para operação integral |
|---|---|---|
| Obras | Cadastro/ciclo de vida, diário e apontamentos de obra | EAP/WBS, planejamento CPM, baseline, caminho crítico, avanço físico por composição, fotos/georreferência e gestão documental |
| Clientes e contratos | Cadastro, contratos, estados e medições percentuais | CRM/propostas/licitações, aditivos, retenções, garantias, medição física detalhada e assinatura eletrônica |
| Compras e fornecedores | Ordem de compra, fornecedor e recebimento integrado ao estoque | Cotação multi-fornecedor, mapa comparativo, alçadas, contratos de fornecimento, qualificação e avaliação |
| Estoque | Itens e movimentação por recebimento de compra | Requisições/reservas por obra, transferência entre almoxarifados, inventário, lote/série, perdas e rastreabilidade |
| Equipamentos | Patrimônio, alocação e manutenção prevista | Telemetria, inspeção pré-uso, combustível, locação, depreciação, custo-hora e disponibilidade |
| Pessoas | Funcionários e alocação em obras | Ponto, jornada, horas extras, benefícios, medicina e segurança ocupacional, eSocial e folha fiscal |
| Financeiro | Receitas, despesas, caixa, lançamentos e competências programadas | Orçamento, fluxo de caixa consolidado, centro de custo, conciliação bancária, impostos, retenções, aprovação e integração bancária |
| Qualidade e segurança | Inspeções de qualidade e incidentes de segurança | Planos de inspeção por etapa, não conformidade/CAPA, permissões de trabalho, treinamentos, evidências e indicadores regulatórios |

Também permanecem fora do escopo observado: subempreiteiros/medições e retenções; emissão/recebimento de NF-e e NFS-e; contabilidade, plano de contas e SPED; gestão de documentos/ART/alvarás/licenças; gestão de propostas e orçamento analítico; BI e relatórios regulatórios. A lista é um mapa inicial de lacunas, não uma declaração de que todas as obrigações legais foram mapeadas.

## 6. Segurança, auditoria e privacidade

### Controles constatados

- JWT Bearer configurado.
- Fora de `Development`, `Authentication:Authority` e `Authentication:Audience` são obrigatórios; a API exige usuário autenticado por política global.
- `RequireHttpsMetadata` fica habilitado fora de `Development`.
- Tratamento global de exceções e respostas Problem Details.
- Configurações sensíveis devem ser injetadas por ambiente/secret store, conforme orientação do README.

### Controles não constatados ou que requerem validação

- A configuração de `Development` não instala a política global de autenticação; não deve ser exposta a redes não confiáveis.
- Autorização por papéis/permissões, escopo por obra/empresa/tenant e segregação de funções.
- Auditoria de negócio imutável para alterações financeiras, aprovações e dados pessoais.
- Política formal de retenção, classificação, minimização e proteção de dados pessoais.
- Gestão/rotação de segredos, criptografia de dados em repouso e plano de resposta a incidentes.
- Rate limiting, proteção contra abuso, política de CORS e cabeçalhos de segurança revisados.
- Logging distribuído com correlação de trace/request e redaction de dados sensíveis.

Esses controles devem ser definidos por avaliação de risco e legislação aplicável. A presença de JWT não equivale a autorização completa, auditoria ou conformidade regulatória.

## 7. CI/CD, observabilidade e operação

Foi adicionado `.github/workflows/ci.yml` nesta revisão para restore, build, testes e publicação de artefatos API/Worker. A execução local equivalente de build/test/publish passou, mas **o workflow remoto do GitHub Actions não foi executado**. Não há deployment automatizado, promoção ou rollback configurado. Também não foram localizados OpenTelemetry, health checks ou endpoints de readiness/liveness. Há logging via `ILogger` no Worker, mas não há evidência de logging/tracing/métricas distribuídos configurados.

### Pipeline recomendada

1. Restore/build .NET 10 com warnings tratados como falhas quando viável.
2. Testes unitários e de aplicação; testes de integração com SQL Server e RabbitMQ descartáveis.
3. Verificação de migrations pendentes e teste de upgrade de esquema.
4. Análise de dependências, segredos, SAST e geração/armazenamento de SBOM.
5. Build de imagens imutáveis, publicação assinada e promoção do mesmo artefato por ambientes.
6. Deploy com estratégia de rollback, migrations compatíveis e aprovação para produção.
7. Smoke tests autenticados, verificação de OpenAPI e acompanhamento pós-deploy.

### Observabilidade recomendada

- OpenTelemetry para traces, métricas e logs correlacionados; propagar trace/correlation ID pela API, Worker e mensagens.
- Health checks separados para processo, SQL Server e RabbitMQ; readiness não deve indicar pronto se dependências críticas estiverem indisponíveis.
- Métricas de duração/erro por caso de uso, pagamentos pendentes/reprocessados, publicação/consumo, dead letters e saturação.
- Logs estruturados com redaction; alertas com SLOs e runbooks operacionais.

## 8. Roadmap recomendado

Prioridade é uma recomendação técnica; sequenciamento final depende de requisitos, legislação e risco de negócio.

### P0 — Segurança e confiabilidade antes de produção

- Completar usuário/vhost de privilégio mínimo, TLS, secret manager e rotação de credenciais RabbitMQ; os fallbacks para credenciais padrão já foram removidos.
- Definir authorization policies por operação, papel e escopo de empresa/obra.
- Adicionar auditoria de ações críticas, gestão de segredos e política de dados pessoais.
- Implementar outbox/inbox idempotentes para integração financeira; incluir DLQ e retry controlado.
- Validar migrations e restrições com SQL Server real em ambiente automatizado.
- Definir backup, restauração, recuperação de desastre e procedimento de rollback.

### P1 — Fluxo central de obra e controles gerenciais

- EAP/WBS, orçamento analítico, cronograma e baseline.
- Planejamento e medição por serviço, quantidade, composição e evidência.
- Solicitação de compra, cotações, mapa comparativo, alçadas, recebimento parcial e vínculo com orçamento.
- Estoque por obra/almoxarifado, reserva, transferência, inventário e rastreabilidade.
- Aprovação de despesas, centros de custo, fluxo de caixa projetado e conciliação bancária.

### P2 — Pessoas, conformidade e cadeia de execução

- Apontamento de jornada e apropriação de horas por obra/atividade.
- Integração especializada de folha, benefícios, encargos e obrigações trabalhistas.
- Subempreiteiros, contratos, boletins, retenções, garantias e avaliação.
- Segurança do trabalho, treinamentos, permissões, planos de inspeção, não conformidades e CAPA.
- Gestão de documentos, revisão, aprovação, validade e evidências de obra.

### P3 — Integrações e escala organizacional

- Integrações fiscais/contábeis e bancárias homologadas, conforme fornecedores e regras aplicáveis.
- BI, indicadores de custo/prazo/produtividade e relatórios consolidados.
- API versioning, catálogo de contratos, rate limiting e integração de parceiros.
- Extrair serviços apenas quando houver limites de domínio e necessidade operacional comprovados; iniciar por um contexto com baixo acoplamento e ownership explícito dos dados.

## 9. Validações executadas nesta revisão

| Verificação | Resultado observado | O que não comprova |
|---|---|---|
| `dotnet build .\CivilConstruction.sln` | Êxito; 0 avisos e 0 erros | Execução em produção, conectividade ou comportamento em carga |
| `dotnet test .\CivilConstruction.Tests\CivilConstruction.Tests.csproj --configuration Release --no-build` | 19 aprovados; 0 falhas; 0 ignorados | Testes de integração SQL Server/RabbitMQ; o projeto usa EF Core InMemory |
| `GET http://localhost:5000/swagger/index.html` | HTTP 200 | Todos os fluxos de negócio e dependências externas |
| `GET http://localhost:5000/swagger/v1/swagger.json` | HTTP 200; OpenAPI 3.0.4; 43 caminhos; esquema Bearer | Autorização efetiva de todos os perfis ou compatibilidade de clientes em produção |

A instância local foi iniciada em `Development`; por isso, a validação Swagger não comprova autenticação de produção. Não foram efetuadas operações reais contra SQL Server ou RabbitMQ; a nova migration da outbox foi gerada e o snapshot está sincronizado, mas nenhuma migration foi aplicada a banco real.

## 10. Limitações e declaração de rigor

- **Não é possível garantir ausência absoluta de bugs ou falhas.** Build e testes cobrem apenas cenários exercitados.
- **SQL Server e RabbitMQ reais não foram validados** nesta revisão.
- **Migrações de banco não foram aplicadas** a uma instância real.
- **Build, testes e Swagger não equivalem a certificação de funcionamento em produção.**
- Não foram executados testes de carga, resiliência, segurança, recuperação de desastre, homologação fiscal ou auditoria de conformidade.
- A cobertura funcional descrita reflete código observado; não representa validação de processos reais de uma construtora nem substitui aprovação dos responsáveis de negócio.
- Integrações REST/gRPC/RabbitMQ, CQRS formal, Event Sourcing, CI/CD e observabilidade só devem ser consideradas existentes quando código/configuração e validação demonstrarem isso. CI foi configurada e build/test/publish passaram localmente; execução remota de workflow e CD ainda não foram verificadas.
