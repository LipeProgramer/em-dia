import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ModalAtualizarKm } from '../features/veiculos/ModalAtualizarKm.tsx';
import * as services from '../api/services.ts';
import { TipoVeiculo, type VeiculoResponse } from '../api/types.ts';

const mockVeiculo: VeiculoResponse = {
  id: 'veic-1',
  apelido: 'Carro Teste',
  tipoVeiculo: TipoVeiculo.Carro,
  placa: 'ABC1234',
  kmAtual: 50000,
  mediaKmMes: 1000,
  criadoEm: '2026-10-15',
};

/**
 * CT50 — F4: Atualizar Km com validação RN07
 */
describe('CT50 — Fluxo F4: Atualizar Km do Veículo', () => {
  it('impede inserção de km menor que o atual e atualiza quando válido', async () => {
    const onSucessoMock = vi.fn();
    const onCloseMock = vi.fn();

    render(
      <ModalAtualizarKm
        veiculo={mockVeiculo}
        isOpen={true}
        onClose={onCloseMock}
        onSucesso={onSucessoMock}
      />
    );

    expect(screen.getByText('Atualizar Odômetro: Carro Teste')).toBeInTheDocument();

    const inputKm = screen.getByLabelText(/Quilometragem Atual/);
    const form = inputKm.closest('form')!;

    // Tenta atualizar para km menor (49000 < 50000)
    fireEvent.change(inputKm, { target: { value: '49000' } });
    fireEvent.submit(form);

    // Deve acusar erro de validação
    expect(screen.getByRole('alert')).toHaveTextContent(
      'A nova quilometragem (49000 km) não pode ser menor que a atual (50000 km).'
    );
    expect(onSucessoMock).not.toHaveBeenCalled();

    // Atualiza para km válido (52500)
    fireEvent.change(inputKm, { target: { value: '52500' } });

    const atualizarKmSpy = vi
      .spyOn(services.veiculosApi, 'atualizarKm')
      .mockResolvedValueOnce({} as never);

    fireEvent.submit(form);

    await waitFor(() => {
      expect(atualizarKmSpy).toHaveBeenCalledWith('veic-1', 52500);
      expect(onSucessoMock).toHaveBeenCalledTimes(1);
      expect(onCloseMock).toHaveBeenCalledTimes(1);
    });
  });
});
