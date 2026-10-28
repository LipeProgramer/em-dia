// Contratos da API (spec §3.4). Enums trafegam como texto.

export type TipoVencimento = 'DataUnica' | 'RecorrentePorTempo' | 'UsoOuTempo' | 'Calculado';
export type StatusItem = 'Vencido' | 'Atencao' | 'EmDia' | 'Encerrado';
export type AnelRadar = 'Centro' | 'Anel1' | 'Anel2' | 'Externo';
export type TipoVeiculo = 'Carro' | 'Moto';
export type SituacaoItem = 'Ativo' | 'Encerrado';

/** Erro no formato ProblemDetails (spec §3.5). */
export interface ProblemDetails {
  title?: string;
  status?: number;
  detail?: string;
  code?: string;
  errors?: Record<string, string[]>;
}

// ── Autenticação e conta ────────────────────────────────────────────────────
export interface UsuarioLogado {
  id: string;
  nome: string;
  precisaCheckup: boolean;
}

export interface LoginResponse {
  accessToken: string;
  expiraEm: string;
  usuario: UsuarioLogado;
}

export interface Preferencias {
  receberEmail: boolean;
  maxReavisos: number;
  intervaloReavisoDias: number;
  modoFocoPadrao: boolean;
}

export interface Usuario {
  id: string;
  nome: string;
  email: string;
  ultimoCheckupEm: string | null;
  precisaCheckup: boolean;
  preferencias: Preferencias;
}

// ── Catálogo e check-up ─────────────────────────────────────────────────────
export interface Categoria {
  id: number;
  nome: string;
  icone: string;
  ordem: number;
}

export interface Modelo {
  id: string;
  nome: string;
  categoriaId: number;
  tipoVencimento: TipoVencimento;
  intervaloDias: number | null;
  intervaloKm: number | null;
  garantiaFabricanteMeses: number | null;
  antecedenciaDias: number;
  codigoCheckup: string | null;
  passos: string[];
}

export interface Pergunta {
  codigo: string;
  texto: string;
  ajuda: string;
  categoriaId: number;
  modelos: Modelo[];
}

export interface CheckupRequest {
  respostas: { codigo: string; resposta: boolean }[];
  veiculos: { ref: string; apelido: string; tipo: TipoVeiculo; kmAtual: number; mediaKmMes: number }[];
  itens: {
    modeloId: string;
    veiculoRef?: string;
    dataBase?: string;
    kmBase?: number;
    dataVencimento?: string;
  }[];
}

export interface ResumoStatus {
  vencidos: number;
  atencao: number;
  emDia: number;
}

export interface CheckupResponse {
  checkupId: string;
  itensCriados: number;
  resumo: ResumoStatus;
  areasEmDia: number;
  areasSemItens: number;
  /** status nulo = nenhum item cadastrado na área */
  areas: { categoriaId: number; categoria: string; status: StatusItem | null; quantidadeItens: number }[];
  destaques: { itemId: string; nome: string; categoriaId: number; status: StatusItem; textoPrazo: string }[];
}

// ── Radar e itens ───────────────────────────────────────────────────────────
export interface ItemResumo {
  id: string;
  nome: string;
  categoriaId: number;
  tipoVencimento: TipoVencimento;
  status: StatusItem;
  anel: AnelRadar;
  diasRestantes: number | null;
  kmRestantes: number | null;
  dataLimite: string | null;
  textoPrazo: string;
  progressoPassos: number;
}

export interface RadarResponse {
  resumo: ResumoStatus;
  modoFoco: boolean;
  itens: ItemResumo[];
}

export interface Passo {
  id: string;
  ordem: number;
  descricao: string;
  concluido: boolean;
}

export interface ItemDetalhe {
  id: string;
  nome: string;
  categoriaId: number;
  modeloItemId: string | null;
  tipoVencimento: TipoVencimento;
  situacao: SituacaoItem;
  status: StatusItem;
  anel: AnelRadar;
  textoPrazo: string;
  diasRestantes: number | null;
  kmRestantes: number | null;
  dataLimite: string | null;
  proximoVencimentoData: string | null;
  proximoVencimentoKm: number | null;
  dataBase: string | null;
  dataVencimento: string | null;
  intervaloDias: number | null;
  intervaloKm: number | null;
  kmBase: number | null;
  garantiaFabricanteMeses: number | null;
  antecedenciaDias: number;
  observacao: string | null;
  veiculo: { id: string; apelido: string } | null;
  passos: Passo[];
  progressoPassos: number;
  historico: { id: string; concluidoEm: string; km: number | null; valorGasto: number | null }[];
}

export interface SalvarItemRequest {
  nome: string;
  categoriaId: number;
  modeloId?: string | null;
  tipoVencimento: TipoVencimento;
  dataBase?: string | null;
  dataVencimento?: string | null;
  intervaloDias?: number | null;
  intervaloKm?: number | null;
  kmBase?: number | null;
  garantiaFabricanteMeses?: number | null;
  veiculoId?: string | null;
  antecedenciaDias: number;
  observacao?: string | null;
  passos?: string[];
}

export interface ConcluirRequest {
  dataConclusao: string;
  km?: number | null;
  valorGasto?: number | null;
  observacao?: string | null;
  confirmarPassosPendentes: boolean;
  novaDataVencimento?: string | null;
}

export interface ConcluirResponse {
  item: ItemDetalhe;
  historicoId: string;
  mensagem: string;
}

// ── Veículos e histórico ────────────────────────────────────────────────────
export interface Veiculo {
  id: string;
  apelido: string;
  tipo: TipoVeiculo;
  kmAtual: number;
  mediaKmMes: number;
  kmAtualizadoEm: string;
  quantidadeItens: number;
}

export interface AtualizarKmResponse {
  kmAtual: number;
  itensAfetados: { id: string; nome: string; status: StatusItem }[];
}

export interface HistoricoItem {
  id: string;
  itemId: string;
  itemNome: string;
  concluidoEm: string;
  km: number | null;
  valorGasto: number | null;
  observacao: string | null;
  vencimentoAnterior: string | null;
  proximoVencimento: string | null;
}
