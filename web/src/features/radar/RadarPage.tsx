import { useQuery } from '@tanstack/react-query';
import { Link, useSearchParams } from 'react-router-dom';
import { mensagemDeErro } from '../../api/client.ts';
import { radarApi } from '../../api/endpoints.ts';
import type { ItemResumo } from '../../api/types.ts';
import { Icon } from '../../components/Icon.tsx';
import { EmptyState, ErrorState, Skeleton, StatusBadge } from '../../components/ui.tsx';
import { useSession } from '../auth/useSession.tsx';
import { RadarChart } from './RadarChart.tsx';

/** Botão do modo foco (T07): só os 3 itens mais urgentes. */
export function ModoFocoToggle({ ativo }: { ativo: boolean }) {
  const [, setParams] = useSearchParams();
  return (
    <button type="button" className={`foco-toggle ${ativo ? 'ativo' : ''}`} aria-pressed={ativo}
            onClick={() => setParams(ativo ? { foco: '0' } : { foco: '1' })}>
      <Icon name="focus" size={16} /> Modo foco
    </button>
  );
}

/** Lista "Precisa de atenção": vencidos e em atenção, do mais urgente ao menos urgente. */
export function ListaAtencao({ itens, titulo }: { itens: ItemResumo[]; titulo: string }) {
  if (itens.length === 0) return null;
  return (
    <section className="lista-itens" aria-labelledby={`lista-${titulo}`}>
      <h2 id={`lista-${titulo}`}>{titulo}</h2>
      <ul>
        {itens.map((i) => (
          <li key={i.id}>
            <Link to={`/itens/${i.id}`} className={`item-linha item-${i.status}`}>
              <StatusBadge status={i.status} />
              <span className="item-nome">{i.nome}</span>
              <span className="item-prazo">{i.textoPrazo}</span>
            </Link>
          </li>
        ))}
      </ul>
    </section>
  );
}

/** T06 · Radar (tela principal) e T07 · Modo foco (/?foco=1). */
export function RadarPage() {
  const { usuario } = useSession();
  const [params] = useSearchParams();
  const focoParam = params.get('foco');
  const foco = focoParam === null ? undefined : focoParam === '1';

  const consulta = useQuery({ queryKey: ['radar', foco], queryFn: () => radarApi.obter(foco) });

  return (
    <div className="pagina">
      <header className="page-header">
        <div>
          <h1>Seu radar</h1>
          {consulta.data && (
            <p className="resumo">
              {consulta.data.resumo.vencidos} {consulta.data.resumo.vencidos === 1 ? 'vencido' : 'vencidos'} ·{' '}
              {consulta.data.resumo.atencao} em atenção · {consulta.data.resumo.emDia} em dia
            </p>
          )}
        </div>
        {consulta.data && <ModoFocoToggle ativo={consulta.data.modoFoco} />}
      </header>

      {consulta.isPending && <Skeleton linhas={5} />}
      {consulta.isError && <ErrorState mensagem={mensagemDeErro(consulta.error)} tentarDeNovo={() => consulta.refetch()} />}

      {consulta.data && consulta.data.itens.length === 0 && (
        <EmptyState
          titulo="Nenhum prazo por aqui ainda."
          texto={`Que tal fazer o check-up${usuario ? `, ${usuario.nome}` : ''}?`}
          acao={<Link to="/checkup" className="btn btn-primario">Fazer o check-up</Link>}
        />
      )}

      {consulta.data && consulta.data.itens.length > 0 && (
        <>
          {consulta.data.modoFoco && (
            <p className="foco-aviso" role="status">Modo foco: só os 3 itens mais urgentes. Um de cada vez.</p>
          )}
          <RadarChart itens={consulta.data.itens} />
          <ListaAtencao titulo="Precisa de atenção" itens={consulta.data.itens.filter((i) => i.status !== 'EmDia')} />
          <ListaAtencao titulo="Em dia" itens={consulta.data.itens.filter((i) => i.status === 'EmDia')} />
        </>
      )}
    </div>
  );
}
