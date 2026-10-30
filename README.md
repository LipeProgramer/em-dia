# Em Dia 📅

> **Radar de vencimentos para adultos com TDAH**
> Documentos, veículo, casa, saúde, contas e garantias — tudo em um lugar, com avisos antecipados.

Trabalho Multidisciplinar · UX + Tópicos Especiais · UNICV – Engenharia de Software
Implementação da **Entrega 2 – Especificação técnica da solução** | Outubro de 2026

| Integrante | RA | Responsabilidade principal |
|---|---|---|
| Felipe Moreira dos Santos | 443777-2026 | API .NET, banco, autenticação, testes da API |
| Felipe Nadab Goldberg Barbosa | 156888-2023 | Frontend React, Figma, acessibilidade, testes de usabilidade |

---

## Tecnologias (spec §1.7)

| Parte | Tecnologia |
|---|---|
| API | .NET 10 (LTS) · ASP.NET Core Web API |
| Dados | Entity Framework Core 10 + Npgsql · PostgreSQL 17 |
| Validação | FluentValidation |
| Segurança | JWT Bearer (HS256) · BCrypt (custo 12) · Rate Limiter do ASP.NET Core |
| Documentação | Swagger / OpenAPI (`/swagger`) |
| E-mail | MailKit (SMTP) |
| Testes da API | xUnit · WebApplicationFactory · Testcontainers (PostgreSQL) |
| Frontend | React 19 · TypeScript · Vite · React Router |
| Front: dados e formulários | axios · TanStack Query · React Hook Form · Zod |
| Testes do front | Vitest · Testing Library |
| Testes de ponta a ponta | Cypress (pasta `e2e/`) |
| Publicação e CI | Docker · GitHub Actions · Vercel (site) · Render (API) · Neon (PostgreSQL) |

## Estrutura (spec §1.3)

```
em-dia/
├── api/                          Solução .NET (EmDia.sln)
│   ├── src/
│   │   ├── EmDia.Api/            Controllers, Middlewares (ProblemDetails), Extensions (JWT, CORS, rate limit, Swagger)
│   │   ├── EmDia.Application/    DTOs, Services, Validators (FluentValidation), Interfaces
│   │   ├── EmDia.Domain/         Entities, Enums, Exceptions, regras RN01–RN15
│   │   └── EmDia.Infrastructure/ Data (DbContext, Configurations, Migrations, Seed), Repositories, Security, Email, Jobs
│   └── tests/
│       ├── EmDia.Domain.Tests/          Testes de unidade (U01–U15 + complementares)
│       └── EmDia.Api.IntegrationTests/  Testes de integração (CT01–CT46)
├── web/src/
│   ├── api/          client.ts (axios + interceptors), endpoints.ts, types.ts
│   ├── features/     auth, checkup, radar, itens, veiculos, historico, perfil
│   ├── components/   Button, Field, StatusBadge, Dialog, BottomNav (AppLayout)…
│   ├── routes/       AppRoutes, PrivateRoute, PublicRoute, CheckupGate
│   ├── styles/       tokens.css, app.css
│   └── test/         CT47–CT51 e validações (Vitest)
├── e2e/              Testes de ponta a ponta com Cypress
├── docs/             desvios, testes, bugs
└── docker-compose.yml  PostgreSQL local (porta 5433)
```

> A pasta `mobile/` traz o app em React Native + Expo (extra), que roda no **Expo Go** e usa a mesma API. Como rodar no celular: [`mobile/README.md`](mobile/README.md).

## Instalar em outro PC (Windows)

> **Só quer usar o sistema?** Não precisa instalar nada: abra <https://em-dia-projeto-facul.vercel.app>.
> O passo a passo abaixo é para rodar e alterar o código no seu computador. Essa cópia usa um **banco próprio, que começa
> vazio**: contas e itens do site online não aparecem nela (e vice-versa).

Faça tudo no **PowerShell**. Na primeira vez leva uns 20 minutos, a maior parte baixando programas.

### 1. Instalar os programas

Pule o que já tiver instalado:

```powershell
winget install --id Git.Git -e
winget install --id Microsoft.DotNet.SDK.10 -e
winget install --id OpenJS.NodeJS.LTS -e
winget install --id Docker.DockerDesktop -e
```

Depois, **reinicie o computador** e **abra o Docker Desktop** uma vez, até ele mostrar que está rodando. Em um PowerShell
novo, confira as versões (.NET 10 e Node 22 ou mais novo):

```powershell
git --version
dotnet --version
node --version
docker --version
```

### 2. Baixar o projeto

```powershell
git clone https://github.com/felipemsantoss/em-dia.git
cd em-dia
```

### 3. Subir o banco de dados

Com o Docker Desktop aberto, na pasta `em-dia`:

```powershell
docker compose up -d
```

Isso cria um PostgreSQL na porta 5433, só para este computador. Os dados ficam guardados entre um uso e outro.

### 4. Criar a chave de login (só na primeira vez)

Cada computador tem a sua chave, e ela nunca vai para o GitHub. Os três comandos criam uma chave aleatória e a guardam
nos segredos do .NET:

```powershell
$bytes = New-Object byte[] 48
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
dotnet user-secrets set "Jwt:Key" ([Convert]::ToBase64String($bytes)) --project api/src/EmDia.Api
```

### 5. Criar as tabelas

```powershell
cd api
dotnet restore
dotnet tool restore
dotnet ef database update --project src/EmDia.Infrastructure --startup-project src/EmDia.Api
```

Isso cria as tabelas e já coloca as categorias, os modelos prontos e as perguntas do check-up.

### 6. Ligar a API

Ainda na pasta `api`. Deixe esta janela aberta enquanto usa o sistema:

```powershell
dotnet run --project src/EmDia.Api --launch-profile http
```

Espere aparecer `Now listening on`. Para conferir, abra o Swagger em <http://localhost:5196/swagger>.

### 7. Ligar o site

Abra **outro PowerShell**, na pasta `em-dia`, e deixe também aberto:

```powershell
cd web
npm install
npm run dev
```

### 8. Testar

Abra <http://localhost:5173> e clique em **Criar conta**. Se o check-up abrir depois do cadastro, está tudo funcionando.

### Nas próximas vezes

Abra o Docker Desktop e use dois PowerShell, a partir da pasta `em-dia`:

```powershell
docker compose up -d
cd api
dotnet run --project src/EmDia.Api --launch-profile http
```

```powershell
cd web
npm run dev
```

Para pegar a versão mais nova do GitHub, na pasta `em-dia`:

```powershell
git pull
cd api
dotnet restore
dotnet ef database update --project src/EmDia.Infrastructure --startup-project src/EmDia.Api
cd ../web
npm install
```

### Se der erro

| O que aparece | O que fazer |
|---|---|
| `winget`, `dotnet`, `node` ou `git` "não é reconhecido" | Feche e abra o PowerShell. Se continuar, reinicie o computador. |
| `npm` diz que a execução de scripts foi desabilitada | Rode `Set-ExecutionPolicy -Scope CurrentUser RemoteSigned` e abra o PowerShell de novo. |
| `docker compose` não conecta ou reclama do "daemon" | O Docker Desktop não está aberto. Abra e espere ele terminar de iniciar. |
| A porta 5433 já está em uso | Outro PostgreSQL está nessa porta. Pare-o, ou troque a porta no `docker-compose.yml` e em `api/src/EmDia.Api/appsettings.Development.json`. |
| A API para com "Jwt:Key ausente ou curta" | Refaça o passo 4. |
| A API fala que não conseguiu conectar ao banco | Confira o passo 3 e se o Docker Desktop está aberto. |
| O site mostra "Algo deu errado" ou "Sem conexão" | A API não está ligada: confira a janela do passo 6. |

### Opcional

**E-mail de verdade.** Sem isto, os avisos e o link de "Esqueci minha senha" só aparecem na janela da API. Em
`api`, com os dados do [Brevo](docs/deploy.md#e-mail-smtp) ou de outro serviço:

```powershell
dotnet user-secrets set "Email:Smtp:Host" "smtp-relay.brevo.com" --project src/EmDia.Api
dotnet user-secrets set "Email:Smtp:Usuario" "seu-login-smtp" --project src/EmDia.Api
dotnet user-secrets set "Email:Smtp:Senha" "sua-chave-smtp" --project src/EmDia.Api
dotnet user-secrets set "Email:Remetente" "seu-remetente-verificado" --project src/EmDia.Api
```

**App no celular (Expo Go).** Veja [`mobile/README.md`](mobile/README.md).

**Rodar os testes:**

```powershell
# Na pasta api. Unitários (Domain), com cobertura
dotnet test tests/EmDia.Domain.Tests --collect:"XPlat Code Coverage"

# Na pasta api. Integração: usa Testcontainers (Docker). Sem Docker, aponte para um PostgreSQL de testes
# (o banco informado é APAGADO e recriado):
#   $env:EMDIA_TEST_DB = "Host=localhost;Port=5433;Database=emdia_test;Username=emdia;Password=emdia_dev"
dotnet test tests/EmDia.Api.IntegrationTests

# Na pasta web
npx vitest run
```

**Ver o sistema sendo testado sozinho (Cypress):** veja [`e2e/README.md`](e2e/README.md). O Cypress abre o site e faz o
caminho inteiro, do cadastro à exclusão da conta, e grava um vídeo.

## Publicar online

Site no **Vercel**, API no **Render** (Docker) e banco no **Neon**, todos no plano gratuito.
Passo a passo em [docs/deploy.md](docs/deploy.md) (arquivos usados: `render.yaml`, `api/Dockerfile` e `web/vercel.json`).

## Rotas principais

| API (prefixo `/api/v1`) | Tela |
|---|---|
| `POST /auth/registrar` · `/auth/login` · `/auth/refresh` · `/auth/logout` | `/cadastro`, `/entrar` |
| `GET /usuarios/me` · `PUT /usuarios/me/preferencias` · `DELETE /usuarios/me` | `/perfil` |
| `GET /categorias` · `/modelos` · `/checkup/perguntas` · `POST /checkup` | `/checkup`, `/checkup/dados`, `/checkup/resultado` |
| `GET /radar?foco=` | `/`, `/?foco=1` |
| `GET/POST /itens` · `GET/PUT/DELETE /itens/{id}` · `POST /itens/{id}/concluir` · `PATCH /itens/{id}/passos/{passoId}` | `/itens`, `/itens/novo`, `/itens/:id`, `/itens/:id/editar` |
| `GET/POST /veiculos` · `PUT/DELETE /veiculos/{id}` · `PATCH /veiculos/{id}/km` | `/perfil/veiculos` |
| `GET /historico` | `/historico` |

## Documentação complementar

| Arquivo | Conteúdo |
|---|---|
| [`docs/desvios.md`](docs/desvios.md) | Decisões que se afastam ou complementam a Entrega 2 |
| [`docs/testes.md`](docs/testes.md) | Resultado de cada caso de teste (U01–U15, CT01–CT51) e cobertura |
| [`docs/bugs.md`](docs/bugs.md) | Bugs encontrados, causas e correções |
| [`docs/deploy.md`](docs/deploy.md) | Publicação online: Vercel, Render e Neon |
| [`docs/roteiro-demonstracao.md`](docs/roteiro-demonstracao.md) | Roteiro da demonstração da Entrega 4 e perguntas prováveis da banca |
| [`docs/Em_Dia_Entrega3_Relatorio_Final.pdf`](docs/Em_Dia_Entrega3_Relatorio_Final.pdf) | Relatório da Entrega 3 (também em `.md` e `.html`) |
| `/swagger` | Documentação interativa da API |
