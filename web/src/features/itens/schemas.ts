import { z } from 'zod';
import { hojeIso } from '../../utils/formato.ts';

// Schemas Zod dos formulários (spec §5.5). Repetem as validações da API para avisar na hora;
// a API continua sendo a fonte da verdade e valida tudo de novo. Mensagens da spec §6.5.

export const msg = {
  email: 'Digite um e-mail válido.',
  senha: 'Use pelo menos 8 caracteres, com letras e números.',
  dataFutura: 'A data não pode estar no futuro.',
  antecedencia: 'Escolha entre 0 e 365 dias.',
  intervaloDias: 'Informe o intervalo em dias.',
  obrigatorio: 'Preencha este campo.',
};

/** Converte o texto do input em número; vazio vira "não informado". */
const numero = (mensagem = msg.obrigatorio) =>
  z.preprocess((v) => (v === '' || v == null ? undefined : Number(v)),
    z.number({ error: mensagem }).int({ error: 'Use um número inteiro.' }));
const numeroOpcional = () =>
  z.preprocess((v) => (v === '' || v == null ? undefined : Number(v)), z.number({ error: 'Número inválido.' }).optional());
const dataOpcional = z.string().optional().transform((v) => (v ? v : undefined));
const naoFutura = (d?: string) => !d || d <= hojeIso();

// ── Conta ───────────────────────────────────────────────────────────────────

export const senhaSchema = z.string()
  .min(8, msg.senha).max(64, msg.senha)
  .regex(/[A-Za-zÀ-ÿ]/, msg.senha)
  .regex(/\d/, msg.senha);

export const cadastroSchema = z.object({
  nome: z.string().trim().min(2, 'O nome deve ter entre 2 e 100 caracteres.').max(100, 'O nome deve ter entre 2 e 100 caracteres.'),
  email: z.string().trim().min(1, msg.email).max(150, 'O e-mail deve ter até 150 caracteres.').pipe(z.email(msg.email)),
  senha: senhaSchema,
  confirmarSenha: z.string(),
}).refine((d) => d.senha === d.confirmarSenha, { path: ['confirmarSenha'], message: 'As senhas não são iguais.' });

export const esqueciSenhaSchema = z.object({
  email: z.string().trim().min(1, msg.email).pipe(z.email(msg.email)),
});

export const redefinirSenhaSchema = z.object({
  senha: senhaSchema,
  confirmarSenha: z.string(),
}).refine((d) => d.senha === d.confirmarSenha, { path: ['confirmarSenha'], message: 'As senhas não são iguais.' });

export const loginSchema = z.object({
  email: z.string().trim().min(1, msg.email).pipe(z.email(msg.email)),
  senha: z.string().min(1, 'Informe a senha.'),
});

export const preferenciasSchema = z.object({
  receberEmail: z.boolean(),
  maxReavisos: numero().pipe(z.number().min(0, 'Escolha entre 0 e 5 reavisos.').max(5, 'Escolha entre 0 e 5 reavisos.')),
  intervaloReavisoDias: numero().pipe(z.number().min(1, 'Escolha entre 1 e 7 dias.').max(7, 'Escolha entre 1 e 7 dias.')),
  modoFocoPadrao: z.boolean(),
});

export const excluirContaSchema = z.object({ senha: z.string().min(1, 'Digite sua senha para confirmar.') });

// ── Veículo ─────────────────────────────────────────────────────────────────

export const veiculoSchema = z.object({
  apelido: z.string().trim().min(1, 'O apelido deve ter de 1 a 50 caracteres.').max(50, 'O apelido deve ter de 1 a 50 caracteres.'),
  tipo: z.enum(['Carro', 'Moto'], { error: 'Escolha Carro ou Moto.' }),
  kmAtual: numero().pipe(z.number().min(0, 'A km deve ser maior ou igual a 0.')),
  mediaKmMes: numero().pipe(z.number().min(1, 'Informe uma média entre 1 e 20.000.').max(20000, 'Informe uma média entre 1 e 20.000.')),
});

/** Atualizar km (T15): número inteiro ≥ km atual. */
export const atualizarKmSchema = (kmAtual: number) => z.object({
  kmAtual: numero().pipe(z.number().min(kmAtual, `A km não pode ser menor que ${kmAtual.toLocaleString('pt-BR')}.`)),
});

// ── Item (T09, T11) ─────────────────────────────────────────────────────────

export const itemSchema = z.object({
  nome: z.string().trim().min(1, 'Informe o nome do item.').max(80, 'O nome deve ter até 80 caracteres.'),
  categoriaId: numero('Escolha uma categoria.').pipe(z.number().min(1, 'Escolha uma categoria.').max(6)),
  tipoVencimento: z.enum(['DataUnica', 'RecorrentePorTempo', 'UsoOuTempo', 'Calculado'], { error: 'Escolha o tipo.' }),
  dataBase: dataOpcional,
  dataVencimento: dataOpcional,
  intervaloDias: numeroOpcional(),
  intervaloKm: numeroOpcional(),
  kmBase: numeroOpcional(),
  garantiaFabricanteMeses: numeroOpcional(),
  veiculoId: z.string().optional().transform((v) => v || undefined),
  antecedenciaDias: numero(msg.antecedencia).pipe(z.number().min(0, msg.antecedencia).max(365, msg.antecedencia)),
  observacao: z.string().max(500, 'A observação deve ter até 500 caracteres.').optional(),
  passos: z.array(z.object({
    descricao: z.string().trim().min(1, 'O passo não pode ficar vazio.').max(120, 'Cada passo deve ter até 120 caracteres.'),
  })).max(10, 'Use até 10 passos.'),
}).superRefine((d, ctx) => {
  const exigir = (campo: keyof typeof d, mensagem: string) => {
    if (d[campo] === undefined) ctx.addIssue({ code: 'custom', path: [campo], message: mensagem });
  };
  if (!naoFutura(d.dataBase)) ctx.addIssue({ code: 'custom', path: ['dataBase'], message: msg.dataFutura });
  if (d.intervaloDias !== undefined && d.intervaloDias <= 0)
    ctx.addIssue({ code: 'custom', path: ['intervaloDias'], message: 'O intervalo deve ser maior que 0.' });
  if (d.intervaloKm !== undefined && d.intervaloKm <= 0)
    ctx.addIssue({ code: 'custom', path: ['intervaloKm'], message: 'O intervalo deve ser maior que 0.' });
  if (d.kmBase !== undefined && d.kmBase < 0)
    ctx.addIssue({ code: 'custom', path: ['kmBase'], message: 'A km deve ser maior ou igual a 0.' });

  switch (d.tipoVencimento) {
    case 'DataUnica':
      exigir('dataVencimento', 'Informe a data de vencimento.');
      break;
    case 'RecorrentePorTempo':
      exigir('dataBase', 'Informe a data da última vez.');
      exigir('intervaloDias', msg.intervaloDias);
      break;
    case 'UsoOuTempo':
      exigir('veiculoId', 'Escolha o veículo.');
      exigir('dataBase', 'Informe a data da última vez.');
      exigir('kmBase', 'Informe a km da última vez.');
      exigir('intervaloKm', 'Informe o intervalo em km.');
      exigir('intervaloDias', msg.intervaloDias);
      break;
    case 'Calculado':
      exigir('dataBase', 'Informe a data da compra.');
      exigir('garantiaFabricanteMeses', 'Informe a garantia do fabricante em meses.');
      break;
  }
});

// ── Concluir (T12) ──────────────────────────────────────────────────────────

export const concluirSchema = (opcoes: { exigeKm: boolean; kmMinima?: number | null }) => z.object({
  dataConclusao: z.string().min(1, 'Informe a data da conclusão.').refine(naoFutura, msg.dataFutura),
  km: numeroOpcional(),
  novaDataVencimento: dataOpcional,
}).superRefine((d, ctx) => {
  if (opcoes.exigeKm && d.km === undefined)
    ctx.addIssue({ code: 'custom', path: ['km'], message: 'Informe a quilometragem.' });
  if (d.km !== undefined && opcoes.kmMinima != null && d.km < opcoes.kmMinima)
    ctx.addIssue({ code: 'custom', path: ['km'], message: `A km não pode ser menor que ${opcoes.kmMinima.toLocaleString('pt-BR')}.` });
  if (d.novaDataVencimento && d.novaDataVencimento <= hojeIso())
    ctx.addIssue({ code: 'custom', path: ['novaDataVencimento'], message: 'A nova validade precisa ser uma data futura.' });
});

export type ItemForm = z.input<typeof itemSchema>;
export type ItemValido = z.output<typeof itemSchema>;
