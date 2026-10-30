# Desvios da Especificação

Registro de toda decisão que se afasta ou complementa a Entrega 2: _o quê_ e _por quê_.

> **Revisão de conformidade (28/10/2026).** O código da branch `main` foi comparado item a item com o PDF da
> Entrega 2 e ajustado para segui-lo. O contrato da API (rotas, corpos e respostas de E01–E25), as regras RN01–RN15,
> o banco (chaves estrangeiras do DER), a segurança (§4) e o frontend (telas T01–T15, Zod + React Hook Form) foram
> reescritos onde divergiam. Os desvios antigos que deixaram de existir estão marcados como **resolvidos**.

---

## Desvios em vigor

### DV01 — .NET 10 e pacotes 10.x
Todos os projetos usam `net10.0`, e os pacotes do ASP.NET Core / EF Core / Npgsql foram atualizados para a linha 10.x
(antes estavam em 9.0.4 com alvo .NET 10). Sem impacto funcional.

### DV02 — Swagger com Swashbuckle
A spec cita "Swagger / OpenAPI". Usamos o Swashbuckle, que mantém a interface interativa em `/swagger`.

### DV03 — Campo `novaDataVencimento` no E18 (Concluir)
A RN05 diz "Data única: se vier nova validade, é atualizada", e a validação do E18 cita `novaDataVencimento`, mas o
corpo de exemplo não traz o campo. Ele foi incluído como **opcional** e só é usado em itens de data única.

### DV04 — Senha do banco de desenvolvimento em `appsettings.Development.json`
É a mesma senha do `docker-compose.yml`, só para o banco local sem dados reais. A **chave do JWT** e as credenciais de
SMTP **não** ficam no repositório: vêm de `dotnet user-secrets` (dev) ou de variáveis de ambiente (servidor), como
pede a spec §4.2/§4.6.

### DV07 — TypeScript sem `enum`
O projeto Vite usa `erasableSyntaxOnly`; os enums da API são tipos de união de strings (`'Vencido' | 'Atencao' …`).

### DV08 — Rotina de avisos não roda sozinha no ambiente de testes
No ambiente `Testing`, o `AvisosWorker` não é iniciado como serviço; os testes chamam `ExecutarAgoraAsync()` com o
relógio controlado (15/10/2026), o que deixa CT38–CT41 determinísticos.

### DV09 — Pergunta `TEM_CARRO` cobre carro e moto
A tabela de dados iniciais (§2.7) cita `TEM_CARRO / TEM_MOTO` para os modelos de veículo, mas `codigo_checkup` é uma
única coluna `varchar(30)`. Para não mudar o modelo de dados, os modelos de veículo usam `TEM_CARRO`, cuja pergunta é
**"Você tem carro ou moto?"**; o tipo (Carro/Moto) é escolhido no cadastro do veículo.

### DV10 — Check-up por área, com modelos complementares
A §2.7 lista "exemplos". O check-up foi organizado **por área**, na ordem do Radar (Veículo → Documentos → Casa →
Saúde → Finanças → Garantias), com uma trilha das áreas, clicável para ir a qualquer área, e a área atual em destaque.
São **17 perguntas** e **19 modelos** prontos; além dos exemplos da §2.7, foram adicionados: Licenciamento e Seguro do
veículo; Passaporte e Certificado digital; Contrato de aluguel e Limpeza do ar-condicionado; Consulta de rotina, Dentista
e Exame de vista; Declaração do Imposto de Renda, IPTU e Seguro de vida; Garantia de eletrodoméstico.
Todos os intervalos são sugestões editáveis; o sistema não faz recomendações de saúde.

### DV18 — Área sem itens não conta como "em dia" (RN11)
A spec não diz como tratar uma área sem nenhum item no resultado do check-up. Antes ela contava como "em dia", e pular
todas as perguntas mostrava "6 de 6 áreas em dia". Agora uma área sem itens fica **sem status** ("Sem itens"), não entra
na conta de áreas em dia, e a resposta do E11 traz `areasSemItens`. Se todas as áreas estiverem vazias, o resultado diz
que o Radar ainda está vazio e oferece adicionar o primeiro item.

### DV11 — Campos extras nas respostas
As respostas seguem os exemplos da §3.4 e trazem alguns campos a mais, úteis ao front:
`precisaCheckup` no E05; `tipoVencimento`, `dataLimite` e `progressoPassos` no formato resumido (E12/E13);
`areasEmDia` e `categoriaId` nos destaques do E11. Nenhum campo do exemplo foi removido ou renomeado.

### DV12 — Refazer o check-up com o mesmo veículo
Para cumprir o CT17 ("mesmo check-up enviado duas vezes não duplica"), um veículo enviado no check-up com um apelido
que o usuário já tem é reaproveitado (em vez de responder 409). O check-up também aceita `veiculoId` de um veículo já
cadastrado, além de `veiculoRef`.

### DV13 — Cookie `Secure` desligado só nos testes automatizados
O cookie de refresh é sempre `HttpOnly; Secure; SameSite=Strict; Path=/api/v1/auth`. A única exceção é o ambiente
`Testing`, porque o servidor de testes em memória usa http. (Navegadores aceitam cookies `Secure` em
`http://localhost`, então o desenvolvimento local funciona normalmente.)

### DV14 — Datas do JWT pelo relógio do sistema
"Hoje" vem da interface `IRelogio` (fuso America/Sao_Paulo), como pede a §6.1. A emissão do JWT, porém, usa o relógio do
sistema, porque é com ele que o middleware JwtBearer confere a validade; assim os testes podem fixar "hoje" em
15/10/2026 sem invalidar os tokens.

### DV15 — Aplicativo `mobile/` (extra)
A pasta `mobile/` (React Native + Expo SDK 58, para o Expo Go) não está prevista na Entrega 2. Ela foi reescrita sobre
o contrato atual da API (E01–E25), com as mesmas regras de validação e mensagens do front web.

### DV17 — Sessão do app nativo sem cookie
O app nativo não tem cookies de navegador. Quando a requisição traz `X-Cliente: mobile`, o login e a renovação
devolvem o token de renovação **no corpo**; o app o guarda no **SecureStore** do aparelho e o reenvia no cabeçalho
`X-Refresh-Token`. Rotação e detecção de reuso continuam iguais. Sem esse cabeçalho (navegador), nada muda: o token
de renovação vai só no cookie HttpOnly e o cabeçalho `X-Refresh-Token` é ignorado. Coberto por um teste de integração.

### DV19 — Publicação atrás de proxy (Vercel → Render)
A spec não define a hospedagem. O site fica no Vercel, que repassa `/api/*` para a API no Render; assim o navegador vê um
único endereço e o cookie `SameSite=Strict` continua valendo. Duas opções, desligadas por padrão, foram adicionadas para o
servidor: `Proxy:Confiar` lê `X-Forwarded-For`/`X-Forwarded-Proto` (IP real no rate limit de login) e
`Database:MigrarAoIniciar` aplica as migrations ao subir. Passo a passo em `docs/deploy.md`.

### DV20 — Notificações no celular (Web Push) e rotina ao acordar (extra)
A spec prevê avisos só por e-mail. Como extra, o site instalado no celular recebe **notificações** (Web Push): uma
tabela a mais, `inscricoes_push` (aparelhos inscritos, apagados em cascata com o usuário), e rotas fora de E01–E25:
`GET /push/chave-publica`, `POST` e `DELETE /push/inscricoes` e `POST /push/teste`. Só são aceitos endereços HTTPS dos
serviços de push conhecidos (Google, Mozilla, Microsoft e Apple), para a API nunca chamar endereços escolhidos pelo
usuário. A rotina das 8h envia o mesmo aviso por e-mail (se ligado) e para os aparelhos; o aviso conta como entregue
se chegou por pelo menos um canal (`canal` = Push só para quem desligou o e-mail). As chaves VAPID são derivadas da
chave do JWT. Como a API dorme no plano gratuito, a rotina também roda quando ela acorda depois das 8h, e um
agendamento do GitHub Actions a acorda todo dia; os registros em `avisos` impedem envio duplicado.

### DV21 — Limite geral de requisições por IP
A spec pede limite só no login (5 por minuto por IP, mantido). Para o servidor gratuito não cair nem esgotar a cota com
uma enxurrada de chamadas, toda a API aceita no máximo **300 requisições por minuto por IP** (`Security:RequisicoesPorMinuto`);
passou disso, responde 429 com `code` `MUITAS_REQUISICOES` e `Retry-After: 60`. O valor é alto de propósito: numa
sala de aula, todos no mesmo Wi-Fi saem pelo mesmo IP. O limitador roda antes da autenticação, então chamadas sem
login também contam.

### DV22 — Teste pelo Swagger sem digitar JSON
Para a demonstração, o Swagger ficou fácil de usar sem erro de digitação, sem mudar o contrato E01–E25 (o site e o app
continuam enviando JSON):
- **Rotas `.../rapido`** (extras): cada POST, PUT e PATCH com corpo JSON ganhou uma versão com os campos como
  **parâmetros** (obrigatórios com `*`, enums como lista de opções, datas `aaaa-mm-dd`). Elas montam o mesmo request
  do contrato, então valem as mesmas validações e regras; faltou um obrigatório → 400 "Campo obrigatório.".
  `POST /auth/registrar/rapido`, `POST /auth/login/rapido` (com o mesmo limite de 5 por minuto),
  `PUT /usuarios/me/preferencias/rapido`, `DELETE /usuarios/me/rapido`, `POST /checkup/rapido` (respostas "sim" em
  lista, até um veículo e um item), `POST /itens/rapido`, `PUT /itens/{id}/rapido`, `POST /itens/{id}/concluir/rapido`,
  `PATCH /itens/{id}/passos/{passoId}/rapido`, `POST /veiculos/rapido`, `PUT /veiculos/{id}/rapido` e
  `PATCH /veiculos/{id}/km/rapido`. GET e DELETE já usavam só parâmetros. As rotas de push ficam só com JSON, porque
  os dados vêm do navegador.
- A **senha** nessas rotas vai num campo de **cabeçalho** (`senha`), nunca no endereço: endereços ficam gravados em
  logs e no histórico do navegador.
- **Documentação completa**: toda rota tem resumo, observações com exemplo, descrição de cada parâmetro e de cada
  resposta (inclusive os `code` de regra, como `PASSOS_PENDENTES`); 401 e 429 aparecem em todas, e as rotas abertas
  ficam sem o cadeado. Um teste de integração falha se alguma rota, parâmetro ou resposta ficar sem descrição.
- **Exemplos válidos** em todos os corpos JSON (cadastro, login, check-up, item, concluir, veículo, km, preferências):
  "Try it out" → "Execute" funciona sem editar; um teste de integração envia cada exemplo para a API.
- O filtro `status` de `GET /itens` virou **lista de opções** (Vencido, Atencao, EmDia, Encerrado).
- No site, o detalhe do item mostra o **id flutuante com botão Copiar**, para colar em `GET /api/v1/itens/{id}`.

### DV23 — Desempenho no plano gratuito
O servidor gratuito do Render tem 0,1 CPU e dorme depois de 15 minutos sem uso; a primeira chamada depois disso
levava perto de 1 minuto. Sem mudar o contrato:
- **API acordada no horário de uso:** rota `GET /health` (sem banco, fora do Swagger) e um agendamento do GitHub
  Actions que a chama a cada 10 minutos, das 6h à meia-noite (~560 h/mês, dentro das 750 h gratuitas). De madrugada a
  API dorme; o banco (Neon) continua dormindo quando não há uso e acorda em menos de 1 segundo.
- **Imagem pré-compilada (ReadyToRun)** e coletor de lixo simples: menos trabalho de compilação na CPU fraca, então
  a API sobe e responde às primeiras chamadas de cada tela mais rápido.
- **Site:** depois de criar, editar ou concluir, a tela usa o item devolvido pela API e recarrega as listas em
  segundo plano, sem esperar; os arquivos do site (com nome versionado) ficam em cache no navegador por 1 ano.
- O BCrypt continua com **custo 12** (spec §4.3): o cálculo da senha é lento de propósito (dificulta ataques), então
  login e cadastro são as chamadas mais demoradas na CPU fraca.

### DV24 — Esqueci minha senha (extra)
A spec não prevê recuperar a senha. Como extra, em **Entrar → Esqueci minha senha** a pessoa informa o e-mail e
recebe um link para criar uma senha nova:
- Rotas `POST /auth/esqueci-senha` e `POST /auth/redefinir-senha` (e as versões `.../rapido`), abertas e com o mesmo
  limite do login (5 por minuto por IP). Telas `/esqueci-senha` e `/redefinir-senha` no site.
- A resposta do pedido é sempre **204**, exista ou não a conta, e o e-mail sai em segundo plano: ninguém descobre
  quais e-mails estão cadastrados. No máximo um pedido por minuto por conta, para não lotar a caixa de entrada.
- O link vale **30 minutos** e **uma vez só**; um pedido novo invalida o anterior. Como no token de renovação, o banco
  guarda só o hash SHA-256 do código (tabela a mais, `redefinicoes_senha`, apagada em cascata com o usuário).
- Ao trocar a senha, todas as sessões abertas são encerradas e o bloqueio por tentativas erradas é desfeito.
- O endereço do link vem da configuração (`Site:Url`, ou a origem do CORS), nunca do cabeçalho Host da requisição,
  que um atacante poderia trocar para receber o link.
- O e-mail usa o mesmo SMTP dos avisos (DV16), pelo Brevo na porta 2525 (o Render gratuito bloqueia a 587). O app Expo (DV15) não tem a função: ele roda com a API local, e o link
  do e-mail abre o site publicado.

### DV25 — Reuso de token encerra a sessão do aparelho, não todas
A spec §4.3 diz que o reuso de um refresh já trocado revoga **todos** os tokens do usuário. Na prática isso derrubava
o PC e o celular juntos sem ataque nenhum: duas abas renovando ao mesmo tempo, ou o celular perdendo a resposta da
renovação (app em segundo plano, API acordando), reenviam o token antigo. Agora, como nos provedores de login:
- Cada login abre uma **sessão do aparelho** (`familia_id` em `refresh_tokens`); as renovações herdam a sessão.
- Reuso de um token trocado revoga **a sessão daquele aparelho** (código `REFRESH_REUTILIZADO`); os outros aparelhos
  continuam logados. Logout também encerra só o aparelho atual.
- Um token trocado há **até 60 segundos** pode ser reenviado sem alarme (corrida entre abas ou resposta perdida),
  desde que a sessão do aparelho continue ativa.
- Trocar a senha (DV24) e excluir a conta continuam encerrando todas as sessões.

### DV26 — Site sem passos, valor gasto e observação; avisos só até a data
Pedido do grupo para deixar o uso mais direto. Muda o que o **site** mostra; a API e o banco seguem o contrato E01–E25:
- **Detalhe do item:** sem a seção de passos.
- **Novo item e editar item:** sem "Passos pequenos" e "Observação". O que o item já tinha (inclusive os passos copiados
  do modelo · RN14) continua salvo e é reenviado sem alteração.
- **Concluir:** pede só a data (e a km, ou a nova validade, quando o tipo exige). Sem valor gasto e observação. Como os
  passos não aparecem, o site envia `confirmarPassosPendentes: true`; a regra RN06 continua na API e nos testes dela.
- **Avisos (e-mail e celular):** terminam no prazo e na data, ex.: "CNH: vence amanhã (30/10/2026)". Saíram o próximo
  passo e a frase final.
- O histórico ainda mostra valor e observação de conclusões antigas. O app Expo (DV15) não mudou.

### DV27 — Testes de ponta a ponta com Cypress (extra)
A spec prevê testes de unidade, de integração e de tela. Como extra, a pasta `e2e/` traz 17 testes em Cypress que abrem o
site e fazem o caminho inteiro, do cadastro à exclusão da conta, para mostrar o sistema sendo testado na apresentação.
Ficam em uma pasta com dependências próprias, fora do site e do CI: o Cypress tem centenas de MB. Por isso não entram
no total de testes do relatório.

### DV16 — Sem SMTP configurado, e-mails só vão para o log
Se `Email:Smtp:Host` estiver vazio, a API usa um serviço de e-mail falso que só escreve as mensagens no log. Com o host
configurado, usa o MailKit. A rotina diária roda às 8h (America/Sao_Paulo).

---

## Desvios resolvidos nesta revisão

| Antes | Situação |
|---|---|
| DV05 — `dotnet ef database update` falhava (Npgsql 9 + PostgreSQL 17) e as migrations eram aplicadas com `psql` | **Resolvido.** Com o Npgsql 10 o comando funciona; o `migration.sql` foi removido. |
| DV06 — `docker-compose.yml` forçava autenticação `md5` | **Resolvido.** Voltou ao padrão (`scram-sha-256`). |
| Rotas fora da spec (`/catalogo/*`, `/checkup/atual`, `/itens/{id}/historico`, `PATCH /usuarios/me`…) | **Resolvido.** Rotas exatamente como na §3.3 (E01–E25). |
| E07 sem senha de confirmação | **Resolvido.** `DELETE /usuarios/me` exige `{ "senha" }` e responde 422 se estiver errada. |
| Check-up ignorava itens de data única e de uso, sem veículos | **Resolvido.** E11 recebe respostas, veículos e itens com os dados por tipo, em uma transação. |
| RN01 gravava a data estimada pela km usando `DateTime.Today` | **Resolvido.** Grava a data do tempo; a data pela km é calculada na leitura. |
| Faltavam FKs do DER (itens/veículos/tokens/check-ups → usuários, itens/modelos → categorias, avisos → itens) | **Resolvido** na migration `AjustesEntrega2`; excluir a conta apaga tudo em cascata. |
| Sem política padrão de autorização, rate limit 10/min, refresh sem detecção de reuso, enums como número | **Resolvido.** Fallback policy, 5/min por IP com 429, reuso revoga todos os tokens, enums como texto. |
| Front sem Zod/React Hook Form e sem T04, T05, T10, T13, T15 | **Resolvido.** Telas e rotas da §5.1, validação com Zod. |
