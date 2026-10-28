import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import Svg, { Circle, G } from 'react-native-svg';
import { colors, radius, spacing, typography } from '../theme';

interface RadarDonutProps {
  atrasados: number;
  hoje: number;
  proximos: number;
  emDia: number;
  onPressCenter?: () => void;
}

export function RadarDonut({
  atrasados,
  hoje,
  proximos,
  emDia,
}: RadarDonutProps) {
  const total = atrasados + hoje + proximos + emDia;

  // Dimensões do Donut
  const size = 130;
  const strokeWidth = 14;
  const center = size / 2;
  const r = (size - strokeWidth) / 2;
  const circumference = 2 * Math.PI * r;

  // Porcentagens para as fatias do círculo
  const safeTotal = total > 0 ? total : 1;
  const pAtrasados = (atrasados / safeTotal) * circumference;
  const pHoje = (hoje / safeTotal) * circumference;
  const pProximos = (proximos / safeTotal) * circumference;
  const pEmDia = (emDia / safeTotal) * circumference;

  // Offsets acumulados (começando do topo -90 graus)
  const offsetAtrasados = 0;
  const offsetHoje = -pAtrasados;
  const offsetProximos = -(pAtrasados + pHoje);
  const offsetEmDia = -(pAtrasados + pHoje + pProximos);

  const temAtraso = atrasados > 0;
  const textoDestaque = temAtraso
    ? `${atrasados}`
    : hoje > 0
    ? `${hoje}`
    : total === 0
    ? '0'
    : '✓';
  const legendaDestaque = temAtraso
    ? 'em atraso'
    : hoje > 0
    ? 'para hoje'
    : total === 0
    ? 'sem itens'
    : 'tudo em dia';

  return (
    <View style={styles.card}>
      {/* Donut Chart */}
      <View style={styles.chartContainer}>
        <Svg width={size} height={size}>
          <G rotation="-90" origin={`${center}, ${center}`}>
            {/* Background ring */}
            <Circle
              cx={center}
              cy={center}
              r={r}
              stroke="#2A3749"
              strokeWidth={strokeWidth}
              fill="none"
            />
            {total > 0 && (
              <>
                {/* Em dia (Verde) */}
                {emDia > 0 && (
                  <Circle
                    cx={center}
                    cy={center}
                    r={r}
                    stroke={colors.success}
                    strokeWidth={strokeWidth}
                    strokeDasharray={`${pEmDia} ${circumference}`}
                    strokeDashoffset={offsetEmDia}
                    strokeLinecap="round"
                    fill="none"
                  />
                )}
                {/* Próximos (Ciano / Esmeralda) */}
                {proximos > 0 && (
                  <Circle
                    cx={center}
                    cy={center}
                    r={r}
                    stroke={colors.primary}
                    strokeWidth={strokeWidth}
                    strokeDasharray={`${pProximos} ${circumference}`}
                    strokeDashoffset={offsetProximos}
                    strokeLinecap="round"
                    fill="none"
                  />
                )}
                {/* Hoje (Âmbar) */}
                {hoje > 0 && (
                  <Circle
                    cx={center}
                    cy={center}
                    r={r}
                    stroke={colors.warning}
                    strokeWidth={strokeWidth}
                    strokeDasharray={`${pHoje} ${circumference}`}
                    strokeDashoffset={offsetHoje}
                    strokeLinecap="round"
                    fill="none"
                  />
                )}
                {/* Atrasados (Vermelho) */}
                {atrasados > 0 && (
                  <Circle
                    cx={center}
                    cy={center}
                    r={r}
                    stroke={colors.error}
                    strokeWidth={strokeWidth}
                    strokeDasharray={`${pAtrasados} ${circumference}`}
                    strokeDashoffset={offsetAtrasados}
                    strokeLinecap="round"
                    fill="none"
                  />
                )}
              </>
            )}
          </G>
        </Svg>

        {/* Informação no centro do Donut */}
        <View style={styles.centerTextContainer}>
          <Text
            style={[
              styles.centerNumber,
              temAtraso ? { color: colors.error } : { color: colors.primary },
            ]}
          >
            {textoDestaque}
          </Text>
          <Text style={styles.centerLabel}>{legendaDestaque}</Text>
        </View>
      </View>

      {/* Legenda Lateral */}
      <View style={styles.legendContainer}>
        <View style={styles.legendRow}>
          <View style={styles.legendLeft}>
            <View style={[styles.dot, { backgroundColor: colors.error }]} />
            <Text style={styles.legendLabel}>Atrasados</Text>
          </View>
          <Text style={[styles.legendValue, { color: colors.error }]}>{atrasados}</Text>
        </View>

        <View style={styles.legendRow}>
          <View style={styles.legendLeft}>
            <View style={[styles.dot, { backgroundColor: colors.warning }]} />
            <Text style={styles.legendLabel}>Hoje</Text>
          </View>
          <Text style={[styles.legendValue, { color: colors.warning }]}>{hoje}</Text>
        </View>

        <View style={styles.legendRow}>
          <View style={styles.legendLeft}>
            <View style={[styles.dot, { backgroundColor: colors.primary }]} />
            <Text style={styles.legendLabel}>Próximos</Text>
          </View>
          <Text style={[styles.legendValue, { color: colors.primary }]}>{proximos}</Text>
        </View>

        <View style={styles.legendRow}>
          <View style={styles.legendLeft}>
            <View style={[styles.dot, { backgroundColor: colors.success }]} />
            <Text style={styles.legendLabel}>Em dia</Text>
          </View>
          <Text style={[styles.legendValue, { color: colors.success }]}>{emDia}</Text>
        </View>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.lg,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: spacing.lg,
  },
  chartContainer: {
    width: 130,
    height: 130,
    alignItems: 'center',
    justifyContent: 'center',
  },
  centerTextContainer: {
    position: 'absolute',
    alignItems: 'center',
    justifyContent: 'center',
  },
  centerNumber: {
    ...typography.h1,
    lineHeight: 32,
    fontWeight: '800',
  },
  centerLabel: {
    ...typography.tiny,
    color: colors.textMuted,
    marginTop: -2,
  },
  legendContainer: {
    flex: 1,
    marginLeft: spacing.lg,
    gap: spacing.sm,
  },
  legendRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  legendLeft: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  dot: {
    width: 8,
    height: 8,
    borderRadius: 4,
    marginRight: spacing.sm,
  },
  legendLabel: {
    ...typography.caption,
    color: colors.textSecondary,
  },
  legendValue: {
    ...typography.caption,
    fontWeight: '700',
  },
});
