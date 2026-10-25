import { api, setAccessToken } from './client.ts';
import type {
  AlterarNomeRequest,
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
} from './types.ts';

// ── Auth API ──────────────────────────────────────────────────────────────────
export const authApi = {
  registrar: async (req: RegistrarRequest): Promise<UsuarioResponse> => {
    const { data } = await api.post<UsuarioResponse>('/auth/registrar', req);
    return data;
  },

  login: async (req: LoginRequest): Promise<LoginResponse> => {
    const { data } = await api.post<LoginResponse>('/auth/login', req);
    setAccessToken(data.accessToken);
    return data;
  },

  refresh: async (): Promise<{ accessToken: string }> => {
    const { data } = await api.post<{ accessToken: string }>('/auth/refresh');
    setAccessToken(data.accessToken);
    return data;
  },

  logout: async (): Promise<void> => {
    await api.post('/auth/logout');
    setAccessToken(null);
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
    setAccessToken(null);
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
    const res = await api.get<CheckupResponse>('/checkup/atual');
    if (res.status === 204) return null;
    return res.data;
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

  marcarPasso: async (
    itemId: string,
    passoId: string,
    concluido: boolean
  ): Promise<ItemResponse> => {
    const { data } = await api.patch<ItemResponse>(
      `/itens/${itemId}/passos/${passoId}`,
      { concluido }
    );
    return data;
  },

  concluir: async (
    itemId: string,
    req: ConcluirItemRequest
  ): Promise<ItemResponse> => {
    const { data } = await api.post<ItemResponse>(`/itens/${itemId}/concluir`, req);
    return data;
  },

  listarHistorico: async (itemId: string): Promise<HistoricoResponse[]> => {
    const { data } = await api.get<HistoricoResponse[]>(`/itens/${itemId}/historico`);
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

  atualizarKm: async (id: string, kmAtual: number): Promise<VeiculoResponse> => {
    const { data } = await api.patch<VeiculoResponse>(`/veiculos/${id}/km`, { kmAtual });
    return data;
  },

  excluir: async (id: string): Promise<void> => {
    await api.delete(`/veiculos/${id}`);
  },
};

// ── Preferências API ──────────────────────────────────────────────────────────
export const preferenciasApi = {
  obter: async (): Promise<PreferenciasResponse> => {
    const { data } = await api.get<PreferenciasResponse>('/usuarios/me/preferencias');
    return data;
  },

  atualizar: async (
    req: AtualizarPreferenciasRequest
  ): Promise<PreferenciasResponse> => {
    const { data } = await api.put<PreferenciasResponse>(
      '/usuarios/me/preferencias',
      req
    );
    return data;
  },
};

// ── Catálogo API ──────────────────────────────────────────────────────────────
export const catalogoApi = {
  listarCategorias: async (): Promise<CategoriaResponse[]> => {
    const { data } = await api.get<CategoriaResponse[]>('/catalogo/categorias');
    return data;
  },

  listarModelos: async (categoriaId?: number): Promise<ModeloItemResponse[]> => {
    const params = categoriaId ? { categoriaId } : {};
    const { data } = await api.get<ModeloItemResponse[]>('/catalogo/modelos', {
      params,
    });
    return data;
  },
};
