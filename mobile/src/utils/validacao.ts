import { z } from 'zod';
import type { TipoVencimento } from '../api/types';
import { brParaIso, hojeIso } from './formato';

// Validações no app (spec §5.5) com as mensagens da spec §6.5. A API continua validando tudo de novo.

export const msg = {
  email: 'Digite um e-mail válido.',
  senha: 'Use pelo menos 8 caracteres, com letras e números.',
  dataFutura: 'A data não pode estar no futuro.',
  dataInvalida: 'Use o formato dd/mm/aaaa.',
  antecedencia: 'Escolha entre 0 e 365 dias.',
  intervaloDias: 'Informe o intervalo em dias.',
};

export type Erros = Record<string, string>;

/** Primeira mensagem de cada campo de um resultado do Zod. */
export function errosDe(resultado: z.ZodSafeParseResult<unknown>): Erros {
  if (resultado.success) return {};
  const erros: Erros = {};
  for (const i of resultado.error.issues) {
    const campo = i.path.join('.');
    erros[campo] ??= i.message;
  }
  return erros;
}

const senha = z.string().min(8, msg.senha).max(64, msg.senha).regex(/[A-Za-zÀ-ÿ]/, msg.senha).regex(/\d/, msg.senha);

export const cadastroSchema = z.object({
  nome: z.string().trim().min(2, 'O nome deve ter entre 2 e 100 caracteres.').max(100, 'O nome deve ter entre 2 e 100 caracteres.'),
  email: z.string().trim().max(150, 'O e-mail deve ter até 150 caracteres.').pipe(z.email(msg.email)),
  senha,
  confirmarSenha: z.string(),
}).refine((d) => d.senha === d.confirmarSenha, { path: ['confirmarSenha'], message: 'As senhas não são iguais.' });

export const loginSchema = z.object({
  email: z.string().trim().pipe(z.email(msg.email)),
  senha: z.string().min(1, 'Informe a senha.'),
});

/** Inteiro a partir do texto digitado; vazio = não informado. */
export const inteiro = (texto: string | undefined) => {
  if (texto === undefined || texto.trim() === '') return undefined;
  const n = Number(texto.replace(/\./g, '').replace(',', '.'));
  return Number.isFinite(n) ? n : NaN;
};

/** Data digitada (dd/mm/aaaa) → ISO; valida formato e, se pedido, que não é futura. */
export function validarData(texto: string, erros: Erros, campo: string, opcoes: { obrigatoria?: string; naoFutura?: boolean; futura?: boolean } = {}) {
  if (!texto.trim()) {
    if (opcoes.obrigatoria) erros[campo] = opcoes.obrigatoria;
    return undefined;
  }
  const iso = brParaIso(texto);
  if (!iso) erros[campo] = msg.dataInvalida;
  else if (opcoes.naoFutura && iso > hojeIso()) erros[campo] = msg.dataFutura;
  else if (opcoes.futura && iso <= hojeIso()) erros[campo] = 'A nova validade precisa ser uma data futura.';
  return iso;
}

export interface ItemTexto {
  nome: string;
  categoriaId: number | null;
  tipoVencimento: TipoVencimento;
  dataBase: string;
  dataVencimento: string;
  intervaloDias: string;
  intervaloKm: string;
  kmBase: string;
  garantiaFabricanteMeses: string;
  veiculoId: string | null;
  antecedenciaDias: string;
  passos: string[];
}

/** Formulário de item (T09/T11): campos obrigatórios conforme o tipo de vencimento. */
export function validarItem(v: ItemTexto): Erros {
  const e: Erros = {};
  if (!v.nome.trim()) e.nome = 'Informe o nome do item.';
  else if (v.nome.trim().length > 80) e.nome = 'O nome deve ter até 80 caracteres.';
  if (!v.categoriaId) e.categoriaId = 'Escolha uma categoria.';

  const ant = inteiro(v.antecedenciaDias);
  if (ant === undefined || Number.isNaN(ant) || ant < 0 || ant > 365) e.antecedenciaDias = msg.antecedencia;

  const positivo = (campo: keyof ItemTexto, texto: string, obrigatorio?: string) => {
    const n = inteiro(texto);
    if (n === undefined) { if (obrigatorio) e[campo] = obrigatorio; }
    else if (Number.isNaN(n) || n <= 0) e[campo] = 'O valor deve ser maior que 0.';
  };

  switch (v.tipoVencimento) {
    case 'DataUnica':
      validarData(v.dataVencimento, e, 'dataVencimento', { obrigatoria: 'Informe a data de vencimento.' });
      break;
    case 'RecorrentePorTempo':
      validarData(v.dataBase, e, 'dataBase', { obrigatoria: 'Informe a data da última vez.', naoFutura: true });
      positivo('intervaloDias', v.intervaloDias, msg.intervaloDias);
      break;
    case 'UsoOuTempo': {
      if (!v.veiculoId) e.veiculoId = 'Escolha o veículo.';
      validarData(v.dataBase, e, 'dataBase', { obrigatoria: 'Informe a data da última vez.', naoFutura: true });
      const kmBase = inteiro(v.kmBase);
      if (kmBase === undefined) e.kmBase = 'Informe a km da última vez.';
      else if (Number.isNaN(kmBase) || kmBase < 0) e.kmBase = 'A km deve ser maior ou igual a 0.';
      positivo('intervaloKm', v.intervaloKm, 'Informe o intervalo em km.');
      positivo('intervaloDias', v.intervaloDias, msg.intervaloDias);
      break;
    }
    case 'Calculado':
      validarData(v.dataBase, e, 'dataBase', { obrigatoria: 'Informe a data da compra.', naoFutura: true });
      positivo('garantiaFabricanteMeses', v.garantiaFabricanteMeses, 'Informe a garantia do fabricante em meses.');
      break;
  }

  if (v.passos.length > 10) e.passos = 'Use até 10 passos.';
  else if (v.passos.some((p) => !p.trim() || p.trim().length > 120)) e.passos = 'Cada passo deve ter de 1 a 120 caracteres.';
  return e;
}

/** Veículo (T15): apelido 1 a 50; km ≥ 0; média 1 a 20.000. */
export function validarVeiculo(v: { apelido: string; kmAtual?: string; mediaKmMes: string }, comKm: boolean): Erros {
  const e: Erros = {};
  if (!v.apelido.trim() || v.apelido.trim().length > 50) e.apelido = 'O apelido deve ter de 1 a 50 caracteres.';
  if (comKm) {
    const kmAtual = inteiro(v.kmAtual);
    if (kmAtual === undefined || Number.isNaN(kmAtual) || kmAtual < 0) e.kmAtual = 'A km deve ser maior ou igual a 0.';
  }
  const media = inteiro(v.mediaKmMes);
  if (media === undefined || Number.isNaN(media) || media < 1 || media > 20000) e.mediaKmMes = 'Informe uma média entre 1 e 20.000.';
  return e;
}
