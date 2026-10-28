import React from 'react';
import { Pressable, StyleSheet, View } from 'react-native';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { Compass, History, List, Plus, User } from 'lucide-react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import type { MainTabParamList } from './types';
import { colors, radius } from '../theme';
import { RadarScreen } from '../screens/radar/RadarScreen';
import { ItensScreen } from '../screens/itens/ItensScreen';
import { HistoricoScreen } from '../screens/historico/HistoricoScreen';
import { PerfilScreen } from '../screens/perfil/PerfilScreen';

const Tab = createBottomTabNavigator<MainTabParamList>();

function Vazia() {
  return null;
}

/** Menu inferior com 5 entradas: Radar · Itens · + Novo (botão central) · Histórico · Perfil (spec §5.2). */
export function MainTabNavigator() {
  const insets = useSafeAreaInsets();
  return (
    <Tab.Navigator
      screenOptions={{
        headerShown: false,
        tabBarStyle: [estilos.barra, { height: 64 + insets.bottom, paddingBottom: 8 + insets.bottom }],
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.textDisabled,
        tabBarLabelStyle: estilos.rotulo,
      }}
    >
      <Tab.Screen name="RadarTab" component={RadarScreen}
        options={{ tabBarLabel: 'Radar', tabBarIcon: ({ color, size }) => <Compass size={size - 2} color={color} /> }} />
      <Tab.Screen name="ItensTab" component={ItensScreen}
        options={{ tabBarLabel: 'Itens', tabBarIcon: ({ color, size }) => <List size={size - 2} color={color} /> }} />
      <Tab.Screen name="NovoTab" component={Vazia}
        options={({ navigation }) => ({
          tabBarLabel: () => null,
          tabBarButton: () => (
            <Pressable onPress={() => navigation.getParent()?.navigate('ItemForm', {})} style={estilos.central}
                       accessibilityRole="button" accessibilityLabel="Novo item">
              <View style={estilos.circulo}><Plus size={26} color="#0F172A" strokeWidth={2.8} /></View>
            </Pressable>
          ),
        })} />
      <Tab.Screen name="HistoricoTab" component={HistoricoScreen}
        options={{ tabBarLabel: 'Histórico', tabBarIcon: ({ color, size }) => <History size={size - 2} color={color} /> }} />
      <Tab.Screen name="PerfilTab" component={PerfilScreen}
        options={{ tabBarLabel: 'Perfil', tabBarIcon: ({ color, size }) => <User size={size - 2} color={color} /> }} />
    </Tab.Navigator>
  );
}

const estilos = StyleSheet.create({
  barra: { backgroundColor: '#111827', borderTopColor: '#1F2937', borderTopWidth: 1, paddingTop: 6 },
  rotulo: { fontSize: 11, fontWeight: '600' },
  central: { top: -14, alignItems: 'center', justifyContent: 'center', width: 64 },
  circulo: { width: 52, height: 52, borderRadius: radius.full, backgroundColor: colors.primary, alignItems: 'center', justifyContent: 'center', elevation: 6 },
});
