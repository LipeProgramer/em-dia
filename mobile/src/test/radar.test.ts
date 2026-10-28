import { describe, it, expect } from 'vitest';
import type { ItemResponse } from '../api/types';
import { TipoVencimento, SituacaoItem } from '../api/types';

describe('Lógica do Radar e Modo Foco Anti-TDAH', () => {
  const itensExemplo: ItemResponse[] = [
    {
      id: 'item-1',
      nome: 'IPVA 2026',
      categoriaId: 2,
      categoriaNome: 'Veículo',
      tipoVencimento: TipoVencimento.DataUnica,
      situacao: SituacaoItem.Ativo,
      dataVencimento: '2026-10-15',
      diasRestantes: -12, // Atrasado
      intervaloDias: null,
      veiculoId: null,
      kmVencimento: null,
      kmRestante: null,
      valorEstimado: 1234.56,
      observacoes: 'Site da Sefaz',
      passos: [],
      criadoEm: '2026-10-01',
    },
    {
      id: 'item-2',
      nome: 'Renovar CNH',
      categoriaId: 1,
      categoriaNome: 'Documento',
      tipoVencimento: TipoVencimento.DataUnica,
      situacao: SituacaoItem.Ativo,
      dataVencimento: '2026-10-27',
      diasRestantes: 0, // Hoje
      intervaloDias: null,
      veiculoId: null,
      kmVencimento: null,
      kmRestante: null,
      valorEstimado: 350.0,
      observacoes: 'Detran',
      passos: [],
      criadoEm: '2026-10-01',
    },
    {
      id: 'item-3',
      nome: 'Revisão do Carro',
      categoriaId: 2,
      categoriaNome: 'Veículo',
      tipoVencimento: TipoVencimento.DataUnica,
      situacao: SituacaoItem.Ativo,
      dataVencimento: '2026-11-05',
      diasRestantes: 5, // Próximos 7 dias
      intervaloDias: null,
      veiculoId: null,
      kmVencimento: null,
      kmRestante: null,
      valorEstimado: 800.0,
      observacoes: 'Concessionária',
      passos: [],
      criadoEm: '2026-10-01',
    },
    {
      id: 'item-4',
      nome: 'Consulta Médica',
      categoriaId: 3,
      categoriaNome: 'Saúde',
      tipoVencimento: TipoVencimento.DataUnica,
      situacao: SituacaoItem.Ativo,
      dataVencimento: '2026-12-01',
      diasRestantes: 32, // Em dia
      intervaloDias: null,
      veiculoId: null,
      kmVencimento: null,
      kmRestante: null,
      valorEstimado: 250.0,
      observacoes: 'Consultório',
      passos: [],
      criadoEm: '2026-10-01',
    },
    {
      id: 'item-5',
      nome: 'Aluguel',
      categoriaId: 4,
      categoriaNome: 'Casa',
      tipoVencimento: TipoVencimento.DataUnica,
      situacao: SituacaoItem.Ativo,
      dataVencimento: '2026-10-20',
      diasRestantes: -7, // Atrasado
      intervaloDias: null,
      veiculoId: null,
      kmVencimento: null,
      kmRestante: null,
      valorEstimado: 2000.0,
      observacoes: 'Transferência Pix',
      passos: [],
      criadoEm: '2026-10-01',
    },
  ];

  it('deve categorizar contagens do radar corretamente', () => {
    const atrasados = itensExemplo.filter((i) => i.diasRestantes < 0).length;
    const hoje = itensExemplo.filter((i) => i.diasRestantes === 0).length;
    const proximos = itensExemplo.filter((i) => i.diasRestantes > 0 && i.diasRestantes <= 7).length;
    const emDia = itensExemplo.filter((i) => i.diasRestantes > 7).length;

    expect(atrasados).toBe(2); // IPVA e Aluguel
    expect(hoje).toBe(1);      // CNH
    expect(proximos).toBe(1);  // Revisão
    expect(emDia).toBe(1);     // Consulta Médica
  });

  it('deve priorizar no Modo Foco exatamente os 3 itens mais críticos', () => {
    // Ordenação por menor diasRestantes (mais atrasados primeiro)
    const foco = [...itensExemplo]
      .sort((a, b) => a.diasRestantes - b.diasRestantes)
      .slice(0, 3);

    expect(foco.length).toBe(3);
    expect(foco[0].nome).toBe('IPVA 2026');   // -12 dias
    expect(foco[1].nome).toBe('Aluguel');     // -7 dias
    expect(foco[2].nome).toBe('Renovar CNH'); // 0 dias (hoje)
  });
});
