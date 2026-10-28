import React, { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { guardarRefresh, lerRefresh, onSessaoExpirada, renovarToken, setAccessToken } from '../api/client';
import { authApi, usuariosApi } from '../api/services';
import type { UsuarioLogado } from '../api/types';

interface Sessao {
  usuario: UsuarioLogado | null;
  carregando: boolean;
  entrar: (email: string, senha: string) => Promise<UsuarioLogado>;
  registrar: (nome: string, email: string, senha: string) => Promise<UsuarioLogado>;
  sair: () => Promise<void>;
  atualizarUsuario: (dados: Partial<UsuarioLogado>) => void;
  encerrarLocalmente: () => Promise<void>;
}

const SessaoContext = createContext<Sessao | null>(null);

/**
 * Sessão do app (spec §4.1/§4.3). Ao abrir, usa o token de renovação guardado no SecureStore para
 * continuar a sessão sem pedir a senha. O token de acesso fica só em memória.
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<UsuarioLogado | null>(null);
  const [carregando, setCarregando] = useState(true);

  const encerrarLocalmente = useCallback(async () => {
    setAccessToken(null);
    await guardarRefresh(null);
    setUsuario(null);
  }, []);

  useEffect(() => {
    onSessaoExpirada(() => setUsuario(null));
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
  }, []);

  const entrar = useCallback(async (email: string, senha: string) => {
    const r = await authApi.login({ email: email.trim(), senha });
    setAccessToken(r.accessToken);
    await guardarRefresh(r.refreshToken);
    setUsuario(r.usuario);
    return r.usuario;
  }, []);

  const registrar = useCallback(async (nome: string, email: string, senha: string) => {
    await authApi.registrar({ nome: nome.trim(), email: email.trim(), senha });
    return entrar(email, senha);
  }, [entrar]);

  const sair = useCallback(async () => {
    try {
      await authApi.logout(await lerRefresh());
    } catch {
      // Sem rede: a sessão local é encerrada mesmo assim
    } finally {
      await encerrarLocalmente();
    }
  }, [encerrarLocalmente]);

  const atualizarUsuario = useCallback(
    (dados: Partial<UsuarioLogado>) => setUsuario((u) => (u ? { ...u, ...dados } : u)),
    [],
  );

  const valor = useMemo(
    () => ({ usuario, carregando, entrar, registrar, sair, atualizarUsuario, encerrarLocalmente }),
    [usuario, carregando, entrar, registrar, sair, atualizarUsuario, encerrarLocalmente],
  );

  return <SessaoContext.Provider value={valor}>{children}</SessaoContext.Provider>;
}

export function useAuth(): Sessao {
  const ctx = useContext(SessaoContext);
  if (!ctx) throw new Error('useAuth precisa estar dentro de <AuthProvider>.');
  return ctx;
}
