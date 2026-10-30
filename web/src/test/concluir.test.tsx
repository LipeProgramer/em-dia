import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import type { ItemDetalhe } from '../api/types.ts';
import { Providers } from './utils.tsx';

const concluir = vi.fn();
vi.mock('../api/endpoints.ts', () => ({ itensApi: { concluir: (...a: unknown[]) => concluir(...a) } }));

const { ConcluirDialog } = await import('../features/itens/ConcluirDialog.tsx');

const item = {
  id: 'i1', nome: 'Filtro', categoriaId: 3, tipoVencimento: 'RecorrentePorTempo', situacao: 'Ativo',
  status: 'Vencido', anel: 'Centro', textoPrazo: 'venceu há 3 dias', kmBase: null,
  passos: [{ id: 'p1', ordem: 1, descricao: 'Comprar refil', concluido: false }], historico: [],
} as unknown as ItemDetalhe;

describe('Concluir (T12)', () => {
  it('CT48 · Conclui direto, sem perguntar sobre passos (DV26); não pede valor gasto nem observação', async () => {
    concluir.mockResolvedValueOnce({ item, historicoId: 'h1', mensagem: 'Boa! Próximo prazo: 13/04/2027.' });
    const user = userEvent.setup();
    render(<Providers><ConcluirDialog item={item} aberto aoFechar={() => {}} /></Providers>);

    expect(screen.queryByLabelText(/Valor gasto/)).not.toBeInTheDocument();
    expect(screen.queryByLabelText(/Observação/)).not.toBeInTheDocument();
    await user.click(screen.getByRole('button', { name: 'Concluir' }));

    expect(await screen.findByText('Boa! Próximo prazo: 13/04/2027.')).toBeInTheDocument();
    expect(concluir).toHaveBeenCalledTimes(1);
    expect(concluir).toHaveBeenLastCalledWith('i1', expect.objectContaining({
      confirmarPassosPendentes: true, valorGasto: null, observacao: null,
    }));
  });

  it('data de conclusão no futuro mostra a mensagem da spec e não chama a API', async () => {
    concluir.mockReset();
    const user = userEvent.setup();
    render(<Providers><ConcluirDialog item={item} aberto aoFechar={() => {}} /></Providers>);

    const data = screen.getByLabelText('Quando você concluiu?');
    await user.clear(data);
    await user.type(data, '2999-01-01');
    await user.click(screen.getByRole('button', { name: 'Concluir' }));

    expect(await screen.findByText('A data não pode estar no futuro.')).toBeInTheDocument();
    expect(concluir).not.toHaveBeenCalled();
  });
});
