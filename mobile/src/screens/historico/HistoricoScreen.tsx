import React, { useState } from 'react';
import { Pressable, Text, View } from 'react-native';
import { useNavigation } from '@react-navigation/native';
import type { NativeStackNavigationProp } from '@react-navigation/native-stack';
import type { RootStackParamList } from '../../navigation/types';
import { historicoApi } from '../../api/services';
import { CampoData, Carregando, ErroTela, Tela, Vazio, estilos, useCarregar } from '../../components/ui';
import { brParaIso, dataBr, dinheiro, km } from '../../utils/formato';
import { colors, radius, spacing, typography } from '../../theme';

/** T13 · Histórico: conclusões da mais recente para a mais antiga (E25). */
export function HistoricoScreen() {
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>();
  const [de, setDe] = useState('');
  const [ate, setAte] = useState('');
  const deIso = brParaIso(de);
  const ateIso = brParaIso(ate);
  const periodoInvalido = !!deIso && !!ateIso && deIso > ateIso;

  const lista = useCarregar(
    () => (periodoInvalido ? Promise.resolve([]) : historicoApi.listar({ de: deIso, ate: ateIso })),
    [deIso, ateIso, periodoInvalido],
  );

  return (
    <Tela titulo="Histórico">
      <View style={{ flexDirection: 'row', gap: spacing.sm }}>
        <View style={{ flex: 1 }}><CampoData label="De" valor={de} aoMudar={setDe} /></View>
        <View style={{ flex: 1 }}>
          <CampoData label="Até" valor={ate} aoMudar={setAte}
                     erro={periodoInvalido ? 'A data inicial deve ser anterior ou igual à final.' : undefined} />
        </View>
      </View>
      {lista.carregando && !lista.dados && <Carregando />}
      {lista.erro != null && <ErroTela erro={lista.erro} tentarDeNovo={lista.recarregar} />}
      {lista.dados?.length === 0 && <Vazio titulo="Nada concluído neste período." texto="Cada conclusão aparece aqui. Pequenos passos contam!" />}
      {lista.dados?.map((h) => (
        <Pressable key={h.id} onPress={() => navigation.navigate('ItemDetail', { itemId: h.itemId })} accessibilityRole="button"
                   style={{ backgroundColor: colors.surface, borderRadius: radius.md, padding: spacing.md, borderLeftWidth: 4, borderLeftColor: colors.primary }}>
          <Text style={[typography.caption, { color: colors.textMuted }]}>{dataBr(h.concluidoEm)}</Text>
          <Text style={estilos.subtitulo}>{h.itemNome}</Text>
          <Text style={estilos.textoApoio}>
            {[h.km != null && km(h.km), h.valorGasto != null && dinheiro(h.valorGasto),
              h.proximoVencimento && `próximo: ${dataBr(h.proximoVencimento)}`].filter(Boolean).join(' · ')}
          </Text>
          {h.observacao && <Text style={estilos.textoApoio}>{h.observacao}</Text>}
        </Pressable>
      ))}
    </Tela>
  );
}
