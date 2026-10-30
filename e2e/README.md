# Testes de ponta a ponta com Cypress

O Cypress abre o site de verdade e clica como uma pessoa, do cadastro à exclusão da conta. São 17 testes em sequência,
em [`cypress/e2e/fluxo-completo.cy.js`](cypress/e2e/fluxo-completo.cy.js). Cada execução cria uma conta nova e a apaga no fim.

Fica nesta pasta, com dependências próprias, para os centenas de MB do Cypress não entrarem na publicação do site nem no
CI. Por isso estes testes **não rodam no CI** e não entram no total de testes do relatório.

## Instalar (só na primeira vez)

```powershell
cd e2e
npm install
```

## Rodar

Primeiro ligue o sistema, como em ["Nas próximas vezes"](../README.md#nas-próximas-vezes) do README principal: o banco, a
API e o site precisam estar no ar. Depois, em **outro PowerShell**, na pasta `e2e`:

| Comando | O que faz | Para que usar |
|---|---|---|
| `npm run abrir` | Abre a janela do Cypress: escolha **E2E Testing**, depois **Edge** e `fluxo-completo.cy.js` | Mostrar ao vivo: a lista de testes fica à esquerda e o site à direita |
| `npm run rodar` | Roda tudo de uma vez, com o navegador visível, e grava um vídeo | Gravar: o vídeo fica em `cypress/videos/fluxo-completo.cy.js.mp4` |
| `npm run rodar:sem-janela` | Roda tudo sem abrir o navegador, e também grava o vídeo | Só conferir se está tudo passando |

Leva cerca de 1 minuto. Para mudar a velocidade, troque a pausa entre os passos, em milissegundos (`0` é o mais rápido):
altere `pausa` em [`cypress.config.js`](cypress.config.js).

## Se der erro

| O que aparece | O que fazer |
|---|---|
| Falha em "Login" logo depois de rodar de novo | O login aceita 5 tentativas por minuto por IP, e cada execução usa 3. Espere 1 minuto. |
| `cy.visit()` não conecta ao `localhost:5173` | O site não está ligado. |
| O cadastro falha com "Algo deu errado" ou "Sem conexão" | A API não está ligada, ou o banco está desligado. |
| O Cypress fecha sozinho com "GPU process isn't usable" ou `ERR_FAILED` | Use os comandos `npm run`: eles já ligam as opções `--no-sandbox --disable-gpu-sandbox`, que valem só para a janela do Cypress. |

## Testar o site online

Por padrão, os testes usam o sistema local, com dados só do seu PC. Para rodar no site publicado (a conta de teste também é
apagada no fim), defina o endereço antes:

```powershell
$env:CYPRESS_BASE_URL = "https://em-dia-projeto-facul.vercel.app"
npm run abrir
```

Para voltar ao local, feche o PowerShell. Esse modo ainda não foi testado. No site online, a API pode levar perto de 1
minuto para acordar se estiver dormindo, e o Cypress espera só 15 segundos: abra o site antes de rodar.
