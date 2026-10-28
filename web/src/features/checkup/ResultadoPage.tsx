import { Link, Navigate } from 'react-router-dom';
import { Icon } from '../../components/Icon.tsx';
import { StatusBadge } from '../../components/ui.tsx';
import { categorias } from '../../utils/formato.ts';
import { useCheckup } from './CheckupContext.tsx';

/**
 * T05 · Resultado do check-up: situação por área e destaques (RN11).
 * Área sem nenhum item aparece como "Sem itens" — pular perguntas não deixa a área "em dia".
 */
export function ResultadoPage() {
  const { resultado } = useCheckup();
  if (!resultado) return <Navigate to="/" replace />;

  const total = resultado.areas.length;
  const vazio = resultado.areasSemItens === total;

  return (
    <main className="checkup" id="conteudo">
      <header className="checkup-topo">
        <p className="checkup-marca">Resultado do check-up</p>
        {vazio ? (
          <>
            <h1 className="resultado-titulo">Seu Radar ainda está vazio</h1>
            <p className="texto-apoio">
              Você não marcou nada no check-up, então ainda não sabemos se algum prazo precisa de atenção.
              Que tal refazer ou adicionar o primeiro item?
            </p>
          </>
        ) : (
          <>
            <h1 className="resultado-titulo">{resultado.areasEmDia} de {total} áreas em dia</h1>
            <p className="texto-apoio">
              {resultado.itensCriados === 1 ? '1 prazo foi adicionado' : `${resultado.itensCriados} prazos foram adicionados`} ao seu Radar.
              {resultado.areasSemItens > 0 &&
                ` ${resultado.areasSemItens === 1 ? '1 área ainda está' : `${resultado.areasSemItens} áreas ainda estão`} sem nada cadastrado.`}
            </p>
          </>
        )}
      </header>

      <ul className="areas">
        {resultado.areas.map((a) => {
          const destaque = resultado.destaques.find((d) => d.categoriaId === a.categoriaId);
          return (
            <li key={a.categoriaId} className={`area ${a.status === null ? 'area-vazia' : ''}`}>
              <div className="area-topo">
                <span className="area-nome">
                  <Icon name={categorias[a.categoriaId]?.icone ?? 'folder'} size={18} /> {a.categoria}
                </span>
                {a.status ? <StatusBadge status={a.status} /> : (
                  <span className="status status-sem-itens"><Icon name="minus" size={14} strokeWidth={2.4} />Sem itens</span>
                )}
              </div>
              <p className="area-detalhe">
                {a.quantidadeItens === 0 ? 'Nada cadastrado por aqui ainda.'
                  : destaque ? `${destaque.nome}: ${destaque.textoPrazo}`
                  : `${a.quantidadeItens} ${a.quantidadeItens === 1 ? 'item' : 'itens'} acompanhados`}
              </p>
            </li>
          );
        })}
      </ul>

      {resultado.destaques.length > 0 && (
        <section className="destaques" aria-labelledby="titulo-destaques">
          <h2 id="titulo-destaques">Comece por aqui</h2>
          <ul>
            {resultado.destaques.map((d) => (
              <li key={d.itemId}>
                <StatusBadge status={d.status} /> <strong>{d.nome}</strong> — {d.textoPrazo}
              </li>
            ))}
          </ul>
        </section>
      )}

      {vazio ? (
        <>
          <Link to="/itens/novo" className="btn btn-primario">Adicionar o primeiro item</Link>
          <Link to="/" className="btn btn-texto">Ir para o Radar</Link>
        </>
      ) : (
        <Link to="/" className="btn btn-primario">Ver meu radar</Link>
      )}
    </main>
  );
}
