import React from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import Svg, { Circle, G, Line, Polygon, Rect } from 'react-native-svg';
import type { AnelRadar, ItemResumo, StatusItem } from '../api/types';
import { colors, radius, spacing, typography } from '../theme';
import { nomesCategorias } from '../utils/formato';
import { StatusBadge, corStatus, textoStatus } from './ui';

// Radar (T06): 6 fatias (uma por área) e 4 anéis (RN03). Forma diferente por status — não depende só da cor.

const faixas: Record<AnelRadar, [number, number]> = {
  Centro: [0, 22], Anel1: [26, 54], Anel2: [58, 84], Externo: [88, 110],
};
const TAMANHO = 240;
const C = TAMANHO / 2;

function Marcador({ status, x, y }: { status: StatusItem; x: number; y: number }) {
  const cor = corStatus(status);
  if (status === 'Vencido') return <Rect x={x - 6} y={y - 6} width={12} height={12} fill={cor} rotation={45} originX={x} originY={y} stroke="#fff" strokeWidth={1.5} />;
  if (status === 'Atencao') return <Polygon points={`${x},${y - 7} ${x + 7},${y + 6} ${x - 7},${y + 6}`} fill={cor} stroke="#fff" strokeWidth={1.5} />;
  return <Circle cx={x} cy={y} r={6} fill={cor} stroke="#fff" strokeWidth={1.5} />;
}

export function RadarChart({ itens, aoTocar }: { itens: ItemResumo[]; aoTocar: (id: string) => void }) {
  const porFatia = new Map<number, ItemResumo[]>();
  for (const i of itens) porFatia.set(i.categoriaId, [...(porFatia.get(i.categoriaId) ?? []), i]);

  const pontos = itens.map((item) => {
    const daFatia = porFatia.get(item.categoriaId)!;
    const inicio = ((item.categoriaId - 1) / 6) * 2 * Math.PI - Math.PI / 2;
    const angulo = inicio + ((daFatia.indexOf(item) + 1) / (daFatia.length + 1)) * (2 * Math.PI / 6);
    const [min, max] = faixas[item.anel];
    const dias = item.diasRestantes ?? 0;
    const fracao = item.anel === 'Centro' ? 0.6
      : item.anel === 'Anel1' ? Math.min(Math.max(dias, 0), 30) / 30
      : item.anel === 'Anel2' ? (Math.min(dias, 90) - 31) / 59
      : Math.min((dias - 90) / 275, 1);
    const r = min + (max - min) * Math.max(0, Math.min(fracao, 1));
    return { item, x: C + r * Math.cos(angulo), y: C + r * Math.sin(angulo) };
  });

  return (
    <View style={estilos.caixa} accessible accessibilityLabel={`Radar com ${itens.length} itens. A lista abaixo traz cada item.`}>
      <Svg width="100%" height={260} viewBox={`0 0 ${TAMANHO} ${TAMANHO}`}>
        <Circle cx={C} cy={C} r={110} fill="none" stroke={colors.border} />
        <Circle cx={C} cy={C} r={86} fill="none" stroke={colors.border} />
        <Circle cx={C} cy={C} r={56} fill={colors.warningMuted} stroke={colors.border} />
        <Circle cx={C} cy={C} r={24} fill={colors.errorMuted} stroke={colors.border} />
        {Array.from({ length: 6 }, (_, i) => {
          const a = (i / 6) * 2 * Math.PI - Math.PI / 2;
          return <Line key={i} x1={C} y1={C} x2={C + 110 * Math.cos(a)} y2={C + 110 * Math.sin(a)} stroke={colors.border} />;
        })}
        {pontos.map(({ item, x, y }) => (
          <G key={item.id} onPress={() => aoTocar(item.id)}>
            <Circle cx={x} cy={y} r={14} fill="transparent" />
            <Marcador status={item.status} x={x} y={y} />
          </G>
        ))}
      </Svg>
      <View style={estilos.legenda}>
        <Text style={estilos.legendaTexto}>◆ Vencido (centro)</Text>
        <Text style={estilos.legendaTexto}>▲ Atenção</Text>
        <Text style={estilos.legendaTexto}>● Em dia</Text>
      </View>
    </View>
  );
}

/** Linha de item: status (cor + símbolo + texto), nome e prazo. Toque abre o detalhe. */
export function ItemLinha({ item, aoTocar }: { item: ItemResumo; aoTocar: () => void }) {
  return (
    <Pressable onPress={aoTocar} accessibilityRole="button"
               accessibilityLabel={`${item.nome}, ${nomesCategorias[item.categoriaId]}: ${textoStatus(item.status)}, ${item.textoPrazo}`}
               style={[estilos.linha, { borderLeftColor: corStatus(item.status) }]}>
      <View style={{ flex: 1, gap: 4 }}>
        <Text style={estilos.nome}>{item.nome}</Text>
        <StatusBadge status={item.status} />
      </View>
      <Text style={estilos.prazo}>{item.textoPrazo}</Text>
    </Pressable>
  );
}

const estilos = StyleSheet.create({
  caixa: { backgroundColor: colors.surface, borderRadius: radius.lg, padding: spacing.md, borderWidth: 1, borderColor: colors.border },
  legenda: { flexDirection: 'row', justifyContent: 'center', gap: spacing.lg, marginTop: spacing.sm },
  legendaTexto: { ...typography.caption, color: colors.textMuted },
  linha: { flexDirection: 'row', alignItems: 'center', gap: spacing.md, minHeight: 60, padding: spacing.md, backgroundColor: colors.surface, borderRadius: radius.md, borderLeftWidth: 5, borderWidth: 1, borderColor: colors.border },
  nome: { ...typography.bodyBold, color: colors.textPrimary },
  prazo: { ...typography.caption, color: colors.textMuted, textAlign: 'right', maxWidth: 130 },
});
