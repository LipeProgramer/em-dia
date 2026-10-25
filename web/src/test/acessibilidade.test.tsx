import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { StatusBadge } from '../components/StatusBadge.tsx';
import { StatusItem } from '../api/types.ts';

/**
 * CT51 — Acessibilidade: status nunca comunicado somente por cor (RNF02).
 */
describe('CT51 — Acessibilidade de Status (RNF02)', () => {
  it('renderiza status Vencido com ícone e texto legível', () => {
    render(<StatusBadge status={StatusItem.Vencido} />);
    const badge = screen.getByRole('status');
    expect(badge).toHaveAttribute('aria-label', 'Status: Vencido');
    expect(badge).toHaveTextContent('Vencido');
    expect(badge).toHaveTextContent('✕');
    expect(badge).toHaveClass('badge-vencido');
  });

  it('renderiza status Atenção com ícone e texto legível', () => {
    render(<StatusBadge status={StatusItem.Atencao} />);
    const badge = screen.getByRole('status');
    expect(badge).toHaveAttribute('aria-label', 'Status: Atenção');
    expect(badge).toHaveTextContent('Atenção');
    expect(badge).toHaveTextContent('⚠');
    expect(badge).toHaveClass('badge-atencao');
  });

  it('renderiza status Em Dia com ícone e texto legível', () => {
    render(<StatusBadge status={StatusItem.EmDia} />);
    const badge = screen.getByRole('status');
    expect(badge).toHaveAttribute('aria-label', 'Status: Em dia');
    expect(badge).toHaveTextContent('Em dia');
    expect(badge).toHaveTextContent('✓');
    expect(badge).toHaveClass('badge-em-dia');
  });

  it('renderiza status Encerrado com ícone e texto legível', () => {
    render(<StatusBadge status={StatusItem.Encerrado} />);
    const badge = screen.getByRole('status');
    expect(badge).toHaveAttribute('aria-label', 'Status: Encerrado');
    expect(badge).toHaveTextContent('Encerrado');
    expect(badge).toHaveTextContent('—');
    expect(badge).toHaveClass('badge-encerrado');
  });
});
