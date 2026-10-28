import React from 'react';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import type { RootStackParamList } from './types';
import { useAuth } from '../context/AuthContext';
import { LoadingIndicator } from '../components/LoadingIndicator';
import { colors } from '../theme';
import { WelcomeScreen } from '../screens/auth/WelcomeScreen';
import { LoginScreen } from '../screens/auth/LoginScreen';
import { RegisterScreen } from '../screens/auth/RegisterScreen';
import { CheckupScreen } from '../screens/checkup/CheckupScreen';
import { MainTabNavigator } from './MainTabNavigator';
import { ItemDetailScreen } from '../screens/itens/ItemDetailScreen';
import { ItemFormScreen } from '../screens/itens/ItemFormScreen';
import { VeiculosScreen } from '../screens/veiculos/VeiculosScreen';
import { SobreScreen } from '../screens/perfil/SobreScreen';

const Stack = createNativeStackNavigator<RootStackParamList>();

/**
 * Rotas públicas (T01, T02) sem sessão; rotas privadas com sessão.
 * Portão do check-up: enquanto precisaCheckup for verdadeiro, o usuário fica no check-up (spec §5.2).
 */
export function RootNavigator() {
  const { usuario, carregando } = useAuth();

  if (carregando) return <LoadingIndicator message="Abrindo seu radar..." />;

  return (
    <Stack.Navigator screenOptions={{ headerShown: false, contentStyle: { backgroundColor: colors.background }, animation: 'slide_from_right' }}>
      {!usuario ? (
        <>
          <Stack.Screen name="Welcome" component={WelcomeScreen} />
          <Stack.Screen name="Login" component={LoginScreen} />
          <Stack.Screen name="Register" component={RegisterScreen} />
        </>
      ) : usuario.precisaCheckup ? (
        <Stack.Screen name="Checkup" component={CheckupScreen} />
      ) : (
        <>
          <Stack.Screen name="MainTabs" component={MainTabNavigator} />
          <Stack.Screen name="ItemDetail" component={ItemDetailScreen} />
          <Stack.Screen name="ItemForm" component={ItemFormScreen} options={{ presentation: 'modal' }} />
          <Stack.Screen name="Veiculos" component={VeiculosScreen} />
          <Stack.Screen name="Checkup" component={CheckupScreen} options={{ presentation: 'modal' }} />
          <Stack.Screen name="Sobre" component={SobreScreen} />
        </>
      )}
    </Stack.Navigator>
  );
}
