import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import {
  CheckCircle2,
  CalendarCheck,
  BellRing,
  ShieldCheck,
  Heart,
} from 'lucide-react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '../../navigation/types';
import { colors, radius, spacing, typography } from '../../theme';
import { Button } from '../../components/Button';

type Props = NativeStackScreenProps<RootStackParamList, 'Welcome'>;

export function WelcomeScreen({ navigation }: Props) {
  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView
        contentContainerStyle={styles.container}
        showsVerticalScrollIndicator={false}
      >
        {/* Top Logo */}
        <View style={styles.logoRow}>
          <View style={styles.logoCircle}>
            <CheckCircle2 size={24} color={colors.primary} strokeWidth={2.5} />
          </View>
          <Text style={styles.logoText}>em-dia</Text>
        </View>

        {/* Hero Title */}
        <View style={styles.heroSection}>
          <Text style={styles.headline}>Menos preocupação.</Text>
          <Text style={[styles.headline, styles.headlineAccent]}>Mais vida.</Text>

          <Text style={styles.description}>
            O Em Dia é um app que te ajuda a não perder prazos importantes. Tudo no lugar
            certo, com um radar visual e lembretes inteligentes, especialmente pensado
            para adultos com TDAH.
          </Text>
        </View>

        {/* Feature List */}
        <View style={styles.featuresList}>
          <View style={styles.featureItem}>
            <View style={styles.featureIcon}>
              <CalendarCheck size={18} color={colors.primary} strokeWidth={2} />
            </View>
            <Text style={styles.featureText}>Organize seus prazos sem sobrecarga</Text>
          </View>

          <View style={styles.featureItem}>
            <View style={styles.featureIcon}>
              <BellRing size={18} color={colors.primary} strokeWidth={2} />
            </View>
            <Text style={styles.featureText}>Receba lembretes inteligentes e gentis</Text>
          </View>

          <View style={styles.featureItem}>
            <View style={styles.featureIcon}>
              <ShieldCheck size={18} color={colors.primary} strokeWidth={2} />
            </View>
            <Text style={styles.featureText}>Evite multas, juros e prejuízos</Text>
          </View>

          <View style={styles.featureItem}>
            <View style={styles.featureIcon}>
              <Heart size={18} color={colors.primary} strokeWidth={2} />
            </View>
            <Text style={styles.featureText}>Tenha mais tranquilidade no dia a dia</Text>
          </View>
        </View>

        {/* Graphic Arc Visual */}
        <View style={styles.arcContainer}>
          <View style={styles.outerArc}>
            <View style={styles.middleArc}>
              <View style={styles.innerArc} />
            </View>
          </View>
          <Text style={styles.arcCaption}>
            Seu radar de vencimentos na palma da mão
          </Text>
        </View>

        {/* Action Buttons */}
        <View style={styles.actionsSection}>
          <Button
            title="Começar"
            onPress={() => navigation.navigate('Register')}
            variant="primary"
            size="lg"
          />

          <TouchableOpacity
            activeOpacity={0.7}
            onPress={() => navigation.navigate('Login')}
            style={styles.loginLink}
          >
            <Text style={styles.loginLinkText}>
              Já tenho uma conta. <Text style={styles.loginLinkAccent}>Entrar</Text>
            </Text>
          </TouchableOpacity>
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
  container: {
    paddingHorizontal: spacing.xl,
    paddingTop: spacing.lg,
    paddingBottom: spacing.xxxl,
    flexGrow: 1,
    justifyContent: 'space-between',
  },
  logoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: spacing.xxl,
  },
  logoCircle: {
    width: 38,
    height: 38,
    borderRadius: radius.full,
    backgroundColor: 'rgba(0, 200, 150, 0.15)',
    borderWidth: 1.5,
    borderColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: spacing.md,
  },
  logoText: {
    ...typography.h2,
    color: colors.textPrimary,
    fontWeight: '800',
    letterSpacing: -0.5,
  },
  heroSection: {
    marginBottom: spacing.xxl,
  },
  headline: {
    ...typography.h1,
    fontSize: 32,
    lineHeight: 38,
    color: colors.textPrimary,
    fontWeight: '800',
  },
  headlineAccent: {
    color: colors.primary,
  },
  description: {
    ...typography.body,
    color: colors.textMuted,
    lineHeight: 22,
    marginTop: spacing.md,
  },
  featuresList: {
    gap: spacing.md,
    marginBottom: spacing.xxl,
  },
  featureItem: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  featureIcon: {
    width: 32,
    height: 32,
    borderRadius: radius.sm,
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: spacing.md,
  },
  featureText: {
    ...typography.body,
    color: colors.textSecondary,
    fontWeight: '500',
  },
  arcContainer: {
    alignItems: 'center',
    justifyContent: 'center',
    marginVertical: spacing.xl,
  },
  outerArc: {
    width: 140,
    height: 70,
    borderTopLeftRadius: 70,
    borderTopRightRadius: 70,
    borderWidth: 4,
    borderBottomWidth: 0,
    borderColor: 'rgba(0, 200, 150, 0.3)',
    alignItems: 'center',
    justifyContent: 'flex-end',
  },
  middleArc: {
    width: 100,
    height: 50,
    borderTopLeftRadius: 50,
    borderTopRightRadius: 50,
    borderWidth: 4,
    borderBottomWidth: 0,
    borderColor: 'rgba(0, 200, 150, 0.6)',
    alignItems: 'center',
    justifyContent: 'flex-end',
  },
  innerArc: {
    width: 60,
    height: 30,
    borderTopLeftRadius: 30,
    borderTopRightRadius: 30,
    backgroundColor: colors.primary,
  },
  arcCaption: {
    ...typography.caption,
    color: colors.textMuted,
    marginTop: spacing.md,
    textAlign: 'center',
  },
  actionsSection: {
    marginTop: spacing.md,
    gap: spacing.md,
  },
  loginLink: {
    paddingVertical: spacing.sm,
    alignItems: 'center',
  },
  loginLinkText: {
    ...typography.body,
    color: colors.textMuted,
  },
  loginLinkAccent: {
    color: colors.primary,
    fontWeight: '700',
  },
});
