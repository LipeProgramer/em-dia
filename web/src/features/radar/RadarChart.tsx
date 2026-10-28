import { useNavigate } from 'react-router-dom';
import type { AnelRadar, ItemResumo, StatusItem } from '../../api/types.ts';
import { categorias } from '../../utils/formato.ts';
import { textoStatus } from '../../components/ui.tsx';

// Faixas de raio de cada anel (RN03): centro = vencido; anel 1 = até 30 dias; anel 2 = 31 a 90; externo = mais de 90.
const faixas: Record<AnelRadar, [number, number]> = {
  Centro: [0, 22],
  Anel1: [26, 54],
  Anel2: [58, 84],
  Externo: [88, 110],
};

const TAMANHO = 240;
const C = TAMANHO / 2;

/** Forma diferente por status, para não depender só da cor (RNF02). */
function Marcador({ status, x, y }: { status: StatusItem; x: number; y: number }) {
  if (status === 'Vencido')
    return <rect x={x - 6} y={y - 6} width={12} height={12} transform={`rotate(45 ${x} ${y})`} className="ponto ponto-vencido" />;
  if (status === 'Atencao')
    return <polygon points={`${x},${y - 7} ${x + 7},${y + 6} ${x - 7},${y + 6}`} className="ponto ponto-atencao" />;
  return <circle cx={x} cy={y} r={6} className="ponto ponto-em-dia" />;
}

/**
 * Radar em SVG acessível (T06): 6 fatias (uma por área) e 4 anéis. Cada ponto é um link com nome,
 * status e prazo para leitores de tela; tocar no ponto abre o detalhe do item (T10).
 */
export function RadarChart({ itens }: { itens: ItemResumo[] }) {
  const navigate = useNavigate();

  const porFatia = new Map<number, ItemResumo[]>();
  for (const i of itens) porFatia.set(i.categoriaId, [...(porFatia.get(i.categoriaId) ?? []), i]);

  const pontos = itens.map((item) => {
    const daFatia = porFatia.get(item.categoriaId)!;
    const posicao = daFatia.indexOf(item);
    const inicio = ((item.categoriaId - 1) / 6) * 2 * Math.PI - Math.PI / 2;
    const angulo = inicio + ((posicao + 1) / (daFatia.length + 1)) * (2 * Math.PI / 6);
    const [min, max] = faixas[item.anel];
    const dias = item.diasRestantes ?? 0;
    const fracao = item.anel === 'Centro' ? 0.6
      : item.anel === 'Anel1' ? Math.min(Math.max(dias, 0), 30) / 30
      : item.anel === 'Anel2' ? (Math.min(dias, 90) - 31) / 59
      : Math.min((dias - 90) / 275, 1);
    const r = min + (max - min) * Math.max(0, Math.min(fracao, 1));
    return { item, x: C + r * Math.cos(angulo), y: C + r * Math.sin(angulo) };
  });

  return (
    <figure className="radar-figura">
      <svg viewBox={`0 0 ${TAMANHO} ${TAMANHO}`} className="radar-svg" role="group"
           aria-label={`Radar com ${itens.length} ${itens.length === 1 ? 'item' : 'itens'}`}>
        <title>Radar de vencimentos</title>
        {[110, 86, 56, 24].map((r, i) => (
          <circle key={r} cx={C} cy={C} r={r} className={`anel anel-${i}`} />
        ))}
        {Array.from({ length: 6 }, (_, i) => {
          const a = (i / 6) * 2 * Math.PI - Math.PI / 2;
          return <line key={i} x1={C} y1={C} x2={C + 110 * Math.cos(a)} y2={C + 110 * Math.sin(a)} className="fatia" />;
        })}
        {pontos.map(({ item, x, y }) => (
          <a
            key={item.id}
            href={`/itens/${item.id}`}
            className="ponto-link"
            aria-label={`${item.nome}, ${categorias[item.categoriaId]?.nome}: ${textoStatus(item.status)}, ${item.textoPrazo}`}
            onClick={(e) => {
              e.preventDefault();
              navigate(`/itens/${item.id}`);
            }}
          >
            <circle cx={x} cy={y} r={14} className="ponto-area" />
            <Marcador status={item.status} x={x} y={y} />
          </a>
        ))}
      </svg>
      <figcaption className="radar-legenda">
        <span><svg width="14" height="14" aria-hidden="true"><rect x="3" y="3" width="8" height="8" transform="rotate(45 7 7)" className="ponto-vencido" /></svg> Vencido (centro)</span>
        <span><svg width="14" height="14" aria-hidden="true"><polygon points="7,1 13,12 1,12" className="ponto-atencao" /></svg> Atenção</span>
        <span><svg width="14" height="14" aria-hidden="true"><circle cx="7" cy="7" r="5" className="ponto-em-dia" /></svg> Em dia</span>
      </figcaption>
    </figure>
  );
}
