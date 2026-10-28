# Em Dia — Aplicativo Mobile Nativo (React Native + Expo + TypeScript)

Aplicativo mobile oficial do **Em Dia** — Radar de vencimentos e lembretes inteligentes desenvolvido especialmente para adultos com TDAH, foco e rotinas com sobrecarga cognitiva.

---

## 📱 Visão Geral do App

O aplicativo foi construído de raiz em **React Native + Expo SDK 58 + TypeScript**, utilizando arquitetura nativa com design system personalizado inspirado no board de alta fidelidade:

- **Paleta de Alto Contraste:**
  - Primária: `#00C896` (Esmeralda / Menta)
  - Alerta / Hoje: `#F59E0B` (Âmbar)
  - Sucesso: `#10B981` (Verde)
  - Erro / Atrasados: `#EF4444` (Vermelho)
  - Superfícies Dark: `#111827`, `#1F2937`, `#374151`
- **Ícones Profissionais:** 100% vetorizados via `lucide-react-native` (zero emojis como ícones).
- **Radar Visual:** Gráfico Donut dinâmico (`react-native-svg`) com fatias por anéis de urgência e contagem centralizada.
- **Modo Foco Anti-Procrastinação:** Exibe exclusivamente os 3 itens prioritários para resolução sem sobrecarga sensorial.
- **Navegação Mobile Nativa:**
  - Barra de abas inferior (`@react-navigation/bottom-tabs`) com botão central flutuante (+)
  - Pilha nativa (`@react-navigation/native-stack`) para detalhes, edição, check-up e modais.
- **Integração Real:** Conectado à API .NET 10 (`EmDia.Api`) e PostgreSQL com persistência de token via `@react-native-async-storage/async-storage`.

---

## 🚀 Como Executar o Aplicativo

### Pré-requisitos
1. **API .NET e PostgreSQL:**
   Certifique-se de que a API está rodando (`dotnet run` na porta `5196`) e o Docker PostgreSQL ativo na porta `5433`.

2. **Node.js:** Versão 18+ (recomendado Node 20 ou 24).

### Passo a Passo

1. Acesse a pasta `mobile`:
   ```bash
   cd mobile
   ```

2. Instale as dependências (já instaladas):
   ```bash
   npm install --legacy-peer-deps
   ```

3. Inicie o servidor Metro / Expo:
   ```bash
   npm start
   ```

4. **Para testar:**
   - **No seu celular físico (Android ou iPhone):** Instale o aplicativo **Expo Go** pela Google Play Store ou App Store e aponte a câmera para o QR Code exibido no terminal.
   - **No Emulador Android:** Pressione `a` no terminal (ou execute `npm run android`). O app se conectará automaticamente à API via `http://10.0.2.2:5196/api/v1`.
   - **No Simulador iOS (macOS):** Pressione `i` no terminal (ou execute `npm run ios`).

---

## 🧪 Testes e Verificação

- **Testes Unitários:**
  ```bash
  npm test
  ```
- **Verificação Estática de Tipos (TypeScript):**
  ```bash
  npm run typecheck
  ```

---

## 📂 Arquitetura de Pastas

```
mobile/
├── src/
│   ├── api/             # Cliente Axios com interceptors e serviços REST
│   ├── theme/           # Tokens (colors, typography, spacing, radius)
│   ├── components/      # Botões, Inputs, Cards, Donut SVG, Badges, Abas
│   ├── context/         # AuthContext (JWT + AsyncStorage) e RadarContext
│   ├── navigation/      # RootNavigator e MainTabNavigator com abas e FAB
│   ├── screens/
│   │   ├── auth/        # WelcomeScreen, LoginScreen, RegisterScreen
│   │   ├── radar/       # RadarScreen (Donut, resumo de hoje, atalhos)
│   │   ├── tarefas/     # TarefasScreen e ItemDetailScreen
│   │   ├── carteira/    # CarteiraScreen com filtros por categoria
│   │   ├── adicionar/   # AdicionarItemScreen com sugestões e categorias
│   │   ├── foco/        # ModoFocoScreen (3 itens essenciais para TDAH)
│   │   ├── notificacoes/# NotificacoesScreen (Lembretes e Sistema)
│   │   ├── perfil/      # PerfilScreen, Preferências e Sobre
│   │   ├── veiculos/    # VeiculosScreen (Odômetro e gerenciamento de km)
│   │   └── checkup/     # CheckupScreen (Onboarding de vencimentos)
│   └── test/            # Suíte de testes unitários do design system e regras
├── app.json             # Metadados do Expo (bundle com.emdia.app)
└── package.json
```
