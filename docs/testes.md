# Resultados dos Testes — Em Dia

> Plano de testes da Entrega 2 (§6). **"Hoje" fixado em 15/10/2026** pela interface `IRelogio` nos testes de unidade e
> de integração. Execução de 28/10/2026, na branch de revisão de conformidade.

## Resumo (critérios de aceite §6.6)

| Nível | Projeto / ferramenta | Testes | Resultado |
|---|---|---|---|
| Unitário (regras do Domain) | `EmDia.Domain.Tests` · xUnit | 50 (U01–U15 + 35 complementares) | ✅ 50/50 |
| Integração da API | `EmDia.Api.IntegrationTests` · xUnit + WebApplicationFactory + PostgreSQL | 55 (CT01–CT46 + extras de E08–E10, E13, E22/E24, E25, check-up sem respostas, sessão do app mobile e notificações no celular) | ✅ 55/55 |
| Frontend | `web/src/test` · Vitest + Testing Library | 17 (CT47–CT51 + validações) | ✅ 17/17 |
| App mobile | `mobile/src/test` · Vitest | 11 (validações, datas e tema) | ✅ 11/11 |
| Integração front–API | Roteiro manual (navegador) | Fluxos F1–F4 | ✅ 4/4 |

- **Cobertura do `EmDia.Domain`:** 97,3% das linhas, 89,5% dos ramos (critério: ≥ 80%). Comando:
  `dotnet test tests/EmDia.Domain.Tests --collect:"XPlat Code Coverage"`.
- **CT08 e CT44:** todas as rotas privadas recusam acesso sem token, e nenhuma resposta expõe senha, hash ou refresh token.
- Relatórios: `dotnet test --logger trx` (pasta `TestResults/`) e `npx vitest run --reporter=verbose`; o CI publica os `.trx`.

## Testes de unidade (U01–U15) — `RegrasNegocioTests.cs`

| ID | Regra | Resultado esperado | Status |
|---|---|---|---|
| U01 | RN01/RN02 | Recorrente 15/04/2026 + 180 dias vence em 12/10/2026 → Vencido | ✅ |
| U02 | RN01 | Faltam 500 km ≈ 6 dias → vence em 21/10/2026 (km vem primeiro) → Atenção | ✅ |
| U03 | RN01 | Tempo vem primeiro → 01/05/2027 → Em dia | ✅ |
| U04 | RN01 | Garantia 10/01/2026 + 12 meses + 90 dias = 10/04/2027 | ✅ |
| U05 | RN02 | CNH 20/11/2026, antecedência 45 → faltam 36 dias → Atenção | ✅ |
| U06 | RN02 | Km 85.200 ≥ 85.000 com data futura → Vencido | ✅ |
| U07 | RN03 | −3, 12, 60, 200 dias → Centro, Anel 1, Anel 2, Externo | ✅ |
| U08 | RN04 | −10, −2, 5, 20, 100 → foco devolve −10, −2 e 5 | ✅ |
| U09 | RN05 | Concluir recorrente em 15/10/2026 → próximo 13/04/2027; passos desmarcados | ✅ |
| U10 | RN05 | Data única sem nova validade → Encerrado | ✅ |
| U11 | RN06 | 1 passo pendente sem confirmação → `PASSOS_PENDENTES` | ✅ |
| U12 | RN07 | Km de 84.500 para 84.000 → "A km não pode ser menor que 84.500." | ✅ |
| U13 | RN08/RN09 | Atenção no dia D, 2 reavisos a cada 3 dias → D, D+3, D+6; não em D+9 | ✅ |
| U14 | RN09 | Concluído após 1 reaviso → novo ciclo começa do aviso antecipado | ✅ |
| U15 | RN10 | IPVA já ativo para o mesmo veículo → não duplica | ✅ |

Complementares (`EntidadesTests.cs` e final de `RegrasNegocioTests.cs`): textos de prazo, campos por tipo (§2.5),
passos (máx. 10, progresso), validações de conclusão, veículo, preferências, bloqueio de login, refresh token,
check-up, aviso e exceções — todos ✅.

## Casos de teste da API (CT01–CT46)

| ID | Cenário | Esperado | Status |
|---|---|---|---|
| CT01 | Cadastro válido | 201; preferências criadas; senha como hash | ✅ |
| CT02 | E-mail repetido (maiúsculas) | 409 | ✅ |
| CT03 | Senha fraca | 400 com erro no campo senha | ✅ |
| CT04 | Login válido | 200; accessToken; cookie HttpOnly | ✅ |
| CT05 | Senha errada | 401 genérico | ✅ |
| CT06 | 5 falhas e depois senha certa | 401 durante o bloqueio | ✅ |
| CT07 | 6 logins em 1 minuto | 429 | ✅ |
| CT08 | `/radar` sem token | 401 | ✅ |
| CT09 | Token expirado | 401 | ✅ |
| CT10 | Refresh válido | 200; novo token; antigo revogado | ✅ |
| CT11 | Reuso de refresh | 401; todos os tokens revogados | ✅ |
| CT12 | Logout | 204; refresh revogado; cookie apagado | ✅ |
| CT13 | Check-up válido | 201; itens; resumo por status e área | ✅ |
| CT14 | Item sem resposta "sim" | 400 | ✅ |
| CT15 | Troca de óleo sem veículo | 422 | ✅ |
| CT16 | dataBase no futuro | 400 no campo | ✅ |
| CT17 | Check-up repetido | Não duplica itens | ✅ |
| CT18 | Criar data única | 201; vencimento e status corretos | ✅ |
| CT19 | Recorrente sem intervalo | 400 | ✅ |
| CT20 | Veículo de outra pessoa | 404 | ✅ |
| CT21 | Item alheio | 404 | ✅ |
| CT22 | Editar intervalo 180 → 365 | 200; recalculado | ✅ |
| CT23 | Editar item encerrado | 422 | ✅ |
| CT24 | Excluir item | 204; passos e histórico removidos | ✅ |
| CT25 | Concluir recorrente | 200; histórico; mensagem positiva | ✅ |
| CT26 | Passos pendentes | 422 `PASSOS_PENDENTES` | ✅ |
| CT27 | Passos pendentes confirmados | 200 | ✅ |
| CT28 | Conclusão amanhã | 400 | ✅ |
| CT29 | Marcar passo | 200; progresso | ✅ |
| CT30 | Passo de outro item | 404 | ✅ |
| CT31 | Radar com os 4 anéis | 200; resumo; ordem por urgência | ✅ |
| CT32 | Modo foco | 3 itens (RN04) | ✅ |
| CT33 | Usuário novo | Resumo zerado; lista vazia | ✅ |
| CT34 | Km menor | 422 | ✅ |
| CT35 | Km que ultrapassa a do item | 200; item vira Vencido | ✅ |
| CT36 | Excluir veículo com itens | 409 | ✅ |
| CT37 | Apelido repetido | 409 | ✅ |
| CT38 | Aviso antecipado | 1 e-mail; aviso número 0 | ✅ |
| CT39 | Rotina 2x no mesmo dia | 1 e-mail | ✅ |
| CT40 | max_reavisos = 1 por 10 dias | Antecipado + 1 reaviso | ✅ |
| CT41 | E-mail desativado | Nenhum e-mail | ✅ |
| CT42 | Excluir conta | 204; tudo removido | ✅ |
| CT43 | Excluir com senha errada | 422; nada removido | ✅ |
| CT44 | Dados sensíveis | Nenhuma resposta com senha/hash/refresh | ✅ |
| CT45 | Erro inesperado | 500 genérico, sem stack trace | ✅ |
| CT46 | Outra origem (CORS) | Sem `Access-Control-Allow-Origin` | ✅ |

## Frontend (CT47–CT51)

| ID | Cenário | Status |
|---|---|---|
| CT47 | Senha sem número → erro abaixo do campo, foco no campo, API não chamada (`cadastro.test.tsx`) | ✅ |
| CT48 | 422 `PASSOS_PENDENTES` → "Concluir mesmo assim?" → reenvia com confirmação (`concluir.test.tsx`) | ✅ |
| CT49 | Token expira → refresh transparente; se falhar, volta ao login (`sessao.test.ts`) | ✅ |
| CT50 | Vencido com cor + símbolo + texto no Radar e na lista (`radar.test.tsx`) | ✅ |
| CT51 | Check-up só com Tab e Enter; foco na pergunta (`checkup.test.tsx`) | ✅ |

## Fluxos ponta a ponta (API + front rodando juntos)

| Fluxo | Roteiro | Status |
|---|---|---|
| F1 | Cadastro → 8 perguntas → completar dados (validação por campo) → resultado "4 de 6 áreas em dia" → Radar | ✅ |
| F2 | Radar → detalhe da troca de óleo → marcar passo → concluir com 1 passo pendente → "Concluir mesmo assim" → comemoração e novo prazo (95.200 km ou 28/10/2027) | ✅ |
| F3 | + Novo → Casa → Filtro do purificador → campos do tipo recorrente, passos copiados → detalhe | ✅ |
| F4 | Perfil → Veículos → atualizar km (menor que a atual recusada; maior marca a troca de óleo como Vencido) | ✅ |
