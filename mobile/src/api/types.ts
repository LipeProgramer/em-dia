// ── Enums como const objects ─────────────────────────────────────────────────
export const TipoVencimento = {
  DataUnica: 1,
  RecorrentePorTempo: 2,
  UsoOuTempo: 3,
  Calculado: 4,
} as const;
export type TipoVencimento = (typeof TipoVencimento)[keyof typeof TipoVencimento];

export const TipoVeiculo = {
  Carro: 1,
  Moto: 2,
} as const;
export type TipoVeiculo = (typeof TipoVeiculo)[keyof typeof TipoVeiculo];

export const AnelRadar = {
  Centro: 0,
  Anel1: 1,
  Anel2: 2,
  Externo: 3,
} as const;
export type AnelRadar = (typeof AnelRadar)[keyof typeof AnelRadar];

export const StatusItem = {
  Vencido: 0,
  Atencao: 1,
  EmDia: 2,
  Encerrado: 3,
} as const;
export type StatusItem = (typeof StatusItem)[keyof typeof StatusItem];

export const SituacaoItem = {
  Ativo: 0,
  Atencao: 1,
  Vencido: 2,
  Encerrado: 3,
} as const;
export type SituacaoItem = (typeof SituacaoItem)[keyof typeof SituacaoItem];

// ── Auth ──────────────────────────────────────────────────────────────────────
export interface RegistrarRequest {
  nome: string;
  email: string;
  senha: string;
}

export interface LoginRequest {
  email: string;
  senha: string;
}

export interface LoginResponse {
  accessToken: string;
  expiraEm: string;
  usuario: UsuarioResponse;
}

export interface UsuarioResponse {
  id: string;
  nome: string;
  email: string;
  precisaCheckup: boolean;
  criadoEm: string;
}

export interface AlterarNomeRequest {
  nome: string;
}

// ── Radar ─────────────────────────────────────────────────────────────────────
export interface PassoResponse {
  id: string;
  ordem: number;
  descricao: string;
  concluido: boolean;
  concluidoEm: string | null;
}

export interface ItemRadarResponse {
  id: string;
  nome: string;
  categoriaId: number;
  categoriaNome?: string;
  anel: AnelRadar;
  status: StatusItem;
  proximoVencimentoData: string | null;
  proximoVencimentoKm: number | null;
  diasRestantes: number;
  progressoPassos: number;
  passos: PassoResponse[];
}

export interface RadarResponse {
  centro: ItemRadarResponse[];
  anel1: ItemRadarResponse[];
  anel2: ItemRadarResponse[];
  externo: ItemRadarResponse[];
  modoFoco: boolean;
}

// ── Itens ─────────────────────────────────────────────────────────────────────
export interface CriarItemRequest {
  nome: string;
  categoriaId: number;
  tipoVencimento: TipoVencimento;
  dataVencimento?: string | null;
  intervaloDias?: number | null;
  veiculoId?: string | null;
  intervaloKm?: number | null;
  valorEstimado?: number | null;
  observacoes?: string | null;
  passos?: string[];
}

export interface EditarItemRequest {
  nome: string;
  tipoVencimento: TipoVencimento;
  dataVencimento?: string | null;
  intervaloDias?: number | null;
  veiculoId?: string | null;
  intervaloKm?: number | null;
  valorEstimado?: number | null;
  observacoes?: string | null;
}

export interface ConcluirItemRequest {
  dataConclusao?: string;
  kmAtual?: number | null;
  observacoes?: string | null;
  recorrenciaManual?: boolean;
}

export interface ItemResponse {
  id: string;
  nome: string;
  categoriaId: number;
  categoriaNome?: string;
  tipoVencimento: TipoVencimento;
  situacao: SituacaoItem;
  status?: StatusItem;
  dataVencimento: string | null;
  diasRestantes: number;
  intervaloDias: number | null;
  veiculoId: string | null;
  veiculoNome?: string | null;
  kmVencimento: number | null;
  kmRestante: number | null;
  valorEstimado: number | null;
  observacoes: string | null;
  passos: PassoResponse[];
  criadoEm: string;
}

// ── Categorias & Modelos ──────────────────────────────────────────────────────
export interface CategoriaResponse {
  id: number;
  nome: string;
  icone: string;
  descricao: string;
}

export interface PassoModeloResponse {
  ordem: number;
  descricao: string;
}

export interface ModeloItemResponse {
  id: string;
  nome: string;
  categoriaId: number;
  tipoVencimento: TipoVencimento;
  intervaloDiasPadrao: number | null;
  intervaloKmPadrao: number | null;
  passosPadrao: PassoModeloResponse[];
}

// ── Veículos ──────────────────────────────────────────────────────────────────
export interface CriarVeiculoRequest {
  nome: string;
  tipo: TipoVeiculo;
  placa?: string | null;
  kmAtual: number;
  mediaKmPorMes?: number | null;
}

export interface EditarVeiculoRequest {
  nome: string;
  tipo: TipoVeiculo;
  placa?: string | null;
  mediaKmPorMes?: number | null;
}

export interface AtualizarKmRequest {
  km: number;
}

export interface VeiculoResponse {
  id: string;
  nome: string;
  tipo: TipoVeiculo;
  placa: string | null;
  kmAtual: number;
  mediaKmPorMes: number | null;
  criadoEm: string;
  atualizadoEm: string;
}

// ── Histórico ─────────────────────────────────────────────────────────────────
export interface HistoricoResponse {
  id: string;
  itemId: string;
  itemNome: string;
  dataConclusao: string;
  kmNoVencimento: number | null;
  observacoes: string | null;
  criadoEm: string;
}

// ── Check-up ──────────────────────────────────────────────────────────────────
export interface RespostaCheckupItem {
  modeloId: string;
  adicionar: boolean;
  dataVencimento?: string | null;
  kmVencimento?: number | null;
}

export interface ExecutarCheckupRequest {
  respostas: RespostaCheckupItem[];
}

export interface CheckupItemResponse {
  modeloId: string;
  modeloNome: string;
  categoriaId: number;
  categoriaNome: string;
}

export interface CheckupResponse {
  id: string;
  concluido: boolean;
  itens: CheckupItemResponse[];
  dataExecucao: string | null;
}

// ── Preferências de Notificação ────────────────────────────────────────────────
export interface PreferenciasResponse {
  notificarAtrasados: boolean;
  notificarHoje: boolean;
  diasAntecedencia: number;
  horarioLembrete: string;
  emailNotificacoes: boolean;
}

export interface AtualizarPreferenciasRequest {
  notificarAtrasados?: boolean;
  notificarHoje?: boolean;
  diasAntecedencia?: number;
  horarioLembrete?: string;
  emailNotificacoes?: boolean;
}

export interface NotificacaoItem {
  id: string;
  titulo: string;
  mensagem: string;
  tipo: 'lembrete' | 'sistema';
  data: string;
  lida: boolean;
}

// ── Erros ─────────────────────────────────────────────────────────────────────
export interface ApiError {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  codigo?: string;
  mensagem?: string;
  errors?: Record<string, string[]>;
}
