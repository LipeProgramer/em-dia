# Plano e Resultados de Testes — Em Dia

> Documento gerado na Fase 2. Atualizado a cada fase.
> **Data de referência nos testes:** 15/10/2026 (IRelogio)

---

## Legenda

| Status | Significado |
|---|---|
| ✅ PASSOU | Teste executado e aprovado |
| ❌ FALHOU | Teste falhou — ver coluna Observações |
| ⏳ PENDENTE | Ainda não implementado |

---

## Resumo Geral de Execução

| Camada / Tipo | Total de Testes | Status |
|---|---|---|
| **Testes Unitários de Domínio (`EmDia.Domain.Tests`)** | 15 | ✅ 15/15 PASSOU |
| **Testes de Integração de API (`EmDia.Api.IntegrationTests`)** | 52 | ✅ 52/52 PASSOU |
| **Testes de Frontend React (`web/src/test`)** | 9 | ✅ 9/9 PASSOU |
| **Total Consolidado** | **76** | **✅ 76/76 PASSOU (100%)** |

---

## Testes Unitários — Domain (U01 a U15)

Executados em: `api/tests/EmDia.Domain.Tests`  
Fase: **1** | Branch: `feature/fase-1-domain` | Commit: `fa7e49b`

| ID | Descrição | Status | Observações |
|---|---|---|---|
| U01 | Item vencido há 1 dia → StatusItem.Atrasado | ✅ PASSOU | |
| U02 | Item vence hoje → StatusItem.Hoje | ✅ PASSOU | |
| U03 | Item vence em 5 dias → StatusItem.Proximo | ✅ PASSOU | |
| U04 | Item vence em 30 dias → StatusItem.EmDia | ✅ PASSOU | |
| U05 | Item sem vencimento → StatusItem.EmDia | ✅ PASSOU | |
| U06 | AnelRadar Vermelho para status Atrasado/Hoje | ✅ PASSOU | |
| U07 | AnelRadar Amarelo para status Proximo | ✅ PASSOU | |
| U08 | AnelRadar Verde para status EmDia | ✅ PASSOU | |
| U09 | Modo Foco: top-3 itens mais urgentes | ✅ PASSOU | |
| U10 | Modo Foco: exclui itens EmDia | ✅ PASSOU | |
| U11 | Aviso antecipado D-30 → numero=1 | ✅ PASSOU | |
| U12 | Reaviso D-7 → numero=2 | ✅ PASSOU | |
| U13 | Reaviso D-0 (dia) → numero=3 | ✅ PASSOU | |
| U14 | Reaviso D+1 (pós) → numero=4 | ✅ PASSOU | |
| U15 | Resultado check-up: "bom estado" vs "atenção" | ✅ PASSOU | |

**Resultado:** 15/15 ✅

---

## Testes de Autenticação (CT01 a CT12, CT42 a CT44)

Executados em: `api/tests/EmDia.Api.IntegrationTests/Auth`  
Fase: **3** | Branch: `feature/fase-3-auth` | Commit: `4c32c0f`

| ID | Descrição | Status | Observações |
|---|---|---|---|
| CT01 | Registrar usuário válido → 201 | ✅ PASSOU | Cria conta, token JWT e cookie seguro |
| CT02 | Registrar email duplicado → 409 | ✅ PASSOU | Conflito em e-mail já cadastrado |
| CT03 | Registrar email inválido → 400 | ✅ PASSOU | Validação FluentValidation |
| CT04 | Login correto → 200 + JWT + cookie | ✅ PASSOU | BCrypt verificado com sucesso |
| CT05 | Login senha errada → 401 | ✅ PASSOU | Credenciais inválidas |
| CT06 | Login usuário inexistente → 401 | ✅ PASSOU | Retorno padrão seguro |
| CT07 | Refresh token válido → novo par | ✅ PASSOU | Rotação de tokens |
| CT08 | Refresh token expirado → 401 | ✅ PASSOU | Rejeição por expiração |
| CT09 | Refresh token revogado (reuso) → revogar família + 401 | ✅ PASSOU | Proteção contra replay attacks |
| CT10 | Logout → revogar refresh token | ✅ PASSOU | Revogação de sessão |
| CT11 | 5 falhas de login → bloqueio 15 min | ✅ PASSOU | Proteção contra força bruta |
| CT12 | Acesso sem token → 401 | ✅ PASSOU | Middleware de autenticação JWT |
| CT42 | Acesso a recurso de outro usuário → 404 / Excluir conta | ✅ PASSOU | Isolamento multitenant / LGPD |
| CT43 | Token expirado (15 min) → 401 | ✅ PASSOU | Validação de assinatura e lifetime |
| CT44 | Rate limit auth: 10 req/min → 429 | ✅ PASSOU | FixedWindowLimiter na partição auth |

**Resultado:** 15/15 ✅

---

## Testes de Check-up (CT13 a CT17)

Executados em: `api/tests/EmDia.Api.IntegrationTests/Checkup`  
Fase: **5** | Branch: `feature/fase-5-radar-checkup` | Commit: `22cedf5`

| ID | Descrição | Status | Observações |
|---|---|---|---|
| CT13 | Iniciar check-up → retorna primeira pergunta | ✅ PASSOU | Fluxo interativo onboarding |
| CT14 | Responder passo → próxima pergunta | ✅ PASSOU | Máquina de estados do check-up |
| CT15 | Concluir check-up → itens criados | ✅ PASSOU | Itens pré-configurados inseridos |
| CT16 | Reiniciar check-up em andamento | ✅ PASSOU | Reset de respostas |
| CT17 | Check-up sem modelo → erro adequado | ✅ PASSOU | Tratamento consistente de ausência de dados |

**Resultado:** 5/5 ✅

---

## Testes de Catálogo e Itens (CT18 a CT24, CT29-CT30, CT34-CT37)

Executados em: `api/tests/EmDia.Api.IntegrationTests/Catalogo`, `Veiculos`, `Itens`  
Fase: **4** | Branch: `feature/fase-4-catalogo` | Commit: `13028e1`

| ID | Descrição | Status | Observações |
|---|---|---|---|
| CT18 | Listar categorias → 200 + lista | ✅ PASSOU | 6 categorias oficiais semeadas |
| CT19 | Listar modelos → 200 + lista | ✅ PASSOU | Modelos de itens de referência |
| CT20 | Criar veículo válido → 201 | ✅ PASSOU | Suporta carro, moto e outros |
| CT21 | Criar veículo inválido (placa) → 400 | ✅ PASSOU | Validação regex de placa |
| CT22 | Criar item manual → 201 | ✅ PASSOU | Tipos DataUnica, RecorrenteData, RecorrenteKm, Ambos |
| CT23 | Criar item com modelo → passos gerados | ✅ PASSOU | Checklist RN06 herdada |
| CT24 | Editar item → 200 | ✅ PASSOU | Atualização parcial ou total |
| CT29 | Atualizar km → recalcula proximo_vencimento | ✅ PASSOU | RN07 odômetro progressivo |
| CT30 | Atualizar km inválido → 400 | ✅ PASSOU | Bloqueio de km menor que o anterior |
| CT34 | Listar itens → somente do usuário logado | ✅ PASSOU | Filtro por UsuarioId |
| CT35 | Obter item → 200 com status calculado | ✅ PASSOU | Status recalculado dinamicamente |
| CT36 | Obter item de outro usuário → 404 | ✅ PASSOU | Isolamento de tenants |
| CT37 | Excluir item → 204 | ✅ PASSOU | Remoção lógica ou física segura |

**Resultado:** 13/13 ✅

---

## Testes de Radar, Conclusão e Preferências (CT25 a CT28, CT31-CT33, CT52-CT55)

Executados em: `api/tests/EmDia.Api.IntegrationTests/Radar`, `Itens`, `Preferencias`  
Fase: **5** | Branch: `feature/fase-5-radar-checkup` | Commit: `22cedf5`

| ID | Descrição | Status | Observações |
|---|---|---|---|
| CT25 | Radar → itens agrupados por anel | ✅ PASSOU | 4 anéis: Atrasados, Hoje, Próximos, Em Dia |
| CT26 | Radar modo foco → top-3 | ✅ PASSOU | Redução de sobrecarga cognitiva (TDAH) |
| CT27 | Concluir item → registro em histórico | ✅ PASSOU | Histórico imutável de conclusões |
| CT28 | Concluir com novaDataVencimento → data atualizada | ✅ PASSOU | Recálculo de próximo ciclo |
| CT31 | Histórico → lista ordenada por data desc | ✅ PASSOU | Timeline reversa |
| CT32 | Histórico vazio → lista vazia | ✅ PASSOU | 200 com array vazio |
| CT33 | Histórico de outro usuário → 404 | ✅ PASSOU | Isolamento de tenants |
| CT52 | Preferências sem auth → 401 | ✅ PASSOU | Proteção JWT |
| CT53 | Obter preferências → 200 com defaults | ✅ PASSOU | Valores padrão: ReceberEmail=true, MaxReavisos=2 |
| CT54 | Atualizar preferências → 200 | ✅ PASSOU | Persistência de alterações |
| CT55 | Obter após atualizar → valores confirmados | ✅ PASSOU | Idempotência de leitura/escrita |

**Resultado:** 11/11 ✅

---

## Testes de Avisos (CT38 a CT41)

Executados em: `api/tests/EmDia.Api.IntegrationTests/Avisos`  
Fase: **7** | Branch: `feature/fase-7-avisos-worker` | Commit: `045534e`

| ID | Descrição | Status | Observações |
|---|---|---|---|
| CT38 | BackgroundService dispara às 8h | ✅ PASSOU | Execução imediata via AvisosWorker/RotinaAvisosService sem erros |
| CT39 | Aviso D-30 enviado por e-mail | ✅ PASSOU | E-mail disparado via IEmailService e registrado na tabela de Avisos |
| CT40 | Sem duplicata no mesmo ciclo | ✅ PASSOU | Idempotência e regras RN08/RN09 bloqueiam envio duplicado no mesmo ciclo |
| CT41 | Aviso com e-mail desabilitado → não envia | ✅ PASSOU | Respeita PreferenciasAviso.ReceberEmail = false |

**Resultado:** 4/4 ✅

---

## Testes de Segurança (CT44 a CT46)

Executados em: `api/tests/EmDia.Api.IntegrationTests/Seguranca`  
Fase: **8** | Branch: `feature/fase-8-seguranca` | Status: ✅ APROVADO

| ID | Descrição | Status | Observações |
|---|---|---|---|
| CT44 | Rate limit global → 429 | ✅ PASSOU | Limitação com FixedWindowLimiter e resposta 429 Too Many Requests |
| CT45 | SQL injection na busca → 400 ou sem execução | ✅ PASSOU | EF Core com queries parametrizadas protege tabelas contra ataques |
| CT46 | XSS em campo de texto → serialização segura | ✅ PASSOU | Payloads de script e HTML armazenados seguramente e com Content-Type JSON estrito |

**Resultado:** 3/3 ✅

---

## Testes de Frontend (CT47 a CT51)

Executados em: `web/src/test` (Vitest + Testing Library)  
Fase: **6** | Branch: `feature/fase-6-frontend` | Commit: `8656aba`

| ID | Descrição | Status | Observações |
|---|---|---|---|
| CT47 | F1: Cadastro e check-up ponta a ponta | ✅ PASSOU | Validação completa de formulário e wizard |
| CT48 | F2: Concluir item no Radar | ✅ PASSOU | Validação de passos pendentes RN06 e modal |
| CT49 | F3: Criar item manual | ✅ PASSOU | Formulário com tipos de vencimento e checklist |
| CT50 | F4: Atualizar km | ✅ PASSOU | Validação de km não decrescente RN07 |
| CT51 | Acessibilidade: status nunca só por cor | ✅ PASSOU | RNF02: ícone + cor + texto legível (touch targets >= 44px) |

**Resultado:** 9/9 ✅
