import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { mensagemDeErro } from '../../api/client.ts';
import { historicoApi } from '../../api/endpoints.ts';
import { EmptyState, ErrorState, Field, PageHeader, Skeleton } from '../../components/ui.tsx';
import { dataBr, dinheiro, km } from '../../utils/formato.ts';

/** T13 · Histórico: conclusões da mais recente para a mais antiga (E25). */
export function HistoricoPage() {
  const [de, setDe] = useState('');
  const [ate, setAte] = useState('');
  const periodoInvalido = !!de && !!ate && de > ate;

  const consulta = useQuery({
    queryKey: ['historico', { de, ate }],
    queryFn: () => historicoApi.listar({ de: de || undefined, ate: ate || undefined }),
    enabled: !periodoInvalido,
  });

  return (
    <div className="pagina">
      <PageHeader titulo="Histórico" />
      <div className="filtro-periodo">
        <Field label="De" type="date" value={de} onChange={(e) => setDe(e.target.value)} />
        <Field label="Até" type="date" value={ate} onChange={(e) => setAte(e.target.value)}
               erro={periodoInvalido ? 'A data inicial deve ser anterior ou igual à final.' : undefined} />
      </div>

      {consulta.isPending && !periodoInvalido && <Skeleton linhas={4} />}
      {consulta.isError && <ErrorState mensagem={mensagemDeErro(consulta.error)} tentarDeNovo={() => consulta.refetch()} />}
      {consulta.data?.length === 0 && (
        <EmptyState titulo="Nada concluído neste período." texto="Cada conclusão aparece aqui. Pequenos passos contam!" />
      )}

      <ol className="linha-do-tempo">
        {consulta.data?.map((h) => (
          <li key={h.id}>
            <p className="tempo-data">{dataBr(h.concluidoEm)}</p>
            <Link to={`/itens/${h.itemId}`} className="tempo-nome">{h.itemNome}</Link>
            <p className="tempo-detalhe">
              {[h.km != null && km(h.km), h.valorGasto != null && dinheiro(h.valorGasto),
                h.proximoVencimento && `próximo: ${dataBr(h.proximoVencimento)}`].filter(Boolean).join(' · ')}
            </p>
            {h.observacao && <p className="tempo-obs">{h.observacao}</p>}
          </li>
        ))}
      </ol>
    </div>
  );
}
