import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  RefreshControl,
  TouchableOpacity,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Target, ChevronRight, PlusCircle } from 'lucide-react-native';
import type { BottomTabScreenProps } from '@react-navigation/bottom-tabs';
import type { CompositeScreenProps } from '@react-navigation/native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type {
  MainTabParamList,
  RootStackParamList,
} from '../../navigation/types';
import { colors, radius, spacing, typography } from '../../theme';
import { Header } from '../../components/Header';
import { RadarDonut } from '../../components/RadarDonut';
import { ItemCard } from '../../components/ItemCard';
import { EmptyState } from '../../components/EmptyState';
import { useRadar } from '../../context/RadarContext';

type Props = CompositeScreenProps<
  BottomTabScreenProps<MainTabParamList, 'RadarTab'>,
  NativeStackScreenProps<RootStackParamList>
>;

export function RadarScreen({ navigation }: Props) {
  const { itens, counts, refreshing, carregarDados } = useRadar();

  // Próximos vencimentos ordenados por urgência (atrasados primeiro, depois próximos)
  const proximosItens = [...itens]
    .sort((a, b) => a.diasRestantes - b.diasRestantes)
    .slice(0, 5);

  return (
    <SafeAreaView style={styles.safeArea} edges={['top', 'left', 'right']}>
      {/* Header com avatar e notificações */}
      <Header
        onPressNotifications={() => navigation.navigate('Notificacoes')}
        onPressProfile={() => navigation.navigate('MainTabs', { screen: 'PerfilTab' })}
      />

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
        {/* Gráfico do Radar (Donut) */}
        <RadarDonut
          atrasados={counts.atrasados}
          hoje={counts.hoje}
          proximos={counts.proximos}
          emDia={counts.emDia}
        />

        {/* Card de Acesso Rápido ao Modo Foco */}
        <TouchableOpacity
          activeOpacity={0.8}
          onPress={() => navigation.navigate('ModoFoco')}
          style={styles.focusCard}
        >
          <View style={styles.focusIconWrapper}>
            <Target size={22} color={colors.primary} strokeWidth={2.2} />
          </View>
          <View style={styles.focusTextContainer}>
            <Text style={styles.focusTitle}>Modo Foco Anti-Procrastinação</Text>
            <Text style={styles.focusSubtitle}>
              Foque apenas nos 3 itens mais urgentes agora sem sobrecarga.
            </Text>
          </View>
          <ChevronRight size={18} color={colors.textMuted} />
        </TouchableOpacity>

        {/* Seção Próximos Vencimentos */}
        <View style={styles.sectionHeader}>
          <Text style={styles.sectionTitle}>Próximos vencimentos</Text>
          <TouchableOpacity
            activeOpacity={0.7}
            onPress={() => navigation.navigate('TarefasTab')}
          >
            <Text style={styles.sectionLink}>Ver todos</Text>
          </TouchableOpacity>
        </View>

        {/* Lista de Itens */}
        {proximosItens.length === 0 ? (
          <EmptyState
            title="Nenhum vencimento no radar"
            description="Tudo calmo por aqui! Que tal cadastrar seu primeiro compromisso para não esquecer?"
            actionTitle="Adicionar Vencimento"
            onAction={() => navigation.navigate('AdicionarItem', {})}
          />
        ) : (
          proximosItens.map((item) => (
            <ItemCard
              key={item.id}
              item={item}
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
  container: {
    paddingHorizontal: spacing.lg,
    paddingBottom: spacing.xxxl,
  },
  focusCard: {
    backgroundColor: 'rgba(0, 200, 150, 0.08)',
    borderWidth: 1.2,
    borderColor: 'rgba(0, 200, 150, 0.3)',
    borderRadius: radius.lg,
    padding: spacing.md + 2,
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: spacing.xl,
  },
  focusIconWrapper: {
    width: 40,
    height: 40,
    borderRadius: radius.md,
    backgroundColor: 'rgba(0, 200, 150, 0.15)',
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: spacing.md,
  },
  focusTextContainer: {
    flex: 1,
    paddingRight: spacing.sm,
  },
  focusTitle: {
    ...typography.bodyBold,
    color: colors.primary,
    fontSize: 14,
  },
  focusSubtitle: {
    ...typography.caption,
    color: colors.textMuted,
    marginTop: 2,
  },
  sectionHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: spacing.md,
  },
  sectionTitle: {
    ...typography.h3,
    color: colors.textPrimary,
    fontWeight: '700',
  },
  sectionLink: {
    ...typography.caption,
    color: colors.primary,
    fontWeight: '700',
  },
});
