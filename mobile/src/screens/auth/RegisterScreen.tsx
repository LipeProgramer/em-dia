import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { ArrowLeft, User, Mail, Lock, Eye, EyeOff, AlertCircle } from 'lucide-react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '../../navigation/types';
import { colors, radius, spacing, typography } from '../../theme';
import { Input } from '../../components/Input';
import { Button } from '../../components/Button';
import { useAuth } from '../../context/AuthContext';

type Props = NativeStackScreenProps<RootStackParamList, 'Register'>;

export function RegisterScreen({ navigation }: Props) {
  const { registrar } = useAuth();
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [mostrarSenha, setMostrarSenha] = useState(false);
  const [loading, setLoading] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  const handleRegister = async () => {
    if (!nome.trim() || !email.trim() || !senha) {
      setErro('Preencha todos os campos obrigatórios.');
      return;
    }

    if (senha.length < 6) {
      setErro('A senha deve ter pelo menos 6 caracteres.');
      return;
    }

    try {
      setLoading(true);
      setErro(null);
      await registrar({
        nome: nome.trim(),
        email: email.trim(),
        senha,
      });
      // AuthContext atualiza o estado de autenticação e transiciona
    } catch (err: any) {
      const msg =
        err?.response?.data?.mensagem ||
        err?.response?.data?.detail ||
        'Não foi possível criar sua conta. Tente novamente.';
      setErro(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
        style={styles.keyboardView}
      >
        <ScrollView
          contentContainerStyle={styles.container}
          keyboardShouldPersistTaps="handled"
        >
          {/* Back button */}
          <TouchableOpacity
            activeOpacity={0.7}
            onPress={() => navigation.goBack()}
            style={styles.backButton}
          >
            <ArrowLeft size={22} color={colors.textPrimary} strokeWidth={2} />
          </TouchableOpacity>

          {/* Header */}
          <View style={styles.header}>
            <Text style={styles.title}>Crie sua conta</Text>
            <Text style={styles.subtitle}>
              Comece agora a organizar seus prazos sem ansiedade nem multas.
            </Text>
          </View>

          {/* Erro */}
          {erro && (
            <View style={styles.errorBanner}>
              <AlertCircle size={18} color={colors.error} strokeWidth={2} />
              <Text style={styles.errorBannerText}>{erro}</Text>
            </View>
          )}

          {/* Formulário */}
          <View style={styles.form}>
            <Input
              label="Nome completo"
              placeholder="Ex: Felipe Barbosa"
              autoCapitalize="words"
              value={nome}
              onChangeText={(t) => {
                setNome(t);
                if (erro) setErro(null);
              }}
              leftIcon={<User size={18} color={colors.textMuted} strokeWidth={2} />}
            />

            <Input
              label="E-mail"
              placeholder="seuemail@exemplo.com"
              keyboardType="email-address"
              autoCapitalize="none"
              autoCorrect={false}
              value={email}
              onChangeText={(t) => {
                setEmail(t);
                if (erro) setErro(null);
              }}
              leftIcon={<Mail size={18} color={colors.textMuted} strokeWidth={2} />}
            />

            <Input
              label="Senha"
              placeholder="Crie uma senha forte"
              secureTextEntry={!mostrarSenha}
              autoCapitalize="none"
              value={senha}
              onChangeText={(t) => {
                setSenha(t);
                if (erro) setErro(null);
              }}
              leftIcon={<Lock size={18} color={colors.textMuted} strokeWidth={2} />}
              rightIcon={
                mostrarSenha ? (
                  <EyeOff size={18} color={colors.textMuted} strokeWidth={2} />
                ) : (
                  <Eye size={18} color={colors.textMuted} strokeWidth={2} />
                )
              }
              onRightIconPress={() => setMostrarSenha((v) => !v)}
              hint="Mínimo de 6 caracteres"
            />

            <Button
              title="Cadastrar e Começar"
              onPress={handleRegister}
              variant="primary"
              size="lg"
              loading={loading}
              style={styles.submitButton}
            />
          </View>

          {/* Rodapé com link de login */}
          <View style={styles.footer}>
            <Text style={styles.footerText}>Já possui uma conta? </Text>
            <TouchableOpacity
              activeOpacity={0.7}
              onPress={() => navigation.navigate('Login')}
            >
              <Text style={styles.footerLink}>Fazer login</Text>
            </TouchableOpacity>
          </View>
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: colors.background,
  },
  keyboardView: {
    flex: 1,
  },
  container: {
    paddingHorizontal: spacing.xl,
    paddingTop: spacing.md,
    paddingBottom: spacing.xxl,
    flexGrow: 1,
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
    marginBottom: spacing.xl,
  },
  header: {
    marginBottom: spacing.xl,
  },
  title: {
    ...typography.h1,
    color: colors.textPrimary,
    fontWeight: '800',
  },
  subtitle: {
    ...typography.body,
    color: colors.textMuted,
    marginTop: spacing.xs,
  },
  errorBanner: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: colors.errorMuted,
    borderWidth: 1,
    borderColor: 'rgba(239, 68, 68, 0.4)',
    borderRadius: radius.md,
    padding: spacing.md,
    marginBottom: spacing.lg,
    gap: spacing.sm,
  },
  errorBannerText: {
    ...typography.caption,
    color: colors.error,
    flex: 1,
    fontWeight: '600',
  },
  form: {
    flex: 1,
  },
  submitButton: {
    marginTop: spacing.md,
  },
  footer: {
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    marginTop: spacing.xl,
  },
  footerText: {
    ...typography.body,
    color: colors.textMuted,
  },
  footerLink: {
    ...typography.bodyBold,
    color: colors.primary,
  },
});
