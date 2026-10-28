# Em Dia — App mobile (React Native + Expo)

App do **Em Dia** para Android e iPhone, feito com **Expo SDK 58** para rodar no **Expo Go**. Usa a mesma API e as
mesmas regras do front web (Entrega 2): check-up, Radar com modo foco, itens com passos, concluir, histórico, perfil e
veículos.

## Como rodar no celular com o Expo Go

O celular e o computador precisam estar **na mesma rede Wi-Fi**.

### 1. Suba o banco e a API escutando na rede local

```powershell
docker compose up -d                       # na raiz do repositório
cd api
dotnet run --project src/EmDia.Api --launch-profile celular
```

O perfil `celular` faz a API escutar em `http://0.0.0.0:5196` (todas as interfaces). Na primeira vez, o Windows pode
perguntar se o .NET pode receber conexões: permita em **redes privadas**. A chave do JWT precisa estar configurada
(veja o README principal).

### 2. Inicie o app

```powershell
cd mobile
npm install
npx expo start
```

Abra o **Expo Go** no celular (Play Store ou App Store) e leia o QR Code do terminal.

O app descobre sozinho o IP do computador (o mesmo do servidor Metro) e chama a API em `http://<IP>:5196/api/v1`.
O endereço usado aparece no fim da tela **Perfil**. Para usar outro endereço, defina a variável antes de iniciar:

```powershell
$env:EXPO_PUBLIC_API_URL = "http://192.168.0.10:5196/api/v1"
npx expo start
```

Se o celular não conseguir falar com a API, a mensagem de erro mostra o endereço que ele tentou. Confira se a API está
no perfil `celular`, se os dois aparelhos estão no mesmo Wi-Fi e se o firewall do Windows liberou a porta 5196.

## Sessão no app (DV17)

O app manda o cabeçalho `X-Cliente: mobile`. Assim a API devolve o token de renovação no corpo, e o app o guarda no
**SecureStore** do aparelho (não há cookies de navegador no app nativo). O token de acesso fica só em memória, e em 401
o app renova a sessão uma vez e repete a requisição, como no front web.

## Telas

| Spec | Tela no app |
|---|---|
| T01, T02 | Entrar, Criar conta |
| T03–T05 | Check-up (perguntas → completar dados → resultado) |
| T06, T07 | Radar (aba) com o botão **Modo foco** |
| T08 | Itens (aba) com filtros por área e status |
| T09, T11 | Novo item (botão **+**) a partir de um modelo ou do zero; Editar |
| T10, T12 | Detalhe do item, com a janela **Concluir** |
| T13 | Histórico (aba) |
| T14 | Perfil (aba): preferências, refazer check-up, veículos, sair, excluir conta |
| T15 | Meus veículos |

## Verificação

```powershell
npx tsc --noEmit        # tipos
npm test                # testes (Vitest)
npx expo-doctor         # dependências compatíveis com o SDK
```

## Estrutura

```
mobile/src/
├── api/          client.ts (axios + renovação + SecureStore), services.ts (E01–E25), types.ts
├── components/   Button, Input, FilterPills, RadarChart (SVG), ui.tsx (StatusBadge, Tela, Janela…)
├── context/      AuthContext (sessão)
├── navigation/   RootNavigator (portão do check-up), MainTabNavigator (Radar · Itens · + · Histórico · Perfil)
├── screens/      auth, checkup, radar, itens, historico, perfil, veiculos
├── utils/        formato.ts (datas dd/mm/aaaa), validacao.ts (mesmas mensagens da spec §6.5)
└── test/         testes de validação e do tema
```
