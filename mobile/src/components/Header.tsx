import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity } from 'react-native';
import { Bell, User } from 'lucide-react-native';
import { colors, radius, spacing, typography } from '../theme';
import { useAuth } from '../context/AuthContext';
import { useRadar } from '../context/RadarContext';

interface HeaderProps {
  onPressNotifications?: () => void;
  onPressProfile?: () => void;
  title?: string;
  subtitle?: string;
}

export function Header({
  onPressNotifications,
  onPressProfile,
  title,
  subtitle,
}: HeaderProps) {
  const { user } = useAuth();
  const { counts } = useRadar();

  const primeiroNome = user?.nome ? user.nome.split(' ')[0] : 'Felipe';
  const displayTitle = title || `Olá, ${primeiroNome} 👋`;
  const displaySubtitle = subtitle || 'Aqui está o seu radar de hoje.';

  const unreadCount = counts.atrasados + counts.hoje;

  return (
    <View style={styles.container}>
      <View style={styles.left}>
        <Text style={styles.title}>{displayTitle}</Text>
        <Text style={styles.subtitle}>{displaySubtitle}</Text>
      </View>

      <View style={styles.right}>
        {/* Notificações com badge */}
        <TouchableOpacity
          activeOpacity={0.7}
          onPress={onPressNotifications}
          style={styles.iconButton}
        >
          <Bell size={20} color={colors.textSecondary} strokeWidth={2} />
          {unreadCount > 0 && (
            <View style={styles.badge}>
              <Text style={styles.badgeText}>
                {unreadCount > 9 ? '9+' : unreadCount}
              </Text>
            </View>
          )}
        </TouchableOpacity>

        {/* Avatar */}
        <TouchableOpacity
          activeOpacity={0.7}
          onPress={onPressProfile}
          style={styles.avatarButton}
        >
          <View style={styles.avatar}>
            <Text style={styles.avatarInitial}>
              {primeiroNome.charAt(0).toUpperCase()}
            </Text>
          </View>
        </TouchableOpacity>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: spacing.lg,
    paddingTop: spacing.md,
    paddingBottom: spacing.lg,
  },
  left: {
    flex: 1,
    paddingRight: spacing.md,
  },
  title: {
    ...typography.h2,
    color: colors.textPrimary,
    fontWeight: '700',
  },
  subtitle: {
    ...typography.caption,
    color: colors.textMuted,
    marginTop: 2,
  },
  right: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
  },
  iconButton: {
    width: 40,
    height: 40,
    borderRadius: radius.full,
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    alignItems: 'center',
    justifyContent: 'center',
  },
  badge: {
    position: 'absolute',
    top: 4,
    right: 4,
    minWidth: 16,
    height: 16,
    borderRadius: 8,
    backgroundColor: colors.error,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 3,
  },
  badgeText: {
    ...typography.tiny,
    color: '#FFFFFF',
    fontSize: 9,
    fontWeight: '800',
  },
  avatarButton: {
    borderRadius: radius.full,
  },
  avatar: {
    width: 40,
    height: 40,
    borderRadius: radius.full,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
  },
  avatarInitial: {
    ...typography.bodyBold,
    color: '#0F172A',
    fontWeight: '800',
    fontSize: 16,
  },
});
