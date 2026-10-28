import React from 'react';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import type { RootStackParamList } from './types';
import { useAuth } from '../context/AuthContext';
import { LoadingIndicator } from '../components/LoadingIndicator';
import { colors } from '../theme';

// Telas de Auth
import { WelcomeScreen } from '../screens/auth/WelcomeScreen';
import { LoginScreen } from '../screens/auth/LoginScreen';
import { RegisterScreen } from '../screens/auth/RegisterScreen';

// Telas Principais
import { MainTabNavigator } from './MainTabNavigator';
import { ItemDetailScreen } from '../screens/tarefas/ItemDetailScreen';
import { AdicionarItemScreen } from '../screens/adicionar/AdicionarItemScreen';
import { ModoFocoScreen } from '../screens/foco/ModoFocoScreen';
import { NotificacoesScreen } from '../screens/notificacoes/NotificacoesScreen';
import { VeiculosScreen } from '../screens/veiculos/VeiculosScreen';
import { CheckupScreen } from '../screens/checkup/CheckupScreen';
import { PreferenciasNotificacaoScreen } from '../screens/perfil/PreferenciasNotificacaoScreen';
import { SobreScreen } from '../screens/perfil/SobreScreen';

const Stack = createNativeStackNavigator<RootStackParamList>();

export function RootNavigator() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <LoadingIndicator message="Carregando seu radar de vencimentos..." />;
  }

  return (
    <Stack.Navigator
      screenOptions={{
        headerShown: false,
        contentStyle: { backgroundColor: colors.background },
        animation: 'slide_from_right',
      }}
    >
      {!isAuthenticated ? (
        // Fluxo de Boas-Vindas e Autenticação
        <>
          <Stack.Screen name="Welcome" component={WelcomeScreen} />
          <Stack.Screen name="Login" component={LoginScreen} />
          <Stack.Screen name="Register" component={RegisterScreen} />
        </>
      ) : (
        // Fluxo Autenticado
        <>
          <Stack.Screen name="MainTabs" component={MainTabNavigator} />
          <Stack.Screen
            name="ItemDetail"
            component={ItemDetailScreen}
            options={{ presentation: 'card' }}
          />
          <Stack.Screen
            name="AdicionarItem"
            component={AdicionarItemScreen}
            options={{ presentation: 'modal' }}
          />
          <Stack.Screen
            name="ModoFoco"
            component={ModoFocoScreen}
            options={{ presentation: 'card' }}
          />
          <Stack.Screen
            name="Notificacoes"
            component={NotificacoesScreen}
            options={{ presentation: 'card' }}
          />
          <Stack.Screen
            name="Veiculos"
            component={VeiculosScreen}
            options={{ presentation: 'card' }}
          />
          <Stack.Screen
            name="Checkup"
            component={CheckupScreen}
            options={{ presentation: 'modal' }}
          />
          <Stack.Screen
            name="PreferenciasNotificacao"
            component={PreferenciasNotificacaoScreen}
            options={{ presentation: 'card' }}
          />
          <Stack.Screen
            name="Sobre"
            component={SobreScreen}
            options={{ presentation: 'card' }}
          />
        </>
      )}
    </Stack.Navigator>
  );
}
