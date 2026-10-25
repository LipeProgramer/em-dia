# Em Dia 📅

> **Radar de vencimentos para adultos com TDAH**  
> Documentos, veículo, casa, saúde, contas e garantias — tudo em um lugar, com avisos antecipados.

Trabalho Multidisciplinar · UX + Tópicos Especiais · UNICV – Engenharia de Software  
**Entrega 3** | Outubro de 2026

| Integrante | RA | Responsabilidade principal |
|---|---|---|
| Felipe Moreira dos Santos | 443777-2026 | API .NET, banco, autenticação, testes da API |
| Felipe Nadab Goldberg Barbosa | 156888-2023 | Frontend React, Figma, acessibilidade, testes de usabilidade |

---

## Tecnologias

| Parte | Tecnologia |
|---|---|
| API | .NET 10 · ASP.NET Core Web API |
| Banco | PostgreSQL 17 · Entity Framework Core · Npgsql |
| Autenticação | JWT Bearer · BCrypt · Rate Limiter |
| Documentação | Swashbuckle / OpenAPI (`/swagger`) |
| E-mail | MailKit (SMTP) |
| Testes da API | xUnit · WebApplicationFactory · Testcontainers |
| Frontend | React 19 · TypeScript · Vite |
| Front: dados | axios · TanStack Query · React Hook Form · Zod |
| Testes do front | Vitest · Testing Library |

## Estrutura do repositório

```
em-dia/
├── api/                    # Solução .NET (EmDia.sln)
│   ├── src/
│   │   ├── EmDia.Api/          # Controllers, Middlewares, Program.cs
│   │   ├── EmDia.Application/  # DTOs, Services, Validators, Interfaces
│   │   ├── EmDia.Domain/       # Entidades, Regras de negócio, Enums, Exceções
│   │   └── EmDia.Infrastructure/ # EF Core, Repositories, Email, Worker
│   └── tests/
│       ├── EmDia.Domain.Tests/          # Testes unitários (U01–U15)
│       └── EmDia.Api.IntegrationTests/  # Testes de integração (CT01–CT51)
├── web/                    # Frontend React + TypeScript (Vite)
├── docs/                   # Documentação complementar
├── docker-compose.yml      # PostgreSQL local
├── .env.example            # Variáveis de ambiente necessárias
└── .gitignore
```

## Como executar (desenvolvimento local)

### Pré-requisitos
- [.NET 10 SDK](https://dot.net/v1/dotnet-install.ps1) (`dotnet --version` deve mostrar `10.x`)
- [Node.js 20+](https://nodejs.org)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### 1. Banco de dados
```bash
docker compose up -d
```

### 2. Segredos da API (nunca no código)
```bash
cd api
dotnet user-secrets set "Jwt:Key" "sua-chave-aqui-minimo-256-bits" --project src/EmDia.Api
dotnet user-secrets set "Email:Password" "sua-senha-smtp" --project src/EmDia.Api
```

### 3. Migrations e seed
```bash
dotnet ef database update --project src/EmDia.Infrastructure --startup-project src/EmDia.Api
```

### 4. API
```powershell
# Em um terminal separado:
cd api
dotnet run --project src/EmDia.Api
# Swagger: http://localhost:5196/swagger
```

### 5. Frontend
```powershell
# Em outro terminal:
cd web
npm install
npm run dev
# http://localhost:5173
```

### 6. Testes
```powershell
# Unitários (Domain)
cd api
dotnet test tests/EmDia.Domain.Tests

# Integração (requer Docker rodando)
dotnet test tests/EmDia.Api.IntegrationTests

# Frontend
cd web
npm test -- --run
```

## Documentação complementar

| Arquivo | Conteúdo |
|---|---|
| [`docs/desvios.md`](docs/desvios.md) | Decisões que se afastam da Entrega 2 |
| [`docs/testes.md`](docs/testes.md) | Resultado de cada caso de teste (U01–U15, CT01–CT55) |
| [`docs/bugs.md`](docs/bugs.md) | Bugs encontrados, causas e correções |
| `api/src/EmDia.Api` → `/swagger` | Documentação interativa da API |

---

*Especificação completa: `docs/Entrega2.pdf` e `docs/Entrega1.pdf`.*
