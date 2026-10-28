# Redesign Em Dia — registro de implementação

## Direção

A interface segue a direção “instrumento calmo”: verde-floresta e sálvia, superfícies claras, tipografia sem serifa, linhas e sombras discretas. O elemento memorável é o Radar de quatro níveis — não gradientes ou ornamentos. O conteúdo prioriza linguagem sem culpa, orientação concreta e hierarquia de urgência.

## Mudanças incluídas

- Identidade visual consistente entre cabeçalho autenticado, navegação, formulários, cards e diálogos.
- Navegação com ícones SVG locais, estado da rota, marca própria e link para pular ao conteúdo.
- Hero do Radar com visual de órbita, contagem derivada dos dados e resumo dos quatro níveis.
- Melhor adaptação a telas estreitas, incluindo apresentação em cartões para a tabela de itens.
- Ícones sem emoji em fluxos principais, alvos de toque amplos e `prefers-reduced-motion`.
- Modal compartilhado com foco inicial, contenção por Tab, Escape, bloqueio de scroll e retorno de foco.
- Confirmação interna para exclusão de itens, veículos e conta; exclusão de conta exige digitar `EXCLUIR`.
- Removida estatística promocional sem fonte e o botão Google que utilizava credenciais fixas como demonstração de login.
- Tela pública deixa explícito que o quadro é apenas ilustrativo e que os itens reais aparecem após autenticação.
- Título, idioma, metadados e favicon alinhados à marca Em Dia.
- Documentação `DESIGN.md` e `UX-CONTRACT.md` para decisões visuais e comportamentais duráveis.

## Limites e validação

- `npx tsc -b`: passou durante a validação do redesign.
- `npm run lint`: não iniciou por ausência do binding nativo Linux do Oxlint na pasta `node_modules` recebida.
- `npm run test:run` e `npm run build`: não iniciaram porque o pacote Rolldown/Vitest também não encontrou o binding nativo Linux. A cópia original continha dependências instaladas para Windows. O pacote de entrega exclui `node_modules` e `dist` antigo; execute `npm ci` e as verificações novamente no sistema de desenvolvimento.
- A API backend não foi fornecida nesta pasta; as chamadas existentes continuam dependentes dela. O redesign não simula o backend nem garante a execução dos fluxos remotos sem a API.
