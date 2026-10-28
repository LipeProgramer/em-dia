import React from 'react';
import {
  ScrollView,
  TouchableOpacity,
  Text,
  StyleSheet,
  ViewStyle,
} from 'react-native';
import { colors, radius, spacing, typography } from '../theme';

export interface PillOption {
  id: string | number;
  label: string;
  count?: number;
}

interface FilterPillsProps {
  options: PillOption[];
  selectedId: string | number;
  onSelect: (id: any) => void;
  style?: ViewStyle;
}

export function FilterPills({
  options,
  selectedId,
  onSelect,
  style,
}: FilterPillsProps) {
  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      contentContainerStyle={[styles.scrollContent, style]}
    >
      {options.map((opt) => {
        const isSelected = opt.id === selectedId;
        return (
          <TouchableOpacity
            key={String(opt.id)}
            activeOpacity={0.7}
            onPress={() => onSelect(opt.id)}
            style={[styles.pill, isSelected ? styles.selectedPill : styles.unselectedPill]}
          >
            <Text
              style={[
                styles.label,
                isSelected ? styles.selectedLabel : styles.unselectedLabel,
              ]}
            >
              {opt.label}
              {typeof opt.count === 'number' ? ` (${opt.count})` : ''}
            </Text>
          </TouchableOpacity>
        );
      })}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  scrollContent: {
    paddingVertical: spacing.xs,
    paddingHorizontal: spacing.lg,
    flexDirection: 'row',
    gap: spacing.sm,
  },
  pill: {
    paddingHorizontal: spacing.md + 2,
    paddingVertical: spacing.sm,
    borderRadius: radius.full,
    alignItems: 'center',
    justifyContent: 'center',
  },
  selectedPill: {
    backgroundColor: colors.primary,
  },
  unselectedPill: {
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
  },
  label: {
    ...typography.caption,
    fontWeight: '600',
  },
  selectedLabel: {
    color: '#0F172A',
    fontWeight: '700',
  },
  unselectedLabel: {
    color: colors.textMuted,
  },
});
