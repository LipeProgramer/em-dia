import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { Providers } from './utils.tsx';

const registrar = vi.fn();
vi.mock('../api/endpoints.ts', () => ({ authApi: { registrar: (...a: unknown[]) => registrar(...a) } }));
vi.mock('../features/auth/useSession.tsx', () => ({ useSession: () => ({ entrar: vi.fn() }) }));

const { CadastroPage } = await import('../features/auth/CadastroPage.tsx');

describe('Cadastro (T02)', () => {
  beforeEach(() => registrar.mockReset());

  it('CT47 · senha sem número → erro abaixo do campo e a API não é chamada', async () => {
    const user = userEvent.setup();
    render(<Providers><CadastroPage /></Providers>);

    await user.type(screen.getByLabelText('Como quer ser chamado?'), 'Rodrigo');
    await user.type(screen.getByLabelText('E-mail'), 'rodrigo@email.com');
    await user.type(screen.getByLabelText('Senha'), 'semnumero');
    await user.type(screen.getByLabelText('Confirmar senha'), 'semnumero');
    await user.click(screen.getByRole('button', { name: 'Criar conta' }));

    const senha = screen.getByLabelText('Senha');
    expect(await screen.findByText('Use pelo menos 8 caracteres, com letras e números.')).toBeInTheDocument();
    expect(senha).toHaveAttribute('aria-invalid', 'true');
    expect(senha.getAttribute('aria-describedby')).toContain('erro');
    expect(senha).toHaveFocus(); // o foco vai para o primeiro campo inválido
    expect(registrar).not.toHaveBeenCalled();
  });

  it('confirmação diferente da senha mostra erro no campo de confirmação', async () => {
    const user = userEvent.setup();
    render(<Providers><CadastroPage /></Providers>);

    await user.type(screen.getByLabelText('Como quer ser chamado?'), 'Rodrigo');
    await user.type(screen.getByLabelText('E-mail'), 'rodrigo@email.com');
    await user.type(screen.getByLabelText('Senha'), 'Senha123');
    await user.type(screen.getByLabelText('Confirmar senha'), 'Senha124');
    await user.click(screen.getByRole('button', { name: 'Criar conta' }));

    expect(await screen.findByText('As senhas não são iguais.')).toBeInTheDocument();
    expect(registrar).not.toHaveBeenCalled();
  });
});
