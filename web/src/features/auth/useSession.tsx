import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { onSessaoExpirada, renovarToken, setAccessToken } from '../../api/client.ts';
import { authApi, usuariosApi } from '../../api/endpoints.ts';
import type { UsuarioLogado } from '../../api/types.ts';

interface Sessao {
  usuario: UsuarioLogado | null;
  carregando: boolean;
  entrar: (email: string, senha: string) => Promise<UsuarioLogado>;
  sair: () => Promise<void>;
  /** Atualiza dados como o nome e o "precisa de check-up" depois de uma ação. */
  atualizarUsuario: (dados: Partial<UsuarioLogado>) => void;
  encerrarLocalmente: () => void;
}

const SessaoContext = createContext<Sessao | null>(null);

/**
 * Sessão do usuário (spec §4.1 e §4.3): o token de acesso fica só em memória; ao recarregar a página,
 * o front chama /auth/refresh — se o cookie for válido, a sessão continua sem pedir a senha.
 */
export function SessionProvider({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<UsuarioLogado | null>(null);
  const [carregando, setCarregando] = useState(true);
  const queryClient = useQueryClient();

  const encerrarLocalmente = useCallback(() => {
    setAccessToken(null);
    setUsuario(null);
    queryClient.clear();
  }, [queryClient]);

  useEffect(() => {
    onSessaoExpirada(encerrarLocalmente);
    let ativo = true;
    renovarToken()
      .then(() => usuariosApi.me())
      .then((me) => ativo && setUsuario({ id: me.id, nome: me.nome, precisaCheckup: me.precisaCheckup }))
      .catch(() => ativo && setUsuario(null))
      .finally(() => ativo && setCarregando(false));
    return () => {
      ativo = false;
      onSessaoExpirada(null);
    };
  }, [encerrarLocalmente]);

  const entrar = useCallback(async (email: string, senha: string) => {
    const resposta = await authApi.login({ email, senha });
    setAccessToken(resposta.accessToken);
    setUsuario(resposta.usuario);
    return resposta.usuario;
  }, []);

  const sair = useCallback(async () => {
    try {
      await authApi.logout();
    } finally {
      encerrarLocalmente();
    }
  }, [encerrarLocalmente]);

  const atualizarUsuario = useCallback(
    (dados: Partial<UsuarioLogado>) => setUsuario((u) => (u ? { ...u, ...dados } : u)),
    [],
  );

  const valor = useMemo(
    () => ({ usuario, carregando, entrar, sair, atualizarUsuario, encerrarLocalmente }),
    [usuario, carregando, entrar, sair, atualizarUsuario, encerrarLocalmente],
  );

  return <SessaoContext.Provider value={valor}>{children}</SessaoContext.Provider>;
}

export function useSession(): Sessao {
  const ctx = useContext(SessaoContext);
  if (!ctx) throw new Error('useSession precisa estar dentro de <SessionProvider>.');
  return ctx;
}
