import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  RefreshControl,
  TouchableOpacity,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Plus, CheckCircle2, MoreVertical } from 'lucide-react-native';
import type { CompositeScreenProps } from '@react-navigation/native';
import type { BottomTabScreenProps } from '@react-navigation/bottom-tabs';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type {
  MainTabParamList,
  RootStackParamList,
} from '../../navigation/types';
import { colors, radius, spacing, typography } from '../../theme';
import { FilterPills, PillOption } from '../../components/FilterPills';
import { ItemCard } from '../../components/ItemCard';
import { EmptyState } from '../../components/EmptyState';
import { useRadar } from '../../context/RadarContext';

type Props = CompositeScreenProps<
  BottomTabScreenProps<MainTabParamList, 'TarefasTab'>,
  NativeStackScreenProps<RootStackParamList>
>;

type AbaFiltro = 'hoje' | 'semana' | 'todas';

export function TarefasScreen({ navigation }: Props) {
  const { itens, refreshing, carregarDados } = useRadar();
  const [abaAtiva, setAbaAtiva] = useState<AbaFiltro>('hoje');

  // Filtra itens com base na aba selecionada
  const itensFiltrados = itens.filter((item) => {
    if (abaAtiva === 'hoje') {
      return item.diasRestantes <= 0; // Atrasados ou vencendo hoje
    }
    if (abaAtiva === 'semana') {
      return item.diasRestantes <= 7; // Próximos 7 dias
    }
    return true; // Todas
  });

  const opcoesAbas: PillOption[] = [
    {
      id: 'hoje',
      label: 'Hoje',
      count: itens.filter((i) => i.diasRestantes <= 0).length,
    },
    {
      id: 'semana',
      label: 'Esta semana',
      count: itens.filter((i) => i.diasRestantes <= 7).length,
    },
    {
      id: 'todas',
      label: 'Todas',
      count: itens.length,
    },
  ];

  return (
    <SafeAreaView style={styles.safeArea} edges={['top', 'left', 'right']}>
      {/* Header da Tela */}
      <View style={styles.header}>
        <Text style={styles.title}>Tarefas</Text>
        <TouchableOpacity
          activeOpacity={0.7}
          onPress={() => navigation.navigate('AdicionarItem', {})}
          style={styles.moreButton}
        >
          <MoreVertical size={20} color={colors.textSecondary} strokeWidth={2} />
        </TouchableOpacity>
      </View>

      {/* Pílulas de filtro */}
      <FilterPills
        options={opcoesAbas}
        selectedId={abaAtiva}
        onSelect={(id) => setAbaAtiva(id as AbaFiltro)}
        style={styles.pills}
      />

      {/* Lista de tarefas */}
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
            title={
              abaAtiva === 'hoje'
                ? 'Nenhuma tarefa pendente para hoje!'
                : abaAtiva === 'semana'
                ? 'Sem vencimentos para esta semana'
                : 'Nenhum vencimento cadastrado'
            }
            description={
              abaAtiva === 'hoje'
                ? 'Você está em dia com seus compromissos imediatos. Aproveite para relaxar!'
                : 'Mantenha seus compromissos futuros organizados adicionando novos prazos.'
            }
            actionTitle="Adicionar Tarefa"
            onAction={() => navigation.navigate('AdicionarItem', {})}
          />
        ) : (
          itensFiltrados.map((item) => (
            <ItemCard
              key={item.id}
              item={item}
              onPress={() => navigation.navigate('ItemDetail', { itemId: item.id, item })}
            />
          ))
        )}
      </ScrollView>

      {/* Floating Action Button (+) */}
      <TouchableOpacity
        activeOpacity={0.8}
        onPress={() => navigation.navigate('AdicionarItem', {})}
        style={styles.fab}
      >
        <Plus size={26} color="#0F172A" strokeWidth={2.6} />
      </TouchableOpacity>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: colors.background,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: spacing.lg,
    paddingTop: spacing.md,
    paddingBottom: spacing.sm,
  },
  title: {
    ...typography.h1,
    color: colors.textPrimary,
    fontWeight: '800',
  },
  moreButton: {
    width: 36,
    height: 36,
    borderRadius: radius.full,
    alignItems: 'center',
    justifyContent: 'center',
  },
  pills: {
    marginBottom: spacing.md,
  },
  container: {
    paddingHorizontal: spacing.lg,
    paddingBottom: spacing.huge + 40,
  },
  fab: {
    position: 'absolute',
    right: spacing.xl,
    bottom: spacing.xxl,
    width: 56,
    height: 56,
    borderRadius: radius.full,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
    elevation: 8,
    shadowColor: colors.primary,
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.35,
    shadowRadius: 8,
  },
});
