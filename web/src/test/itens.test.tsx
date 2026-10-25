import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ItemFormView } from '../features/itens/ItemFormView.tsx';
import * as services from '../api/services.ts';
import { TipoVencimento } from '../api/types.ts';

const queryClient = new QueryClient({
  defaultOptions: { queries: { retry: false } },
});

/**
 * CT49 — F3: Criar item manual
 */
describe('CT49 — Fluxo F3: Criar Item Manual', () => {
  it('preenche campos, adiciona passos e submete com sucesso', async () => {
    vi.spyOn(services.catalogoApi, 'listarCategorias').mockResolvedValue([
      { id: 2, nome: 'Documentos', icone: 'id-card', ordem: 2 },
    ]);
    vi.spyOn(services.catalogoApi, 'listarModelos').mockResolvedValue([]);
    vi.spyOn(services.veiculosApi, 'listar').mockResolvedValue([]);

    const criarSpy = vi
      .spyOn(services.itensApi, 'criar')
      .mockResolvedValueOnce({} as never);

    render(
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <ItemFormView />
        </BrowserRouter>
      </QueryClientProvider>
    );

    // Preenche nome
    const inputNome = screen.getByLabelText('Nome do Item *');
    fireEvent.change(inputNome, { target: { value: 'Renovar Passaporte' } });

    // Preenche data de vencimento (default TipoVencimento.DataUnica)
    const inputDataVenc = screen.getByLabelText('Data de Vencimento *');
    fireEvent.change(inputDataVenc, { target: { value: '2027-05-20' } });

    // Adiciona um passo no checklist
    const inputPasso = screen.getByPlaceholderText(
      'Ex: Agendar vistoria, levar documentos...'
    );
    fireEvent.change(inputPasso, { target: { value: 'Pagar Guia GRU' } });
    fireEvent.click(screen.getByRole('button', { name: '+ Adicionar' }));

    expect(screen.getByText('1. Pagar Guia GRU')).toBeInTheDocument();

    // Submete o formulário
    const btnSalvar = screen.getByRole('button', { name: 'Criar Item' });
    fireEvent.click(btnSalvar);

    await waitFor(() => {
      expect(criarSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          nome: 'Renovar Passaporte',
          tipoVencimento: TipoVencimento.DataUnica,
          dataVencimento: '2027-05-20',
          passos: ['Pagar Guia GRU'],
        })
      );
    });
  });
});
