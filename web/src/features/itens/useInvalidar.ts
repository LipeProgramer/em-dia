import { useQueryClient } from '@tanstack/react-query';
import type { ItemDetalhe } from '../../api/types.ts';

/**
 * Consultas recarregadas depois de cada ação (spec §5.6):
 * check-up, criar, editar, excluir ou concluir item e atualizar km recarregam Radar e itens.
 * A recarga roda em segundo plano: a tela segue na hora, sem esperar uma chamada por lista.
 */
export function useInvalidar() {
  const queryClient = useQueryClient();
  return (...chaves: string[][]) => {
    for (const queryKey of chaves) void queryClient.invalidateQueries({ queryKey });
    return Promise.resolve();
  };
}

/** Guarda o item que a API acabou de devolver: o detalhe abre na hora, sem buscar de novo. */
export function useGuardarItem() {
  const queryClient = useQueryClient();
  return (item: ItemDetalhe) => queryClient.setQueryData(chaves.item(item.id), item);
}

export const chaves = {
  radar: ['radar'],
  itens: ['itens'],
  item: (id: string) => ['item', id],
  veiculos: ['veiculos'],
  historico: ['historico'],
  me: ['me'],
};
