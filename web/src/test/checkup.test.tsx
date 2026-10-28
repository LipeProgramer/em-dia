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
    expect(screen.getByText('Pergunta 1 de 3')).toBeInTheDocument();
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
});
