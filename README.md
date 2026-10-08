# Civil Construction

## Arquitetura

Consulte [a avaliação arquitetural, a matriz de validação e o roadmap](docs/architecture-validation-roadmap.md) para conhecer o estado constatado, as lacunas, os riscos e as limitações das verificações.
O [plano de arquitetura-alvo e implementação](docs/target-architecture-implementation.md) detalha tarefas mandatórias, exemplos técnicos, critérios de aceite e a evolução proposta para microserviços, CQRS, Event Sourcing, outbox, gRPC, CI/CD e observabilidade. Esses exemplos não significam que as capacidades estejam implementadas.

A solução usa camadas com dependências voltadas para o domínio e módulos operacionais para:

- `CivilConstruction.Domain`: entidades, contratos de repositório e eventos.
- `CivilConstruction.Application`: casos de uso, DTOs e portas, incluindo `IUnitOfWork`.
- `CivilConstruction.Infrastructure`: SQL Server, repositórios, RabbitMQ e implementação das portas.
- `CivilConstruction.API`: composição e exposição HTTP.
- `CivilConstruction.Worker`: geração agendada de competências e publicação de eventos de pagamento.

Os módulos cobertos incluem cadastro e ciclo de vida de obras e contratos, clientes e fornecedores, medições físicas e geração de contas a receber, contas a pagar/receber com liquidação, compras e recebimento integrado ao estoque, patrimônio/alocação/manutenção prevista de equipamentos, cadastro e alocação de funcionários, diário de obra, inspeções de qualidade, ocorrências de segurança, programação recorrente de pagamentos e publicação de ordens de pagamento via RabbitMQ.

Funcionários e financeiro estão separados por responsabilidade, mas ainda compartilham o mesmo modelo de persistência e banco. Por isso, esta solução é um monólito modular com um worker independente, não dois microserviços isolados. Antes de extrair serviços, cada domínio precisa ter propriedade exclusiva dos dados, contratos de integração versionados, migração dos dados e implantação/observabilidade independentes. A separação prematura aumentaria a complexidade distribuída sem criar autonomia real.

Esta é uma base extensível de gestão, não um ERP certificado ou solução pronta para produção integral. Orçamentos analíticos/EAP, propostas e licitações, subempreiteiros, locação e depreciação contábil, aprovações multinível, documentos/ART/alvarás, diário fotográfico, apontamento individual de ponto/horas extras, medicina ocupacional, folha fiscal, impostos, retenções tributárias brasileiras, conciliação bancária, emissão de NF-e/NFS-e, contabilidade/SPED, centros de custo e BI ainda exigem módulos, integrações e regras validadas por especialistas. A medição é percentual do valor contratado, não um boletim físico detalhado por composição/quantidade. Valores de folha são brutos configurados: não calculam tributos, encargos, horas ou benefícios, e eventos RabbitMQ não executam transferências bancárias.

**Segurança:** fora do ambiente `Development`, a API exige `Authentication:Authority` e `Authentication:Audience` e valida tokens JWT HTTPS de um provedor de identidade configurado. O modo de desenvolvimento não aplica autenticação para facilitar testes locais; nunca o use em ambiente exposto. Ainda é necessário definir autorização por papéis/tenant e auditoria conforme a política da empresa. O Worker publica instruções de pagamento, mas não inicia transferências.

## Requisitos

- .NET SDK 10
- SQL Server
- RabbitMQ

As versões de pacotes NuGet são centralizadas em `Directory.Packages.props`; o framework alvo é definido em `Directory.Build.props`.

## Compilar e testar

```powershell
dotnet restore .\CivilConstruction.sln
dotnet build .\CivilConstruction.sln
dotnet test .\CivilConstruction.Tests\CivilConstruction.Tests.csproj
```

## Swagger / OpenAPI

Execute a API no perfil Development para testar os contratos HTTP pela interface interativa:

```powershell
dotnet run --project .\CivilConstruction.API\CivilConstruction.API.csproj --launch-profile http
```

Abra `http://localhost:5000/swagger` (documento OpenAPI: `http://localhost:5000/swagger/v1/swagger.json`). A interface e o documento Swagger ficam disponíveis apenas em `Development`; em produção, configure o provedor JWT e use um processo controlado para publicar a documentação. As operações que consultam ou alteram dados também precisam de SQL Server configurado e acessível.

## Banco de dados

Configure `ConnectionStrings:DefaultConnection` por variável de ambiente ou secret store. As migrações do modelo atual estão versionadas. As migrações não são executadas automaticamente durante a inicialização da API; instale a ferramenta e aplique-as como etapa de implantação:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
dotnet ef database update --project .\CivilConstruction.Infrastructure\CivilConstruction.Infrastructure.csproj --startup-project .\CivilConstruction.API\CivilConstruction.API.csproj
```

Para alterações futuras de modelo, crie uma nova migração nomeada antes de aplicá-la:

```powershell
dotnet ef migrations add NomeDaAlteracao --project .\CivilConstruction.Infrastructure\CivilConstruction.Infrastructure.csproj --startup-project .\CivilConstruction.API\CivilConstruction.API.csproj --output-dir Migrations
dotnet ef database update --project .\CivilConstruction.Infrastructure\CivilConstruction.Infrastructure.csproj --startup-project .\CivilConstruction.API\CivilConstruction.API.csproj
```

## Configuração

`RabbitMQ:Host`, `RabbitMQ:UserName`, `RabbitMQ:Password`, `RabbitMQ:QueueFuncionarios` e `RabbitMQ:QueuePagamentos` podem ser definidos por configuração padrão do .NET, incluindo variáveis de ambiente (`RabbitMQ__UserName` e `RabbitMQ__Password`). Usuário não vazio, diferente de `guest`, e senha são obrigatórios; não há mais fallback para credenciais públicas conhecidas. O Compose exige `RABBITMQ_DEFAULT_USER`, `RABBITMQ_DEFAULT_PASS` e `MSSQL_SA_PASSWORD`; configure valores locais fortes no ambiente antes de iniciar os serviços e configure as credenciais RabbitMQ correspondentes para API/Worker. Não grave valores reais em arquivos versionados nem reutilize os de desenvolvimento em produção. A API e o Worker requerem a mesma `ConnectionStrings:DefaultConnection`; em produção, cada serviço deve receber somente credenciais/permissões necessárias. Configure `Authentication:Authority` e `Authentication:Audience` para a API em produção. Injete segredos por secret store; não armazene segredos nos arquivos versionados. O Worker consulta competências vencidas na inicialização e a cada hora; em falhas, tenta novamente após cinco minutos.

Eventos de funcionário cadastrado e pagamentos programados são gravados numa tabela outbox na mesma transação SQL da operação de negócio; o Worker publica-os em RabbitMQ com `MessageId`, tipo e versão estáveis, confirmação do publisher e retry limitado. A entrega continua sendo pelo menos uma vez: se o processo cair depois do broker confirmar e antes de registrar o sucesso no SQL, a mensagem poderá ser reenviada. Consumidores (ainda não incluídos nesta solução) devem deduplicar por `MessageId` numa inbox transacional antes de executar efeitos; mensagens esgotadas ficam em estado dead-letter na outbox para tratamento operacional. RabbitMQ não realiza transferência bancária.

## API de gestão

Os endpoints estão organizados em `/api/obras`, `/api/clientes`, `/api/contratos`, `/api/fornecedores`, `/api/compras`, `/api/estoque`, `/api/equipamentos`, `/api/folha`, `/api/financeiro`, `/api/apontamentos`, `/api/alocacoes`, `/api/seguranca` e `/api/qualidade`. Exemplos de chamada e fluxos estão em `CivilConstruction.API/GestaoConstrucao.http`. Compras só alteram estoque ao serem recebidas; contrato/medição possuem transições de estado; faturas emitidas criam contas a receber; folha gera competências idempotentes. Estados são serializados como texto.
