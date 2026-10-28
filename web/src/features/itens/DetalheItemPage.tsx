import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { mensagemDeErro } from '../../api/client.ts';
import { itensApi } from '../../api/endpoints.ts';
import type { ItemDetalhe } from '../../api/types.ts';
import { Alerta, Button, Dialog, ErrorState, PageHeader, Progresso, Skeleton, StatusBadge } from '../../components/ui.tsx';
import { categorias, dataBr, dinheiro, km } from '../../utils/formato.ts';
import { ConcluirDialog } from './ConcluirDialog.tsx';
import { chaves, useInvalidar } from './useInvalidar.ts';

/** T10 · Detalhe do item: status, prazo, passos, histórico (E14, E17, E19). */
export function DetalheItemPage() {
  const { id = '' } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const invalidar = useInvalidar();
  const [concluindo, setConcluindo] = useState(false);
  const [excluindo, setExcluindo] = useState(false);
  const [erroAcao, setErroAcao] = useState<string | null>(null);

  const consulta = useQuery({ queryKey: chaves.item(id), queryFn: () => itensApi.obter(id) });

  // Marcar passo com atualização otimista: a caixa marca na hora e volta atrás se a API falhar (spec §5.6)
  const marcar = useMutation({
    mutationFn: ({ passoId, concluido }: { passoId: string; concluido: boolean }) => itensApi.marcarPasso(id, passoId, concluido),
    onMutate: async ({ passoId, concluido }) => {
      setErroAcao(null);
      await queryClient.cancelQueries({ queryKey: chaves.item(id) });
      const anterior = queryClient.getQueryData<ItemDetalhe>(chaves.item(id));
      if (anterior) {
        const passos = anterior.passos.map((p) => (p.id === passoId ? { ...p, concluido } : p));
        queryClient.setQueryData<ItemDetalhe>(chaves.item(id), {
          ...anterior, passos, progressoPassos: passos.filter((p) => p.concluido).length / passos.length,
        });
      }
      return { anterior };
    },
    onError: (e, _v, ctx) => {
      if (ctx?.anterior) queryClient.setQueryData(chaves.item(id), ctx.anterior);
      setErroAcao(mensagemDeErro(e));
    },
    onSettled: () => queryClient.invalidateQueries({ queryKey: chaves.item(id) }),
  });

  const excluir = useMutation({
    mutationFn: () => itensApi.excluir(id),
    onSuccess: async () => {
      await invalidar(chaves.radar, chaves.itens, chaves.historico, chaves.veiculos);
      navigate('/itens', { replace: true });
    },
    onError: (e) => setErroAcao(mensagemDeErro(e)),
  });

  if (consulta.isPending) return <div className="pagina"><Skeleton linhas={6} /></div>;
  if (consulta.isError)
    return <div className="pagina"><PageHeader titulo="Item" voltar="/" /><ErrorState mensagem={mensagemDeErro(consulta.error)} tentarDeNovo={() => consulta.refetch()} /></div>;

  const item = consulta.data;
  const feitos = item.passos.filter((p) => p.concluido).length;
  const encerrado = item.situacao === 'Encerrado';

  return (
    <div className="pagina">
      <PageHeader titulo={item.nome} voltar="/" />

      <section className={`detalhe-status item-${item.status}`}>
        <StatusBadge status={item.status} />
        <span className="detalhe-prazo">{item.textoPrazo}</span>
      </section>

      <dl className="detalhe-dados">
        <div><dt>Área</dt><dd>{categorias[item.categoriaId]?.nome}</dd></div>
        {!encerrado && (
          <div>
            <dt>Próximo vencimento</dt>
            <dd>
              {item.proximoVencimentoKm != null ? `${km(item.proximoVencimentoKm)} ou ` : ''}
              {dataBr(item.proximoVencimentoData)}
              {item.proximoVencimentoKm != null && ', o que vier primeiro'}
            </dd>
          </div>
        )}
        {item.veiculo && <div><dt>Veículo</dt><dd>{item.veiculo.apelido}</dd></div>}
        <div><dt>Aviso</dt><dd>{item.antecedenciaDias} dias antes</dd></div>
        {item.observacao && <div><dt>Observação</dt><dd>{item.observacao}</dd></div>}
      </dl>

      {item.passos.length > 0 && (
        <section className="passos" aria-labelledby="titulo-passos">
          <h2 id="titulo-passos">Passos <span className="contagem">{feitos} de {item.passos.length}</span></h2>
          <Progresso valor={item.progressoPassos} rotulo={`${feitos} de ${item.passos.length} passos feitos`} />
          <ul>
            {item.passos.map((p) => (
              <li key={p.id}>
                <label className="check passo">
                  <input type="checkbox" checked={p.concluido} disabled={encerrado}
                         onChange={(e) => marcar.mutate({ passoId: p.id, concluido: e.target.checked })} />
                  <span>{p.descricao}</span>
                </label>
              </li>
            ))}
          </ul>
        </section>
      )}

      {erroAcao && <Alerta>{erroAcao}</Alerta>}
      {!encerrado && <Button onClick={() => setConcluindo(true)} icone="check">Concluir</Button>}

      <section className="historico-curto" aria-labelledby="titulo-historico">
        <h2 id="titulo-historico">Histórico</h2>
        {item.historico.length === 0 ? <p className="texto-apoio">Ainda não foi concluído.</p> : (
          <ul>
            {item.historico.map((h) => (
              <li key={h.id}>{dataBr(h.concluidoEm)}{h.km != null && ` · ${km(h.km)}`}{h.valorGasto != null && ` · ${dinheiro(h.valorGasto)}`}</li>
            ))}
          </ul>
        )}
      </section>

      <div className="acoes-secundarias">
        {!encerrado && <Link to={`/itens/${id}/editar`}>Editar</Link>}
        <button type="button" className="link-perigo" onClick={() => setExcluindo(true)}>Excluir</button>
      </div>

      <ConcluirDialog item={item} aberto={concluindo} aoFechar={() => setConcluindo(false)} />

      <Dialog aberto={excluindo} titulo="Excluir este item?" aoFechar={() => setExcluindo(false)}>
        <p>Os passos e o histórico deste item também serão apagados.</p>
        <Button variante="perigo" carregando={excluir.isPending} onClick={() => excluir.mutate()}>Excluir</Button>
        <Button variante="secundario" onClick={() => setExcluindo(false)}>Cancelar</Button>
      </Dialog>
    </div>
  );
}
