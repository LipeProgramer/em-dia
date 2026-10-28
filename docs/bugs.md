# Registro de Bugs — Em Dia

> Cada bug tem: como apareceu, causa raiz e correção aplicada.

---

## BUG-01 — Npgsql 9.x falhava ao autenticar no PostgreSQL 17 (SCRAM)
- **Como apareceu:** `dotnet ef database update` lançava `28P01`.
- **Correção:** pacotes atualizados para EF Core / Npgsql 10; o comando passou a funcionar e o contorno com `psql`
  (DV05) foi removido. ✅ Resolvido

## BUG-02 — `appsettings.json` ausente no projeto Infrastructure (design-time)
- **Correção:** a factory de design-time lê o `appsettings` da Api, com fallback para a connection string local. ✅

## BUG-03 — TypeScript com `erasableSyntaxOnly` rejeitava `enum`
- **Correção:** enums viraram tipos de união de strings. ✅

## BUG-04 — Data por km calculada com `DateTime.Today` e gravada no banco
- **Como apareceu:** revisão da RN01: o status de troca de óleo ficava desatualizado depois de atualizar a km e
  dependia do relógio do servidor.
- **Causa raiz:** `RecalcularProximoVencimento` usava `DateTime.Today` e gravava a menor data entre tempo e km.
- **Correção:** grava só a data do tempo e a km alvo; `DataEfetiva(hoje, veiculo)` calcula a data pela km na leitura. ✅

## BUG-05 — Excluir conta não apagava os dados
- **Como apareceu:** revisão do DER: faltavam as chaves estrangeiras de `itens`, `veiculos`, `checkups` e
  `refresh_tokens` para `usuarios`.
- **Correção:** FKs com `CASCADE` na migration `AjustesEntrega2`; teste CT42 confere que nada sobra. ✅

## BUG-06 — Concluir item devolvia 500
- **Como apareceu:** testes CT23, CT25 e CT27.
- **Causa raiz:** o histórico criado pelo Domain já tem `id`; o EF entendia que era um registro existente e tentava
  um `UPDATE`.
- **Correção:** ids uuid configurados como gerados pela aplicação (`ValueGeneratedNever`), como diz a spec §2.5. ✅

## BUG-07 — Erro 400 sem o campo `errors`
- **Como apareceu:** teste manual do cadastro com senha fraca.
- **Causa raiz:** o middleware serializava pelo tipo declarado (`ProblemDetails`) e perdia o `errors` do
  `ValidationProblemDetails`.
- **Correção:** serialização pelo tipo real. ✅

## BUG-08 — Contador de falhas de login não zerava depois do bloqueio
- **Causa raiz:** após os 15 minutos, a próxima senha errada bloqueava de novo na hora.
- **Correção:** o contador volta a zero quando o bloqueio é aplicado (teste "5 falhas seguidas bloqueiam"). ✅

## BUG-09 — Foco não ia para a primeira pergunta do check-up
- **Como apareceu:** teste CT51 (teclado).
- **Causa raiz:** o efeito de foco só reagia à troca de pergunta, não ao fim do carregamento.
- **Correção:** o efeito também depende da lista de perguntas. ✅

## BUG-10 — Resultado do check-up mostrava o destaque na área errada
- **Como apareceu:** teste dos fluxos no navegador: "Documentos" exibia a troca de óleo.
- **Correção:** o destaque traz `categoriaId` e cada área usa o seu. ✅

## BUG-11 — Observação em branco gravada no histórico
- **Como apareceu:** teste de unidade complementar.
- **Correção:** espaços em branco viram `null`, como no item. ✅

## BUG-13 — Pular todo o check-up mostrava "6 de 6 áreas em dia"
- **Como apareceu:** teste no celular: pulando todas as perguntas, o resultado dizia que tudo estava em dia.
- **Causa raiz:** área sem nenhum item era contada como "em dia".
- **Correção:** área sem itens fica sem status ("Sem itens") e não conta como em dia; com tudo vazio, o resultado
  diz que o Radar ainda está vazio (DV18). Testes de unidade e de integração cobrem o caso. ✅

## BUG-12 — App mobile incompatível com a API e com o Expo Go
- **Como apareceu:** revisão do app: chamava rotas que não existem (`/modelos-item`, `/preferencias-aviso`, `/checkup/atual`),
  usava enums numéricos e bibliotecas nativas de SDKs antigos, que o Expo Go (SDK 58) não carrega.
- **Correção:** camada de dados e telas reescritas sobre o contrato E01–E25; dependências alinhadas com
  `expo install --fix`; endereço da API descoberto pelo IP do Metro; sessão com SecureStore (DV17). ✅
