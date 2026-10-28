import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { ArrowLeft, Bell, BellRing, Info, CheckCheck } from 'lucide-react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '../../navigation/types';
import { colors, radius, spacing, typography } from '../../theme';
import { FilterPills, PillOption } from '../../components/FilterPills';
import { EmptyState } from '../../components/EmptyState';
import { useRadar } from '../../context/RadarContext';

type Props = NativeStackScreenProps<RootStackParamList, 'Notificacoes'>;

type AbaNotificacao = 'todas' | 'lembretes' | 'sistema';

export function NotificacoesScreen({ navigation }: Props) {
  const { notificacoes } = useRadar();
  const [abaAtiva, setAbaAtiva] = useState<AbaNotificacao>('todas');

  const opcoesAbas: PillOption[] = [
    { id: 'todas', label: 'Todas', count: notificacoes.length },
    {
      id: 'lembretes',
      label: 'Lembretes',
      count: notificacoes.filter((n) => n.tipo === 'lembrete').length,
    },
    {
      id: 'sistema',
      label: 'Sistema',
      count: notificacoes.filter((n) => n.tipo === 'sistema').length,
    },
  ];

  const notificacoesFiltradas = notificacoes.filter((n) => {
    if (abaAtiva === 'lembretes') return n.tipo === 'lembrete';
    if (abaAtiva === 'sistema') return n.tipo === 'sistema';
    return true;
  });

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
        <Text style={styles.headerTitle}>Notificações</Text>
        <View style={{ width: 40 }} />
      </View>

      {/* Pílulas de Filtro */}
      <FilterPills
        options={opcoesAbas}
        selectedId={abaAtiva}
        onSelect={(id) => setAbaAtiva(id as AbaNotificacao)}
        style={styles.pills}
      />

      {/* Lista de Notificações */}
      <ScrollView
        contentContainerStyle={styles.container}
        showsVerticalScrollIndicator={false}
      >
        {notificacoesFiltradas.length === 0 ? (
          <EmptyState
            title="Caixa de notificações limpa"
            description="Nenhuma notificação recente pendente para sua atenção."
          />
        ) : (
          notificacoesFiltradas.map((item) => {
            const isLembrete = item.tipo === 'lembrete';

            return (
              <View key={item.id} style={styles.notifCard}>
                <View
                  style={[
                    styles.iconCircle,
                    {
                      backgroundColor: isLembrete
                        ? 'rgba(0, 200, 150, 0.15)'
                        : 'rgba(56, 189, 248, 0.15)',
                    },
                  ]}
                >
                  {isLembrete ? (
                    <BellRing size={20} color={colors.primary} strokeWidth={2} />
                  ) : (
                    <Info size={20} color="#38BDF8" strokeWidth={2} />
                  )}
                </View>

                <View style={styles.notifContent}>
                  <Text style={styles.notifTitle}>{item.titulo}</Text>
                  <Text style={styles.notifMessage}>{item.mensagem}</Text>
                  <Text style={styles.notifTime}>{item.data}</Text>
                </View>
              </View>
            );
          })
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
  pills: {
    marginVertical: spacing.md,
  },
  container: {
    paddingHorizontal: spacing.lg,
    paddingBottom: spacing.xxxl,
    gap: spacing.sm + 2,
  },
  notifCard: {
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.lg,
    padding: spacing.md,
    flexDirection: 'row',
  },
  iconCircle: {
    width: 40,
    height: 40,
    borderRadius: radius.md,
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: spacing.md,
  },
  notifContent: {
    flex: 1,
  },
  notifTitle: {
    ...typography.bodyBold,
    color: colors.textPrimary,
    fontSize: 14,
  },
  notifMessage: {
    ...typography.caption,
    color: colors.textMuted,
    lineHeight: 18,
    marginTop: 2,
    marginBottom: spacing.xs,
  },
  notifTime: {
    ...typography.tiny,
    color: colors.textDisabled,
  },
});
