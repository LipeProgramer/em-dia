import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { Route, Routes } from 'react-router-dom';
import { Providers } from './utils.tsx';

const obter = vi.fn();
const editar = vi.fn();
const categorias = vi.fn();
const listarVeiculos = vi.fn();
vi.mock('../api/endpoints.ts', () => ({
  itensApi: { obter: (...a: unknown[]) => obter(...a), editar: (...a: unknown[]) => editar(...a) },
  catalogoApi: { categorias: () => categorias(), modelos: vi.fn() },
  veiculosApi: { listar: () => listarVeiculos() },
}));

const { EditarItemPage } = await import('../features/itens/ItemFormPage.tsx');

const item = {
  id: 'i1', nome: 'Troca de óleo', categoriaId: 1, tipoVencimento: 'UsoOuTempo', situacao: 'Ativo',
  dataBase: '2026-03-10', dataVencimento: null, intervaloDias: 180, intervaloKm: 10000, kmBase: 45000,
  garantiaFabricanteMeses: null, veiculo: { id: 'v1', apelido: 'Gol prata' }, antecedenciaDias: 30,
  observacao: null, passos: [],
};

/** As listas das opções só chegam depois do item: é o que acontece ao abrir a tela direto pelo endereço. */
const atrasada = <T,>(valor: T) => new Promise<T>((ok) => setTimeout(() => ok(valor), 30));

describe('Editar item (T11)', () => {
  beforeEach(() => {
    editar.mockReset();
    obter.mockResolvedValue(item);
    editar.mockResolvedValue(item);
    categorias.mockImplementation(() => atrasada([
      { id: 1, nome: 'Veículo', icone: 'car', ordem: 1 },
      { id: 2, nome: 'Documentos', icone: 'file', ordem: 2 },
    ]));
    listarVeiculos.mockImplementation(() => atrasada([{ id: 'v1', apelido: 'Gol prata', kmAtual: 52000 }]));
  });

  it('com as opções chegando depois, categoria e veículo mostram o valor salvo do item', async () => {
    render(
      <Providers rota="/itens/i1/editar">
        <Routes><Route path="/itens/:id/editar" element={<EditarItemPage />} /></Routes>
      </Providers>,
    );

    const categoria = await screen.findByLabelText('Categoria');
    await screen.findByRole('option', { name: 'Documentos' });
    await waitFor(() => expect(categoria).toHaveValue('1'));
    expect(screen.getByLabelText('Veículo')).toHaveValue('v1');
  });

  it('não pede passos nem observação, e salvar não apaga os que o item já tinha (DV26)', async () => {
    obter.mockResolvedValue({ ...item, observacao: 'Oficina do João', passos: [{ id: 'p1', descricao: 'Levar o carro', concluido: false }] });
    const user = userEvent.setup();
    render(
      <Providers rota="/itens/i1/editar">
        <Routes><Route path="/itens/:id/editar" element={<EditarItemPage />} /></Routes>
      </Providers>,
    );

    await screen.findByRole('option', { name: 'Documentos' });
    expect(screen.queryByText('Passos pequenos')).not.toBeInTheDocument();
    expect(screen.queryByLabelText(/Observação/)).not.toBeInTheDocument();
    await user.click(screen.getByRole('button', { name: 'Salvar alterações' }));

    await waitFor(() => expect(editar).toHaveBeenCalled());
    expect(editar.mock.calls.at(-1)![1]).toMatchObject({ observacao: 'Oficina do João', passos: ['Levar o carro'] });
  });

  it('salvar sem mexer mantém a categoria e o veículo', async () => {
    const user = userEvent.setup();
    render(
      <Providers rota="/itens/i1/editar">
        <Routes><Route path="/itens/:id/editar" element={<EditarItemPage />} /></Routes>
      </Providers>,
    );

    await screen.findByRole('option', { name: 'Documentos' });
    await user.click(screen.getByRole('button', { name: 'Salvar alterações' }));

    await waitFor(() => expect(editar).toHaveBeenCalled());
    expect(editar.mock.calls[0][1]).toMatchObject({ categoriaId: 1, veiculoId: 'v1' });
  });
});
