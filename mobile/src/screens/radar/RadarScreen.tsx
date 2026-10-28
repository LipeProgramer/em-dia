import React, { useState } from 'react';
import { Pressable, RefreshControl, ScrollView, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useNavigation } from '@react-navigation/native';
import type { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { Crosshair } from 'lucide-react-native';
import type { RootStackParamList } from '../../navigation/types';
import { radarApi } from '../../api/services';
import { useAuth } from '../../context/AuthContext';
import { Button } from '../../components/Button';
import { RadarChart, ItemLinha } from '../../components/RadarChart';
import { Carregando, ErroTela, Vazio, estilos, useCarregar } from '../../components/ui';
import { colors, radius, spacing, typography } from '../../theme';

/** T06 · Radar (tela principal) e T07 · Modo foco: só os 3 itens mais urgentes (E12 · RN02–RN04). */
export function RadarScreen() {
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>();
  const { usuario } = useAuth();
  const [foco, setFoco] = useState<boolean | undefined>(undefined); // indefinido = preferência do usuário
  const radar = useCarregar(() => radarApi.obter(foco), [foco]);
  const abrir = (id: string) => navigation.navigate('ItemDetail', { itemId: id });

  const d = radar.dados;
  return (
    <SafeAreaView style={estilos.tela} edges={['top']}>
      <ScrollView contentContainerStyle={estilos.conteudo}
                  refreshControl={<RefreshControl refreshing={false} onRefresh={radar.recarregar} tintColor={colors.primary} />}>
        <View style={{ flexDirection: 'row', alignItems: 'center', paddingTop: spacing.md }}>
          <View style={{ flex: 1 }}>
            <Text style={[typography.h1, { color: colors.textPrimary }]} accessibilityRole="header">Seu radar</Text>
            {d && (
              <Text style={estilos.textoApoio}>
                {d.resumo.vencidos} vencido(s) · {d.resumo.atencao} em atenção · {d.resumo.emDia} em dia
              </Text>
            )}
          </View>
          {d && (
            <Pressable onPress={() => setFoco(!d.modoFoco)} accessibilityRole="switch" accessibilityState={{ checked: d.modoFoco }}
                       style={{ flexDirection: 'row', alignItems: 'center', gap: 6, minHeight: 44, paddingHorizontal: spacing.md,
                         borderRadius: radius.full, borderWidth: 1.5, borderColor: colors.primary,
                         backgroundColor: d.modoFoco ? colors.primary : 'transparent' }}>
              <Crosshair size={16} color={d.modoFoco ? '#0F172A' : colors.primary} />
              <Text style={{ color: d.modoFoco ? '#0F172A' : colors.primary, fontWeight: '700' }}>Modo foco</Text>
            </Pressable>
          )}
        </View>

        {radar.carregando && !d && <Carregando />}
        {radar.erro != null && <ErroTela erro={radar.erro} tentarDeNovo={radar.recarregar} />}

        {d && d.itens.length === 0 && (
          <Vazio titulo="Nenhum prazo por aqui ainda." texto={`Que tal fazer o check-up, ${usuario?.nome}?`}
                 acao={<Button title="Fazer o check-up" onPress={() => navigation.navigate('Checkup')} />} />
        )}

        {d && d.itens.length > 0 && (
          <>
            {d.modoFoco && <Text style={[estilos.textoApoio, { color: colors.primary }]}>Modo foco: só os 3 itens mais urgentes. Um de cada vez.</Text>}
            <RadarChart itens={d.itens} aoTocar={abrir} />
            {d.itens.some((i) => i.status !== 'EmDia') && <Text style={estilos.subtitulo}>Precisa de atenção</Text>}
            {d.itens.filter((i) => i.status !== 'EmDia').map((i) => <ItemLinha key={i.id} item={i} aoTocar={() => abrir(i.id)} />)}
            {d.itens.some((i) => i.status === 'EmDia') && <Text style={estilos.subtitulo}>Em dia</Text>}
            {d.itens.filter((i) => i.status === 'EmDia').map((i) => <ItemLinha key={i.id} item={i} aoTocar={() => abrir(i.id)} />)}
          </>
        )}
      </ScrollView>
    </SafeAreaView>
  );
}
