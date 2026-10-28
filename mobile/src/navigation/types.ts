import type { NavigatorScreenParams } from '@react-navigation/native';

export type RootStackParamList = {
  // Sem sessão (T01, T02)
  Welcome: undefined;
  Login: undefined;
  Register: undefined;

  // Com sessão
  Checkup: undefined;                         // T03 → T04 → T05
  MainTabs: NavigatorScreenParams<MainTabParamList>;
  ItemDetail: { itemId: string };             // T10 (+ T12 em janela)
  ItemForm: { itemId?: string };              // T09 (novo) e T11 (editar)
  Veiculos: undefined;                        // T15
  Sobre: undefined;
};

/** Menu inferior: Radar · Itens · + Novo · Histórico · Perfil (spec §5.2). */
export type MainTabParamList = {
  RadarTab: undefined;
  ItensTab: undefined;
  NovoTab: undefined;
  HistoricoTab: undefined;
  PerfilTab: undefined;
};
