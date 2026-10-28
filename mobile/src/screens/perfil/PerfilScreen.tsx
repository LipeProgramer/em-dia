import React from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  TouchableOpacity,
  Alert,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import {
  User,
  Car,
  Bell,
  Target,
  Palette,
  HelpCircle,
  Info,
  LogOut,
  ChevronRight,
  Edit2,
  CheckCircle,
} from 'lucide-react-native';
import type { CompositeScreenProps } from '@react-navigation/native';
import type { BottomTabScreenProps } from '@react-navigation/bottom-tabs';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type {
  MainTabParamList,
  RootStackParamList,
} from '../../navigation/types';
import { colors, radius, spacing, typography } from '../../theme';
import { useAuth } from '../../context/AuthContext';

type Props = CompositeScreenProps<
  BottomTabScreenProps<MainTabParamList, 'PerfilTab'>,
  NativeStackScreenProps<RootStackParamList>
>;

interface MenuItemProps {
  icon: any;
  label: string;
  value?: string;
  onPress: () => void;
  destructive?: boolean;
}

function MenuItem({ icon: Icon, label, value, onPress, destructive }: MenuItemProps) {
  return (
    <TouchableOpacity
      activeOpacity={0.7}
      onPress={onPress}
      style={styles.menuItem}
    >
      <View style={styles.menuLeft}>
        <View
          style={[
            styles.menuIconCircle,
            destructive && { backgroundColor: colors.errorMuted },
          ]}
        >
          <Icon
            size={18}
            color={destructive ? colors.error : colors.textSecondary}
            strokeWidth={2}
          />
        </View>
        <Text
          style={[
            styles.menuLabel,
            destructive && { color: colors.error, fontWeight: '700' },
          ]}
        >
          {label}
        </Text>
      </View>

      <View style={styles.menuRight}>
        {value && <Text style={styles.menuValue}>{value}</Text>}
        {!destructive && <ChevronRight size={18} color={colors.borderLight} />}
      </View>
    </TouchableOpacity>
  );
}

export function PerfilScreen({ navigation }: Props) {
  const { user, logout } = useAuth();

  const handleLogout = () => {
    Alert.alert('Sair da conta', 'Deseja realmente sair da sua conta?', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Sair',
        style: 'destructive',
        onPress: async () => {
          await logout();
        },
      },
    ]);
  };

  const nomeExibicao = user?.nome || 'Felipe Nadab';
  const emailExibicao = user?.email || 'felipe@emdia.com';
  const inicial = nomeExibicao.charAt(0).toUpperCase();

  return (
    <SafeAreaView style={styles.safeArea} edges={['top', 'left', 'right']}>
      <ScrollView
        contentContainerStyle={styles.container}
        showsVerticalScrollIndicator={false}
      >
        <Text style={styles.headerTitle}>Perfil e configurações</Text>

        {/* Card do Usuário */}
        <View style={styles.userCard}>
          <View style={styles.avatarCircle}>
            <Text style={styles.avatarInitial}>{inicial}</Text>
          </View>
          <View style={styles.userInfo}>
            <Text style={styles.userName}>{nomeExibicao}</Text>
            <Text style={styles.userEmail}>{emailExibicao}</Text>
          </View>
          <TouchableOpacity
            activeOpacity={0.7}
            onPress={() =>
              Alert.alert('Editar Dados', 'Função de atualização de perfil acessível em breve.')
            }
            style={styles.editBtn}
          >
            <Edit2 size={18} color={colors.textSecondary} strokeWidth={2} />
          </TouchableOpacity>
        </View>

        {/* Menu Principal */}
        <View style={styles.menuSection}>
          <Text style={styles.sectionHeader}>Gerenciamento</Text>

          <View style={styles.menuGroup}>
            <MenuItem
              icon={User}
              label="Meus dados"
              onPress={() =>
                Alert.alert('Meus Dados', `Nome: ${nomeExibicao}\nE-mail: ${emailExibicao}`)
              }
            />
            <View style={styles.divider} />
            <MenuItem
              icon={Car}
              label="Veículos"
              onPress={() => navigation.navigate('Veiculos')}
            />
            <View style={styles.divider} />
            <MenuItem
              icon={CheckCircle}
              label="Check-up de Vencimentos"
              onPress={() => navigation.navigate('Checkup')}
            />
          </View>

          <Text style={styles.sectionHeader}>Preferências</Text>
          <View style={styles.menuGroup}>
            <MenuItem
              icon={Bell}
              label="Preferências de notificação"
              onPress={() => navigation.navigate('PreferenciasNotificacao')}
            />
            <View style={styles.divider} />
            <MenuItem
              icon={Target}
              label="Modo foco"
              onPress={() => navigation.navigate('ModoFoco')}
            />
            <View style={styles.divider} />
            <MenuItem
              icon={Palette}
              label="Tema"
              value="Escuro (Auto)"
              onPress={() =>
                Alert.alert('Tema', 'O tema escuro de alto contraste do Em Dia foi desenhado especialmente para conforto visual e foco.')
              }
            />
          </View>

          <Text style={styles.sectionHeader}>Suporte & Legal</Text>
          <View style={styles.menuGroup}>
            <MenuItem
              icon={HelpCircle}
              label="Ajuda e suporte"
              onPress={() =>
                Alert.alert(
                  'Ajuda & Suporte',
                  'Precisa de suporte? Entre em contato pelo e-mail suporte@emdia.app.'
                )
              }
            />
            <View style={styles.divider} />
            <MenuItem
              icon={Info}
              label="Sobre o Em Dia"
              value="v1.0.0"
              onPress={() => navigation.navigate('Sobre')}
            />
          </View>

          {/* Sair */}
          <View style={[styles.menuGroup, { marginTop: spacing.md }]}>
            <MenuItem
              icon={LogOut}
              label="Sair da conta"
              destructive
              onPress={handleLogout}
            />
          </View>
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
    paddingHorizontal: spacing.lg,
    paddingBottom: spacing.huge,
  },
  headerTitle: {
    ...typography.h1,
    color: colors.textPrimary,
    fontWeight: '800',
    marginTop: spacing.md,
    marginBottom: spacing.lg,
  },
  userCard: {
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.lg,
    padding: spacing.md + 2,
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: spacing.xl,
  },
  avatarCircle: {
    width: 52,
    height: 52,
    borderRadius: radius.full,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: spacing.md,
  },
  avatarInitial: {
    ...typography.h2,
    color: '#0F172A',
    fontWeight: '800',
  },
  userInfo: {
    flex: 1,
  },
  userName: {
    ...typography.bodyBold,
    color: colors.textPrimary,
    fontSize: 16,
  },
  userEmail: {
    ...typography.caption,
    color: colors.textMuted,
    marginTop: 2,
  },
  editBtn: {
    width: 38,
    height: 38,
    borderRadius: radius.md,
    backgroundColor: colors.surfaceElevated,
    alignItems: 'center',
    justifyContent: 'center',
  },
  menuSection: {
    gap: spacing.sm,
  },
  sectionHeader: {
    ...typography.caption,
    color: colors.textMuted,
    fontWeight: '700',
    textTransform: 'uppercase',
    letterSpacing: 0.5,
    marginTop: spacing.md,
    marginBottom: spacing.xs,
  },
  menuGroup: {
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: radius.lg,
    overflow: 'hidden',
  },
  menuItem: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: spacing.md,
    paddingHorizontal: spacing.md + 2,
  },
  menuLeft: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
  },
  menuIconCircle: {
    width: 32,
    height: 32,
    borderRadius: radius.sm,
    backgroundColor: colors.surfaceElevated,
    alignItems: 'center',
    justifyContent: 'center',
  },
  menuLabel: {
    ...typography.body,
    color: colors.textPrimary,
    fontWeight: '500',
  },
  menuRight: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
  },
  menuValue: {
    ...typography.caption,
    color: colors.textMuted,
  },
  divider: {
    height: 1,
    backgroundColor: colors.border,
    marginLeft: 56,
  },
});
