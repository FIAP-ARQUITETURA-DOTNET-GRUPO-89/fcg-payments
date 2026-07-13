# FcgPayments

Microsservi&ccedil;o de processamento de pagamentos simulado da plataforma **FCG Games**. Consome eventos de pedidos criados pelo CatalogAPI, simula a aprova&ccedil;&atilde;o/rejei&ccedil;&atilde;o do pagamento e publica o resultado de volta para a plataforma.

## Sum&aacute;rio

- [Vis&atilde;o Geral](#vis&atilde;o-geral)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Arquitetura](#arquitetura)
- [Estrutura da Solu&ccedil;&atilde;o](#estrutura-da-solu&ccedil;&atilde;o)
- [Pr&eacute;-requisitos](#pr&eacute;-requisitos)
- [Executando Localmente](#executando-localmente)
- [Eventos (Mensageria)](#eventos-mensageria)
- [Endpoint da API](#endpoint-da-api)
- [Banco de Dados](#banco-de-dados)
- [Autentica&ccedil;&atilde;o JWT](#autentica&ccedil;&atilde;o-jwt)
- [Testes](#testes)
- [Vari&aacute;veis de Ambiente](#vari&aacute;veis-de-ambiente)

## Vis&atilde;o Geral

O PaymentsAPI faz parte de uma arquitetura orientada a eventos composta por dois microsservi&ccedil;os:

```
CatalogAPI                         PaymentsAPI
(publica OrderPlacedEvent) ──────> Worker (consome e processa)
(consome PaymentProcessedEvent) <── Worker (publica resultado)
```

O **Worker** consome `OrderPlacedEvent` do RabbitMQ, simula o processamento do pagamento (com taxa de aprova&ccedil;&atilde;o configur&aacute;vel), persiste o resultado no PostgreSQL e publica `PaymentProcessedEvent` de volta.

A **API** exp&otilde;e um endpoint administrativo de consulta para diagn&oacute;stico.

## Tecnologias Utilizadas

| Categoria | Tecnologia |
|---|---|
| Runtime | .NET 10 |
| API | ASP.NET Core Minimal API |
| Orquestra&ccedil;&atilde;o local | .NET Aspire |
| Mensageria | MassTransit + RabbitMQ |
| Banco de dados | Entity Framework Core + PostgreSQL (Npgsql) |
| Media&ccedil;&atilde;o | MediatR + CQRS |
| Valida&ccedil;&atilde;o | FluentValidation |
| Autentica&ccedil;&atilde;o | JWT Bearer |
| Logs | Serilog |
| Testes unit&aacute;rios | xUnit v3, Shouldly, NSubstitute |
| Testes de integra&ccedil;&atilde;o | Aspire Testing, Testcontainers, Respawn |

## Arquitetura

O projeto segue **Clean Architecture** com **CQRS**, separando comandos (escrita) de queries (leitura).

### Camadas

| Projeto | Responsabilidade |
|---|---|
| `FcgPayments.Api` | Endpoints HTTP, middlewares e configura&ccedil;&atilde;o da API |
| `FcgPayments.Worker` | Consumer MassTransit (`OrderPlacedConsumer`) |
| `FcgPayments.Application` | Commands, queries, handlers e validators |
| `FcgPayments.Domain` | Entidades, enums, interfaces de reposit&oacute;rio e servi&ccedil;os de dom&iacute;nio |
| `FcgPayments.Infrastructure` | EF Core, reposit&oacute;rios, configura&ccedil;&atilde;o MassTransit |
| `FcgPayments.IoC` | Registro de depend&ecirc;ncias |
| `FcgPayments.SharedKernel` | Settings, exce&ccedil;&otilde;es e componentes transversais |
| `FcgPayments.ServiceDefaults` | Configura&ccedil;&otilde;es compartilhadas do Aspire |
| `FcgPayments.AppHost` | Orquestra&ccedil;&atilde;o Aspire (PostgreSQL, RabbitMQ, API, Worker) |

### Fluxo de processamento

1. `OrderPlacedConsumer` recebe `OrderPlacedEvent` do RabbitMQ
2. Delega para `ProcessPaymentHandler` via MediatR
3. Handler verifica idempot&ecirc;ncia por `OrderId` (se j&aacute; existe, republica o resultado sem reprocessar)
4. Simula aprova&ccedil;&atilde;o/rejei&ccedil;&atilde;o via `IPaymentApprovalStrategy` (taxa configur&aacute;vel)
5. Persiste `Payment` no PostgreSQL
6. Publica `PaymentProcessedEvent` no RabbitMQ

## Estrutura da Solu&ccedil;&atilde;o

```text
src/
├── FcgPayments.Api                 # API REST (consulta administrativa)
├── FcgPayments.AppHost             # Aspire AppHost (orquestra tudo)
├── FcgPayments.Application         # Casos de uso (CQRS)
├── FcgPayments.Domain              # Entidades e regras de neg&oacute;cio
├── FcgPayments.Infrastructure      # Persist&ecirc;ncia e mensageria
├── FcgPayments.IoC                 # Inje&ccedil;&atilde;o de depend&ecirc;ncias
├── FcgPayments.ServiceDefaults     # Defaults do Aspire
├── FcgPayments.SharedKernel        # Componentes compartilhados
└── FcgPayments.Worker              # Consumer de eventos

tests/
├── FcgPayments.UnitTests           # Testes unit&aacute;rios
└── FcgPayments.IntegrationTests    # Testes de integra&ccedil;&atilde;o (Aspire + Testcontainers)
```

## Pr&eacute;-requisitos

| Ferramenta | Vers&atilde;o m&iacute;nima | Verificar |
|---|---|---|
| .NET SDK | 10.0 | `dotnet --version` |
| Docker Desktop | Qualquer recente | Deve estar **rodando** |
| .NET Aspire workload | &mdash; | `dotnet workload list` |

Instalar o Aspire workload (se necess&aacute;rio):

```bash
dotnet workload install aspire
```

## Executando Localmente

### 1. Restaurar depend&ecirc;ncias e compilar

```bash
dotnet restore
dotnet build
```

### 2. Executar com Aspire (recomendado)

O Aspire orquestra PostgreSQL, RabbitMQ (com Management Plugin), pgAdmin, a API e o Worker automaticamente via Docker:

```bash
dotnet run --project src/FcgPayments.AppHost
```

O Aspire Dashboard abre no navegador e mostra todos os recursos:

| Recurso | Descri&ccedil;&atilde;o |
|---|---|
| `fcgpayments-api` | API REST |
| `fcgpayments-worker` | Consumer de eventos |
| `Postgres` | Banco de dados + pgAdmin |
| `rabbitmq` | Broker de mensagens + Management UI |

A URL da API &eacute; exibida no Dashboard (ex: `https://localhost:7072`).

## Eventos (Mensageria)

### Evento consumido

**`OrderPlacedEvent`** &mdash; publicado pelo CatalogAPI quando um usu&aacute;rio realiza uma compra.

```csharp
public sealed record OrderPlacedEvent(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Price,
    DateTime CreatedAtUtc);
```

### Evento publicado

**`PaymentProcessedEvent`** &mdash; publicado ap&oacute;s o processamento do pagamento.

```csharp
public sealed record PaymentProcessedEvent(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Price,
    PaymentStatus Status,       // Approved | Rejected
    DateTime ProcessedAtUtc);
```

### Configura&ccedil;&atilde;o de retry (MassTransit)

O consumer utiliza retry exponencial configur&aacute;vel:

| Par&acirc;metro | Default | Descri&ccedil;&atilde;o |
|---|---|---|
| `MassTransit:RetryLimit` | `5` | N&uacute;mero m&aacute;ximo de tentativas |
| `MassTransit:MinIntervalSeconds` | `1` | Intervalo m&iacute;nimo entre retries |
| `MassTransit:MaxIntervalSeconds` | `30` | Intervalo m&aacute;ximo entre retries |
| `MassTransit:IntervalDeltaSeconds` | `2` | Fator de crescimento do backoff |

## Endpoint da API

| M&eacute;todo | Rota | Policy | Descri&ccedil;&atilde;o |
|---|---|---|---|
| `GET` | `/api/payments/{orderId}` | `AdminPolicy` | Consulta o pagamento pelo `OrderId` |

**Respostas:**

- `200 OK` &mdash; retorna `PaymentResponse` com `OrderId`, `Amount`, `Status`, `ProcessedAt`
- `401 Unauthorized` &mdash; token JWT ausente ou inv&aacute;lido
- `403 Forbidden` &mdash; usu&aacute;rio n&atilde;o possui role `Admin`
- `404 Not Found` &mdash; nenhum pagamento encontrado para o `OrderId`

## Banco de Dados

O Aspire cria e gerencia o PostgreSQL automaticamente. As migrations s&atilde;o aplicadas na inicializa&ccedil;&atilde;o da aplica&ccedil;&atilde;o.

### Entidade `Payment`

| Coluna | Tipo | Descri&ccedil;&atilde;o |
|---|---|---|
| `Id` | `Guid` | Identificador &uacute;nico |
| `OrderId` | `Guid` | ID do pedido (**&iacute;ndice &uacute;nico** &mdash; garante idempot&ecirc;ncia) |
| `UserId` | `Guid` | ID do usu&aacute;rio |
| `GameId` | `Guid` | ID do jogo |
| `Amount` | `decimal` | Valor do pagamento |
| `Status` | `enum` | `Pending`, `Approved` ou `Rejected` |
| `ProcessedAt` | `DateTime?` | Data/hora do processamento |
| `Reason` | `string?` | Motivo (preenchido em rejei&ccedil;&otilde;es) |

### Criar migration manualmente

```bash
dotnet ef migrations add NomeDaMigration -p src/FcgPayments.Infrastructure -s src/FcgPayments.Api --output-dir Database/Migrations
```

## Autentica&ccedil;&atilde;o JWT

A API valida tokens JWT (n&atilde;o emite). A configura&ccedil;&atilde;o padr&atilde;o de desenvolvimento:

```json
{
  "JwtSettings": {
    "Issuer": "FcgPayments-Issuer",
    "SecurityKey": "FcgPayments_Secret_Key_2026_High_Security_Token",
    "ExpirationHours": 2
  }
}
```

### Gerando token manualmente (jwt.io)

**Header:**

```json
{ "alg": "HS256", "typ": "JWT" }
```

**Payload Admin:**

```json
{
  "iss": "FcgPayments-Issuer",
  "sub": "1",
  "name": "Administrador",
  "role": "Admin",
  "exp": 1893456000
}
```

**Payload Customer:**

```json
{
  "iss": "FcgPayments-Issuer",
  "sub": "2",
  "name": "Cliente",
  "role": "Customer",
  "exp": 1893456000
}
```

**Secret:** `FcgPayments_Secret_Key_2026_High_Security_Token`

**Uso:**

```http
Authorization: Bearer {TOKEN}
```

### Policies

| Policy | Roles permitidas |
|---|---|
| `AdminPolicy` | Admin |
| `CustomerPolicy` | Admin, Customer |

## Testes

### Testes unit&aacute;rios (n&atilde;o precisa de Docker)

```bash
dotnet test tests/FcgPayments.UnitTests
```

Cobertura:
- `Payment` &mdash; constru&ccedil;&atilde;o, aprova&ccedil;&atilde;o, rejei&ccedil;&atilde;o, valida&ccedil;&otilde;es
- `ProcessPaymentHandler` &mdash; aprova&ccedil;&atilde;o, rejei&ccedil;&atilde;o, idempot&ecirc;ncia
- `PaymentRepository` &mdash; persist&ecirc;ncia e consulta
- `RandomPaymentApprovalStrategy` &mdash; taxas 0%, 50%, 100%, valores inv&aacute;lidos

### Testes de integra&ccedil;&atilde;o (requer Docker Desktop rodando)

```bash
dotnet test tests/FcgPayments.IntegrationTests
```

O Aspire sobe containers de PostgreSQL e RabbitMQ via Testcontainers. A primeira execu&ccedil;&atilde;o &eacute; mais lenta (pull das imagens).

Cobertura:
- `GET /api/payments/{orderId}` &mdash; 200, 404, 401, 403
- `OrderPlacedConsumer` &mdash; processamento do evento e idempot&ecirc;ncia (evento duplicado)

### Todos os testes

```bash
dotnet test
```

### Cobertura de c&oacute;digo

```bash
dotnet test tests/FcgPayments.UnitTests --collect:"XPlat Code Coverage" --settings .runsettings
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:"Html;MarkdownSummary"
```

Abra `coverage-report/index.html` para visualizar o relat&oacute;rio.

## Vari&aacute;veis de Ambiente

Quando executado pelo **Aspire AppHost**, todas as vari&aacute;veis s&atilde;o injetadas automaticamente. Para execu&ccedil;&atilde;o manual ou em produ&ccedil;&atilde;o:

| Vari&aacute;vel / Setting | Descri&ccedil;&atilde;o | Exemplo |
|---|---|---|
| `ConnectionStrings:Default` | PostgreSQL | `Host=localhost;Database=fcgpayments-db;...` |
| `ConnectionStrings:rabbitmq` | RabbitMQ | `amqp://guest:guest@localhost:5672` |
| `JwtSettings:Issuer` | Emissor do token JWT | `FcgPayments-Issuer` |
| `JwtSettings:SecurityKey` | Chave de assinatura do JWT | `FcgPayments_Secret_Key_2026_...` |
| `PaymentSimulation:ApprovalRate` | Taxa de aprova&ccedil;&atilde;o (0.0 a 1.0) | `0.9` (90% aprovados) |
| `MassTransit:RetryLimit` | Tentativas de retry do consumer | `5` |
