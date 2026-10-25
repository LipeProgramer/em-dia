# Registro de Bugs — Em Dia

> Cada bug tem: como apareceu, causa raiz, correção aplicada e commit/branch.

---

## BUG-01 — Npgsql 9.x falha autenticação com PostgreSQL 17 via rede (SCRAM/SASL)

| Campo | Valor |
|---|---|
| **Como apareceu** | `dotnet ef database update` lança `28P01: autenticação do tipo senha falhou` mesmo com `trust` no pg_hba |
| **Fase** | Fase 2 |
| **Branch** | `feature/fase-2-infrastructure` |
| **Causa raiz** | O Npgsql 9.x inicia o handshake SASL (SCRAM-SHA-256) antes de receber a resposta do pg_hba do PostgreSQL 17. Mesmo com `trust` configurado, o servidor anuncia SCRAM como mecanismo preferencial e o Npgsql tenta autenticar com ele. A senha do usuário `emdia` foi armazenada com `scram-sha-256` (padrão do PostgreSQL 17), causando falha na autenticação via Npgsql mesmo que a connection string tenha `Password`. |
| **Correção** | Workaround: usar `dotnet ef migrations script` para gerar SQL e aplicar via `docker cp + psql` dentro do container (Unix socket, sem autenticação SCRAM). Registrado em `docs/desvios.md` como DV05. |
| **Status** | ⚠️ Contornado em dev — não afeta runtime da aplicação |

---

## BUG-02 — `appsettings.json` ausente no projeto Infrastructure

| Campo | Valor |
|---|---|
| **Como apareceu** | `EmDiaDbContextFactory` tenta ler `appsettings.json` de dois diretórios; em desenvolvimento, o arquivo não existia na pasta procurada |
| **Fase** | Fase 2 |
| **Branch** | `feature/fase-2-infrastructure` |
| **Causa raiz** | O path calculado por `AppContext.BaseDirectory` no design-time aponta para `bin/Debug/net10.0/` e a busca por `../../../../EmDia.Api` não encontra o diretório no ambiente de desenvolvimento do dev |
| **Correção** | A factory tem fallback para a connection string hardcoded com `SSL Mode=Disable`. O `appsettings.Development.json` será criado no projeto `EmDia.Api` na Fase 3. |
| **Status** | ✅ Contornado via fallback |
