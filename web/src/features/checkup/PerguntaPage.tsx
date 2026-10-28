import { useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button, ErrorState, Progresso, Skeleton } from '../../components/ui.tsx';
import { useCheckup } from './CheckupContext.tsx';

/**
 * T03 · Check-up: uma pergunta sim/não por tela (RNF02/RNF03). "Continuar" só acontece depois de
 * responder ou pular. Tudo funciona só com Tab e Enter; o foco vai para a nova pergunta.
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

  const avancar = (resposta: boolean | null) => {
    responder(pergunta.codigo, resposta);
    if (indice + 1 >= total) navigate('/checkup/dados');
    else setIndice(indice + 1);
  };

  const respostaAtual = respostas[pergunta.codigo];

  return (
    <main className="checkup" id="conteudo">
      <header className="checkup-topo">
        <p className="checkup-marca">Check-up</p>
        <p className="checkup-contador">Pergunta {indice + 1} de {total}</p>
        <Progresso valor={(indice + 1) / total} rotulo={`Pergunta ${indice + 1} de ${total}`} />
      </header>

      <section className="checkup-pergunta" aria-live="polite">
        <h1 ref={tituloRef} tabIndex={-1}>{pergunta.texto}</h1>
        {pergunta.ajuda && <p className="checkup-ajuda">{pergunta.ajuda}</p>}
      </section>

      <div className="checkup-acoes">
        <Button onClick={() => avancar(true)} aria-pressed={respostaAtual === true}>Sim</Button>
        <Button variante="secundario" onClick={() => avancar(false)} aria-pressed={respostaAtual === false}>Não</Button>
        <Button variante="texto" onClick={() => avancar(null)}>Pular esta pergunta</Button>
        {indice > 0 && (
          <Button variante="texto" icone="arrow-left" onClick={() => setIndice(indice - 1)}>Pergunta anterior</Button>
        )}
      </div>
      <p className="checkup-rodape">Você pode ajustar tudo depois.</p>
    </main>
  );
}
