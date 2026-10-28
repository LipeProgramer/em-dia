import React, { useCallback, useState, type ReactNode } from 'react';
import { ActivityIndicator, KeyboardAvoidingView, Modal, Platform, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useFocusEffect, useNavigation } from '@react-navigation/native';
import { AlertCircle, AlertTriangle, ArrowLeft, CheckCircle2, Minus, X } from 'lucide-react-native';
import type { StatusItem } from '../api/types';
import { mensagemDeErro } from '../api/client';
import { colors, radius, spacing, typography } from '../theme';
import { Button } from './Button';
import { Input } from './Input';
import { mascaraData } from '../utils/formato';

// Componentes do app: status com cor + símbolo + texto (RNF02), áreas de toque ≥ 44 px, estados de tela.

const infoStatus: Record<StatusItem, { texto: string; cor: string; fundo: string; Icone: typeof AlertCircle }> = {
  Vencido: { texto: 'Vencido', cor: colors.error, fundo: colors.errorMuted, Icone: AlertCircle },
  Atencao: { texto: 'Atenção', cor: colors.warning, fundo: colors.warningMuted, Icone: AlertTriangle },
  EmDia: { texto: 'Em dia', cor: colors.success, fundo: colors.successMuted, Icone: CheckCircle2 },
  Encerrado: { texto: 'Concluído', cor: colors.textMuted, fundo: colors.surfaceElevated, Icone: Minus },
};

export const corStatus = (s: StatusItem) => infoStatus[s].cor;
export const textoStatus = (s: StatusItem) => infoStatus[s].texto;

export function StatusBadge({ status }: { status: StatusItem }) {
  const { texto, cor, fundo, Icone } = infoStatus[status];
  return (
    <View style={[estilos.badge, { backgroundColor: fundo }]} accessibilityLabel={`Status: ${texto}`}>
      <Icone size={13} color={cor} strokeWidth={2.6} />
      <Text style={[estilos.badgeTexto, { color: cor }]}>{texto}</Text>
    </View>
  );
}

/** Tela com área segura, título e botão de voltar opcional. */
export function Tela({ titulo, voltar, acao, children, rolar = true }: {
  titulo: string;
  voltar?: boolean;
  acao?: ReactNode;
  children: ReactNode;
  rolar?: boolean;
}) {
  const navigation = useNavigation();
  // O teclado sobe a tela (iOS) e fecha ao arrastar; no iPhone o teclado numérico não tem botão de fechar
  const corpo = rolar
    ? <ScrollView contentContainerStyle={estilos.conteudo} keyboardShouldPersistTaps="handled"
                  keyboardDismissMode="on-drag" automaticallyAdjustKeyboardInsets>{children}</ScrollView>
    : <View style={[estilos.conteudo, { flex: 1 }]}>{children}</View>;
  return (
    <SafeAreaView style={estilos.tela} edges={['top']}>
      <View style={estilos.topo}>
        {voltar && (
          <Pressable onPress={() => navigation.goBack()} style={estilos.botaoIcone} accessibilityRole="button" accessibilityLabel="Voltar">
            <ArrowLeft size={22} color={colors.textPrimary} />
          </Pressable>
        )}
        <Text style={estilos.titulo} accessibilityRole="header" numberOfLines={2}>{titulo}</Text>
        {acao}
      </View>
      <KeyboardAvoidingView style={{ flex: 1 }} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
        {corpo}
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

export function Carregando() {
  return <View style={estilos.centro}><ActivityIndicator color={colors.primary} size="large" /></View>;
}

export function ErroTela({ erro, tentarDeNovo }: { erro: unknown; tentarDeNovo: () => void }) {
  return (
    <View style={estilos.estado} accessibilityRole="alert">
      <AlertCircle size={28} color={colors.error} />
      <Text style={estilos.estadoTexto}>{mensagemDeErro(erro)}</Text>
      <Button title="Tentar de novo" variant="outline" onPress={tentarDeNovo} />
    </View>
  );
}

export function Vazio({ titulo, texto, acao }: { titulo: string; texto?: string; acao?: ReactNode }) {
  return (
    <View style={estilos.estado}>
      <Text style={estilos.estadoTitulo}>{titulo}</Text>
      {texto && <Text style={estilos.estadoTexto}>{texto}</Text>}
      {acao}
    </View>
  );
}

export function Alerta({ texto, tipo = 'erro' }: { texto: string; tipo?: 'erro' | 'sucesso' }) {
  const cor = tipo === 'erro' ? colors.error : colors.success;
  return (
    <View style={[estilos.alerta, { borderColor: cor }]} accessibilityRole="alert">
      {tipo === 'erro' ? <AlertCircle size={18} color={cor} /> : <CheckCircle2 size={18} color={cor} />}
      <Text style={[estilos.alertaTexto, { color: cor }]}>{texto}</Text>
    </View>
  );
}

export function Progresso({ valor }: { valor: number }) {
  return (
    <View style={estilos.progresso} accessibilityRole="progressbar" accessibilityValue={{ min: 0, max: 100, now: Math.round(valor * 100) }}>
      <View style={[estilos.progressoBarra, { width: `${Math.round(valor * 100)}%` }]} />
    </View>
  );
}

/** Campo de data com máscara dd/mm/aaaa (funciona no Expo Go sem módulos nativos extras). */
export function CampoData({ label, valor, aoMudar, erro, dica }: {
  label: string; valor: string; aoMudar: (v: string) => void; erro?: string; dica?: string;
}) {
  return (
    <Input label={label} value={valor} onChangeText={(t) => aoMudar(mascaraData(t))} placeholder="dd/mm/aaaa"
           keyboardType="number-pad" maxLength={10} error={erro} hint={dica} />
  );
}

/** Opções em pílulas, para escolhas curtas (categoria, tipo, veículo). */
export function Escolha<T extends string | number>({ label, opcoes, valor, aoEscolher, erro }: {
  label: string; opcoes: { id: T; rotulo: string }[]; valor: T | null; aoEscolher: (v: T) => void; erro?: string;
}) {
  return (
    <View style={{ marginBottom: spacing.md }}>
      <Text style={estilos.rotulo}>{label}</Text>
      <View style={estilos.pilulas}>
        {opcoes.map((o) => {
          const ativo = o.id === valor;
          return (
            <Pressable key={String(o.id)} onPress={() => aoEscolher(o.id)} accessibilityRole="radio"
                       accessibilityState={{ selected: ativo }}
                       style={[estilos.pilula, ativo && estilos.pilulaAtiva]}>
              <Text style={[estilos.pilulaTexto, ativo && estilos.pilulaTextoAtivo]}>{o.rotulo}</Text>
            </Pressable>
          );
        })}
      </View>
      {erro && <Text style={estilos.erro}>{erro}</Text>}
    </View>
  );
}

/** Janela modal simples. */
export function Janela({ aberta, titulo, aoFechar, children }: {
  aberta: boolean; titulo: string; aoFechar: () => void; children: ReactNode;
}) {
  return (
    <Modal visible={aberta} transparent animationType="slide" onRequestClose={aoFechar}>
      <KeyboardAvoidingView style={estilos.fundoJanela} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
        <View style={estilos.janela}>
          <View style={estilos.janelaTopo}>
            <Text style={estilos.janelaTitulo} accessibilityRole="header">{titulo}</Text>
            <Pressable onPress={aoFechar} style={estilos.botaoIcone} accessibilityRole="button" accessibilityLabel="Fechar">
              <X size={22} color={colors.textPrimary} />
            </Pressable>
          </View>
          <ScrollView keyboardShouldPersistTaps="handled" keyboardDismissMode="on-drag">{children}</ScrollView>
        </View>
      </KeyboardAvoidingView>
    </Modal>
  );
}

/** Carrega dados ao abrir a tela e sempre que ela volta a ficar em foco (lista sempre atualizada). */
export function useCarregar<T>(buscar: () => Promise<T>, deps: unknown[] = []) {
  const [dados, setDados] = useState<T | null>(null);
  const [erro, setErro] = useState<unknown>(null);
  const [carregando, setCarregando] = useState(true);

  // eslint-disable-next-line react-hooks/exhaustive-deps
  const recarregar = useCallback(async () => {
    setErro(null);
    try {
      setDados(await buscar());
    } catch (e) {
      setErro(e);
    } finally {
      setCarregando(false);
    }
  }, deps);

  useFocusEffect(useCallback(() => { void recarregar(); }, [recarregar]));
  return { dados, setDados, erro, carregando, recarregar };
}

export const estilos = StyleSheet.create({
  tela: { flex: 1, backgroundColor: colors.background },
  topo: { flexDirection: 'row', alignItems: 'center', gap: spacing.sm, paddingHorizontal: spacing.lg, paddingVertical: spacing.md },
  titulo: { ...typography.h1, color: colors.textPrimary, flex: 1 },
  conteudo: { padding: spacing.lg, paddingTop: 0, gap: spacing.md, paddingBottom: spacing.huge },
  botaoIcone: { width: 44, height: 44, alignItems: 'center', justifyContent: 'center' },
  centro: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: spacing.xxl },
  estado: { alignItems: 'center', gap: spacing.md, padding: spacing.xxl, backgroundColor: colors.surface, borderRadius: radius.lg },
  estadoTitulo: { ...typography.h3, color: colors.textPrimary, textAlign: 'center' },
  estadoTexto: { ...typography.body, color: colors.textMuted, textAlign: 'center' },
  badge: { flexDirection: 'row', alignItems: 'center', gap: 4, paddingHorizontal: 8, paddingVertical: 3, borderRadius: radius.full, alignSelf: 'flex-start' },
  badgeTexto: { ...typography.caption, fontWeight: '700' },
  alerta: { flexDirection: 'row', gap: spacing.sm, alignItems: 'center', borderWidth: 1, borderRadius: radius.md, padding: spacing.md },
  alertaTexto: { ...typography.body, flex: 1 },
  progresso: { height: 8, borderRadius: radius.full, backgroundColor: colors.border, overflow: 'hidden' },
  progressoBarra: { height: '100%', backgroundColor: colors.primary },
  rotulo: { ...typography.caption, color: colors.textSecondary, fontWeight: '600', marginBottom: spacing.xs },
  pilulas: { flexDirection: 'row', flexWrap: 'wrap', gap: spacing.sm },
  pilula: { minHeight: 40, paddingHorizontal: spacing.md, borderRadius: radius.full, borderWidth: 1.2, borderColor: colors.border, justifyContent: 'center' },
  pilulaAtiva: { backgroundColor: colors.primary, borderColor: colors.primary },
  pilulaTexto: { ...typography.bodyBold, color: colors.textSecondary },
  pilulaTextoAtivo: { color: '#0F172A' },
  erro: { ...typography.caption, color: colors.error, marginTop: spacing.xs },
  fundoJanela: { flex: 1, backgroundColor: colors.overlay, justifyContent: 'flex-end' },
  janela: { backgroundColor: colors.surface, borderTopLeftRadius: radius.xxl, borderTopRightRadius: radius.xxl, padding: spacing.lg, maxHeight: '90%' },
  janelaTopo: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', marginBottom: spacing.md },
  janelaTitulo: { ...typography.h2, color: colors.textPrimary, flex: 1 },
  cartao: { backgroundColor: colors.surface, borderRadius: radius.lg, padding: spacing.lg, borderWidth: 1, borderColor: colors.border, gap: spacing.xs },
  textoApoio: { ...typography.body, color: colors.textMuted },
  subtitulo: { ...typography.h3, color: colors.textPrimary },
});
