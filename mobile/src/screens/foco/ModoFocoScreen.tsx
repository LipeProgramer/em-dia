import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { ArrowLeft, Target, CheckCircle2 } from 'lucide-react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '../../navigation/types';
import { colors, radius, spacing, typography } from '../../theme';
import { ItemCard } from '../../components/ItemCard';
import { Button } from '../../components/Button';
import { EmptyState } from '../../components/EmptyState';
import { useRadar } from '../../context/RadarContext';

type Props = NativeStackScreenProps<RootStackParamList, 'ModoFoco'>;

export function ModoFocoScreen({ navigation }: Props) {
  const { itensFoco, modoFocoAtivo, alternarModoFoco } = useRadar();

  return (
    <SafeAreaView style={styles.safeArea}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity
          activeOpacity={0.7}
          onPress={() => navigation.goBack()}
          style={styles.backButton}
        >
          <ArrowLeft size={22} color={colors.textPrimary} strokeWidth={2} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Modo foco</Text>
        <View style={{ width: 40 }} />
      </View>

      <ScrollView
        contentContainerStyle={styles.container}
        showsVerticalScrollIndicator={false}
      >
        {/* Subtítulo TDAH */}
        <Text style={styles.subtitle}>
          Mostre apenas os 3 itens mais importantes para você resolver agora, sem se
          sentir sobrecarregado.
        </Text>

        {/* Alvo Central Visual */}
        <View style={styles.targetWrapper}>
          <View style={styles.targetOuterCircle}>
            <View style={styles.targetInnerCircle}>
              <Target size={42} color={colors.primary} strokeWidth={2} />
            </View>
          </View>
        </View>

        {/* 3 Itens Prioritários */}
        <View style={styles.itemsSection}>
          <Text style={styles.sectionTitle}>Prioridades Máximas</Text>

          {itensFoco.length === 0 ? (
            <EmptyState
              title="Sem itens urgentes no momento!"
              description="Parabéns! Você não possui nenhuma pendência crítica no radar agora."
            />
          ) : (
            itensFoco.map((item) => (
              <ItemCard
                key={item.id}
                item={item}
                showBadge={true}
                showDate={false}
                onPress={() => navigation.navigate('ItemDetail', { itemId: item.id, item })}
              />
            ))
          )}
        </View>

        {/* Botão de Ação */}
        <View style={styles.actions}>
          <Button
            title={modoFocoAtivo ? 'Desativar modo foco' : 'Ativar modo foco'}
            onPress={alternarModoFoco}
            variant={modoFocoAtivo ? 'secondary' : 'primary'}
            size="lg"
          />

          <TouchableOpacity
            activeOpacity={0.7}
            onPress={() => navigation.navigate('MainTabs', { screen: 'TarefasTab' })}
            style={styles.linkVerTudo}
          >
            <Text style={styles.linkVerTudoText}>Ver tudo</Text>
          </TouchableOpacity>
        </View>
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
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: spacing.lg,
    paddingVertical: spacing.sm,
  },
  backButton: {
    width: 40,
    height: 40,
    borderRadius: radius.md,
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    alignItems: 'center',
    justifyContent: 'center',
  },
  headerTitle: {
    ...typography.h3,
    color: colors.textPrimary,
    fontWeight: '700',
  },
  container: {
    paddingHorizontal: spacing.lg,
    paddingBottom: spacing.xxxl,
  },
  subtitle: {
    ...typography.body,
    color: colors.textMuted,
    textAlign: 'center',
    lineHeight: 22,
    marginTop: spacing.sm,
    marginBottom: spacing.xl,
    paddingHorizontal: spacing.md,
  },
  targetWrapper: {
    alignItems: 'center',
    justifyContent: 'center',
    marginVertical: spacing.lg,
  },
  targetOuterCircle: {
    width: 110,
    height: 110,
    borderRadius: radius.full,
    backgroundColor: 'rgba(0, 200, 150, 0.08)',
    borderWidth: 1.5,
    borderColor: 'rgba(0, 200, 150, 0.25)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  targetInnerCircle: {
    width: 76,
    height: 76,
    borderRadius: radius.full,
    backgroundColor: 'rgba(0, 200, 150, 0.18)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  itemsSection: {
    marginTop: spacing.xl,
    gap: spacing.xs,
  },
  sectionTitle: {
    ...typography.caption,
    color: colors.textSecondary,
    fontWeight: '700',
    textTransform: 'uppercase',
    letterSpacing: 0.5,
    marginBottom: spacing.sm,
  },
  actions: {
    marginTop: spacing.xxl,
    gap: spacing.md,
  },
  linkVerTudo: {
    paddingVertical: spacing.sm,
    alignItems: 'center',
  },
  linkVerTudoText: {
    ...typography.bodyBold,
    color: colors.textSecondary,
  },
});
