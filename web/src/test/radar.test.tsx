import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ModalConcluirItem } from '../features/radar/ModalConcluirItem.tsx';
import * as services from '../api/services.ts';
import { AnelRadar, StatusItem, type ItemRadarResponse } from '../api/types.ts';

const mockItemComPassos: ItemRadarResponse = {
  id: 'item-1',
  nome: 'Troca de Óleo',
  categoriaId: 1,
  anel: AnelRadar.Centro,
  status: StatusItem.Vencido,
  proximoVencimentoData: '2026-10-15',
  proximoVencimentoKm: 40000,
  diasRestantes: -2,
  progressoPassos: 0.5,
  passos: [
    { id: 'p1', ordem: 1, descricao: 'Comprar óleo', concluido: true, concluidoEm: null },
    { id: 'p2', ordem: 2, descricao: 'Ir à oficina', concluido: false, concluidoEm: null },
  ],
};

/**
 * CT48 — F2: Conclusão de item no Radar e verificação de passos pendentes (RN06)
 */
describe('CT48 — Fluxo F2: Concluir Item no Radar', () => {
  it('exibe alerta de passos pendentes e bloqueia sem confirmação explícita', async () => {
    const onSucessoMock = vi.fn();
    const onCloseMock = vi.fn();

    render(
      <ModalConcluirItem
        item={mockItemComPassos}
        isOpen={true}
        onClose={onCloseMock}
        onSucesso={onSucessoMock}
      />
    );

    // Deve exibir o título e o alerta de passos pendentes
    expect(screen.getByText('Concluir: Troca de Óleo')).toBeInTheDocument();
    expect(screen.getByText(/passo\(s\) pendente\(s\)/)).toBeInTheDocument();

    // Tenta submeter sem marcar o checkbox de confirmação
    const btnConfirmar = screen.getByRole('button', { name: 'Confirmar Conclusão' });
    fireEvent.click(btnConfirmar);

    // Deve exibir mensagem de erro impedindo a conclusão
    expect(
      screen.getByText(/Este item ainda tem 1 passo\(s\) não marcado\(s\)/)
    ).toBeInTheDocument();
    expect(onSucessoMock).not.toHaveBeenCalled();

    // Marca a confirmação de passos pendentes
    const checkbox = screen.getByLabelText(
      'Confirmar e concluir mesmo com passos pendentes'
    );
    fireEvent.click(checkbox);

    const concluirSpy = vi
      .spyOn(services.itensApi, 'concluir')
      .mockResolvedValueOnce({} as never);

    // Submete novamente
    fireEvent.click(btnConfirmar);

    await waitFor(() => {
      expect(concluirSpy).toHaveBeenCalledTimes(1);
      expect(onSucessoMock).toHaveBeenCalledTimes(1);
      expect(onCloseMock).toHaveBeenCalledTimes(1);
    });
  });
});
