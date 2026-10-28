import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { itensApi, catalogoApi } from '../../api/services.ts';
import { StatusBadge } from '../../components/StatusBadge.tsx';
import { Icon } from '../../components/Icon.tsx';
import { ConfirmDialog } from '../../components/ConfirmDialog.tsx';

export function ItensView() {
  const queryClient = useQueryClient();
  const [categoriaFiltro, setCategoriaFiltro] = useState<number | undefined>(undefined);
  const [itemParaExcluir, setItemParaExcluir] = useState<{ id: string; nome: string } | null>(null);

  const { data: categorias } = useQuery({
    queryKey: ['categorias'],
    queryFn: catalogoApi.listarCategorias,
  });

  const {
    data: itens,
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ['itens', categoriaFiltro],
    queryFn: () => itensApi.listar(categoriaFiltro),
  });

  const excluirMutation = useMutation({
    mutationFn: (id: string) => itensApi.excluir(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['itens'] });
      queryClient.invalidateQueries({ queryKey: ['radar'] });
      setItemParaExcluir(null);
    },
    onError: () => {
      queryClient.invalidateQueries({ queryKey: ['itens'] });
      queryClient.invalidateQueries({ queryKey: ['radar'] });
    },
  });

  const handleExcluir = (id: string, nome: string) => { excluirMutation.reset(); setItemParaExcluir({ id, nome }); };
  const confirmarExclusao = () => {
    if (itemParaExcluir) excluirMutation.mutate(itemParaExcluir.id);
  };

  return (
    <div className="itens-page">
      <header className="page-header">
        <div>
          <h1>Meus Itens</h1>
          <p>Gerencie todos os seus prazos cadastrados e tarefas vinculadas.</p>
        </div>
        <Link to="/itens/novo" className="btn btn-primary">
          <Icon name="plus" size={17} /> Novo item
        </Link>
      </header>

      {/* Barra de Filtro de Categorias */}
      <div className="filter-bar" role="toolbar" aria-label="Filtro por categoria">
        <button
          type="button"
          className={`filter-chip ${categoriaFiltro === undefined ? 'filter-chip-active' : ''}`}
          onClick={() => setCategoriaFiltro(undefined)}
        >
          Todas
        </button>
        {categorias?.map((cat) => (
          <button
            key={cat.id}
            type="button"
            className={`filter-chip ${
              categoriaFiltro === cat.id ? 'filter-chip-active' : ''
            }`}
            onClick={() => setCategoriaFiltro(cat.id)}
          >
            {cat.nome}
          </button>
        ))}
      </div>

      {isLoading && (
        <div className="loading-state" role="status">
          <span className="spinner" aria-hidden="true"></span>
          <span>Carregando itens...</span>
        </div>
      )}

      {isError && (
        <div className="alert alert-error" role="alert">
          <p>Erro ao carregar os itens.</p>
          <button
            type="button"
            className="btn btn-outline btn-sm"
            onClick={() => refetch()}
          >
            Tentar novamente
          </button>
        </div>
      )}

      {!isLoading && itens && itens.length === 0 && (
        <div className="empty-state-card">
          <div className="empty-icon" aria-hidden="true">
            <Icon name="folder" size={28} />
          </div>
          <h2>Nenhum item encontrado</h2>
          <p>
            {categoriaFiltro !== undefined
              ? 'Nenhum item cadastrado nesta categoria.'
              : 'Você ainda não cadastrou nenhum item.'}
          </p>
          <Link to="/itens/novo" className="btn btn-primary">
            Cadastrar primeiro item
          </Link>
        </div>
      )}

      {!isLoading && itens && itens.length > 0 && (
        <div className="itens-table-container">
          <table className="itens-table" aria-label="Lista de itens">
            <thead>
              <tr>
                <th scope="col">Nome</th>
                <th scope="col">Status</th>
                <th scope="col">Próximo Vencimento</th>
                <th scope="col">Passos</th>
                <th scope="col" className="text-right">
                  Ações
                </th>
              </tr>
            </thead>
            <tbody>
              {itens.map((item) => {
                const totalPassos = item.passos.length;
                const concluidos = item.passos.filter((p) => p.concluido).length;

                return (
                  <tr key={item.id}>
                    <td>
                      <strong>{item.nome}</strong>
                      {item.observacao && (
                        <p className="item-sub-desc">{item.observacao}</p>
                      )}
                    </td>
                    <td>
                      <StatusBadge status={item.status} />
                    </td>
                    <td>
                      {item.proximoVencimentoData ? (
                        <span>
                          {item.proximoVencimentoData}{' '}
                          <small>({item.diasRestantes} dias)</small>
                        </span>
                      ) : (
                        <span>Sem data fixa</span>
                      )}
                      {item.proximoVencimentoKm !== null && (
                        <div className="sub-km">
                          {item.proximoVencimentoKm.toLocaleString()} km
                        </div>
                      )}
                    </td>
                    <td>
                      {totalPassos > 0 ? (
                        <span>
                          {concluidos}/{totalPassos} feitos
                        </span>
                      ) : (
                        <span className="text-muted">Nenhum</span>
                      )}
                    </td>
                    <td className="text-right table-actions">
                      <Link
                        to={`/itens/${item.id}/editar`}
                        className="btn btn-outline btn-sm"
                        aria-label={`Editar ${item.nome}`}
                      >
                        Editar
                      </Link>
                      <button
                        type="button"
                        className="btn btn-danger-outline btn-sm"
                        onClick={() => handleExcluir(item.id, item.nome)}
                        aria-label={`Excluir ${item.nome}`}
                      >
                        Excluir
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      <ConfirmDialog
        isOpen={!!itemParaExcluir}
        title={`Excluir ${itemParaExcluir?.nome ? `“${itemParaExcluir.nome}”` : 'item'}?`}
        description="O item será removido dos seus itens cadastrados e deixará de aparecer no Radar. Esta ação não pode ser desfeita por aqui."
        confirmLabel="Excluir item"
        isPending={excluirMutation.isPending}
        error={excluirMutation.isError ? 'Não foi possível excluir o item. Confira sua conexão e tente novamente.' : null}
        onClose={() => { if (!excluirMutation.isPending) { setItemParaExcluir(null); excluirMutation.reset(); } }}
        onConfirm={confirmarExclusao}
      />
    </div>
  );
}
