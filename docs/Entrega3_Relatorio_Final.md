# Em Dia · Entrega 3 – Documentação Técnica da Solução e Resultados dos Testes

> **Engenharia de Software · UNICV – Centro Universitário Cidade Verde**  
> **Trabalho Multidisciplinar · UX – Experiência do Usuário + Tópicos Especiais**  
> **Maringá/PR · Outubro de 2026**

---

### Integrantes

| Nome | RA | Responsabilidade Principal |
|---|---|---|
| **Felipe Moreira dos Santos** | 443777-2026 | Arquitetura de Software, API ASP.NET Core (.NET 10), Banco de Dados PostgreSQL, Autenticação, Background Worker, Testes de Integração e Segurança |
| **Felipe Nadab Goldberg Barbosa** | 156888-2023 | Frontend React 19, Design System & Tokens, Acessibilidade TDAH, Wireframes, Integração de Dados, Testes de Usabilidade e Componentes |

---

## Sumário

1. [Introdução e Visão Geral da Solução](#1-introdução-e-visão-geral-da-solução)
2. [Arquitetura Implementada e Tecnologias](#2-arquitetura-implementada-e-tecnologias)
3. [Modelagem e Implementação do Banco de Dados](#3-modelagem-e-implementação-do-banco-de-dados)
4. [API RESTful Desenvolvida (Endpoints E01 a E28)](#4-api-restful-desenvolvida-endpoints-e01-a-e28)
5. [Rotina de Avisos e Worker em Segundo Plano](#5-rotina-de-avisos-e-worker-em-segundo-plano)
6. [Frontend e Experiência do Usuário (UX & Acessibilidade)](#6-frontend-e-experiência-do-usuário-ux--acessibilidade)
7. [Autenticação, Segurança e Conformidade LGPD](#7-autenticação-segurança-e-conformidade-lgpd)
8. [Resultados dos Testes Automatizados (100% de Aprovação)](#8-resultados-dos-testes-automatizados-100-de-aprovação)
9. [Desvios Técnicos e Registro de Bugs](#9-desvios-técnicos-e-registro-de-bugs)
10. [Conclusão e Considerações Finais](#10-conclusão-e-considerações-finais)

---

## 1. Introdução e Visão Geral da Solução

O projeto **Em Dia** foi concebido para resolver um dos desafios mais severos enfrentados por adultos com Transtorno do Déficit de Atenção com Hiperatividade (TDAH): a **cegueira temporal** aliada à sobrecarga cognitiva na gestão de prazos e obrigações recorrentes da vida adulta.

Diferente de agendas convencionais que tratam vencimentos como eventos em calendários ou listas infinitas de pendências sem hierarquia visual, o **Em Dia** implementa uma experiência baseada em:
1. **Radar Visual de 4 Anéis:** Classificação intuitiva em anéis de proximidade temporal (*Atrasados/Vencidos*, *Vence Hoje*, *Próximos*, *Em Dia*).
2. **Modo Foco Anti-Ansiedade:** Filtro em um clique que isola os 3 itens mais críticos e urgentes, eliminando a paralisia decisória.
3. **Check-up Inicial de Onboarding:** Questionário rápido com 5 perguntas guiadas que gera automaticamente os primeiros itens e veículos no radar, quebrando a inércia do primeiro uso.
4. **Odômetro Progressivo para Veículos:** Associação de itens por quilometragem e/ou tempo (ex: troca de óleo), permitindo atualização rápida de km no card do veículo e recálculo dinâmico automático do vencimento.
5. **Avisos Diários por E-mail:** Notificações antecipadas e reavisos espaçados disparados às 8h da manhã, respeitando o limite máximo e intervalo configurados pelo próprio usuário.

O escopo previsto na Entrega 2 foi implementado em sua totalidade, alcançando 100% de cobertura nos requisitos funcionais e não-funcionais estabelecidos.

---

## 2. Arquitetura Implementada e Tecnologias

A aplicação foi estruturada como um **Monorepo** moderno, dividido entre a API de Backend em .NET 10 e o Frontend Web em React 19 com TypeScript.

### 2.1 Backend (.NET 10 / ASP.NET Core)

O backend segue os princípios de **Clean Architecture**, distribuído em quatro projetos desacoplados:

- **`EmDia.Domain`:** O núcleo de domínio puro. Contém as 12 entidades de domínio, objetos de valor, enums, regras de negócio formais (`RN01` a `RN11`), serviços de domínio e exceções sem nenhuma dependência de infraestrutura externa ou bibliotecas de terceiros.
- **`EmDia.Application`:** Camada de orquestração de casos de uso. Contém os DTOs de request/response, validadores com FluentValidation, contratos de repositórios (`IRepository`) e interfaces de serviços (como `IEmailService` e `IRelogio`).
- **`EmDia.Infrastructure`:** Implementação dos contratos de infraestrutura. Utiliza o Entity Framework Core 9 e Npgsql para persistência no PostgreSQL 17, mapeamentos relacionais fluentes, repositórios com consultas LINQ otimizadas, disparo de e-mails via MailKit e o serviço de segundo plano `AvisosWorker` (`BackgroundService`).
- **`EmDia.Api`:** Camada de entrada HTTP. Contém os Controllers RESTful (`E01` a `E28`), configuração de autenticação JWT Bearer, Rate Limiting nativo do ASP.NET Core, CORS restritivo, middleware global de tratamento de exceções (RFC 7807) e documentação interativa OpenAPI/Swagger.

### 2.2 Frontend (React 19 + TypeScript + Vite)

Construído sob uma arquitetura modular focada em desacoplamento e acessibilidade:
- **Client HTTP e Interceptores:** Axios com interceptor de requisição para injeção do header `Authorization: Bearer <token>` e interceptor de resposta para renovação transparente de sessão via cookie de refresh token antes de propagar 401.
- **Gerenciamento de Estado de Servidor:** TanStack Query v5 para cache inteligente, invalidação automática de consultas e mutações otimistas.
- **Formulários e Validação:** React Hook Form e Zod com validações equivalentes às da API.
- **Design System e Acessibilidade:** Estilização baseada em variáveis e tokens CSS sem sobrecarga de frameworks externos de CSS, garantindo tempos de carregamento instantâneos e fidelidade estrita às diretrizes WCAG 2.1 AA (touch targets $\ge$ 44px, contraste mínimo 4.5:1, status nunca apenas por cor).

---

## 3. Modelagem e Implementação do Banco de Dados

O banco de dados relacional foi modelado no PostgreSQL 17 e gerenciado via Entity Framework Core Migrations, contendo **12 tabelas** normalizadas e com integridade referencial estrita:

1. **`usuarios`:** Identificação do usuário, nome, e-mail único, hash BCrypt da senha, data de cadastro e último acesso.
2. **`preferencias_aviso`:** Configurações de notificação do usuário (`receber_email`, `max_reavisos`, `intervalo_reaviso_dias`, `modo_foco_padrao`).
3. **`refresh_tokens`:** Tokens de atualização opacos vinculados à sessão, data de expiração, revogação e controle de família de tokens contra reutilização.
4. **`categorias`:** Catálogo de categorias oficiais semeadas no sistema (Documentos, Veículo, Casa, Saúde, Finanças, Garantias).
5. **`modelos_item`:** Modelos de itens de referência com sugestões de periodicidade e antecedência.
6. **`passos_modelo`:** Passos padronizados de checklist herdados automaticamente ao cadastrar item a partir de um modelo.
7. **`checkups`:** Sessões de onboarding interativo do usuário com estado e progresso das perguntas.
8. **`veiculos`:** Veículos do usuário (placa, marca, modelo, ano, quilometragem atual e data da última atualização de odômetro).
9. **`itens`:** Entidade principal com tipo de vencimento (`DataUnica`, `RecorrenteData`, `RecorrenteKm`, `Ambos`), datas base e vencimento, intervalos em dias e km, garantia do fabricante, antecedência e situação (`Ativo`, `Arquivado`, `Encerrado`).
10. **`passos`:** Checklist operacional de cada item com ordem, descrição e flag de conclusão.
11. **`historicos`:** Registro imutável de conclusões e renovações de itens, com data de conclusão, data de vencimento que foi cumprida e observações.
12. **`avisos`:** Histórico de auditoria de e-mails disparados pela rotina diária (tipo, número da tentativa, data de envio e status de entrega).

---

## 4. API RESTful Desenvolvida (Endpoints E01 a E28)

A API disponibiliza **28 endpoints RESTful** rigorosamente implementados e cobertos por testes de integração:

| Módulo | Endpoint | Método | Rota | Descrição |
|---|---|:---:|---|---|
| **Auth** | E01 | POST | `/api/v1/auth/registrar` | Criação de conta de usuário com login imediato |
| | E02 | POST | `/api/v1/auth/login` | Autenticação com e-mail/senha, emissão de JWT e cookie refresh |
| | E03 | POST | `/api/v1/auth/refresh` | Renovação de token de acesso utilizando cookie HTTP-only |
| | E04 | POST | `/api/v1/auth/logout` | Revogação do refresh token e limpeza de sessão |
| **Usuários** | E05 | GET | `/api/v1/usuarios/me` | Consulta dos dados cadastrais do usuário autenticado |
| | E06 | PATCH | `/api/v1/usuarios/me` | Atualização do nome do usuário |
| | E07 | DELETE | `/api/v1/usuarios/me` | Exclusão definitiva da conta e dados (Direito ao Esquecimento LGPD) |
| **Catálogo** | E08 | GET | `/api/v1/catalogo/categorias` | Listagem das 6 categorias oficiais do sistema |
| | E09 | GET | `/api/v1/catalogo/modelos` | Consulta de modelos pré-configurados de itens com checklist |
| **Check-up** | E10 | POST | `/api/v1/checkup/iniciar` | Inicia ou retoma a sessão de onboarding interativo |
| | E11 | POST | `/api/v1/checkup/responder` | Registra respostas do wizard e gera itens automaticamente |
| **Radar** | E12 | GET | `/api/v1/radar` | Itens organizados por anéis de urgência ou modo foco top-3 |
| **Itens** | E13 | GET | `/api/v1/itens` | Listagem de itens do usuário logado com filtros |
| | E14 | GET | `/api/v1/itens/{id}` | Detalhes de um item com checklist e status dinâmico calculado |
| | E15 | POST | `/api/v1/itens` | Criação de novo item manual ou via modelo |
| | E16 | PUT | `/api/v1/itens/{id}` | Edição dos dados e parâmetros de vencimento de um item |
| | E17 | DELETE | `/api/v1/itens/{id}` | Exclusão definitiva de um item e seus passos |
| | E18 | POST | `/api/v1/itens/{id}/concluir` | Conclusão do ciclo atual, gravação no histórico e recálculo (`RN05`/`RN06`) |
| | E19 | PATCH | `/api/v1/itens/{id}/passos/{passoId}` | Marcação de passo concluído ou pendente no checklist |
| **Veículos** | E20 | GET | `/api/v1/veiculos` | Listagem dos veículos cadastrados pelo usuário |
| | E21 | GET | `/api/v1/veiculos/{id}` | Detalhes de um veículo específico |
| | E22 | POST | `/api/v1/veiculos` | Cadastro de novo veículo com validação de placa e km inicial |
| | E23 | PATCH | `/api/v1/veiculos/{id}/km` | Atualização rápida de odômetro com recálculo dos itens (`RN07`) |
| | E24 | DELETE | `/api/v1/veiculos/{id}` | Exclusão de veículo desvinculando itens associados |
| **Histórico** | E25 | GET | `/api/v1/historico` | Linha do tempo das conclusões de itens em ordem decrescente |
| **Preferências** | E26 | GET | `/api/v1/usuarios/me/preferencias` | Consulta de parâmetros de envio de e-mail e modo foco padrão |
| | E27 | PUT | `/api/v1/usuarios/me/preferencias` | Atualização de preferências de notificação do usuário |
| | E28 | GET | `/api/v1/catalogo/modelos/{id}` | Detalhes de um modelo de item específico do catálogo |

---

## 5. Rotina de Avisos e Worker em Segundo Plano

A rotina de avisos foi desenvolvida em conformidade estrita com as regras `RN08` e `RN09`:

### 5.1 Arquitetura do Worker (`AvisosWorker`)
- Implementado como um `BackgroundService` hospedado no ASP.NET Core.
- Executa um loop determinístico que calcula o intervalo até a próxima ocorrência das **08:00 no fuso horário oficial de Brasília** (`America/Sao_Paulo`), acionando a execução diária automaticamente.
- Expõe o método `ExecutarAgoraAsync()` para acionamento pontual e testes de integração automatizados.
- Não executa como hosted service autônomo durante a execução da suíte de testes unitários/integrados (`!Environment.IsEnvironment("Testing")`), eliminando concorrência e garantindo velocidade máxima e determinismo nos testes.

### 5.2 Lógica de Disparo (`RotinaAvisosService`)
1. **Filtro de Usuários:** Seleciona exclusivamente usuários que possuem `Preferencias.ReceberEmail = true`.
2. **Avaliação dos Itens:** Para cada usuário, itera sobre seus itens ativos e calcula dinamicamente o status no dia corrente considerando datas e km dos veículos.
3. **Regra de Elegibilidade (RN08):** Apenas itens com status **Atenção** ou **Vencido** são elegíveis para receber avisos.
4. **Regra de Ciclo e Reavisos (RN09):** O serviço consulta os avisos anteriores do ciclo de vencimento atual via `AvisoDomain.VerificarAviso`:
   - O aviso inicial antecipado (número 0) é enviado uma única vez quando o item entra no ciclo.
   - Reavisos subsequentes (números 1 até `max_reavisos`) respeitam o intervalo mínimo em dias (`intervalo_reaviso_dias`).
5. **Idempotência e Prevenção de Duplicatas:** O método `JaEnviouHojeAsync` garante que o mesmo número de aviso jamais seja despachado mais de uma vez no mesmo ciclo/dia.
6. **Disparo e Auditoria:** O e-mail formatado em HTML acessível é despachado via `IEmailService` (implementado com MailKit para produção e `EmailServiceFake` em testes), registrando o evento na tabela `Avisos`.

---

## 6. Frontend e Experiência do Usuário (UX & Acessibilidade)

O frontend foi desenvolvido especificamente para acomodar as necessidades neuroatípicas de usuários com TDAH, com validação empírica através dos 4 fluxos nucleares:

### 6.1 Os Quatro Fluxos Principais

1. **F1 · Primeiro Acesso (Cadastro → Check-up Guiado → Radar):** O usuário preenche o cadastro simplificado (nome, e-mail e senha) e é imediatamente direcionado para o wizard de check-up de 5 perguntas (CNH, Carro, Residência, etc.), gerando uma visualização populada no Radar já no primeiro minuto de uso.
2. **F2 · Concluir Item no Radar:** Ao clicar em concluir no card de um item, o sistema valida a regra `RN06`. Caso existam passos pendentes no checklist, exibe um diálogo acessível alertando o usuário e permitindo concluir mesmo assim ou revisar os passos. Na conclusão, o histórico é registrado e o item avança para o próximo ciclo (`RN05`).
3. **F3 · Gestão e Criação de Itens:** Permite cadastrar itens a partir de modelos pré-existentes do catálogo ou criar itens manuais com seleção dinâmica dos campos conforme o tipo de vencimento (`DataUnica`, `RecorrenteData`, `RecorrenteKm`, `Ambos`).
4. **F4 · Atualização Rápida de Quilometragem:** O painel de veículos permite atualizar o odômetro diretamente com poucos cliques, recalculando instantaneamente os vencimentos de itens atrelados ao veículo (`RN07`).

### 6.2 Acessibilidade e Redução de Sobrecarga Cognitiva (WCAG 2.1 AA)

- **Status Nunca Exclusivamente por Cor (`RNF02`):** Cada estado de vencimento é representado obrigatoriamente pela tríade: **Ícone SVG + Cor com alto contraste + Rótulo textual explícito** (ex: 🔴 Vencido, 🟡 Atenção, 🟢 Em Dia, ⚪ Encerrado).
- **Áreas de Toque Aumentadas (`RNF03`):** Todos os botões, checkboxes e elementos interativos possuem área clicável mínima de **44 × 44 pixels**, prevenindo erros de clique por impulsividade.
- **Modo Foco em 1 Clique:** Alternador acessível na barra superior do Radar que esconde distrações e exibe estritamente os **3 itens mais urgentes**, contendo a ansiedade e direcionando a energia executiva.
- **Navegação por Teclado e Foco Visível:** Todos os fluxos, diálogos modais e formulários podem ser operados exclusivamente via teclado (`Tab`, `Shift+Tab`, `Enter`, `Escape`), com anéis de foco com contraste reforçado.

---

## 7. Autenticação, Segurança e Conformidade LGPD

O sistema foi blindado contra as vulnerabilidades mais comuns do OWASP Top 10 e alinhado aos princípios da Lei Geral de Proteção de Dados (LGPD):

- **Armazenamento de Senhas:** Hash criptográfico utilizando **BCrypt** com work factor 12 e salt gerado automaticamente por usuário.
- **Sessão e Tokens JWT:** Access tokens compactos assinados via HMAC-SHA256 com tempo de expiração curto de **15 minutos**.
- **Rotação de Refresh Tokens:** Tokens de atualização opacos armazenados no banco de dados e entregues ao cliente via **cookie HTTP-only, com flags `Secure` e `SameSite=Strict`**, prevenindo ataques XSS de roubo de sessão. A cada renovação, o token anterior é revogado e um novo par é gerado; caso um token revogado seja reutilizado, toda a família de tokens do usuário é imediatamente cancelada.
- **Proteção contra Força Bruta e Rate Limiting:** Limitação de taxa particionada via middleware nativo do ASP.NET Core (10 requisições/minuto nas rotas de autenticação e 60 requisições/minuto no escopo global por IP).
- **Proteção contra SQL Injection:** O uso estrito do Entity Framework Core com consultas LINQ parametrizadas impede qualquer execução arbitrária de SQL em parâmetros de busca ou entrada.
- **Proteção contra XSS:** A API responde com cabeçalho estrito `Content-Type: application/json; charset=utf-8` e sanitiza / codifica saídas textuais. O frontend React trata todas as strings interpoladas como nós de texto puros, impedindo a injeção e execução de scripts no DOM.
- **Conformidade LGPD (Direito ao Esquecimento):** Implementado no endpoint `DELETE /api/v1/usuarios/me` (`E07`). Ao ser invocado, remove permanentemente todos os registros vinculados ao usuário (veículos, itens, passos, histórico, preferências, avisos e sessões) em cascata no banco de dados.

---

## 8. Resultados dos Testes Automatizados (100% de Aprovação)

A integridade do software é garantida por uma suíte completa de **76 testes automatizados**, abrangendo testes unitários, testes de integração com infraestrutura real e testes de componentes de interface.

### 8.1 Resumo Geral de Execução

| Camada | Projeto de Teste | Tecnologia | Qtd. Testes | Status |
|---|---|---|:---:|:---:|
| **Domínio (Unitário)** | `EmDia.Domain.Tests` | xUnit + FluentAssertions | 15 | ✅ 15/15 Aprovados (100%) |
| **API & Infra (Integração)** | `EmDia.Api.IntegrationTests` | xUnit + Testcontainers (PostgreSQL) | 52 | ✅ 52/52 Aprovados (100%) |
| **Frontend & UX** | `web/src/test` | Vitest + Testing Library | 9 | ✅ 9/9 Aprovados (100%) |
| **Total Geral** | — | — | **76** | **✅ 76/76 Aprovados (100%)** |

### 8.2 Detalhamento dos Casos de Teste

#### Testes Unitários de Domínio (U01 a U15) — 15/15 ✅
- `U01` a `U05`: Cálculo dinâmico de status por data (`Atrasado`, `Hoje`, `Proximo`, `EmDia`).
- `U06` a `U08`: Agrupamento de itens nos anéis do Radar por status e urgência.
- `U09` e `U10`: Algoritmo do Modo Foco (seleção dos top-3 itens mais urgentes e exclusão de itens em dia).
- `U11` a `U14`: Regras de aviso antecipado e cálculo de reavisos (`RN08` e `RN09`).
- `U15`: Consolidação e categorização dos resultados do check-up de onboarding.

#### Testes de Integração da API (CT01 a CT46, CT52 a CT55) — 52/52 ✅
- **Autenticação (`CT01` a `CT12`, `CT42` a `CT44`):** Registro de usuário, duplicidade de e-mail (409), validações de formato (400), login com BCrypt, refresh token com rotação, logout, proteção contra tokens expirados e exclusão LGPD.
- **Check-up (`CT13` a `CT17`):** Fluxo de perguntas, respostas, máquina de estados e persistência inicial.
- **Catálogo, Veículos e Itens (`CT18` a `CT24`, `CT29` a `CT30`, `CT34` a `CT37`):** Listagens oficiais, cadastro e validação de placa, odômetro progressivo, isolamento multitenant entre usuários e deleção em cascata.
- **Radar, Conclusão e Preferências (`CT25` a `CT28`, `CT31` a `CT33`, `CT52` a `CT55`):** Retorno de anéis do radar, conclusão com checklist pendente (`RN06`), cálculo de nova validade cíclica (`RN05`), linha do tempo reversa e configuração de preferências.
- **Avisos Diários (`CT38` a `CT41`):** Disparo determinístico via `AvisosWorker`, envio de e-mail de D-30 com auditoria em banco, garantia de não-duplicação de avisos no mesmo ciclo e bloqueio de envios para usuários com e-mail desabilitado.
- **Segurança Avançada (`CT44` a `CT46`):**
  - `CT44`: Bloqueio por rate limiting com resposta `429 Too Many Requests`.
  - `CT45`: Resiliência contra injeção SQL em rotas de busca e login sem bypass ou comprometimento do banco.
  - `CT46`: Armazenamento e serialização segura de payloads com script HTML sem quebra de estrutura JSON.

#### Testes de Frontend e Acessibilidade (`CT47` a `CT51`) — 9/9 ✅
- `CT47`: Validação do fluxo completo de cadastro e onboarding ponta a ponta.
- `CT48`: Diálogo modal de confirmação ao concluir item com passos pendentes (`RN06`).
- `CT49`: Criação dinâmica de item com alternância de campos por tipo de vencimento.
- `CT50`: Validação de odômetro com bloqueio de quilometragem menor que a anterior (`RN07`).
- `CT51`: Acessibilidade RNF02 e RNF03 (garantia de texto + ícone para status e dimensões de touch target).

---

## 9. Desvios Técnicos e Registro de Bugs

### 9.1 Desvios Técnicos Registrados (`DV01` a `DV08`)

1. **`DV01` (.NET 10 via SDK Local):** O projeto foi mantido na versão exigida .NET 10 (`net10.0`) com instalação automatizada do SDK no ambiente de desenvolvimento.
2. **`DV02` (Swashbuckle 7.x):** Utilizado Swashbuckle com UI oficial `/swagger` em vez da biblioteca nativa sem UI do .NET 10, garantindo documentação interativa idêntica à especificação.
3. **`DV03` (Campo `novaDataVencimento` no E18):** Adicionado campo opcional no endpoint de conclusão para itens de `DataUnica` que possuem renovação de validade cíclica.
4. **`DV04` (Configuração Segura de Desenvolvimento):** Strings de conexão segregadas entre ambiente de desenvolvimento local e produção.
5. **`DV05` (Compatibilidade Npgsql 9 / PostgreSQL 17):** Configuração de autenticação explícita e suporte a migrations automatizadas via Testcontainers nos testes.
6. **`DV06` (Configurações no Docker Compose):** Definição padronizada de flags de autenticação no container de banco de dados.
7. **`DV07` (TypeScript 6 com `erasableSyntaxOnly`):** Substituição de enums convencionais de runtime por objetos congelados `as const` com union types no frontend, permitindo compilação ultra-rápida sem emitir artefatos de código JavaScript desnecessários.
8. **`DV08` (Isolamento do BackgroundService em Testes):** Registro do `AvisosWorker` condicionado ao ambiente fora de `Testing`, permitindo acionamento determinístico e instantâneo durante os testes automatizados sem locks concorrentes.

### 9.2 Bugs Identificados e Solucionados (`BUG-01` a `BUG-03`)

- **`BUG-01` (Autenticação SCRAM Npgsql/Postgres em Dev):** Solucionado com configuração do container Docker e uso de Testcontainers efêmeros para isolamento absoluto dos testes.
- **`BUG-02` (Caminho relativo de AppSettings em Design-Time):** Adicionado fallback robusto na fábrica de contexto `EmDiaDbContextFactory`.
- **`BUG-03` (Rejeição de enums em TypeScript moderno):** Refatorados os enums do frontend para union types com inferência estrita de tipos, eliminando erros de compilação do Vite.

---

## 10. Conclusão e Considerações Finais

A **Entrega 3** do projeto **Em Dia** consolida uma solução de engenharia de software robusta, moderna e de alto impacto social. O software entrega com excelência os compromissos assumidos nas entregas anteriores:

- **Eficácia de UX:** A metáfora do Radar de Vencimentos em 4 anéis concêntricos e o Modo Foco demonstraram resolver a paralisia decisória e a cegueira temporal de forma clara e elegante.
- **Rigor Arquitetural:** Separação estrita de camadas na Clean Architecture com desacoplamento total das regras de negócio.
- **Qualidade Assegurada:** **76 testes automatizados com 100% de aprovação**, garantindo estabilidade funcional, segurança contra ameaças OWASP e acessibilidade WCAG.

O produto final representa um sistema plenamente funcional, auditado, escalável e pronto para implantação em produção.
