import { api } from './client.ts';
import type {
  AtualizarKmResponse, Categoria, CheckupRequest, CheckupResponse, ConcluirRequest, ConcluirResponse,
  HistoricoItem, ItemDetalhe, ItemResumo, LoginResponse, Modelo, Pergunta, Preferencias, RadarResponse,
  SalvarItemRequest, StatusItem, TipoVeiculo, Usuario, Veiculo,
} from './types.ts';

// Uma função por endpoint (E01–E25 · spec §3.3).

export const authApi = {
  registrar: (dados: { nome: string; email: string; senha: string }) =>
    api.post<{ id: string; nome: string; email: string }>('/auth/registrar', dados).then((r) => r.data),
  login: (dados: { email: string; senha: string }) =>
    api.post<LoginResponse>('/auth/login', dados).then((r) => r.data),
  logout: () => api.post('/auth/logout'),
};

export const usuariosApi = {
  me: () => api.get<Usuario>('/usuarios/me').then((r) => r.data),
  atualizarPreferencias: (p: Preferencias) =>
    api.put<Preferencias>('/usuarios/me/preferencias', p).then((r) => r.data),
  excluirConta: (senha: string) => api.delete('/usuarios/me', { data: { senha } }),
};

/** Extra (DV20) · notificações no celular. */
export const pushApi = {
  chavePublica: () => api.get<{ chavePublica: string }>('/push/chave-publica').then((r) => r.data.chavePublica),
  inscrever: (dados: { endpoint: string; p256dh: string; auth: string }) => api.post('/push/inscricoes', dados),
  remover: (endpoint: string) => api.delete('/push/inscricoes', { data: { endpoint } }),
  testar: () => api.post<{ enviadas: number }>('/push/teste').then((r) => r.data.enviadas),
};

export const catalogoApi = {
  categorias: () => api.get<Categoria[]>('/categorias').then((r) => r.data),
  modelos: (categoriaId?: number) =>
    api.get<Modelo[]>('/modelos', { params: { categoriaId } }).then((r) => r.data),
  perguntas: () => api.get<Pergunta[]>('/checkup/perguntas').then((r) => r.data),
  fazerCheckup: (dados: CheckupRequest) => api.post<CheckupResponse>('/checkup', dados).then((r) => r.data),
};

export const radarApi = {
  obter: (foco?: boolean) => api.get<RadarResponse>('/radar', { params: { foco } }).then((r) => r.data),
};

export const itensApi = {
  listar: (filtros: { categoriaId?: number; status?: StatusItem }) =>
    api.get<ItemResumo[]>('/itens', { params: filtros }).then((r) => r.data),
  obter: (id: string) => api.get<ItemDetalhe>(`/itens/${id}`).then((r) => r.data),
  criar: (dados: SalvarItemRequest) => api.post<ItemDetalhe>('/itens', dados).then((r) => r.data),
  editar: (id: string, dados: SalvarItemRequest) => api.put<ItemDetalhe>(`/itens/${id}`, dados).then((r) => r.data),
  excluir: (id: string) => api.delete(`/itens/${id}`),
  concluir: (id: string, dados: ConcluirRequest) =>
    api.post<ConcluirResponse>(`/itens/${id}/concluir`, dados).then((r) => r.data),
  marcarPasso: (id: string, passoId: string, concluido: boolean) =>
    api.patch<{ passoId: string; concluido: boolean; progressoPassos: number }>(
      `/itens/${id}/passos/${passoId}`, { concluido }).then((r) => r.data),
};

export interface VeiculoForm {
  apelido: string;
  tipo: TipoVeiculo;
  kmAtual: number;
  mediaKmMes: number;
}

export const veiculosApi = {
  listar: () => api.get<Veiculo[]>('/veiculos').then((r) => r.data),
  criar: (dados: VeiculoForm) => api.post<Veiculo>('/veiculos', dados).then((r) => r.data),
  editar: (id: string, dados: Omit<VeiculoForm, 'kmAtual'>) =>
    api.put<Veiculo>(`/veiculos/${id}`, dados).then((r) => r.data),
  atualizarKm: (id: string, kmAtual: number) =>
    api.patch<AtualizarKmResponse>(`/veiculos/${id}/km`, { kmAtual }).then((r) => r.data),
  excluir: (id: string) => api.delete(`/veiculos/${id}`),
};

export const historicoApi = {
  listar: (filtros: { itemId?: string; de?: string; ate?: string }) =>
    api.get<HistoricoItem[]>('/historico', { params: filtros }).then((r) => r.data),
};
