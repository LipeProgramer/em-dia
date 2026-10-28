import React from 'react';
import { View, Text, StyleSheet, ViewStyle } from 'react-native';
import { colors, radius, spacing, typography } from '../theme';

export type BadgeVariant = 'emDia' | 'atrasado' | 'atencao' | 'hoje' | 'proximo' | 'neutro';

interface BadgeProps {
  label: string;
  variant?: BadgeVariant;
  style?: ViewStyle;
}

export function Badge({ label, variant = 'neutro', style }: BadgeProps) {
  return (
    <View style={[styles.badge, styles[variant], style]}>
      <Text style={[styles.text, styles[`text_${variant}`]]}>{label}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  badge: {
    paddingHorizontal: spacing.sm + 2,
    paddingVertical: spacing.xxs + 1,
    borderRadius: radius.full,
    alignSelf: 'flex-start',
    alignItems: 'center',
    justifyContent: 'center',
  },
  text: {
    ...typography.tiny,
    fontWeight: '600',
  },
  // Variantes
  emDia: {
    backgroundColor: colors.successMuted,
  },
  text_emDia: {
    color: colors.success,
  },
  atrasado: {
    backgroundColor: colors.errorMuted,
  },
  text_atrasado: {
    color: colors.error,
  },
  atencao: {
    backgroundColor: colors.warningMuted,
  },
  text_atencao: {
    color: colors.warning,
  },
  hoje: {
    backgroundColor: colors.warningMuted,
  },
  text_hoje: {
    color: colors.warning,
  },
  proximo: {
    backgroundColor: colors.primaryMuted,
  },
  text_proximo: {
    color: colors.primary,
  },
  neutro: {
    backgroundColor: 'rgba(107, 114, 128, 0.2)',
  },
  text_neutro: {
    color: colors.textMuted,
  },
});
