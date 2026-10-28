import { useQuery } from '@tanstack/react-query';
import { Link, useSearchParams } from 'react-router-dom';
import { mensagemDeErro } from '../../api/client.ts';
import { catalogoApi, itensApi } from '../../api/endpoints.ts';
import type { StatusItem } from '../../api/types.ts';
import { EmptyState, ErrorState, PageHeader, Skeleton, StatusBadge } from '../../components/ui.tsx';
import { categorias as infoCategorias, dataBr } from '../../utils/formato.ts';

const filtrosStatus: { valor: StatusItem | ''; rotulo: string }[] = [
  { valor: '', rotulo: 'Todos' },
  { valor: 'Vencido', rotulo: 'Vencidos' },
  { valor: 'Atencao', rotulo: 'Atenção' },
  { valor: 'EmDia', rotulo: 'Em dia' },
  { valor: 'Encerrado', rotulo: 'Concluídos' },
];

/** T08 · Itens por categoria, com filtros (E08, E13). */
export function ItensPage() {
  const [params, setParams] = useSearchParams();
  const categoriaId = params.get('categoriaId') ? Number(params.get('categoriaId')) : undefined;
  const status = (params.get('status') || undefined) as StatusItem | undefined;

  const categorias = useQuery({ queryKey: ['categorias'], queryFn: catalogoApi.categorias, staleTime: Infinity });
  const itens = useQuery({ queryKey: ['itens', { categoriaId, status }], queryFn: () => itensApi.listar({ categoriaId, status }) });

  const filtrar = (chave: string, valor?: string) => {
    const novo = new URLSearchParams(params);
    if (valor) novo.set(chave, valor);
    else novo.delete(chave);
    setParams(novo);
  };

  return (
    <div className="pagina">
      <PageHeader titulo="Itens">
        <Link to="/itens/novo" className="btn btn-primario btn-pequeno">+ Novo</Link>
      </PageHeader>

      <div className="filtros" role="group" aria-label="Filtrar por categoria">
        <button type="button" className={`chip ${!categoriaId ? 'ativo' : ''}`} aria-pressed={!categoriaId}
                onClick={() => filtrar('categoriaId')}>Todas</button>
        {categorias.data?.map((c) => (
          <button type="button" key={c.id} className={`chip ${categoriaId === c.id ? 'ativo' : ''}`}
                  aria-pressed={categoriaId === c.id} onClick={() => filtrar('categoriaId', String(c.id))}>
            {c.nome}
          </button>
        ))}
      </div>
      <div className="filtros" role="group" aria-label="Filtrar por situação">
        {filtrosStatus.map((f) => (
          <button type="button" key={f.rotulo} className={`chip ${(status ?? '') === f.valor ? 'ativo' : ''}`}
                  aria-pressed={(status ?? '') === f.valor} onClick={() => filtrar('status', f.valor)}>
            {f.rotulo}
          </button>
        ))}
      </div>

      {itens.isPending && <Skeleton linhas={4} />}
      {itens.isError && <ErrorState mensagem={mensagemDeErro(itens.error)} tentarDeNovo={() => itens.refetch()} />}
      {itens.data?.length === 0 && (
        <EmptyState titulo="Nenhum item com esses filtros."
                    acao={<Link to="/itens/novo" className="btn btn-secundario">Adicionar um item</Link>} />
      )}

      <ul className="cartoes">
        {itens.data?.map((i) => (
          <li key={i.id}>
            <Link to={`/itens/${i.id}`} className={`cartao-item item-${i.status}`}>
              <div className="cartao-topo">
                <span className="cartao-categoria">{infoCategorias[i.categoriaId]?.nome}</span>
                <StatusBadge status={i.status} />
              </div>
              <p className="cartao-nome">{i.nome}</p>
              <p className="cartao-prazo">{i.textoPrazo}{i.dataLimite ? ` · ${dataBr(i.dataLimite)}` : ''}</p>
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
}
