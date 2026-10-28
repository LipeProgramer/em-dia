import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { ArrowLeft, CheckCircle2, ShieldCheck, HeartHandshake } from 'lucide-react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '../../navigation/types';
import { colors, radius, spacing, typography } from '../../theme';

type Props = NativeStackScreenProps<RootStackParamList, 'Sobre'>;

export function SobreScreen({ navigation }: Props) {
  return (
    <SafeAreaView style={styles.safeArea}>
      <View style={styles.header}>
        <TouchableOpacity
          activeOpacity={0.7}
          onPress={() => navigation.goBack()}
          style={styles.backButton}
        >
          <ArrowLeft size={22} color={colors.textPrimary} strokeWidth={2} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Sobre o Em Dia</Text>
        <View style={{ width: 40 }} />
      </View>

      <ScrollView contentContainerStyle={styles.container}>
        <View style={styles.logoSection}>
          <View style={styles.logoCircle}>
            <CheckCircle2 size={36} color={colors.primary} strokeWidth={2.5} />
          </View>
          <Text style={styles.appName}>Em Dia</Text>
          <Text style={styles.appTagline}>Radar de vencimentos para mentes neurodivergentes</Text>
          <Text style={styles.appVersion}>Versão 1.0.0 (Build Mobile 2026)</Text>
        </View>

        <View style={styles.card}>
          <Text style={styles.cardTitle}>Nossa Missão</Text>
          <Text style={styles.cardBody}>
            O aplicativo Em Dia foi projetado especificamente para adultos com TDAH e
            rotinas sobrecarregadas. Em vez de listas intermináveis e notificações
            estressantes, nós usamos representações visuais claras, priorização por
            urgência e passos pequenos para transformar o medo do esquecimento em
            paz de espírito.
          </Text>
        </View>

        <View style={styles.card}>
          <Text style={styles.cardTitle}>Engenharia & Arquitetura</Text>
          <Text style={styles.cardBody}>
            • Frontend: React Native + Expo + TypeScript (Mobile Nativo){'\n'}
            • Backend: .NET 10 Web API + PostgreSQL 17{'\n'}
            • Design System: Tokens de alto contraste e Lucide Icons{'\n'}
            • Trabalho Multidisciplinar UNICV Engenharia de Software
          </Text>
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
    paddingTop: spacing.lg,
  },
  logoSection: {
    alignItems: 'center',
    marginBottom: spacing.xxl,
  },
  logoCircle: {
    width: 64,
    height: 64,
    borderRadius: radius.full,
    backgroundColor: 'rgba(0, 200, 150, 0.15)',
    borderWidth: 2,
    borderColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: spacing.md,
  },
  appName: {
    ...typography.h1,
    color: colors.textPrimary,
    fontWeight: '800',
  },
  appTagline: {
    ...typography.caption,
    color: colors.textMuted,
    textAlign: 'center',
    marginTop: 4,
  },
  appVersion: {
    ...typography.tiny,
    color: colors.textDisabled,
    marginTop: 6,
  },
  card: {
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.lg,
    padding: spacing.lg,
    marginBottom: spacing.md,
  },
  cardTitle: {
    ...typography.bodyBold,
    color: colors.primary,
    marginBottom: spacing.xs,
  },
  cardBody: {
    ...typography.body,
    color: colors.textSecondary,
    lineHeight: 22,
  },
});
