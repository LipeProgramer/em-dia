import { describe, it, expect } from 'vitest';
import { colors, radius, spacing, typography } from '../theme';

describe('Design System Tokens', () => {
  it('deve possuir a paleta exata do quadro de design', () => {
    // Cores Principais do board Em Dia
    expect(colors.primary).toBe('#00C896');
    expect(colors.secondary).toBe('#F59E0B');
    expect(colors.success).toBe('#10B981');
    expect(colors.warning).toBe('#F59E0B');
    expect(colors.error).toBe('#EF4444');

    // Neutros Dark
    expect(colors.background).toBe('#111827');
    expect(colors.surface).toBe('#1F2937');
    expect(colors.border).toBe('#374151');
    expect(colors.textPrimary).toBe('#FFFFFF');
  });

  it('deve possuir escala consistente de espaçamento e raios de borda', () => {
    expect(spacing.sm).toBe(8);
    expect(spacing.md).toBe(12);
    expect(spacing.lg).toBe(16);
    expect(spacing.xl).toBe(20);

    expect(radius.sm).toBe(8);
    expect(radius.md).toBe(12);
    expect(radius.lg).toBe(16);
    expect(radius.full).toBe(9999);
  });

  it('deve possuir tipografia hierárquica legível', () => {
    expect(typography.h1.fontSize).toBe(28);
    expect(typography.h2.fontSize).toBe(20);
    expect(typography.h3.fontSize).toBe(16);
    expect(typography.body.fontSize).toBe(14);
    expect(typography.caption.fontSize).toBe(12);
  });
});
