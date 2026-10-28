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
import { ArrowLeft, Mail, Lock, Eye, EyeOff, AlertCircle } from 'lucide-react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '../../navigation/types';
import { colors, radius, spacing, typography } from '../../theme';
import { Input } from '../../components/Input';
import { Button } from '../../components/Button';
import { useAuth } from '../../context/AuthContext';

type Props = NativeStackScreenProps<RootStackParamList, 'Login'>;

export function LoginScreen({ navigation }: Props) {
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [mostrarSenha, setMostrarSenha] = useState(false);
  const [loading, setLoading] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  const handleLogin = async () => {
    if (!email.trim() || !senha) {
      setErro('Preencha seu e-mail e senha.');
      return;
    }

    try {
      setLoading(true);
      setErro(null);
      await login({ email: email.trim(), senha });
      // Redirecionamento é automático pelo AuthContext
    } catch (err: any) {
      const msg =
        err?.response?.data?.mensagem ||
        err?.response?.data?.title ||
        'Não foi possível entrar. Verifique seu e-mail e senha.';
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
            <Text style={styles.title}>Bem-vindo de volta</Text>
            <Text style={styles.subtitle}>
              Acesse sua conta para ver seus prazos no radar.
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
              placeholder="Digite sua senha"
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
            />

            <Button
              title="Entrar"
              onPress={handleLogin}
              variant="primary"
              size="lg"
              loading={loading}
              style={styles.submitButton}
            />
          </View>

          {/* Rodapé com link de cadastro */}
          <View style={styles.footer}>
            <Text style={styles.footerText}>Ainda não tem uma conta? </Text>
            <TouchableOpacity
              activeOpacity={0.7}
              onPress={() => navigation.navigate('Register')}
            >
              <Text style={styles.footerLink}>Cadastre-se</Text>
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
