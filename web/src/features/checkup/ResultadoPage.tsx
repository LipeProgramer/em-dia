import { Link, Navigate } from 'react-router-dom';
import { Icon } from '../../components/Icon.tsx';
import { StatusBadge } from '../../components/ui.tsx';
import { categorias } from '../../utils/formato.ts';
import { useCheckup } from './CheckupContext.tsx';

/** T05 · Resultado do check-up: situação por área e destaques (RN11). */
export function ResultadoPage() {
  const { resultado } = useCheckup();
  if (!resultado) return <Navigate to="/" replace />;

  const total = resultado.areas.length;

  return (
    <main className="checkup" id="conteudo">
      <header className="checkup-topo">
        <p className="checkup-marca">Resultado do check-up</p>
        <h1 className="resultado-titulo">{resultado.areasEmDia} de {total} áreas em dia</h1>
        <p className="texto-apoio">
          {resultado.itensCriados === 1 ? '1 prazo foi adicionado' : `${resultado.itensCriados} prazos foram adicionados`} ao seu Radar.
        </p>
      </header>

      <ul className="areas">
        {resultado.areas.map((a) => {
          const destaque = resultado.destaques.find((d) => d.categoriaId === a.categoriaId);
          return (
            <li key={a.categoriaId} className="area">
              <div className="area-topo">
                <span className="area-nome">
                  <Icon name={categorias[a.categoriaId]?.icone ?? 'folder'} size={18} /> {a.categoria}
                </span>
                <StatusBadge status={a.status} />
              </div>
              <p className="area-detalhe">
                {a.quantidadeItens === 0 ? 'Nada cadastrado por aqui.'
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

      <Link to="/" className="btn btn-primario">Ver meu radar</Link>
    </main>
  );
}
