import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  RefreshControl,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import type { CompositeScreenProps } from '@react-navigation/native';
import type { BottomTabScreenProps } from '@react-navigation/bottom-tabs';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type {
  MainTabParamList,
  RootStackParamList,
} from '../../navigation/types';
import { colors, spacing, typography } from '../../theme';
import { FilterPills, PillOption } from '../../components/FilterPills';
import { ItemCard } from '../../components/ItemCard';
import { EmptyState } from '../../components/EmptyState';
import { useRadar } from '../../context/RadarContext';

type Props = CompositeScreenProps<
  BottomTabScreenProps<MainTabParamList, 'CarteiraTab'>,
  NativeStackScreenProps<RootStackParamList>
>;

export function CarteiraScreen({ navigation }: Props) {
  const { itens, refreshing, carregarDados } = useRadar();
  const [categoriaAtiva, setCategoriaAtiva] = useState<number | 'todas'>('todas');

  const opcoesCategorias: PillOption[] = [
    { id: 'todas', label: 'Todos', count: itens.length },
    { id: 1, label: 'Documentos', count: itens.filter((i) => i.categoriaId === 1).length },
    { id: 2, label: 'Veículos', count: itens.filter((i) => i.categoriaId === 2).length },
    { id: 3, label: 'Saúde', count: itens.filter((i) => i.categoriaId === 3).length },
    { id: 4, label: 'Casa', count: itens.filter((i) => i.categoriaId === 4).length },
    { id: 5, label: 'Produtos', count: itens.filter((i) => i.categoriaId === 5).length },
  ];

  const itensFiltrados = itens.filter((item) => {
    if (categoriaAtiva === 'todas') return true;
    return item.categoriaId === categoriaAtiva;
  });

  return (
    <SafeAreaView style={styles.safeArea} edges={['top', 'left', 'right']}>
      {/* Header */}
      <View style={styles.header}>
        <Text style={styles.title}>Carteira</Text>
        <Text style={styles.subtitle}>
          Todos os seus documentos, bens e obrigações reunidos.
        </Text>
      </View>

      {/* Pílulas de Categoria */}
      <FilterPills
        options={opcoesCategorias}
        selectedId={categoriaAtiva}
        onSelect={(id) => setCategoriaAtiva(id as number | 'todas')}
        style={styles.pills}
      />

      {/* Lista de Itens da Carteira */}
      <ScrollView
        contentContainerStyle={styles.container}
        showsVerticalScrollIndicator={false}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={carregarDados}
            tintColor={colors.primary}
            colors={[colors.primary]}
          />
        }
      >
        {itensFiltrados.length === 0 ? (
          <EmptyState
            title="Nenhum item nesta categoria"
            description="Organize seus documentos, veículos ou contas adicionando um novo item."
            actionTitle="Adicionar Item"
            onAction={() =>
              navigation.navigate('AdicionarItem', {
                categoriaId: typeof categoriaAtiva === 'number' ? categoriaAtiva : undefined,
              })
            }
          />
        ) : (
          itensFiltrados.map((item) => (
            <ItemCard
              key={item.id}
              item={item}
              showBadge={true}
              showDate={true}
              onPress={() => navigation.navigate('ItemDetail', { itemId: item.id, item })}
            />
          ))
        )}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: colors.background,
  },
  header: {
    paddingHorizontal: spacing.lg,
    paddingTop: spacing.md,
    paddingBottom: spacing.xs,
  },
  title: {
    ...typography.h1,
    color: colors.textPrimary,
    fontWeight: '800',
  },
  subtitle: {
    ...typography.caption,
    color: colors.textMuted,
    marginTop: 2,
  },
  pills: {
    marginVertical: spacing.md,
  },
  container: {
    paddingHorizontal: spacing.lg,
    paddingBottom: spacing.huge,
  },
});
