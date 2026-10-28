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

### DV10 — Modelos complementares nos dados iniciais
A §2.7 lista "exemplos". Para cobrir as 6 áreas do Radar e as 8 perguntas do wireframe ("Pergunta 2 de 8"), foram
adicionados: Licenciamento (Veículo), Passaporte (Documentos), Consulta de rotina (Saúde) e Declaração do Imposto de
Renda (Finanças). Todos os intervalos são sugestões editáveis; o sistema não faz recomendações de saúde.

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

### DV15 — Aplicativo `mobile/` fora do escopo
A pasta `mobile/` (React Native/Expo) não está prevista na Entrega 2 e foi criada sobre o contrato antigo da API.
Ela **não foi atualizada** e precisa ser adaptada às rotas atuais se for mantida.

### DV16 — Sem SMTP configurado, e-mails só vão para o log
Se `Email:Smtp:Host` estiver vazio, a API usa um serviço de e-mail falso que só registra as mensagens. Com o host
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
