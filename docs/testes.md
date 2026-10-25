# Plano e Resultados de Testes — Em Dia

> Documento gerado na Fase 2. Atualizado a cada fase.
> **Data de referência nos testes:** 15/10/2026 (IRelogio fixo)

---

## Legenda

| Status | Significado |
|---|---|
| ✅ PASSOU | Teste executado e aprovado |
| ❌ FALHOU | Teste falhou — ver coluna Observações |
| ⏳ PENDENTE | Ainda não implementado |

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

Fase: **3** | Status: ⏳ PENDENTE

| ID | Descrição | Status |
|---|---|---|
| CT01 | Registrar usuário válido → 201 | ⏳ |
| CT02 | Registrar email duplicado → 409 | ⏳ |
| CT03 | Registrar email inválido → 400 | ⏳ |
| CT04 | Login correto → 200 + JWT + cookie | ⏳ |
| CT05 | Login senha errada → 401 | ⏳ |
| CT06 | Login usuário inexistente → 401 | ⏳ |
| CT07 | Refresh token válido → novo par | ⏳ |
| CT08 | Refresh token expirado → 401 | ⏳ |
| CT09 | Refresh token revogado (reuso) → revogar família + 401 | ⏳ |
| CT10 | Logout → revogar refresh token | ⏳ |
| CT11 | 5 falhas de login → bloqueio 15 min | ⏳ |
| CT12 | Acesso sem token → 401 | ⏳ |
| CT42 | Acesso a recurso de outro usuário → 404 | ⏳ |
| CT43 | Token expirado (15 min) → 401 | ⏳ |
| CT44 | Rate limit: 10 req/min → 429 | ⏳ |

---

## Testes de Check-up (CT13 a CT17)

Fase: **5** | Status: ⏳ PENDENTE

| ID | Descrição | Status |
|---|---|---|
| CT13 | Iniciar check-up → retorna primeira pergunta | ⏳ |
| CT14 | Responder passo → próxima pergunta | ⏳ |
| CT15 | Concluir check-up → itens criados | ⏳ |
| CT16 | Reiniciar check-up em andamento | ⏳ |
| CT17 | Check-up sem modelo → erro adequado | ⏳ |

---

## Testes de Catálogo e Itens (CT18 a CT24, CT29-CT30, CT34-CT37)

Fase: **4** | Status: ⏳ PENDENTE

| ID | Descrição | Status |
|---|---|---|
| CT18 | Listar categorias → 200 + lista | ⏳ |
| CT19 | Listar modelos → 200 + lista | ⏳ |
| CT20 | Criar veículo válido → 201 | ⏳ |
| CT21 | Criar veículo inválido (placa) → 400 | ⏳ |
| CT22 | Criar item manual → 201 | ⏳ |
| CT23 | Criar item com modelo → passos gerados | ⏳ |
| CT24 | Editar item → 200 | ⏳ |
| CT29 | Atualizar km → recalcula proximo_vencimento | ⏳ |
| CT30 | Atualizar km inválido → 400 | ⏳ |
| CT34 | Listar itens → somente do usuário logado | ⏳ |
| CT35 | Obter item → 200 com status calculado | ⏳ |
| CT36 | Obter item de outro usuário → 404 | ⏳ |
| CT37 | Excluir item → 204 | ⏳ |

---

## Testes de Radar e Histórico (CT25 a CT28, CT31-CT33)

Fase: **5** | Status: ⏳ PENDENTE

| ID | Descrição | Status |
|---|---|---|
| CT25 | Radar → itens agrupados por anel | ⏳ |
| CT26 | Radar modo foco → top-3 | ⏳ |
| CT27 | Concluir item → registro em histórico | ⏳ |
| CT28 | Concluir com novaDataVencimento → data atualizada | ⏳ |
| CT31 | Histórico → lista ordenada por data desc | ⏳ |
| CT32 | Histórico vazio → lista vazia | ⏳ |
| CT33 | Histórico de outro usuário → 404 | ⏳ |

---

## Testes de Avisos (CT38 a CT41)

Fase: **6** | Status: ⏳ PENDENTE

| ID | Descrição | Status |
|---|---|---|
| CT38 | BackgroundService dispara às 8h | ⏳ |
| CT39 | Aviso D-30 enviado por e-mail | ⏳ |
| CT40 | Sem duplicata no mesmo ciclo | ⏳ |
| CT41 | Aviso com e-mail desabilitado → não envia | ⏳ |

---

## Testes de Segurança (CT44 a CT46)

Fase: **8** | Status: ⏳ PENDENTE

| ID | Descrição | Status |
|---|---|---|
| CT44 | Rate limit global → 429 | ⏳ |
| CT45 | SQL injection na busca → 400 sem execução | ⏳ |
| CT46 | XSS em campo de texto → sanitizado | ⏳ |

---

## Testes de Frontend (CT47 a CT51)

Fase: **7** | Status: ⏳ PENDENTE

| ID | Descrição | Status |
|---|---|---|
| CT47 | F1: Cadastro e check-up ponta a ponta | ⏳ |
| CT48 | F2: Concluir item no Radar | ⏳ |
| CT49 | F3: Criar item manual | ⏳ |
| CT50 | F4: Atualizar km | ⏳ |
| CT51 | Acessibilidade: status nunca só por cor | ⏳ |
