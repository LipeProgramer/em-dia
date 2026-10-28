import { useQueryClient } from '@tanstack/react-query';

/**
 * Consultas recarregadas depois de cada ação (spec §5.6):
 * check-up, criar, editar, excluir ou concluir item e atualizar km recarregam Radar e itens.
 */
export function useInvalidar() {
  const queryClient = useQueryClient();
  return (...chaves: string[][]) =>
    Promise.all(chaves.map((queryKey) => queryClient.invalidateQueries({ queryKey })));
}

export const chaves = {
  radar: ['radar'],
  itens: ['itens'],
  item: (id: string) => ['item', id],
  veiculos: ['veiculos'],
  historico: ['historico'],
  me: ['me'],
};
