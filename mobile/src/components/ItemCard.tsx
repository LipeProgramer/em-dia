import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity } from 'react-native';
import {
  FileText,
  Car,
  HeartPulse,
  Home,
  Package,
  Layers,
  CheckCircle2,
  Circle,
  AlertCircle,
} from 'lucide-react-native';
import { colors, radius, spacing, typography } from '../theme';
import { Badge } from './Badge';
import type { ItemResponse } from '../api/types';

interface ItemCardProps {
  item: ItemResponse;
  onPress: () => void;
  onToggleComplete?: () => void;
  showBadge?: boolean;
  showDate?: boolean;
}

export function ItemCard({
  item,
  onPress,
  onToggleComplete,
  showBadge = true,
  showDate = true,
}: ItemCardProps) {
  // Escolha do ícone da categoria
  const renderCategoryIcon = () => {
    const iconSize = 20;
    const catId = item.categoriaId;

    let IconComp: any = FileText;
    let iconColor: string = colors.primary;

    if (catId === 1) {
      // Documentos
      IconComp = FileText;
      iconColor = '#38BDF8';
    } else if (catId === 2) {
      // Veículos
      IconComp = Car;
      iconColor = colors.primary;
    } else if (catId === 3) {
      // Saúde
      IconComp = HeartPulse;
      iconColor = '#F472B6';
    } else if (catId === 4) {
      // Casa / Contas
      IconComp = Home;
      iconColor = '#FBBF24';
    } else if (catId === 5) {
      // Produto / Garantia
      IconComp = Package;
      iconColor = '#A78BFA';
    } else {
      IconComp = Layers;
      iconColor = colors.textMuted;
    }

    return (
      <View style={[styles.iconWrapper, { backgroundColor: `${iconColor}15` }]}>
        <IconComp size={iconSize} color={iconColor} strokeWidth={2} />
      </View>
    );
  };

  // Status badge logic
  const isAtrasado = item.diasRestantes < 0;
  const isHoje = item.diasRestantes === 0;
  const isAtencao = item.diasRestantes > 0 && item.diasRestantes <= 7;

  let badgeVariant: 'atrasado' | 'hoje' | 'atencao' | 'emDia' = 'emDia';
  let badgeLabel = 'Em dia';

  if (isAtrasado) {
    badgeVariant = 'atrasado';
    badgeLabel = 'Atrasado';
  } else if (isHoje) {
    badgeVariant = 'hoje';
    badgeLabel = 'Hoje';
  } else if (isAtencao) {
    badgeVariant = 'atencao';
    badgeLabel = `${item.diasRestantes}d`;
  }

  // Formatação da data para formato amigável (ex: "20/out")
  const formatarData = (dataIso: string | null) => {
    if (!dataIso) return '';
    try {
      const d = new Date(dataIso);
      const dia = String(d.getDate()).padStart(2, '0');
      const meses = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];
      return `${dia}/${meses[d.getMonth()]}`;
    } catch {
      return '';
    }
  };

  const subtitulo = [
    item.categoriaNome || 'Geral',
    isAtrasado
      ? `${Math.abs(item.diasRestantes)} dias em atraso`
      : isHoje
      ? 'Vence hoje'
      : `${item.diasRestantes} dias`,
  ]
    .filter(Boolean)
    .join(' · ');

  return (
    <TouchableOpacity
      activeOpacity={0.7}
      onPress={onPress}
      style={[
        styles.card,
        isAtrasado && styles.cardAtrasado,
      ]}
    >
      {/* Ícone da categoria */}
      {renderCategoryIcon()}

      {/* Título e Subtítulo */}
      <View style={styles.textContainer}>
        <Text style={styles.title} numberOfLines={1}>
          {item.nome}
        </Text>
        <Text style={styles.subtitle} numberOfLines={1}>
          {subtitulo}
        </Text>
      </View>

      {/* Lado direito: Data ou Badge */}
      <View style={styles.rightContainer}>
        {showDate && item.dataVencimento && (
          <Text style={styles.dateText}>{formatarData(item.dataVencimento)}</Text>
        )}
        {showBadge && <Badge label={badgeLabel} variant={badgeVariant} />}
      </View>
    </TouchableOpacity>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: colors.surface,
    borderRadius: radius.md + 2,
    borderWidth: 1,
    borderColor: colors.border,
    paddingVertical: spacing.md,
    paddingHorizontal: spacing.md + 2,
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: spacing.sm + 2,
  },
  cardAtrasado: {
    borderColor: 'rgba(239, 68, 68, 0.4)',
  },
  iconWrapper: {
    width: 42,
    height: 42,
    borderRadius: radius.md,
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: spacing.md,
  },
  textContainer: {
    flex: 1,
    justifyContent: 'center',
  },
  title: {
    ...typography.bodyBold,
    color: colors.textPrimary,
    fontSize: 15,
    marginBottom: 2,
  },
  subtitle: {
    ...typography.caption,
    color: colors.textMuted,
  },
  rightContainer: {
    alignItems: 'flex-end',
    justifyContent: 'center',
    marginLeft: spacing.sm,
    gap: 4,
  },
  dateText: {
    ...typography.caption,
    color: colors.textSecondary,
    fontWeight: '600',
  },
});
