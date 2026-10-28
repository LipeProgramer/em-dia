import { api, setAccessToken } from './client';
import type {
  AlterarNomeRequest,
  AtualizarKmRequest,
  AtualizarPreferenciasRequest,
  CategoriaResponse,
  CheckupResponse,
  ConcluirItemRequest,
  CriarItemRequest,
  CriarVeiculoRequest,
  EditarItemRequest,
  EditarVeiculoRequest,
  ExecutarCheckupRequest,
  HistoricoResponse,
  ItemResponse,
  LoginRequest,
  LoginResponse,
  ModeloItemResponse,
  PreferenciasResponse,
  RadarResponse,
  RegistrarRequest,
  UsuarioResponse,
  VeiculoResponse,
} from './types';

// ── Auth API ──────────────────────────────────────────────────────────────────
export const authApi = {
  registrar: async (req: RegistrarRequest): Promise<UsuarioResponse> => {
    const { data } = await api.post<UsuarioResponse>('/auth/registrar', req);
    return data;
  },

  login: async (req: LoginRequest): Promise<LoginResponse> => {
    const { data } = await api.post<LoginResponse>('/auth/login', req);
    await setAccessToken(data.accessToken);
    return data;
  },

  logout: async (): Promise<void> => {
    try {
      await api.post('/auth/logout');
    } catch {
      // Ignora falhas de rede no logout
    } finally {
      await setAccessToken(null);
    }
  },

  getMe: async (): Promise<UsuarioResponse> => {
    const { data } = await api.get<UsuarioResponse>('/usuarios/me');
    return data;
  },

  alterarNome: async (req: AlterarNomeRequest): Promise<UsuarioResponse> => {
    const { data } = await api.patch<UsuarioResponse>('/usuarios/me', req);
    return data;
  },

  excluirConta: async (): Promise<void> => {
    await api.delete('/usuarios/me');
    await setAccessToken(null);
  },
};

// ── Radar API ─────────────────────────────────────────────────────────────────
export const radarApi = {
  obter: async (): Promise<RadarResponse> => {
    const { data } = await api.get<RadarResponse>('/radar');
    return data;
  },
};

// ── Check-up API ──────────────────────────────────────────────────────────────
export const checkupApi = {
  obterAtual: async (): Promise<CheckupResponse | null> => {
    try {
      const res = await api.get<CheckupResponse>('/checkup/atual');
      if (res.status === 204) return null;
      return res.data;
    } catch {
      return null;
    }
  },

  executar: async (req: ExecutarCheckupRequest): Promise<CheckupResponse> => {
    const { data } = await api.post<CheckupResponse>('/checkup', req);
    return data;
  },
};

// ── Itens API ─────────────────────────────────────────────────────────────────
export const itensApi = {
  listar: async (categoriaId?: number): Promise<ItemResponse[]> => {
    const params = categoriaId ? { categoriaId } : {};
    const { data } = await api.get<ItemResponse[]>('/itens', { params });
    return data;
  },

  obterPorId: async (id: string): Promise<ItemResponse> => {
    const { data } = await api.get<ItemResponse>(`/itens/${id}`);
    return data;
  },

  criar: async (req: CriarItemRequest): Promise<ItemResponse> => {
    const { data } = await api.post<ItemResponse>('/itens', req);
    return data;
  },

  editar: async (id: string, req: EditarItemRequest): Promise<ItemResponse> => {
    const { data } = await api.put<ItemResponse>(`/itens/${id}`, req);
    return data;
  },

  excluir: async (id: string): Promise<void> => {
    await api.delete(`/itens/${id}`);
  },

  concluir: async (id: string, req: ConcluirItemRequest): Promise<void> => {
    await api.post(`/itens/${id}/concluir`, req);
  },

  obterHistorico: async (id: string): Promise<HistoricoResponse[]> => {
    const { data } = await api.get<HistoricoResponse[]>(`/itens/${id}/historico`);
    return data;
  },
};

// ── Passos API ────────────────────────────────────────────────────────────────
export const passosApi = {
  marcarPasso: async (itemId: string, passoId: string, concluido: boolean): Promise<void> => {
    await api.put(`/itens/${itemId}/passos/${passoId}`, { concluido });
  },
};

// ── Categorias & Modelos API ──────────────────────────────────────────────────
export const categoriasApi = {
  listar: async (): Promise<CategoriaResponse[]> => {
    const { data } = await api.get<CategoriaResponse[]>('/categorias');
    return data;
  },
};

export const modelosApi = {
  listar: async (categoriaId?: number): Promise<ModeloItemResponse[]> => {
    const params = categoriaId ? { categoriaId } : {};
    const { data } = await api.get<ModeloItemResponse[]>('/modelos-item', { params });
    return data;
  },
};

// ── Veículos API ──────────────────────────────────────────────────────────────
export const veiculosApi = {
  listar: async (): Promise<VeiculoResponse[]> => {
    const { data } = await api.get<VeiculoResponse[]>('/veiculos');
    return data;
  },

  obterPorId: async (id: string): Promise<VeiculoResponse> => {
    const { data } = await api.get<VeiculoResponse>(`/veiculos/${id}`);
    return data;
  },

  criar: async (req: CriarVeiculoRequest): Promise<VeiculoResponse> => {
    const { data } = await api.post<VeiculoResponse>('/veiculos', req);
    return data;
  },

  editar: async (id: string, req: EditarVeiculoRequest): Promise<VeiculoResponse> => {
    const { data } = await api.put<VeiculoResponse>(`/veiculos/${id}`, req);
    return data;
  },

  excluir: async (id: string): Promise<void> => {
    await api.delete(`/veiculos/${id}`);
  },

  atualizarKm: async (id: string, req: AtualizarKmRequest): Promise<void> => {
    await api.patch(`/veiculos/${id}/km`, req);
  },
};

// ── Preferências de Notificação API ──────────────────────────────────────────
export const preferenciasApi = {
  obter: async (): Promise<PreferenciasResponse> => {
    const { data } = await api.get<PreferenciasResponse>('/preferencias-aviso');
    return data;
  },

  atualizar: async (req: AtualizarPreferenciasRequest): Promise<PreferenciasResponse> => {
    const { data } = await api.put<PreferenciasResponse>('/preferencias-aviso', req);
    return data;
  },
};
