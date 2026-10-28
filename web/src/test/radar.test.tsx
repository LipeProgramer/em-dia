import { render, screen, within } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import type { ItemResumo } from '../api/types.ts';
import { StatusBadge } from '../components/ui.tsx';
import { ListaAtencao } from '../features/radar/RadarPage.tsx';
import { RadarChart } from '../features/radar/RadarChart.tsx';
import { Providers } from './utils.tsx';

const ipva: ItemResumo = {
  id: 'i1', nome: 'IPVA', categoriaId: 1, tipoVencimento: 'RecorrentePorTempo', status: 'Vencido', anel: 'Centro',
  diasRestantes: -5, kmRestantes: null, dataLimite: '2026-10-10', textoPrazo: 'venceu há 5 dias', progressoPassos: 0,
};

describe('Status acessível (RNF02)', () => {
  it('CT50 · item Vencido no Radar e na lista → cor + símbolo + texto "Vencido"', () => {
    render(<Providers><ListaAtencao titulo="Precisa de atenção" itens={[ipva]} /></Providers>);

    const link = screen.getByRole('link', { name: /IPVA/ });
    const badge = within(link).getByText('Vencido');
    expect(badge).toHaveClass('status-vencido');          // cor
    expect(badge.querySelector('svg')).not.toBeNull();     // símbolo
    expect(link).toHaveTextContent('venceu há 5 dias');    // texto do prazo
  });

  it('CT50 · o ponto no Radar tem nome, status e prazo para leitores de tela', () => {
    render(<Providers><RadarChart itens={[ipva]} /></Providers>);

    expect(screen.getByRole('link', { name: 'IPVA, Veículo: Vencido, venceu há 5 dias' })).toBeInTheDocument();
    expect(screen.getByText('Vencido (centro)')).toBeInTheDocument(); // legenda com forma e texto
  });

  it('cada status tem um texto próprio', () => {
    render(<><StatusBadge status="Atencao" /><StatusBadge status="EmDia" /><StatusBadge status="Encerrado" /></>);
    expect(screen.getByText('Atenção')).toHaveClass('status-atencao');
    expect(screen.getByText('Em dia')).toHaveClass('status-em-dia');
    expect(screen.getByText('Concluído')).toHaveClass('status-encerrado');
  });
});
