import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  Alert,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { ArrowLeft, CheckCircle2, Circle, Sparkles } from 'lucide-react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '../../navigation/types';
import { colors, radius, spacing, typography } from '../../theme';
import { checkupApi, modelosApi } from '../../api/services';
import type { ModeloItemResponse } from '../../api/types';
import { Button } from '../../components/Button';
import { useRadar } from '../../context/RadarContext';

type Props = NativeStackScreenProps<RootStackParamList, 'Checkup'>;

export function CheckupScreen({ navigation }: Props) {
  const { carregarDados } = useRadar();
  const [modelos, setModelos] = useState<ModeloItemResponse[]>([]);
  const [selecionados, setSelecionados] = useState<Record<string, boolean>>({});
  const [salvando, setSalvando] = useState(false);

  useEffect(() => {
    modelosApi
      .listar()
      .then((res) => {
        setModelos(res);
        // Pré-seleciona os mais comuns
        const inicial: Record<string, boolean> = {};
        res.slice(0, 3).forEach((m) => {
          inicial[m.id] = true;
        });
        setSelecionados(inicial);
      })
      .catch(() => {});
  }, []);

  const toggleSelecao = (id: string) => {
    setSelecionados((prev) => ({
      ...prev,
      [id]: !prev[id],
    }));
  };

  const handleConcluirCheckup = async () => {
    try {
      setSalvando(true);
      const respostas = Object.keys(selecionados).map((modeloId) => ({
        modeloId,
        adicionar: !!selecionados[modeloId],
        dataVencimento: new Date(Date.now() + 30 * 86400000).toISOString().split('T')[0],
      }));

      await checkupApi.executar({ respostas });
      await carregarDados();
      Alert.alert('Radar Atualizado!', 'Os itens selecionados agora fazem parte do seu radar de vencimentos.');
      navigation.goBack();
    } catch {
      Alert.alert('Erro', 'Não foi possível concluir o check-up.');
    } finally {
      setSalvando(false);
    }
  };

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
        <Text style={styles.headerTitle}>Check-up Inicial</Text>
        <View style={{ width: 40 }} />
      </View>

      <ScrollView contentContainerStyle={styles.container}>
        <View style={styles.banner}>
          <View style={styles.bannerIcon}>
            <Sparkles size={22} color={colors.primary} strokeWidth={2.2} />
          </View>
          <View style={styles.bannerText}>
            <Text style={styles.bannerTitle}>Check-up Anti-Esquecimento</Text>
            <Text style={styles.bannerSubtitle}>
              Selecione o que você possui na sua vida que costuma esquecer. Nós
              colocamos tudo no radar com passos prontos!
            </Text>
          </View>
        </View>

        <Text style={styles.sectionTitle}>Modelos Recomendados</Text>

        <View style={styles.list}>
          {modelos.map((m) => {
            const isSelected = !!selecionados[m.id];
            return (
              <TouchableOpacity
                key={m.id}
                activeOpacity={0.7}
                onPress={() => toggleSelecao(m.id)}
                style={[styles.itemCard, isSelected && styles.itemCardSelected]}
              >
                <View style={styles.checkIcon}>
                  {isSelected ? (
                    <CheckCircle2 size={22} color={colors.primary} strokeWidth={2.5} />
                  ) : (
                    <Circle size={22} color={colors.textMuted} strokeWidth={2} />
                  )}
                </View>

                <View style={styles.itemInfo}>
                  <Text style={styles.itemTitle}>{m.nome}</Text>
                  <Text style={styles.itemDesc}>
                    {m.passosPadrao?.length || 3} passos automatizados inclusos
                  </Text>
                </View>
              </TouchableOpacity>
            );
          })}
        </View>

        <Button
          title="Adicionar Selecionados ao Meu Radar"
          onPress={handleConcluirCheckup}
          variant="primary"
          size="lg"
          loading={salvando}
          style={styles.actionBtn}
        />
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
  banner: {
    backgroundColor: 'rgba(0, 200, 150, 0.08)',
    borderWidth: 1,
    borderColor: 'rgba(0, 200, 150, 0.3)',
    borderRadius: radius.lg,
    padding: spacing.md + 2,
    flexDirection: 'row',
    alignItems: 'center',
    marginVertical: spacing.md,
  },
  bannerIcon: {
    width: 42,
    height: 42,
    borderRadius: radius.md,
    backgroundColor: 'rgba(0, 200, 150, 0.15)',
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: spacing.md,
  },
  bannerText: {
    flex: 1,
  },
  bannerTitle: {
    ...typography.bodyBold,
    color: colors.primary,
  },
  bannerSubtitle: {
    ...typography.caption,
    color: colors.textMuted,
    marginTop: 2,
    lineHeight: 18,
  },
  sectionTitle: {
    ...typography.caption,
    color: colors.textSecondary,
    fontWeight: '700',
    textTransform: 'uppercase',
    letterSpacing: 0.5,
    marginTop: spacing.md,
    marginBottom: spacing.sm,
  },
  list: {
    gap: spacing.sm,
  },
  itemCard: {
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.lg,
    padding: spacing.md,
    flexDirection: 'row',
    alignItems: 'center',
  },
  itemCardSelected: {
    borderColor: colors.primary,
    backgroundColor: '#162334',
  },
  checkIcon: {
    marginRight: spacing.md,
  },
  itemInfo: {
    flex: 1,
  },
  itemTitle: {
    ...typography.bodyBold,
    color: colors.textPrimary,
  },
  itemDesc: {
    ...typography.caption,
    color: colors.textMuted,
    marginTop: 2,
  },
  actionBtn: {
    marginTop: spacing.xl,
  },
});
