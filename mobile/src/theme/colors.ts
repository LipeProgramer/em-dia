export const colors = {
  // Brand / Primary
  primary: '#00C896', // Emerald / Mint do design
  primaryDark: '#00A37A',
  primaryLight: '#33D3AA',
  primaryMuted: 'rgba(0, 200, 150, 0.15)',

  // Secondary & Accents
  secondary: '#F59E0B', // Amber
  secondaryMuted: 'rgba(245, 158, 11, 0.15)',
  success: '#10B981', // Verde Sucesso
  successMuted: 'rgba(16, 185, 129, 0.15)',
  warning: '#F59E0B', // Aviso / Hoje
  warningMuted: 'rgba(245, 158, 11, 0.15)',
  error: '#EF4444', // Atrasado / Erro
  errorMuted: 'rgba(239, 68, 68, 0.15)',

  // Neutros Dark Mode
  background: '#111827', // Darkest background (Gray 900)
  surface: '#1F2937',    // Card / Container surface (Gray 800)
  surfaceElevated: '#283548', // Modal / Popover surface
  surfaceInput: '#182232',    // Input fields background
  border: '#374151',     // Divider and subtle borders (Gray 700)
  borderLight: '#4B5563', // Hover / active borders

  // Texto & Ícones
  textPrimary: '#FFFFFF', // High contrast
  textSecondary: '#E5E7EB', // Gray 200
  textMuted: '#9CA3AF',   // Gray 400
  textDisabled: '#6B7280', // Gray 500

  // Radar Rings
  radarCenter: '#EF4444', // Vermelho (Atrasados)
  radarRing1: '#F59E0B',  // Âmbar (Hoje / Urgente)
  radarRing2: '#00C896',  // Ciano/Esmeralda (Próximos)
  radarRing3: '#10B981',  // Verde (Em dia)

  // Backdrop
  overlay: 'rgba(0, 0, 0, 0.75)',
} as const;

export type Colors = typeof colors;
