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
| **Por quê** | A RN05 diz "Data única: se vier nova validade, é atualizada". O campo é necessário para esse caso. Será incluído como campo **opcional** no DTO `ConcluirItemRequest` somente quando `tipoVencimento == DataUnica`. |
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
