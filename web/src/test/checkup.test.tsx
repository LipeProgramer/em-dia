import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Route, Routes } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import { Providers } from './utils.tsx';

vi.mock('../api/endpoints.ts', () => ({
  catalogoApi: {
    perguntas: async () => [
      { codigo: 'TEM_CARRO', texto: 'Você tem carro ou moto?', ajuda: '', categoriaId: 1, modelos: [] },
      { codigo: 'TEM_CNH', texto: 'Você tem CNH?', ajuda: '', categoriaId: 2, modelos: [] },
      { codigo: 'TEM_PET', texto: 'Você tem pet?', ajuda: '', categoriaId: 4, modelos: [] },
    ],
  },
}));

const { CheckupProvider } = await import('../features/checkup/CheckupContext.tsx');
const { PerguntaPage } = await import('../features/checkup/PerguntaPage.tsx');

describe('Check-up (T03)', () => {
  it('CT51 · navegar só com Tab e Enter → todas as perguntas respondíveis; foco visível', async () => {
    const user = userEvent.setup();
    render(
      <Providers rota="/checkup">
        <CheckupProvider>
          <Routes>
            <Route path="/checkup" element={<PerguntaPage />} />
            <Route path="/checkup/dados" element={<p>Completar dados</p>} />
          </Routes>
        </CheckupProvider>
      </Providers>,
    );

    // Pergunta 1: o foco começa no título; Tab leva ao "Sim"
    expect(await screen.findByRole('heading', { name: 'Você tem carro ou moto?' })).toHaveFocus();
    expect(screen.getByText(/Pergunta 1 de 3/)).toBeInTheDocument();
    expect(screen.getByRole('list', { name: 'Áreas do check-up' })).toBeInTheDocument();
    await user.tab();
    expect(screen.getByRole('button', { name: 'Sim' })).toHaveFocus();
    await user.keyboard('{Enter}');

    // Pergunta 2: responde "Não" com Tab, Tab, Enter
    expect(await screen.findByRole('heading', { name: 'Você tem CNH?' })).toHaveFocus();
    await user.tab();
    await user.tab();
    expect(screen.getByRole('button', { name: 'Não' })).toHaveFocus();
    await user.keyboard('{Enter}');

    // Pergunta 3: pula
    expect(await screen.findByRole('heading', { name: 'Você tem pet?' })).toHaveFocus();
    await user.tab();
    await user.tab();
    await user.tab();
    expect(screen.getByRole('button', { name: 'Pular esta pergunta' })).toHaveFocus();
    await user.keyboard('{Enter}');

    expect(await screen.findByText('Completar dados')).toBeInTheDocument();
  });

  it('clicar em uma área vai para a primeira pergunta dela, para frente e para trás, sem responder nada', async () => {
    const user = userEvent.setup();
    render(
      <Providers rota="/checkup">
        <CheckupProvider>
          <Routes><Route path="/checkup" element={<PerguntaPage />} /></Routes>
        </CheckupProvider>
      </Providers>,
    );

    await screen.findByRole('heading', { name: 'Você tem carro ou moto?' });
    expect(screen.queryByRole('button', { name: /Pular a área/ })).not.toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Pular esta pergunta' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Veículo' })).toHaveAttribute('aria-current', 'step');

    await user.click(screen.getByRole('button', { name: 'Saúde' }));
    expect(await screen.findByRole('heading', { name: 'Você tem pet?' })).toHaveFocus();
    expect(screen.getByRole('button', { name: 'Saúde' })).toHaveAttribute('aria-current', 'step');
    expect(screen.getByRole('button', { name: 'Pergunta anterior' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Veículo' })).not.toHaveClass('feita');

    await user.click(screen.getByRole('button', { name: 'Veículo' }));
    expect(await screen.findByRole('heading', { name: 'Você tem carro ou moto?' })).toBeInTheDocument();
    await user.click(screen.getByRole('button', { name: 'Sim' }));
    await screen.findByRole('heading', { name: 'Você tem CNH?' });
    expect(screen.getByRole('button', { name: 'Veículo' })).toHaveClass('feita');
  });
});
