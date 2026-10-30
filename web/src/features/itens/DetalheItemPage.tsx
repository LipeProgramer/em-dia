import { useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { mensagemDeErro } from '../../api/client.ts';
import { itensApi } from '../../api/endpoints.ts';
import { Alerta, Button, Dialog, ErrorState, PageHeader, Skeleton, StatusBadge } from '../../components/ui.tsx';
import { categorias, dataBr, dinheiro, km } from '../../utils/formato.ts';
import { ConcluirDialog } from './ConcluirDialog.tsx';
import { chaves, useInvalidar } from './useInvalidar.ts';

/** T10 · Detalhe do item: status, prazo e histórico (E14, E17). */
export function DetalheItemPage() {
  const { id = '' } = useParams();
  const navigate = useNavigate();
  const invalidar = useInvalidar();
  const [concluindo, setConcluindo] = useState(false);
  const [excluindo, setExcluindo] = useState(false);
  const [erroAcao, setErroAcao] = useState<string | null>(null);

  const consulta = useQuery({ queryKey: chaves.item(id), queryFn: () => itensApi.obter(id) });

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
        <p>O histórico deste item também será apagado.</p>
        <Button variante="perigo" carregando={excluir.isPending} onClick={() => excluir.mutate()}>Excluir</Button>
        <Button variante="secundario" onClick={() => setExcluindo(false)}>Cancelar</Button>
      </Dialog>

      <IdFlutuante id={item.id} />
    </div>
  );
}

/** Id do item sempre à vista, com botão de copiar, para usar em GET /api/v1/itens/{id} no Swagger (DV22). */
function IdFlutuante({ id }: { id: string }) {
  const [copiado, setCopiado] = useState(false);

  const copiar = async () => {
    try {
      await navigator.clipboard.writeText(id);
      setCopiado(true);
      setTimeout(() => setCopiado(false), 2000);
    } catch {
      window.prompt('Copie o id do item:', id);
    }
  };

  return (
    <aside className="id-flutuante" aria-label="Id do item para usar no Swagger">
      <span className="id-flutuante-rotulo">ID do item</span>
      <code className="id-flutuante-valor">{id}</code>
      <button type="button" className="id-flutuante-copiar" onClick={copiar}>{copiado ? 'Copiado!' : 'Copiar'}</button>
    </aside>
  );
}
