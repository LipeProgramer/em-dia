import { render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { Route, Routes } from 'react-router-dom';
import { Providers } from './utils.tsx';

const obter = vi.fn();
vi.mock('../api/endpoints.ts', () => ({ itensApi: { obter: (...a: unknown[]) => obter(...a) } }));

const { DetalheItemPage } = await import('../features/itens/DetalheItemPage.tsx');

describe('Detalhe do item (T10)', () => {
  it('não mostra os passos, mesmo quando o item tem passos (DV26)', async () => {
    obter.mockResolvedValue({
      id: 'i1', nome: 'CNH', categoriaId: 2, tipoVencimento: 'DataUnica', situacao: 'Ativo', status: 'Atencao',
      textoPrazo: 'vence amanhã', proximoVencimentoData: '2026-10-30', proximoVencimentoKm: null, antecedenciaDias: 30,
      observacao: null, veiculo: null, progressoPassos: 0, historico: [],
      passos: [{ id: 'p1', ordem: 1, descricao: 'Agendar exame médico', concluido: false }],
    });
    render(
      <Providers rota="/itens/i1">
        <Routes><Route path="/itens/:id" element={<DetalheItemPage />} /></Routes>
      </Providers>,
    );

    expect(await screen.findByRole('button', { name: 'Concluir' })).toBeInTheDocument();
    expect(screen.queryByText('Agendar exame médico')).not.toBeInTheDocument();
    expect(screen.queryByText(/^Passos/)).not.toBeInTheDocument();
  });
});
