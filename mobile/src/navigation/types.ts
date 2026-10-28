import type { NavigatorScreenParams } from '@react-navigation/native';
import type { ItemResponse } from '../api/types';

export type RootStackParamList = {
  // Telas Não Autenticadas
  Welcome: undefined;
  Login: undefined;
  Register: undefined;

  // App Principal
  MainTabs: NavigatorScreenParams<MainTabParamList>;

  // Modais e Telas em Pilha
  ItemDetail: { itemId: string; item?: ItemResponse };
  AdicionarItem: { categoriaId?: number };
  ModoFoco: undefined;
  Notificacoes: undefined;
  Veiculos: undefined;
  Checkup: undefined;
  PreferenciasNotificacao: undefined;
  Sobre: undefined;
};

export type MainTabParamList = {
  RadarTab: undefined;
  TarefasTab: undefined;
  AdicionarTab: undefined;
  CarteiraTab: undefined;
  PerfilTab: undefined;
};
