# Publicar o Em Dia online

O Em Dia tem três partes, e cada uma fica num serviço gratuito:

| Parte | Onde | O que faz |
|---|---|---|
| Site (React) | **Vercel** | Telas. Repassa tudo que começa com `/api/` para a API (`web/vercel.json`). |
| API (.NET 10) | **Render** | Regras, login e Swagger. Sobe pelo `api/Dockerfile`, configurada pelo `render.yaml`. |
| Banco (PostgreSQL) | **Neon** | Guarda os dados. As tabelas e o catálogo são criados sozinhos quando a API sobe. |

Como o Vercel repassa `/api/*` para o Render, o navegador enxerga site e API no mesmo endereço. Por isso o cookie de
login (`SameSite=Strict`) funciona e não é preciso mexer no código do site.

> Faça os passos **nesta ordem**: banco → API → site. Use o repositório `felipemsantoss/em-dia` (branch `main`).

---

## 1. Banco no Neon

1. Entre em <https://neon.tech> com a sua conta do GitHub e crie um projeto chamado `emdia`.
   - **Region:** `AWS US East (N. Virginia)` (a mesma região da API, para ficar rápido).
   - **Postgres version:** 17.
2. Na tela **Connect**, escolha **Parameters only** e anote: host, database, user e password.
3. Monte a connection string **neste formato** (a API não aceita o formato `postgresql://...`):

   ```
   Host=SEU-HOST.neon.tech;Database=neondb;Username=SEU-USUARIO;Password=SUA-SENHA;SSL Mode=Require
   ```

   Guarde essa linha só para o passo 2. Ela tem a senha do banco: não coloque no repositório nem em prints.

## 2. API no Render

1. Entre em <https://render.com> com a sua conta do GitHub.
2. **New → Blueprint** e escolha o repositório `felipemsantoss/em-dia`. O Render lê o `render.yaml` e mostra o serviço
   `emdia-api-felipemsantoss`.
3. Ele vai pedir dois valores:
   - `ConnectionStrings__Default`: a linha do passo 1.
   - `Cors__Origens__0`: o endereço do site no Vercel, ex.: `https://em-dia.vercel.app` (sem `/` no fim).
     Se ainda não souber, coloque qualquer um e corrija depois em **Environment**.
4. Clique em **Apply**. A primeira subida leva uns 5 minutos.
   A chave do JWT (`Jwt__Key`) é gerada pelo próprio Render e não aparece em lugar nenhum do código.
5. Quando ficar **Live**, abra `https://emdia-api-felipemsantoss.onrender.com/swagger`. Se o Swagger abrir, a API e o
   banco estão funcionando.

> **Confira a URL.** Ela precisa ser exatamente `https://emdia-api-felipemsantoss.onrender.com`. Se o Render der outro
> endereço, troque-o em `web/vercel.json` (linha do `destination`), faça commit e push.

## 3. Site no Vercel

1. No projeto do Vercel, em **Settings → Build and Deployment**:
   - **Root Directory:** `web`
   - **Framework Preset:** Vite (o Vercel detecta sozinho; build `npm run build`, saída `dist`).
2. Em **Settings → Git**, confira se o projeto está ligado a `felipemsantoss/em-dia`, branch `main`.
3. Faça um novo deploy (**Deployments → Redeploy**, ou um push qualquer).
4. Abra o site e crie uma conta. Deve ir direto para o check-up.

---

## Antes de apresentar

- **A API fica acordada das 6h à meia-noite.** No plano gratuito do Render, a API "dorme" depois de 15 minutos sem uso
  e leva perto de 1 minuto para acordar. O agendamento `.github/workflows/manter-acordado.yml` chama
  `https://emdia-api-felipemsantoss.onrender.com/health` a cada 10 minutos nesse horário (~560 h/mês, dentro das 750 h
  gratuitas do Render; se tiver outro serviço gratuito no Render, as horas são somadas). O GitHub pode atrasar
  agendamentos; por garantia, uns 2 minutos antes de apresentar, abra o `/health` e espere aparecer `ok`.
- Para não depender do GitHub, dá para cadastrar o mesmo endereço `/health` num monitor gratuito (ex.: UptimeRobot,
  a cada 5 minutos).
- Sem as variáveis de e-mail (abaixo), os avisos por e-mail só aparecem no log do Render (**Logs**) (DV16).

## Avisos de vencimento: e-mail e celular

A rotina de avisos roda todo dia depois das 8h (Brasília). Como a API dorme no plano gratuito, o agendamento
`.github/workflows/avisos.yml` (GitHub Actions) acorda a API às 8h15 e às 12h; ao acordar depois das 8h, ela roda a
rotina do dia. Rodar mais de uma vez não duplica nada (cada aviso fica gravado em `avisos`).

### E-mail (SMTP)

Crie uma conta de envio e, no Render, em **Environment**, adicione as variáveis (os valores ficam só lá):

| Variável | Brevo (grátis, 300 e-mails/dia) | Gmail (com verificação em 2 etapas) |
|---|---|---|
| `Email__Smtp__Host` | `smtp-relay.brevo.com` | `smtp.gmail.com` |
| `Email__Smtp__Porta` | **`2525`** (o Render grátis bloqueia a 587) | `587` (não funciona no Render grátis) |
| `Email__Smtp__Usuario` | o login SMTP mostrado no Brevo | o seu endereço Gmail |
| `Email__Smtp__Senha` | a chave SMTP do Brevo | uma **senha de app** do Google (não a senha da conta) |
| `Email__Remetente` | um remetente verificado no Brevo | o mesmo endereço Gmail |

Salve com **Save, rebuild, and deploy**. Cada usuário escolhe no Perfil se quer receber por e-mail.

O mesmo SMTP envia o link de **Esqueci minha senha** (DV24), que chega no e-mail de quem pediu. O link abre o site
definido em `Site__Url` (no `render.yaml`: `https://em-dia-projeto-facul.vercel.app`; confira em **Environment**
se o endereço do site mudar). Sem SMTP configurado, o e-mail (com o link) só aparece em **Logs** no Render.

**Passo a passo no Brevo (grátis):**
1. Crie a conta em <https://www.brevo.com> e confirme o e-mail.
2. **Senders, Domains & Dedicated IPs → Senders → Add a sender**: cadastre o e-mail que vai aparecer como
   remetente (ex.: o seu Gmail) e confirme pelo link que o Brevo manda. Esse é o `Email__Remetente`.
3. **SMTP & API → SMTP**: copie o **Login** (`Email__Smtp__Usuario`) e gere uma **SMTP key**
   (`Email__Smtp__Senha`).
4. No Render, em **Environment**, adicione as 5 variáveis da tabela e salve. Use a **porta 2525**: o plano gratuito do
   Render bloqueia as portas de e-mail 25, 465 e 587 (o envio fica esperando até dar "The operation has timed out").
5. Teste: no site, **Entrar → Esqueci minha senha** com o seu e-mail. Confira também o spam; com remetente Gmail
   enviado pelo Brevo, os primeiros e-mails costumam cair lá.

### Notificação no celular (Web Push)

Não precisa configurar nada no servidor: as chaves VAPID são derivadas da chave do JWT (DV20). Cada pessoa ativa no
próprio aparelho, em **Perfil → Notificações no celular → Ativar notificações**:

- **iPhone (iOS 16.4 ou mais novo):** primeiro **Safari → Compartilhar → Adicionar à Tela de Início**; abra o Em Dia
  pelo ícone da tela de início e ative lá. No Safari comum o iPhone não deixa ativar.
- **Android:** funciona no Chrome direto ou no app instalado.
- O botão **Enviar notificação de teste** confere na hora se o aparelho está recebendo.

## Testar pelo Swagger

O Swagger fica em `https://emdia-api-felipemsantoss.onrender.com/swagger` e permite chamar todas as rotas (E01–E25):

1. `POST /api/v1/auth/registrar` → **Try it out** → preencha o corpo → **Execute** (resposta `201`).
2. `POST /api/v1/auth/login` com o mesmo e-mail e senha → copie o `accessToken` da resposta.
3. Clique em **Authorize** (cadeado no topo), cole o token **sem** a palavra `Bearer` e confirme.
4. Agora as rotas protegidas funcionam: `GET /api/v1/checkup/perguntas`, `POST /api/v1/checkup`, `GET /api/v1/radar`…

O token vale 15 minutos; depois disso, faça login de novo e troque no **Authorize**.

Todos os corpos JSON já vêm com um **exemplo válido**: é só **Try it out → Execute**. Para não mexer em JSON, cada
POST, PUT e PATCH tem uma versão **`.../rapido`** logo abaixo da original, com cada campo como parâmetro (os
obrigatórios têm `*`, os tipos são listas de opções). Ex.: `POST /api/v1/auth/login/rapido` (a senha vai no campo
de cabeçalho `senha`), `POST /api/v1/itens/rapido`, `PUT /api/v1/itens/{id}/rapido`,
`POST /api/v1/itens/{id}/concluir/rapido`, `POST /api/v1/veiculos/rapido`. Copie o `id` da resposta — ou do botão
**Copiar** no detalhe do item no site — e use em `GET /api/v1/itens/{id}`, nos `PUT` e no `DELETE`. Cada rota
descreve os parâmetros e o que cada resposta (200, 400, 404, 409, 422…) significa.

## Se der erro

| Sintoma | Causa provável | O que fazer |
|---|---|---|
| Site mostra "Algo deu errado" em tudo | API dormindo ou fora do ar | Abra o `/swagger` da API e espere; veja **Logs** no Render. |
| `/swagger` não abre e o log fala de conexão com o banco | Connection string errada | Confira o formato do passo 1 (com `SSL Mode=Require`). |
| Site abre, mas recarregar uma página dá 404 | `vercel.json` não foi lido | Confira se o **Root Directory** do Vercel é `web`. |
| Login funciona, mas a sessão some ao recarregar | Site chamando a API por outro endereço | O site deve chamar só `/api/...` do próprio Vercel; confira o `vercel.json`. |

## Configuração usada no servidor

Definida no `render.yaml`. Os valores secretos ficam só no painel do Render:

| Variável | Valor | Para quê |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Liga HSTS e desliga o modo de desenvolvimento. |
| `PORT` | `8080` | Porta em que a API escuta dentro do contêiner. |
| `ConnectionStrings__Default` | *(secreto)* | Banco do Neon. |
| `Jwt__Key` | *(gerado pelo Render)* | Assinatura dos tokens (256 bits). |
| `Cors__Origens__0` | endereço do Vercel | Origem permitida, caso o site chame a API direto. |
| `Proxy__Confiar` | `true` | Lê o IP real do usuário e o HTTPS vindos do Vercel/Render (rate limit de login por IP). |
| `Database__MigrarAoIniciar` | `true` | Aplica as migrations e os dados iniciais ao subir. |
| `Email__Smtp__*`, `Email__Remetente` | *(secretos, opcionais)* | Envio dos avisos por e-mail (seção acima). |
| `Push__VapidPrivada` | *(opcional)* | Chave VAPID própria (32 bytes em base64url); sem ela, é derivada da chave do JWT. |
