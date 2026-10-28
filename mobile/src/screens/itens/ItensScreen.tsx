import React, { useState } from 'react';
import { View } from 'react-native';
import { useNavigation } from '@react-navigation/native';
import type { NativeStackNavigationProp } from '@react-navigation/native-stack';
import type { RootStackParamList } from '../../navigation/types';
import type { StatusItem } from '../../api/types';
import { itensApi } from '../../api/services';
import { FilterPills } from '../../components/FilterPills';
import { ItemLinha } from '../../components/RadarChart';
import { Button } from '../../components/Button';
import { Carregando, ErroTela, Tela, Vazio, useCarregar } from '../../components/ui';
import { nomesCategorias } from '../../utils/formato';
import { spacing } from '../../theme';

const filtrosStatus: { id: StatusItem | 'todos'; label: string }[] = [
  { id: 'todos', label: 'Todos' },
  { id: 'Vencido', label: 'Vencidos' },
  { id: 'Atencao', label: 'Atenção' },
  { id: 'EmDia', label: 'Em dia' },
  { id: 'Encerrado', label: 'Concluídos' },
];

/** T08 · Itens por categoria, com filtros (E13). */
export function ItensScreen() {
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>();
  const [categoria, setCategoria] = useState<number | 0>(0);
  const [status, setStatus] = useState<StatusItem | 'todos'>('todos');
  const lista = useCarregar(
    () => itensApi.listar({ categoriaId: categoria || undefined, status: status === 'todos' ? undefined : status }),
    [categoria, status],
  );

  return (
    <Tela titulo="Itens">
      <FilterPills selectedId={categoria} onSelect={setCategoria}
                   options={[{ id: 0, label: 'Todas' }, ...Object.entries(nomesCategorias).map(([id, nome]) => ({ id: Number(id), label: nome }))]} />
      <FilterPills selectedId={status} onSelect={setStatus} options={filtrosStatus} />
      {lista.carregando && !lista.dados && <Carregando />}
      {lista.erro != null && <ErroTela erro={lista.erro} tentarDeNovo={lista.recarregar} />}
      {lista.dados?.length === 0 && (
        <Vazio titulo="Nenhum item com esses filtros."
               acao={<Button title="Adicionar um item" variant="outline" onPress={() => navigation.navigate('ItemForm', {})} />} />
      )}
      <View style={{ gap: spacing.sm }}>
        {lista.dados?.map((i) => <ItemLinha key={i.id} item={i} aoTocar={() => navigation.navigate('ItemDetail', { itemId: i.id })} />)}
      </View>
    </Tela>
  );
}
