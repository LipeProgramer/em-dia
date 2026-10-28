import React, {
  createContext,
  useContext,
  useEffect,
  useState,
  useCallback,
  type ReactNode,
} from 'react';
import { authApi } from '../api/services';
import { loadStoredAuth, setAccessToken } from '../api/client';
import type {
  LoginRequest,
  RegistrarRequest,
  UsuarioResponse,
} from '../api/types';

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

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UsuarioResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const refreshUser = useCallback(async () => {
    try {
      const me = await authApi.getMe();
      setUser(me);
    } catch {
      setUser(null);
      await setAccessToken(null);
    }
  }, []);

  // Tentativa inicial de restaurar sessão com token salvo no AsyncStorage
  useEffect(() => {
    let isMounted = true;
    const initAuth = async () => {
      try {
        const token = await loadStoredAuth();
        if (token) {
          const me = await authApi.getMe();
          if (isMounted) setUser(me);
        } else {
          if (isMounted) setUser(null);
        }
      } catch {
        if (isMounted) {
          setUser(null);
          await setAccessToken(null);
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
      await setAccessToken(null);
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

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth deve ser usado dentro de um AuthProvider');
  }
  return context;
}
