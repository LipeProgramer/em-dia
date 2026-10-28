import { useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon } from '../../components/Icon.tsx';
import { Button, ErrorState, Progresso, Skeleton } from '../../components/ui.tsx';
import { categorias } from '../../utils/formato.ts';
import { useCheckup } from './CheckupContext.tsx';

/**
 * T03 · Check-up por área: Veículo → Documentos → Casa → Saúde → Finanças → Garantias, uma pergunta sim/não
 * por tela (RNF02/RNF03). Dá para pular uma pergunta ou a área inteira. Tudo funciona só com Tab e Enter;
 * o foco vai para a nova pergunta.
 */
export function PerguntaPage() {
  const { perguntas, carregando, erro, recarregar, indice, setIndice, responder, respostas } = useCheckup();
  const navigate = useNavigate();
  const tituloRef = useRef<HTMLHeadingElement>(null);

  // O foco vai para a pergunta quando ela aparece (inclusive a primeira, depois de carregar)
  useEffect(() => {
    tituloRef.current?.focus();
  }, [indice, perguntas]);

  if (carregando) return <main className="checkup"><Skeleton /></main>;
  if (erro || !perguntas) return <main className="checkup"><ErrorState tentarDeNovo={recarregar} /></main>;

  const total = perguntas.length;
  const pergunta = perguntas[Math.min(indice, total - 1)];
  const areas = [...new Set(perguntas.map((p) => p.categoriaId))];
  const areaAtual = pergunta.categoriaId;
  const posicaoArea = areas.indexOf(areaAtual);
  const daArea = perguntas.filter((p) => p.categoriaId === areaAtual);
  const nomeArea = categorias[areaAtual]?.nome ?? 'Área';

  const irPara = (proximo: number) => {
    if (proximo >= total) navigate('/checkup/dados');
    else setIndice(proximo);
  };

  const avancar = (resposta: boolean | null) => {
    responder(pergunta.codigo, resposta);
    irPara(indice + 1);
  };

  /** Pula o resto da área: as perguntas que faltam ficam sem resposta. */
  const pularArea = () => {
    for (const p of daArea.filter((p) => perguntas.indexOf(p) >= indice)) responder(p.codigo, null);
    const proxima = perguntas.findIndex((p, i) => i > indice && p.categoriaId !== areaAtual);
    irPara(proxima === -1 ? total : proxima);
  };

  const respostaAtual = respostas[pergunta.codigo];

  return (
    <main className="checkup" id="conteudo">
      <header className="checkup-topo">
        <p className="checkup-marca">Check-up</p>
        <ol className="trilha-areas" aria-label="Áreas do check-up">
          {areas.map((id, i) => (
            <li key={id} className={i < posicaoArea ? 'feita' : i === posicaoArea ? 'atual' : ''}
                aria-current={i === posicaoArea ? 'step' : undefined}>
              <Icon name={i < posicaoArea ? 'check' : categorias[id]?.icone ?? 'folder'} size={16} />
              <span>{categorias[id]?.nome}</span>
            </li>
          ))}
        </ol>
        <p className="checkup-contador">
          Área {posicaoArea + 1} de {areas.length} · <strong>{nomeArea}</strong> · Pergunta {indice + 1} de {total}
        </p>
        <Progresso valor={(indice + 1) / total} rotulo={`Pergunta ${indice + 1} de ${total}`} />
      </header>

      <section className="checkup-pergunta" aria-live="polite">
        <p className="checkup-area"><Icon name={categorias[areaAtual]?.icone ?? 'folder'} size={18} /> {nomeArea}</p>
        <h1 ref={tituloRef} tabIndex={-1}>{pergunta.texto}</h1>
        {pergunta.ajuda && <p className="checkup-ajuda">{pergunta.ajuda}</p>}
      </section>

      <div className="checkup-acoes">
        <Button onClick={() => avancar(true)} aria-pressed={respostaAtual === true}>Sim</Button>
        <Button variante="secundario" onClick={() => avancar(false)} aria-pressed={respostaAtual === false}>Não</Button>
        <Button variante="texto" onClick={() => avancar(null)}>Pular esta pergunta</Button>
        <Button variante="texto" onClick={pularArea}>Pular a área {nomeArea}</Button>
        {indice > 0 && (
          <Button variante="texto" icone="arrow-left" onClick={() => setIndice(indice - 1)}>Pergunta anterior</Button>
        )}
      </div>
      <p className="checkup-rodape">Você pode ajustar tudo depois.</p>
    </main>
  );
}
