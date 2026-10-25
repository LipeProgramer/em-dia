// ── Enums como const objects (compatível com erasableSyntaxOnly) ──────────────
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
export interface ItemResponse {
  id: string;
  nome: string;
  categoriaId: number;
  tipoVencimento: TipoVencimento;
  dataBase: string | null;
  dataVencimento: string | null;
  intervaloDias: number | null;
  intervaloKm: number | null;
  kmBase: number | null;
  garantiaFabricanteMeses: number | null;
  antecedenciaDias: number;
  proximoVencimentoData: string | null;
  proximoVencimentoKm: number | null;
  situacao: SituacaoItem;
  status: StatusItem;
  diasRestantes: number;
  progressoPassos: number;
  veiculoId: string | null;
  modeloItemId: string | null;
  observacao: string | null;
  passos: PassoResponse[];
  criadoEm: string;
  atualizadoEm: string;
}

export interface CriarItemRequest {
  nome: string;
  categoriaId: number;
  tipoVencimento: TipoVencimento;
  dataBase?: string | null;
  dataVencimento?: string | null;
  intervaloDias?: number | null;
  intervaloKm?: number | null;
  kmBase?: number | null;
  garantiaFabricanteMeses?: number | null;
  antecedenciaDias: number;
  veiculoId?: string | null;
  modeloItemId?: string | null;
  observacao?: string | null;
  passos?: string[];
}

export type EditarItemRequest = CriarItemRequest;

export interface ConcluirItemRequest {
  dataConclusao: string;
  kmNaConclusao?: number | null;
  valorGasto?: number | null;
  novaDataVencimento?: string | null;
  observacao?: string | null;
  confirmarPassosPendentes?: boolean;
}

export interface HistoricoResponse {
  id: string;
  itemId: string;
  concluidoEm: string;
  kmNaConclusao: number | null;
  valorGasto: number | null;
  vencimentoAnterior: string | null;
  proximoVencimento: string | null;
  observacao: string | null;
  criadoEm: string;
}

// ── Veículos ──────────────────────────────────────────────────────────────────
export interface VeiculoResponse {
  id: string;
  apelido: string;
  tipoVeiculo: TipoVeiculo;
  placa: string | null;
  kmAtual: number;
  mediaKmMes: number | null;
  criadoEm: string;
}

export interface CriarVeiculoRequest {
  apelido: string;
  tipoVeiculo: TipoVeiculo;
  placa?: string | null;
  kmAtual: number;
  mediaKmMes?: number | null;
}

export type EditarVeiculoRequest = CriarVeiculoRequest;

// ── Checkup ───────────────────────────────────────────────────────────────────
export interface CheckupResponse {
  id: string;
  realizadoEm: string;
  itensCriados: number;
  respostas: Record<string, boolean>;
}

export interface ExecutarCheckupRequest {
  respostas: Record<string, boolean>;
}

// ── Preferências ──────────────────────────────────────────────────────────────
export interface PreferenciasResponse {
  receberEmail: boolean;
  maxReavisos: number;
  intervaloReavisoDias: number;
  modoFocoPadrao: boolean;
  atualizadoEm: string;
}

export interface AtualizarPreferenciasRequest {
  receberEmail: boolean;
  maxReavisos: number;
  intervaloReavisoDias: number;
  modoFocoPadrao: boolean;
}

// ── Catálogo ──────────────────────────────────────────────────────────────────
export interface CategoriaResponse {
  id: number;
  nome: string;
  icone: string;
  ordem: number;
}

export interface PassoModeloResponse {
  id: string;
  ordem: number;
  descricao: string;
}

export interface ModeloItemResponse {
  id: string;
  nome: string;
  categoriaId: number;
  tipoVencimento: TipoVencimento;
  intervaloDias: number | null;
  intervaloKm: number | null;
  garantiaFabricanteMeses: number | null;
  antecedenciaDias: number;
  codigoCheckup: string | null;
  passos: PassoModeloResponse[];
}

// ── Erro de API ───────────────────────────────────────────────────────────────
export interface ApiError {
  codigo: string;
  mensagem: string;
  detalhe?: string;
}
