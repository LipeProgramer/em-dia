import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { CheckupView } from '../features/checkup/CheckupView.tsx';
import * as services from '../api/services.ts';

// Mock do hook useAuth
vi.mock('../features/auth/AuthContext.tsx', () => ({
  useAuth: () => ({
    user: { id: 'user-1', nome: 'Teste', precisaCheckup: true },
    refreshUser: vi.fn(),
  }),
}));

/**
 * CT47 — F1: Fluxo de Check-up inicial interativo
 */
describe('CT47 — Fluxo F1: Check-up Inicial', () => {
  it('permite selecionar respostas e finalizar gerando itens', async () => {
    const executarSpy = vi
      .spyOn(services.checkupApi, 'executar')
      .mockResolvedValueOnce({
        id: 'chk-1',
        realizadoEm: new Date().toISOString(),
        itensCriados: 2,
        respostas: { TEM_PET: true, COMPROU_ELETRONICO: true },
      });

    render(
      <BrowserRouter>
        <CheckupView />
      </BrowserRouter>
    );

    expect(screen.getByText('Configuração Rápida em 1 Minuto')).toBeInTheDocument();
    expect(screen.getByText('Você tem animal de estimação?')).toBeInTheDocument();

    // Clica no "Sim" da primeira pergunta (TEM_PET)
    const botoesSim = screen.getAllByRole('button', { name: 'Sim' });
    fireEvent.click(botoesSim[0]);

    // Submete o check-up
    const btnFinalizar = screen.getByRole('button', {
      name: 'Finalizar e Gerar Radar',
    });
    fireEvent.click(btnFinalizar);

    await waitFor(() => {
      expect(executarSpy).toHaveBeenCalledTimes(1);
    });

    // Deve exibir tela de sucesso
    await waitFor(() => {
      expect(screen.getByText('Check-up Concluído!')).toBeInTheDocument();
      expect(screen.getByText(/2 item\(ns\) automático\(s\)/)).toBeInTheDocument();
    });
  });
});
