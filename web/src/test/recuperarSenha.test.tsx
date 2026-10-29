import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { erroDaApi, Providers } from './utils.tsx';

const esqueciSenha = vi.fn();
const redefinirSenha = vi.fn();
vi.mock('../api/endpoints.ts', () => ({
  authApi: {
    esqueciSenha: (...a: unknown[]) => esqueciSenha(...a),
    redefinirSenha: (...a: unknown[]) => redefinirSenha(...a),
  },
}));

const { EsqueciSenhaPage, RedefinirSenhaPage } = await import('../features/auth/RecuperarSenhaPages.tsx');

describe('Esqueci minha senha (DV24)', () => {
  beforeEach(() => {
    esqueciSenha.mockReset();
    redefinirSenha.mockReset();
  });

  it('pede o link e mostra a mesma confirmação, com ou sem conta', async () => {
    esqueciSenha.mockResolvedValue({});
    const user = userEvent.setup();
    render(<Providers rota="/esqueci-senha"><EsqueciSenhaPage /></Providers>);

    await user.type(screen.getByLabelText('E-mail da conta'), 'prof@email.com');
    await user.click(screen.getByRole('button', { name: 'Enviar link' }));

    expect(esqueciSenha).toHaveBeenCalledWith('prof@email.com');
    expect(await screen.findByText(/o link chega em instantes/)).toBeInTheDocument();
  });

  it('link vencido ou já usado → explica e oferece pedir outro', async () => {
    redefinirSenha.mockRejectedValue(erroDaApi(422, { code: 'LINK_INVALIDO', detail: 'Este link expirou.' }));
    const user = userEvent.setup();
    render(<Providers rota="/redefinir-senha?token=abc"><RedefinirSenhaPage /></Providers>);

    await user.type(screen.getByLabelText('Nova senha'), 'NovaSenha456');
    await user.type(screen.getByLabelText('Confirmar nova senha'), 'NovaSenha456');
    await user.click(screen.getByRole('button', { name: 'Salvar nova senha' }));

    expect(redefinirSenha).toHaveBeenCalledWith('abc', 'NovaSenha456');
    expect(await screen.findByText(/expirou ou já foi usado/)).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Pedir novo link' })).toHaveAttribute('href', '/esqueci-senha');
  });
});
