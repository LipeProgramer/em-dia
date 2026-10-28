import React from 'react';
import { View, StyleSheet, TouchableOpacity } from 'react-native';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import {
  Compass,
  Briefcase,
  Plus,
  CheckSquare,
  User,
} from 'lucide-react-native';
import type { MainTabParamList } from './types';
import { colors, radius, spacing } from '../theme';
import { RadarScreen } from '../screens/radar/RadarScreen';
import { CarteiraScreen } from '../screens/carteira/CarteiraScreen';
import { TarefasScreen } from '../screens/tarefas/TarefasScreen';
import { PerfilScreen } from '../screens/perfil/PerfilScreen';

const Tab = createBottomTabNavigator<MainTabParamList>();

// Componente placeholder para a aba central (ação que abre o modal de Adicionar)
function DummyAdicionarScreen() {
  return null;
}

export function MainTabNavigator() {
  return (
    <Tab.Navigator
      screenOptions={{
        headerShown: false,
        tabBarStyle: styles.tabBar,
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.textDisabled,
        tabBarLabelStyle: styles.tabLabel,
      }}
    >
      <Tab.Screen
        name="RadarTab"
        component={RadarScreen}
        options={{
          tabBarLabel: 'Início',
          tabBarIcon: ({ color, size }) => (
            <Compass size={size - 2} color={color} strokeWidth={2.2} />
          ),
        }}
      />

      <Tab.Screen
        name="CarteiraTab"
        component={CarteiraScreen}
        options={{
          tabBarLabel: 'Carteira',
          tabBarIcon: ({ color, size }) => (
            <Briefcase size={size - 2} color={color} strokeWidth={2.2} />
          ),
        }}
      />

      {/* Botão Central (+) Flutuante */}
      <Tab.Screen
        name="AdicionarTab"
        component={DummyAdicionarScreen}
        options={({ navigation }) => ({
          tabBarLabel: () => null,
          tabBarButton: () => (
            <TouchableOpacity
              activeOpacity={0.85}
              onPress={() => (navigation as any).navigate('AdicionarItem', {})}
              style={styles.centerFab}
            >
              <View style={styles.fabInner}>
                <Plus size={26} color="#0F172A" strokeWidth={2.8} />
              </View>
            </TouchableOpacity>
          ),
        })}
      />

      <Tab.Screen
        name="TarefasTab"
        component={TarefasScreen}
        options={{
          tabBarLabel: 'Tarefas',
          tabBarIcon: ({ color, size }) => (
            <CheckSquare size={size - 2} color={color} strokeWidth={2.2} />
          ),
        }}
      />

      <Tab.Screen
        name="PerfilTab"
        component={PerfilScreen}
        options={{
          tabBarLabel: 'Perfil',
          tabBarIcon: ({ color, size }) => (
            <User size={size - 2} color={color} strokeWidth={2.2} />
          ),
        }}
      />
    </Tab.Navigator>
  );
}

const styles = StyleSheet.create({
  tabBar: {
    backgroundColor: '#111827',
    borderTopColor: '#1F2937',
    borderTopWidth: 1,
    height: 64,
    paddingBottom: 8,
    paddingTop: 6,
  },
  tabLabel: {
    fontSize: 11,
    fontWeight: '600',
  },
  centerFab: {
    top: -16,
    justifyContent: 'center',
    alignItems: 'center',
    width: 60,
  },
  fabInner: {
    width: 52,
    height: 52,
    borderRadius: radius.full,
    backgroundColor: colors.primary,
    justifyContent: 'center',
    alignItems: 'center',
    elevation: 8,
    shadowColor: colors.primary,
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.4,
    shadowRadius: 6,
  },
});
