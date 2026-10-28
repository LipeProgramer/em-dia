import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  Switch,
  Alert,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import {
  ArrowLeft,
  MoreVertical,
  Car,
  FileText,
  HeartPulse,
  Home,
  Package,
  Layers,
  Calendar,
  DollarSign,
  Globe,
  Bell,
  CheckCircle2,
  Circle,
  Trash2,
} from 'lucide-react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '../../navigation/types';
import { colors, radius, spacing, typography } from '../../theme';
import { Badge } from '../../components/Badge';
import { Button } from '../../components/Button';
import { useRadar } from '../../context/RadarContext';
import { itensApi, passosApi } from '../../api/services';
import type { ItemResponse } from '../../api/types';

type Props = NativeStackScreenProps<RootStackParamList, 'ItemDetail'>;

export function ItemDetailScreen({ route, navigation }: Props) {
  const { itemId, item: itemParam } = route.params;
  const { itens, concluirItem, carregarDados } = useRadar();
  const [item, setItem] = useState<ItemResponse | null>(
    itemParam || itens.find((i) => i.id === itemId) || null
  );
  const [lembreteAtivado, setLembreteAtivado] = useState(true);
  const [loadingAcao, setLoadingAcao] = useState(false);

  useEffect(() => {
    itensApi.obterPorId(itemId).then(setItem).catch(() => {});
  }, [itemId]);

  if (!item) {
    return (
      <SafeAreaView style={styles.safeArea}>
        <View style={styles.loadingContainer}>
          <Text style={styles.loadingText}>Carregando detalhes do vencimento...</Text>
        </View>
      </SafeAreaView>
    );
  }

  const isAtrasado = item.diasRestantes < 0;
  const isHoje = item.diasRestantes === 0;

  let badgeVariant: 'atrasado' | 'hoje' | 'emDia' = 'emDia';
  let badgeLabel = 'Em dia';

  if (isAtrasado) {
    badgeVariant = 'atrasado';
    badgeLabel = 'Atrasado';
  } else if (isHoje) {
    badgeVariant = 'hoje';
    badgeLabel = 'Vence Hoje';
  }

  // Categoria e Ícone
  const renderBigIcon = () => {
    const size = 36;
    if (item.categoriaId === 2) return <Car size={size} color={colors.primary} strokeWidth={2} />;
    if (item.categoriaId === 1) return <FileText size={size} color="#38BDF8" strokeWidth={2} />;
    if (item.categoriaId === 3) return <HeartPulse size={size} color="#F472B6" strokeWidth={2} />;
    if (item.categoriaId === 4) return <Home size={size} color="#FBBF24" strokeWidth={2} />;
    if (item.categoriaId === 5) return <Package size={size} color="#A78BFA" strokeWidth={2} />;
    return <Layers size={size} color={colors.primary} strokeWidth={2} />;
  };

  const formatarDataCompleta = (dataIso: string | null) => {
    if (!dataIso) return 'Não informado';
    try {
      const d = new Date(dataIso);
      return d.toLocaleDateString('pt-BR', {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
      });
    } catch {
      return dataIso;
    }
  };

  const handleConcluir = async () => {
    try {
      setLoadingAcao(true);
      await concluirItem(item.id);
      Alert.alert('Sucesso!', 'Vencimento marcado como resolvido. Menos um peso na sua mente!');
      navigation.goBack();
    } catch {
      Alert.alert('Erro', 'Não foi possível concluir o vencimento.');
    } finally {
      setLoadingAcao(false);
    }
  };

  const handleExcluir = () => {
    Alert.alert(
      'Excluir Vencimento',
      'Tem certeza de que deseja remover este vencimento do seu radar?',
      [
        { text: 'Cancelar', style: 'cancel' },
        {
          text: 'Excluir',
          style: 'destructive',
          onPress: async () => {
            try {
              await itensApi.excluir(item.id);
              await carregarDados();
              navigation.goBack();
            } catch {
              Alert.alert('Erro', 'Não foi possível excluir o vencimento.');
            }
          },
        },
      ]
    );
  };

  const handleTogglePasso = async (passoId: string, concluidoAtual: boolean) => {
    try {
      await passosApi.marcarPasso(item.id, passoId, !concluidoAtual);
      // Atualiza estado local
      setItem((prev) => {
        if (!prev) return null;
        return {
          ...prev,
          passos: prev.passos.map((p) =>
            p.id === passoId ? { ...p, concluido: !concluidoAtual } : p
          ),
        };
      });
    } catch {
      // Falha silenciosa ou retry
    }
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      {/* Top Header */}
      <View style={styles.topHeader}>
        <TouchableOpacity
          activeOpacity={0.7}
          onPress={() => navigation.goBack()}
          style={styles.headerButton}
        >
          <ArrowLeft size={22} color={colors.textPrimary} strokeWidth={2} />
        </TouchableOpacity>

        <TouchableOpacity
          activeOpacity={0.7}
          onPress={handleExcluir}
          style={styles.headerButton}
        >
          <Trash2 size={20} color={colors.error} strokeWidth={2} />
        </TouchableOpacity>
      </View>

      <ScrollView
        contentContainerStyle={styles.container}
        showsVerticalScrollIndicator={false}
      >
        {/* Banner Central do Item */}
        <View style={styles.heroCard}>
          <View style={styles.bigIconCircle}>{renderBigIcon()}</View>
          <Text style={styles.itemTitle}>{item.nome}</Text>
          <Text style={styles.itemSubtitle}>
            {item.categoriaNome || 'Geral'} ·{' '}
            {isAtrasado
              ? `${Math.abs(item.diasRestantes)} dias em atraso`
              : isHoje
              ? 'Vence hoje'
              : `${item.diasRestantes} dias restantes`}
          </Text>
          <Badge label={badgeLabel} variant={badgeVariant} style={styles.badge} />
        </View>

        {/* Card de Informações */}
        <View style={styles.infoSection}>
          <Text style={styles.sectionHeading}>Informações</Text>

          <View style={styles.infoCard}>
            <View style={styles.infoRow}>
              <View style={styles.infoLabelContainer}>
                <Calendar size={18} color={colors.textMuted} strokeWidth={2} />
                <Text style={styles.infoLabel}>Vencimento</Text>
              </View>
              <Text style={styles.infoValue}>
                {formatarDataCompleta(item.dataVencimento)}
              </Text>
            </View>

            <View style={styles.divider} />

            <View style={styles.infoRow}>
              <View style={styles.infoLabelContainer}>
                <DollarSign size={18} color={colors.textMuted} strokeWidth={2} />
                <Text style={styles.infoLabel}>Valor estimado</Text>
              </View>
              <Text style={styles.infoValue}>
                {item.valorEstimado
                  ? `R$ ${item.valorEstimado.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}`
                  : 'R$ 0,00'}
              </Text>
            </View>

            <View style={styles.divider} />

            <View style={styles.infoRow}>
              <View style={styles.infoLabelContainer}>
                <Globe size={18} color={colors.textMuted} strokeWidth={2} />
                <Text style={styles.infoLabel}>Onde pagar</Text>
              </View>
              <Text style={styles.infoValue}>
                {item.observacoes || 'Site oficial / App bancário'}
              </Text>
            </View>
          </View>
        </View>

        {/* Checklist / Passos interativos para TDAH */}
        {item.passos && item.passos.length > 0 && (
          <View style={styles.passosSection}>
            <Text style={styles.sectionHeading}>Etapas para resolver</Text>
            <View style={styles.passosCard}>
              {item.passos.map((passo) => (
                <TouchableOpacity
                  key={passo.id}
                  activeOpacity={0.7}
                  onPress={() => handleTogglePasso(passo.id, passo.concluido)}
                  style={styles.passoRow}
                >
                  {passo.concluido ? (
                    <CheckCircle2 size={20} color={colors.primary} strokeWidth={2.2} />
                  ) : (
                    <Circle size={20} color={colors.textMuted} strokeWidth={2} />
                  )}
                  <Text
                    style={[
                      styles.passoText,
                      passo.concluido && styles.passoConcluido,
                    ]}
                  >
                    {passo.descricao}
                  </Text>
                </TouchableOpacity>
              ))}
            </View>
          </View>
        )}

        {/* Switch de Lembretes */}
        <View style={styles.switchCard}>
          <View style={styles.switchLeft}>
            <Bell size={20} color={colors.primary} strokeWidth={2} />
            <Text style={styles.switchLabel}>Lembrete ativado</Text>
          </View>
          <Switch
            value={lembreteAtivado}
            onValueChange={setLembreteAtivado}
            trackColor={{ false: colors.border, true: colors.primaryDark }}
            thumbColor={lembreteAtivado ? colors.primary : colors.textMuted}
          />
        </View>

        {/* Ações */}
        <View style={styles.actions}>
          <Button
            title="Marcar como resolvido"
            onPress={handleConcluir}
            variant="primary"
            size="lg"
            loading={loadingAcao}
          />
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
  loadingContainer: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  loadingText: {
    ...typography.body,
    color: colors.textMuted,
  },
  topHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: spacing.lg,
    paddingVertical: spacing.sm,
  },
  headerButton: {
    width: 40,
    height: 40,
    borderRadius: radius.md,
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    alignItems: 'center',
    justifyContent: 'center',
  },
  container: {
    paddingHorizontal: spacing.lg,
    paddingBottom: spacing.xxxl,
  },
  heroCard: {
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: spacing.xl,
  },
  bigIconCircle: {
    width: 72,
    height: 72,
    borderRadius: radius.full,
    backgroundColor: colors.surface,
    borderWidth: 1.5,
    borderColor: colors.border,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: spacing.md,
  },
  itemTitle: {
    ...typography.h1,
    color: colors.textPrimary,
    fontWeight: '800',
    textAlign: 'center',
  },
  itemSubtitle: {
    ...typography.body,
    color: colors.textMuted,
    marginTop: spacing.xxs,
    marginBottom: spacing.sm,
  },
  badge: {
    marginTop: spacing.xs,
  },
  infoSection: {
    marginTop: spacing.lg,
  },
  sectionHeading: {
    ...typography.h3,
    color: colors.textPrimary,
    fontWeight: '700',
    marginBottom: spacing.sm,
  },
  infoCard: {
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.md + 2,
  },
  infoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: spacing.sm,
  },
  infoLabelContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
  },
  infoLabel: {
    ...typography.body,
    color: colors.textSecondary,
  },
  infoValue: {
    ...typography.bodyBold,
    color: colors.textPrimary,
  },
  divider: {
    height: 1,
    backgroundColor: colors.border,
    marginVertical: spacing.xs,
  },
  passosSection: {
    marginTop: spacing.xl,
  },
  passosCard: {
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.md,
    gap: spacing.md,
  },
  passoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
  },
  passoText: {
    ...typography.body,
    color: colors.textPrimary,
    flex: 1,
  },
  passoConcluido: {
    textDecorationLine: 'line-through',
    color: colors.textMuted,
  },
  switchCard: {
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.md,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginTop: spacing.xl,
  },
  switchLeft: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
  },
  switchLabel: {
    ...typography.bodyBold,
    color: colors.textPrimary,
  },
  actions: {
    marginTop: spacing.xxl,
    gap: spacing.md,
  },
});
