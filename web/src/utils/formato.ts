// Datas no formato ISO 8601 (AAAA-MM-DD) para a API e dd/mm/aaaa para a tela.

/** Data de hoje no fuso do navegador, em ISO (AAAA-MM-DD). */
export function hojeIso(): string {
  const d = new Date();
  const mes = String(d.getMonth() + 1).padStart(2, '0');
  const dia = String(d.getDate()).padStart(2, '0');
  return `${d.getFullYear()}-${mes}-${dia}`;
}

export function dataBr(iso: string | null | undefined): string {
  if (!iso) return '—';
  const [a, m, d] = iso.slice(0, 10).split('-');
  return `${d}/${m}/${a}`;
}

export const km = (valor: number | null | undefined) =>
  valor == null ? '—' : `${valor.toLocaleString('pt-BR')} km`;

export const dinheiro = (valor: number | null | undefined) =>
  valor == null ? '—' : valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });

export const categorias: Record<number, { nome: string; icone: 'car' | 'id-card' | 'droplet' | 'paw' | 'folder' | 'shield-check' }> = {
  1: { nome: 'Veículo', icone: 'car' },
  2: { nome: 'Documentos', icone: 'id-card' },
  3: { nome: 'Casa', icone: 'droplet' },
  4: { nome: 'Saúde', icone: 'paw' },
  5: { nome: 'Finanças', icone: 'folder' },
  6: { nome: 'Garantias', icone: 'shield-check' },
};

export const tiposVencimento = {
  DataUnica: 'Data única (ex.: validade da CNH)',
  RecorrentePorTempo: 'Recorrente por tempo (ex.: a cada 180 dias)',
  UsoOuTempo: 'Uso ou tempo (km ou dias, o que vier primeiro)',
  Calculado: 'Garantia (compra + garantia + 90 dias)',
} as const;
