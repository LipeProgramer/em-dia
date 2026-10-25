import {
  createContext,
  useEffect,
  useState,
  useCallback,
  type ReactNode,
} from 'react';
import { authApi } from '../../api/services.ts';
import { setAccessToken } from '../../api/client.ts';
import type {
  LoginRequest,
  RegistrarRequest,
  UsuarioResponse,
} from '../../api/types.ts';

interface AuthContextType {
  user: UsuarioResponse | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (req: LoginRequest) => Promise<UsuarioResponse>;
  registrar: (req: RegistrarRequest) => Promise<UsuarioResponse>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
  updateNome: (nome: string) => Promise<void>;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UsuarioResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const refreshUser = useCallback(async () => {
    try {
      const me = await authApi.getMe();
      setUser(me);
    } catch {
      setUser(null);
      setAccessToken(null);
    }
  }, []);

  // Tentativa inicial de restaurar sessão com refresh_token via cookie
  useEffect(() => {
    let isMounted = true;
    const initAuth = async () => {
      try {
        await authApi.refresh();
        const me = await authApi.getMe();
        if (isMounted) setUser(me);
      } catch {
        if (isMounted) {
          setUser(null);
          setAccessToken(null);
        }
      } finally {
        if (isMounted) setIsLoading(false);
      }
    };

    initAuth();
    return () => {
      isMounted = false;
    };
  }, []);

  const login = async (req: LoginRequest): Promise<UsuarioResponse> => {
    const res = await authApi.login(req);
    setUser(res.usuario);
    return res.usuario;
  };

  const registrar = async (req: RegistrarRequest): Promise<UsuarioResponse> => {
    await authApi.registrar(req);
    // Realiza login automaticamente após cadastro
    const loginRes = await authApi.login({ email: req.email, senha: req.senha });
    setUser(loginRes.usuario);
    return loginRes.usuario;
  };

  const logout = async () => {
    try {
      await authApi.logout();
    } finally {
      setUser(null);
      setAccessToken(null);
    }
  };

  const updateNome = async (nome: string) => {
    const updated = await authApi.alterarNome({ nome });
    setUser(updated);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        login,
        registrar,
        logout,
        refreshUser,
        updateNome,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export { useAuth } from './useAuth.ts';
