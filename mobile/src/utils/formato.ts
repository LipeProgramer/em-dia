// Datas: a API usa ISO 8601 (AAAA-MM-DD); a tela usa dd/mm/aaaa.

/** Data de hoje no fuso do aparelho, em ISO. */
export function hojeIso(): string {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

export function dataBr(iso: string | null | undefined): string {
  if (!iso) return '—';
  const [a, m, d] = iso.slice(0, 10).split('-');
  return `${d}/${m}/${a}`;
}

/** "15/10/2026" → "2026-10-15"; devolve undefined se a data não existir no calendário. */
export function brParaIso(br: string): string | undefined {
  const m = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(br.trim());
  if (!m) return undefined;
  const [, d, mes, a] = m;
  const data = new Date(Number(a), Number(mes) - 1, Number(d));
  if (data.getFullYear() !== Number(a) || data.getMonth() !== Number(mes) - 1 || data.getDate() !== Number(d)) return undefined;
  return `${a}-${mes}-${d}`;
}

/** Máscara de digitação: "15102026" → "15/10/2026". */
export function mascaraData(texto: string): string {
  const n = texto.replace(/\D/g, '').slice(0, 8);
  if (n.length <= 2) return n;
  if (n.length <= 4) return `${n.slice(0, 2)}/${n.slice(2)}`;
  return `${n.slice(0, 2)}/${n.slice(2, 4)}/${n.slice(4)}`;
}

export const km = (valor: number | null | undefined) =>
  valor == null ? '—' : `${valor.toLocaleString('pt-BR')} km`;

export const dinheiro = (valor: number | null | undefined) =>
  valor == null ? '—' : `R$ ${valor.toFixed(2).replace('.', ',')}`;

export const nomesCategorias: Record<number, string> = {
  1: 'Veículo', 2: 'Documentos', 3: 'Casa', 4: 'Saúde', 5: 'Finanças', 6: 'Garantias',
};

export const tiposVencimento = {
  DataUnica: 'Data única',
  RecorrentePorTempo: 'Recorrente',
  UsoOuTempo: 'Uso ou tempo',
  Calculado: 'Garantia',
} as const;
