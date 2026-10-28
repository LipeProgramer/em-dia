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
import { ArrowLeft, Bell, Clock, Calendar, Mail } from 'lucide-react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '../../navigation/types';
import { colors, radius, spacing, typography } from '../../theme';
import { preferenciasApi } from '../../api/services';
import { Button } from '../../components/Button';

type Props = NativeStackScreenProps<RootStackParamList, 'PreferenciasNotificacao'>;

export function PreferenciasNotificacaoScreen({ navigation }: Props) {
  const [notificarAtrasados, setNotificarAtrasados] = useState(true);
  const [notificarHoje, setNotificarHoje] = useState(true);
  const [emailNotificacoes, setEmailNotificacoes] = useState(false);
  const [diasAntecedencia, setDiasAntecedencia] = useState(3);
  const [salvando, setSalvando] = useState(false);

  useEffect(() => {
    preferenciasApi
      .obter()
      .then((res) => {
        setNotificarAtrasados(res.notificarAtrasados);
        setNotificarHoje(res.notificarHoje);
        setEmailNotificacoes(res.emailNotificacoes);
        setDiasAntecedencia(res.diasAntecedencia);
      })
      .catch(() => {});
  }, []);

  const handleSalvar = async () => {
    try {
      setSalvando(true);
      await preferenciasApi.atualizar({
        notificarAtrasados,
        notificarHoje,
        emailNotificacoes,
        diasAntecedencia,
        horarioLembrete: '08:00',
      });
      Alert.alert('Sucesso!', 'Preferências de lembretes salvas com sucesso.');
      navigation.goBack();
    } catch {
      Alert.alert('Erro', 'Não foi possível salvar as preferências.');
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
        <Text style={styles.headerTitle}>Preferências de Alerta</Text>
        <View style={{ width: 40 }} />
      </View>

      <ScrollView contentContainerStyle={styles.container}>
        <Text style={styles.sectionSubtitle}>
          Configure como e quando o Em Dia deve lembrá-lo dos seus prazos para evitar
          ansiedade e multas.
        </Text>

        <View style={styles.cardGroup}>
          <View style={styles.row}>
            <View style={styles.rowLeft}>
              <Bell size={20} color={colors.error} strokeWidth={2} />
              <View style={styles.rowTexts}>
                <Text style={styles.rowTitle}>Avisar itens atrasados</Text>
                <Text style={styles.rowDesc}>Receba alerta urgente se algum prazo vencer</Text>
              </View>
            </View>
            <Switch
              value={notificarAtrasados}
              onValueChange={setNotificarAtrasados}
              trackColor={{ false: colors.border, true: colors.primaryDark }}
              thumbColor={notificarAtrasados ? colors.primary : colors.textMuted}
            />
          </View>

          <View style={styles.divider} />

          <View style={styles.row}>
            <View style={styles.rowLeft}>
              <Clock size={20} color={colors.warning} strokeWidth={2} />
              <View style={styles.rowTexts}>
                <Text style={styles.rowTitle}>Avisar no dia do vencimento</Text>
                <Text style={styles.rowDesc}>Lembrete pela manhã às 08:00</Text>
              </View>
            </View>
            <Switch
              value={notificarHoje}
              onValueChange={setNotificarHoje}
              trackColor={{ false: colors.border, true: colors.primaryDark }}
              thumbColor={notificarHoje ? colors.primary : colors.textMuted}
            />
          </View>

          <View style={styles.divider} />

          <View style={styles.row}>
            <View style={styles.rowLeft}>
              <Mail size={20} color={colors.primary} strokeWidth={2} />
              <View style={styles.rowTexts}>
                <Text style={styles.rowTitle}>Receber e-mails de resumo</Text>
                <Text style={styles.rowDesc}>Receba um e-mail diário com seus prazos</Text>
              </View>
            </View>
            <Switch
              value={emailNotificacoes}
              onValueChange={setEmailNotificacoes}
              trackColor={{ false: colors.border, true: colors.primaryDark }}
              thumbColor={emailNotificacoes ? colors.primary : colors.textMuted}
            />
          </View>
        </View>

        <Button
          title="Salvar Preferências"
          onPress={handleSalvar}
          variant="primary"
          size="lg"
          loading={salvando}
          style={styles.saveBtn}
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
    paddingTop: spacing.md,
  },
  sectionSubtitle: {
    ...typography.body,
    color: colors.textMuted,
    lineHeight: 22,
    marginBottom: spacing.xl,
  },
  cardGroup: {
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.lg,
    padding: spacing.md,
  },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: spacing.sm,
  },
  rowLeft: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
    flex: 1,
    paddingRight: spacing.sm,
  },
  rowTexts: {
    flex: 1,
  },
  rowTitle: {
    ...typography.bodyBold,
    color: colors.textPrimary,
  },
  rowDesc: {
    ...typography.caption,
    color: colors.textMuted,
    marginTop: 2,
  },
  divider: {
    height: 1,
    backgroundColor: colors.border,
    marginVertical: spacing.sm,
  },
  saveBtn: {
    marginTop: spacing.xxl,
  },
});
