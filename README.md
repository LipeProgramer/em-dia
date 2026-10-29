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
├── docs/             desvios, testes, bugs
└── docker-compose.yml  PostgreSQL local (porta 5433)
```

> A pasta `mobile/` traz o app em React Native + Expo (extra), que roda no **Expo Go** e usa a mesma API. Como rodar no celular: [`mobile/README.md`](mobile/README.md).

## Como executar (desenvolvimento local)

### Pré-requisitos
- [.NET 10 SDK](https://dot.net)
- [Node.js 22+](https://nodejs.org)
- Docker Desktop (para o banco local e os testes de integração) **ou** um PostgreSQL 17 na porta 5433

### 1. Banco de dados
```powershell
docker compose up -d
```

### 2. Segredos da API (nunca no código — spec §4.2 e §4.6)
```powershell
cd api
dotnet user-secrets set "Jwt:Key" "<uma chave aleatória com pelo menos 32 caracteres>" --project src/EmDia.Api
# Opcional, para enviar e-mails de verdade (sem isso, os avisos só são registrados em log):
dotnet user-secrets set "Email:Smtp:Host" "smtp.seuprovedor.com" --project src/EmDia.Api
dotnet user-secrets set "Email:Smtp:Usuario" "usuario" --project src/EmDia.Api
dotnet user-secrets set "Email:Smtp:Senha" "senha" --project src/EmDia.Api
```

### 3. Migrations e dados iniciais
```powershell
dotnet tool restore
dotnet ef database update --project src/EmDia.Infrastructure --startup-project src/EmDia.Api
```

### 4. API
```powershell
dotnet run --project src/EmDia.Api --launch-profile http
# Swagger: http://localhost:5196/swagger
```

### 5. Frontend
```powershell
cd web
npm install
npm run dev
# http://localhost:5173 (as chamadas /api vão para a API pelo proxy do Vite)
```

### 6. Testes
```powershell
# Unitários (Domain) — com cobertura
cd api
dotnet test tests/EmDia.Domain.Tests --collect:"XPlat Code Coverage"

# Integração — usa Testcontainers (Docker). Sem Docker, aponte para um PostgreSQL de testes
# (o banco informado é APAGADO e recriado):
#   $env:EMDIA_TEST_DB = "Host=localhost;Port=5433;Database=emdia_test;Username=emdia;Password=emdia_dev"
dotnet test tests/EmDia.Api.IntegrationTests

# Frontend
cd ../web
npx vitest run
```

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
