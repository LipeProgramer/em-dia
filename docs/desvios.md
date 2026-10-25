# Desvios da Especificação

Registro de toda decisão que se afasta ou complementa a Entrega 2.
Conforme instrução do projeto: _o quê_, _por quê_ e o _commit/branch_ onde foi aplicado.

---

## DV01 — Versão do .NET: 8 → 10 (instalação manual)

| Campo | Valor |
|---|---|
| **O quê** | O projeto foi scaffolded com `net8.0`; a spec exige `.NET 10`. |
| **Por quê** | O SDK .NET 10.0.402 não estava na PATH padrão. Foi instalado via script oficial da Microsoft em `%LOCALAPPDATA%\Microsoft\dotnet10` durante a Fase 0. Todos os `.csproj` foram atualizados para `net10.0`. |
| **Branch** | `feature/fase-0-ambiente` |
| **Impacto** | Nenhum nos requisitos funcionais. |

---

## DV02 — Swagger: Swashbuckle 7.x (não o nativo do .NET 10)

| Campo | Valor |
|---|---|
| **O quê** | A spec cita "Swashbuckle / OpenAPI". O .NET 9/10 introduziu o `Microsoft.AspNetCore.OpenApi` nativo, mas sem UI interativa embutida. |
| **Por quê** | O Swashbuckle 7.x (`Swashbuckle.AspNetCore 7.3.x`) tem suporte oficial para .NET 10 e mantém a UI `/swagger` idêntica à citada na spec. A alternativa nativa exigiria Scalar UI como pacote extra. Swashbuckle foi escolhido por minimizar o delta com a especificação. |
| **Branch** | `feature/fase-0-ambiente` |
| **Impacto** | Nenhum nos requisitos funcionais. O endpoint `/swagger` funciona igual. |

---

## DV03 — Campo `novaDataVencimento` no E18 (Concluir)

| Campo | Valor |
|---|---|
| **O quê** | A spec menciona `novaDataVencimento` nas validações do E18 mas não inclui no corpo de exemplo. |
| **Por quê** | A RN05 diz "Data Única: se vier nova validade, é atualizada". O campo é necessário para esse caso. Será incluído como campo **opcional** no DTO `ConcluirItemRequest` somente quando `tipoVencimento == DataUnica`. |
| **Branch** | A ser implementado na Fase 4. |
| **Impacto** | Nenhum; complementa a spec em vez de contradizê-la. |

---

## DV04 — Senha em `appsettings.Development.json` (desenvolvimento local)

| Campo | Valor |
|---|---|
| **O quê** | A string de conexão em `appsettings.Development.json` contém a senha do banco de dev (`emdia_dev`). |
| **Por quê** | É a mesma senha definida no `docker-compose.yml` (já existente no repo). Em dev local, sem dados reais, o risco é aceitável e facilita o onboarding. Em produção, a senha vem de variável de ambiente. |
| **Branch** | `feature/fase-0-ambiente` |
| **Impacto** | Zero em produção; para dev local, esperado. |

---

## DV05 — Autenticação Npgsql 9.x com PostgreSQL 17: uso de `psql` interno para migrations

| Campo | Valor |
|---|---|
| **O quê** | O `dotnet ef database update` falha com `28P01` mesmo com `trust` no pg_hba. O Npgsql 9.x inicia SASL/SCRAM antes de receber a resposta do pg_hba. |
| **Por quê** | Incompatibilidade entre Npgsql 9.x e PostgreSQL 17 no Windows com Docker. O método usado: `dotnet ef migrations script --output migration.sql` gera o SQL idempotente, que é copiado via `docker cp` e executado com `psql` dentro do container (sem rede). O banco resultante é idêntico ao que o `dotnet ef database update` produziria. |
| **Branch** | `feature/fase-2-infrastructure` |
| **Impacto** | Apenas o processo de apply de migrations em dev. A Api em runtime usa a mesma connection string com `SSL Mode=Disable` e funciona normalmente pois o servidor aceita a senha corretamente via conexão de aplicação. |

---

## DV06 — `docker-compose.yml` com `POSTGRES_HOST_AUTH_METHOD=md5` e `POSTGRES_INITDB_ARGS`

| Campo | Valor |
|---|---|
| **O quê** | O `docker-compose.yml` original não tinha configurações de autenticação explícitas. |
| **Por quê** | Para garantir compatibilidade com Npgsql 9.x em dev, foram adicionados `POSTGRES_HOST_AUTH_METHOD=md5` e `POSTGRES_INITDB_ARGS="--auth-host=md5"`. Em produção, scram-sha-256 é preferível. |
| **Branch** | `feature/fase-2-infrastructure` |
| **Impacto** | Apenas desenvolvimento local. |

---

## DV07 — TypeScript 6 com `erasableSyntaxOnly` no Frontend

| Campo | Valor |
|---|---|
| **O quê** | Enums TypeScript convencionais substituídos por objetos `as const` e union types. |
| **Por quê** | Vite / TS 6 configurado com `erasableSyntaxOnly: true` rejeita enums com código em tempo de execução. O uso de `const StatusItem = { ... } as const` e `type StatusItem = typeof StatusItem[keyof typeof StatusItem]` garante tipos estritos sem violar o padrão de apagamento de tipos. |
| **Branch** | `feature/fase-6-frontend` |
| **Impacto** | Nenhum; total compatibilidade com os tipos da API .NET. |

---

## DV08 — Execução condicional de BackgroundService no ambiente de testes

| Campo | Valor |
|---|---|
| **O quê** | `AvisosWorker` registrado como Singleton e adicionado a `AddHostedService` apenas quando `!Environment.IsEnvironment("Testing")`. |
| **Por quê** | Evita que workers autônomos em background concorram com fixtures de Testcontainers durante a execução concorrente da suíte de testes xUnit, ao mesmo tempo permitindo que testes invoquem `AvisosWorker.ExecutarAgoraAsync()` sob demanda de forma determinística. |
| **Branch** | `feature/fase-7-avisos-worker` |
| **Impacto** | Aumenta o determinismo e velocidade dos testes de integração. |
