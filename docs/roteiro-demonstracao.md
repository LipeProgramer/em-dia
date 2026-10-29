# Roteiro da demonstração (Entrega 4)

Demonstração ao vivo do fluxo **Usuário → Frontend → API → Banco de dados → Retorno**, no sistema publicado.
Tempo previsto: **8 a 10 minutos**. Os slides que acompanham estão na apresentação.

| O quê | Endereço |
|---|---|
| Site (Vercel) | https://em-dia-projeto-facul.vercel.app |
| API e Swagger (Render) | https://emdia-api-felipemsantoss.onrender.com/swagger |
| Banco (Neon) | Painel do Neon → projeto `emdia` → **Tables** |

---

## Antes de começar (10 minutos antes)

1. **Acorde a API:** abra o `/swagger` e espere carregar. No plano gratuito, ela dorme após 15 minutos sem uso e leva
   cerca de 1 minuto para acordar.
2. Abra três abas: o **site**, o **Swagger** e o **Neon** (em **Tables**).
3. No site, abra as ferramentas do navegador (**F12 → Rede/Network**, filtro **Fetch/XHR**): é por ali que se mostra o
   caminho Frontend → API → Retorno.
4. Tenha à mão um e-mail novo para a conta da demonstração (ex.: `demo.banca1@emdia.dev`) e uma senha forte.
5. Plano B: se a internet falhar, rode local com `iniciar-site.cmd` e a API (README, "Como executar").

---

## Parte 1 — Cadastro, validações e login (≈ 2 min) · *sugestão: Felipe Nadab (front)*

| Passo | Ação | O que mostrar e falar |
|---|---|---|
| 1.1 | Em **Criar conta**, clique em **Criar conta** com tudo vazio | **Validação no front** (Zod): mensagens embaixo de cada campo, foco no primeiro inválido. Nada foi enviado à API. |
| 1.2 | Preencha com a senha `abc` | Regra de senha: mínimo de 8 caracteres, com letra e número (mesma regra no front e na API). |
| 1.3 | Preencha certo e envie | Na aba **Rede**: `POST /api/v1/auth/registrar` → **201**. A senha vai para a API e é gravada só como **hash BCrypt**. |
| 1.4 | No **Neon → Tables → usuarios**, atualize | O usuário apareceu no banco: **Usuário → Front → API → Banco**. A coluna `senha_hash` não tem a senha. |
| 1.5 | Saia (**Perfil → Sair**) e entre com a **senha errada** | `POST /auth/login` → **401** "E-mail ou senha inválidos." Mensagem genérica, para não revelar quais e-mails existem. |
| 1.6 | Entre com a senha certa | **200** com o token de acesso (15 min) e o cookie de renovação `HttpOnly` (7 dias). |

## Parte 2 — Check-up e Radar (≈ 2 min) · *sugestão: Felipe Nadab (front)*

| Passo | Ação | O que mostrar e falar |
|---|---|---|
| 2.1 | O portão leva ao **check-up** | Primeiro acesso: 17 perguntas sim/não **por área** (Veículo → Garantias), com a trilha das áreas. |
| 2.2 | **Sim** em "Você tem carro ou moto?" e em mais 2 ou 3 perguntas; use **Pular a área** em uma | Perguntas vêm da API (`GET /checkup/perguntas`), cadastradas no banco. |
| 2.3 | Em **Ver resultado**, deixe uma data **no passado** (ex.: IPVA vencido) | **Validação de datas e km**; os campos aparecem agrupados por área. |
| 2.4 | Resultado | `POST /checkup` cria todos os itens **numa só transação**. "X de 6 áreas em dia"; área sem itens aparece como **Sem itens** (não conta como em dia). |
| 2.5 | **Ver meu radar** | **Radar em 4 anéis (RN03):** centro = vencido, até 30 dias, 31 a 90, mais de 90. Status com **cor + símbolo + texto** (acessibilidade). |
| 2.6 | **Modo foco** | **RN04:** só os 3 itens mais urgentes, vencidos primeiro. |

## Parte 3 — Consultas, alterações e regras de negócio (≈ 3 min) · *sugestão: Felipe Moreira (API)*

| Passo | Ação | O que mostrar e falar |
|---|---|---|
| 3.1 | **Itens**, filtre por categoria e por situação | **Consulta** com filtros (`GET /itens?categoriaId=&status=`). O status não é gravado: é calculado na leitura. |
| 3.2 | Abra o item vencido → **Concluir** sem marcar os passos | **Regra RN06:** a API responde **422 `PASSOS_PENDENTES`** e o front pergunta **"Concluir mesmo assim?"**. |
| 3.3 | **Concluir mesmo assim** | **RN05/RN15:** histórico gravado, novo prazo calculado e mensagem positiva ("Boa! Próxima vez: …"). O item sai do centro do Radar. |
| 3.4 | **Histórico** | A conclusão aparece (`GET /historico`), vinda da tabela `historicos`. |
| 3.5 | **Perfil › Veículos → Atualizar km** com um valor **menor** que o atual | **RN07:** a km nunca diminui → **422 `KM_MENOR`**, com a mensagem de erro na tela. |
| 3.6 | Atualize com um valor **maior** | Os itens do veículo (ex.: troca de óleo) **recalculam o status** pela km. |
| 3.7 | Abra um item → **Editar**, mude o intervalo → **Salvar** | **Alteração** (`PUT /itens/{id}`); o próximo vencimento é recalculado (RN01). |
| 3.8 | **Perfil → Salvar preferências** (ex.: antecedência) | **Alteração** de preferências de aviso (`PUT /usuarios/me/preferencias`). |
| 3.9 | (Opcional, no celular) **Perfil → Notificações no celular → Enviar notificação de teste** | **Extra (DV20):** a notificação aparece na tela do celular; a rotina das 8h usa o mesmo caminho para avisar dos prazos. |

## Parte 4 — Exclusões e tratamento de erros (≈ 1,5 min) · *sugestão: Felipe Moreira (API)*

| Passo | Ação | O que mostrar e falar |
|---|---|---|
| 4.1 | **Perfil › Veículos → Excluir** o veículo que tem itens | **RN13:** **409 `VEICULO_COM_ITENS`**: o veículo fica protegido enquanto houver itens. |
| 4.2 | Abra um item → **Excluir** | **Exclusão** (`DELETE /itens/{id}` → **204**). Some do Radar e do banco (passos e histórico em cascata). |
| 4.3 | Na aba **Rede**, mostre uma resposta de erro | Todos os erros seguem **ProblemDetails**: `title`, `status`, `detail` e `code` (regras) ou `errors` por campo (400). Nunca stack trace. |

## Parte 5 — API pelo Swagger (≈ 1,5 min) · *sugestão: Felipe Moreira (API)*

| Passo | Ação | O que mostrar e falar |
|---|---|---|
| 5.1 | `GET /api/v1/radar` sem token → **Execute** | **401**: toda rota exige login, exceto registrar, login e refresh. |
| 5.2 | `POST /api/v1/auth/login` com a conta da demo → copie o `accessToken` | Autenticação JWT (HS256). |
| 5.3 | **Authorize** → cole o token → `GET /api/v1/radar` | **200**: os mesmos dados que o site mostra. 25 endpoints (E01–E25) documentados. |
| 5.4 | (Opcional) `POST /api/v1/auth/registrar` com corpo vazio | **400** com `errors` por campo: a validação existe também na API, não só no front. |

## Fechamento (≈ 30 s)

**Perfil → Excluir conta**, digitando a senha. Diga: **LGPD (RF18)**, a conta e **todos os dados** são apagados em
cascata. No **Neon**, a tabela `usuarios` não tem mais a conta da demonstração.

---

## Perguntas prováveis da banca

| Pergunta | Resposta curta |
|---|---|
| Por que o status não é gravado no banco? | Depende da data de hoje e da km atual; gravado, ficaria velho no dia seguinte. É calculado na leitura (Domain). |
| Onde ficam as regras de negócio? | Na camada **Domain** (entidades `Item`, `Veiculo` e serviços `RadarDomain`, `AvisoDomain`, `CheckupDomain`), testadas sem banco e sem API. |
| Como o front fala com a API? | axios em `/api/v1`, JSON; token de acesso só em memória; se der 401, tenta `/auth/refresh` uma vez e repete a chamada. |
| E a API com o banco? | Entity Framework Core 10 + Npgsql; migrations versionam o banco e criam o catálogo; repositórios filtram sempre pelo dono (RN12). |
| Como evitam ataque de força bruta? | 5 tentativas de login por minuto por IP (429) e bloqueio da conta por 15 min após 5 falhas. Além disso, toda a API aceita no máximo 300 requisições por minuto por IP (DV21). |
| E se alguém tentar ver o item de outra pessoa? | O id do usuário vem só do token; recurso de outro usuário responde **404**. |
| Como sabem que funciona? | 137 testes automatizados (50 unitários, 59 de integração com PostgreSQL real, 17 do front, 11 do app), cobertura de 97,3% no Domain e CI no GitHub Actions a cada push. |
| Por que Vercel + Render + Neon? | Gratuitos e separados por função. O Vercel repassa `/api/*` para o Render, então o navegador vê um só endereço e o cookie `SameSite=Strict` funciona. |
