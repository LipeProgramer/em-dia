# Em Dia — Radar de vencimentos

Aplicação web React/TypeScript para centralizar vencimentos da vida prática e ajudar o usuário a identificar o que precisa de atenção. O frontend é pensado como uma extensão web do produto mobile e mantém o Radar de quatro níveis, o Modo Foco, o Check-up inicial, checklists, manutenção por quilometragem e preferências de aviso.

## Tecnologias

- React 19, TypeScript e Vite
- React Router para rotas
- TanStack Query e Axios para comunicação com a API
- CSS próprio, tokens semânticos e ícones SVG locais

## Executar localmente

Requer uma versão recente do Node.js compatível com o Vite 8 e npm.

```bash
npm ci
npm run dev
```

Por padrão, o Vite sobe em `http://localhost:5173` e encaminha `/api` para `http://localhost:5196`. Inicie a API .NET do projeto na porta `5196` ou ajuste `vite.config.ts` para o endereço correto do backend.

## Verificações

```bash
npm run lint
npx tsc -b
npm run test:run
npm run build
```

Os testes funcionais de integração com a API precisam do backend/configuração esperados pelo projeto. O frontend foi verificado por TypeScript durante o redesign; lint, testes Vitest e build devem ser executados novamente após `npm ci` no ambiente de desenvolvimento, já que a pasta `node_modules` da cópia original não é portátil entre Windows e Linux.

## Documentação de design

- [`DESIGN.md`](./DESIGN.md): identidade visual, tokens e princípios de composição.
- [`UX-CONTRACT.md`](./UX-CONTRACT.md): fluxos, estados, confirmação e critérios de acessibilidade.
