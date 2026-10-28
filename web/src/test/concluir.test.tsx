import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import type { ItemDetalhe } from '../api/types.ts';
import { erroDaApi, Providers } from './utils.tsx';

const concluir = vi.fn();
vi.mock('../api/endpoints.ts', () => ({ itensApi: { concluir: (...a: unknown[]) => concluir(...a) } }));

const { ConcluirDialog } = await import('../features/itens/ConcluirDialog.tsx');

const item = {
  id: 'i1', nome: 'Filtro', categoriaId: 3, tipoVencimento: 'RecorrentePorTempo', situacao: 'Ativo',
  status: 'Vencido', anel: 'Centro', textoPrazo: 'venceu há 3 dias', kmBase: null,
  passos: [{ id: 'p1', ordem: 1, descricao: 'Comprar refil', concluido: false }], historico: [],
} as unknown as ItemDetalhe;

describe('Concluir (T12)', () => {
  it('CT48 · API responde 422 PASSOS_PENDENTES → pergunta "Concluir mesmo assim?"; ao confirmar, reenvia', async () => {
    concluir
      .mockRejectedValueOnce(erroDaApi(422, {
        title: 'Regra de negócio', status: 422, code: 'PASSOS_PENDENTES',
        detail: 'Este item ainda tem 1 passo sem marcar. Quer concluir mesmo assim?',
      }))
      .mockResolvedValueOnce({ item, historicoId: 'h1', mensagem: 'Boa! Próximo prazo: 13/04/2027.' });
    const user = userEvent.setup();
    render(<Providers><ConcluirDialog item={item} aberto aoFechar={() => {}} /></Providers>);

    await user.click(screen.getByRole('button', { name: 'Concluir' }));

    expect(await screen.findByText('Este item ainda tem 1 passo sem marcar. Quer concluir mesmo assim?')).toBeInTheDocument();
    expect(concluir).toHaveBeenLastCalledWith('i1', expect.objectContaining({ confirmarPassosPendentes: false }));

    await user.click(screen.getByRole('button', { name: 'Concluir mesmo assim' }));

    expect(await screen.findByText('Boa! Próximo prazo: 13/04/2027.')).toBeInTheDocument();
    expect(concluir).toHaveBeenLastCalledWith('i1', expect.objectContaining({ confirmarPassosPendentes: true }));
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
