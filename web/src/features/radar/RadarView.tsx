import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { radarApi, itensApi, preferenciasApi } from '../../api/services.ts';
import { StatusBadge } from '../../components/StatusBadge.tsx';
import { Icon } from '../../components/Icon.tsx';
import { ModalConcluirItem } from './ModalConcluirItem.tsx';
import { ModalHistoricoItem } from './ModalHistoricoItem.tsx';
import type { ItemRadarResponse } from '../../api/types.ts';

export function RadarView() {
  const queryClient = useQueryClient();

  const {
    data: radar,
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ['radar'],
    queryFn: radarApi.obter,
  });

  // Modais
  const [itemParaConcluir, setItemParaConcluir] = useState<ItemRadarResponse | null>(null);
  const [itemParaHistorico, setItemParaHistorico] = useState<ItemRadarResponse | null>(null);

  // Lista de passos expandidos por item ID
  const [passosExpandidos, setPassosExpandidos] = useState<Record<string, boolean>>({});

  const toggleExpandirPassos = (itemId: string) => {
    setPassosExpandidos((prev) => ({ ...prev, [itemId]: !prev[itemId] }));
  };

  // Mutação para marcar/desmarcar passos
  const marcarPassoMutation = useMutation({
    mutationFn: ({
      itemId,
      passoId,
      concluido,
    }: {
      itemId: string;
      passoId: string;
      concluido: boolean;
    }) => itensApi.marcarPasso(itemId, passoId, concluido),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['radar'] });
    },
  });

  // Mutação para alternar Modo Foco
  const alternarModoFocoMutation = useMutation({
    mutationFn: async (novoModoFoco: boolean) => {
      const prefs = await preferenciasApi.obter();
      await preferenciasApi.atualizar({
        receberEmail: prefs.receberEmail,
        maxReavisos: prefs.maxReavisos,
        intervaloReavisoDias: prefs.intervaloReavisoDias,
        modoFocoPadrao: novoModoFoco,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['radar'] });
    },
  });

  if (isLoading) {
    return (
      <div className="radar-page">
        <div className="loading-state" role="status">
          <span className="spinner" aria-hidden="true"></span>
          <span>Carregando seu Radar de vencimentos...</span>
        </div>
      </div>
    );
  }

  if (isError || !radar) {
    return (
      <div className="radar-page">
        <div className="alert alert-error" role="alert">
          <p>Não foi possível carregar o Radar de vencimentos.</p>
          <button
            type="button"
            className="btn btn-outline btn-sm"
            onClick={() => refetch()}
          >
            Tentar novamente
          </button>
        </div>
      </div>
    );
  }

  const totalItens =
    radar.centro.length +
    radar.anel1.length +
    radar.anel2.length +
    radar.externo.length;

  const formatarPrazo = (item: ItemRadarResponse) => {
    const partes: string[] = [];
    if (item.diasRestantes < 0) {
      const diasAtraso = Math.abs(item.diasRestantes);
      partes.push(`Vencido há ${diasAtraso} ${diasAtraso === 1 ? 'dia' : 'dias'}`);
    } else if (item.diasRestantes === 0) {
      partes.push('Vence hoje!');
    } else {
      partes.push(`Vence em ${item.diasRestantes} dias`);
    }

    if (item.proximoVencimentoData) {
      partes.push(`(${item.proximoVencimentoData})`);
    }

    if (item.proximoVencimentoKm !== null) {
      partes.push(`ou aos ${item.proximoVencimentoKm.toLocaleString()} km`);
    }

    return partes.join(' ');
  };

  const renderCardItem = (item: ItemRadarResponse) => {
    const expandido = !!passosExpandidos[item.id];
    const totalPassos = item.passos.length;
    const passosConcluidos = item.passos.filter((p) => p.concluido).length;

    return (
      <article key={item.id} className="item-card" aria-labelledby={`title-${item.id}`}>
        <div className="item-card-header">
          <div>
            <h3 id={`title-${item.id}`} className="item-card-title">
              {item.nome}
            </h3>
            <p className="item-card-deadline">{formatarPrazo(item)}</p>
          </div>
          <StatusBadge status={item.status} />
        </div>

        {totalPassos > 0 && (
          <div className="item-passos-section">
            <div className="progress-bar-container" aria-hidden="true">
              <div
                className="progress-bar-fill"
                style={{ width: `${Math.round(item.progressoPassos * 100)}%` }}
              ></div>
            </div>
            <div className="passos-summary">
              <span>
                {passosConcluidos} de {totalPassos} passos concluídos
              </span>
              <button
                type="button"
                className="btn-link"
                onClick={() => toggleExpandirPassos(item.id)}
                aria-expanded={expandido}
              >
                {expandido ? <>Ocultar checklist <Icon name="chevron-down" size={14} /></> : <>Ver checklist <Icon name="chevron-right" size={14} /></>}
              </button>
            </div>

            {expandido && (
              <ul className="passos-checklist" aria-label="Passos para conclusão">
                {item.passos.map((passo) => (
                  <li key={passo.id} className="passo-check-item">
                    <label className="passo-label">
                      <input
                        type="checkbox"
                        checked={passo.concluido}
                        onChange={(e) =>
                          marcarPassoMutation.mutate({
                            itemId: item.id,
                            passoId: passo.id,
                            concluido: e.target.checked,
                          })
                        }
                        className="checkbox-input"
                      />
                      <span
                        className={
                          passo.concluido ? 'passo-texto-concluido' : 'passo-texto'
                        }
                      >
                        {passo.ordem}. {passo.descricao}
                      </span>
                    </label>
                  </li>
                ))}
              </ul>
            )}
          </div>
        )}

        <div className="item-card-actions">
          <button
            type="button"
            className="btn btn-outline btn-sm"
            onClick={() => setItemParaHistorico(item)}
            aria-label={`Ver histórico de ${item.nome}`}
          >
            Histórico
          </button>
          <button
            type="button"
            className="btn btn-primary btn-sm"
            onClick={() => setItemParaConcluir(item)}
            aria-label={`Concluir ou renovar ${item.nome}`}
          >
            Concluir / Renovar
          </button>
        </div>
      </article>
    );
  };

  return (
    <div className="radar-page">
      {/* Introdução do Radar e representação dos quatro níveis de urgência */}
      <section className="radar-header-card" aria-labelledby="radar-page-title">
        <div className="radar-header-info">
          <span className="section-eyebrow">SEU PAINEL DE PRAZOS</span>
          <h1 id="radar-page-title">Radar de vencimentos<span className="heading-period">.</span></h1>
          <p>Veja o que pede atenção agora. O restante pode esperar — um passo de cada vez.</p>
          <div className="radar-header-actions">
            <Link to="/itens/novo" className="btn btn-primary radar-add-action">
              <Icon name="plus" size={17} /> Novo item
            </Link>
            <Link to="/checkup" className="radar-secondary-action">
              Fazer Check-up <Icon name="arrow-right" size={16} />
            </Link>
          </div>
        </div>

        <div className="radar-header-aside">
          <div className="radar-orbit" aria-hidden="true">
            <svg viewBox="0 0 220 220" focusable="false">
              <circle className="orbit-ring orbit-ring-outer" cx="110" cy="110" r="96" />
              <circle className="orbit-ring orbit-ring-mid" cx="110" cy="110" r="72" />
              <circle className="orbit-ring orbit-ring-near" cx="110" cy="110" r="48" />
              <circle className="orbit-ring orbit-ring-core" cx="110" cy="110" r="25" />
              <path className="orbit-axis" d="M110 14v192M14 110h192" />
              <circle className="orbit-center-dot" cx="110" cy="110" r="4" />
              <circle className="orbit-point orbit-point-one" cx="110" cy="61" r="4" />
              <circle className="orbit-point orbit-point-two" cx="159" cy="110" r="4" />
              <circle className="orbit-point orbit-point-three" cx="76" cy="76" r="4" />
            </svg>
            <div className="radar-orbit-center">
              <strong>{totalItens}</strong>
              <span>{totalItens === 1 ? 'item visível' : 'itens visíveis'}</span>
            </div>
          </div>
          <label className="focus-toggle-label" htmlFor="toggle-foco">
            <span className="focus-toggle-icon"><Icon name="focus" size={19} /></span>
            <span className="focus-toggle-text">
              <strong>Modo Foco</strong>
              <small>Mostrar só as 3 prioridades</small>
            </span>
            <input
              id="toggle-foco"
              type="checkbox"
              className="toggle-switch"
              checked={radar.modoFoco}
              onChange={(e) => alternarModoFocoMutation.mutate(e.target.checked)}
              disabled={alternarModoFocoMutation.isPending}
            />
          </label>
          {alternarModoFocoMutation.isError && (
            <p className="focus-error" role="alert">Não foi possível alterar o Modo Foco. Tente novamente.</p>
          )}
        </div>
      </section>

      <section className="radar-ring-summary" aria-label="Resumo dos níveis do Radar">
        <div className="ring-summary-item summary-urgent">
          <span className="summary-marker" aria-hidden="true" />
          <span className="summary-copy"><strong>Vencidos e hoje</strong><small>Resolver primeiro</small></span>
          <strong className="summary-count">{radar.centro.length}</strong>
        </div>
        <div className="ring-summary-item summary-attention">
          <span className="summary-marker" aria-hidden="true" />
          <span className="summary-copy"><strong>Próximos</strong><small>Estão chegando</small></span>
          <strong className="summary-count">{radar.anel1.length}</strong>
        </div>
        <div className="ring-summary-item summary-ontrack">
          <span className="summary-marker" aria-hidden="true" />
          <span className="summary-copy"><strong>Em dia</strong><small>Sem urgência agora</small></span>
          <strong className="summary-count">{radar.anel2.length}</strong>
        </div>
        <div className="ring-summary-item summary-later">
          <span className="summary-marker" aria-hidden="true" />
          <span className="summary-copy"><strong>Prazos longos</strong><small>Para acompanhar depois</small></span>
          <strong className="summary-count">{radar.externo.length}</strong>
        </div>
      </section>

      {/* Alerta de Modo Foco ativo */}
      {radar.modoFoco && (
        <div className="focus-active-note" role="status">
          <Icon name="focus" size={17} />
          <span><strong>Modo Foco ativado.</strong> Esta visualização mostra até 3 prioridades para ajudar você a começar.</span>
        </div>
      )}

      {/* Caso sem nenhum item */}
      {totalItens === 0 ? (
        <div className="radar-empty-card">
          <div className="empty-icon" aria-hidden="true">
            <Icon name="check-circle" size={28} />
          </div>
          <h2>Seu Radar está vazio e em dia!</h2>
          <p>
            Você não possui nenhum item monitorado no momento. Use o Check-up
            inicial ou crie um novo item para começar.
          </p>
          <div className="empty-actions">
            <Link to="/checkup" className="btn btn-primary">
              Fazer Check-up
            </Link>
            <Link to="/itens/novo" className="btn btn-outline">
              Cadastrar Item Manual
            </Link>
          </div>
        </div>
      ) : (
        <div className="radar-rings-container">
          {/* ANEL CENTRO: Máxima Urgência */}
          <section className="radar-ring ring-centro" aria-labelledby="ring-centro-title">
            <header className="ring-header">
              <span className="ring-indicator ring-indicator-centro" aria-hidden="true" />
              <h2 id="ring-centro-title" className="ring-title">
                Centro do Radar — Vencidos e Hoje ({radar.centro.length})
              </h2>
            </header>
            {radar.centro.length === 0 ? (
              <p className="ring-empty-msg">Nenhum item vencido ou vencendo hoje. Ótimo trabalho!</p>
            ) : (
              <div className="ring-items-grid">{radar.centro.map(renderCardItem)}</div>
            )}
          </section>

          {/* ANEL 1: Atenção (Próximos dias) */}
          <section className="radar-ring ring-anel1" aria-labelledby="ring-anel1-title">
            <header className="ring-header">
              <span className="ring-indicator ring-indicator-anel1" aria-hidden="true" />
              <h2 id="ring-anel1-title" className="ring-title">
                Anel 1 — Próximos do Vencimento ({radar.anel1.length})
              </h2>
            </header>
            {radar.anel1.length === 0 ? (
              <p className="ring-empty-msg">Nenhum item em estado de atenção imediata.</p>
            ) : (
              <div className="ring-items-grid">{radar.anel1.map(renderCardItem)}</div>
            )}
          </section>

          {/* ANEL 2: Em Dia */}
          <section className="radar-ring ring-anel2" aria-labelledby="ring-anel2-title">
            <header className="ring-header">
              <span className="ring-indicator ring-indicator-anel2" aria-hidden="true" />
              <h2 id="ring-anel2-title" className="ring-title">
                Anel 2 — Em Dia ({radar.anel2.length})
              </h2>
            </header>
            {radar.anel2.length === 0 ? (
              <p className="ring-empty-msg">Nenhum item nesta faixa.</p>
            ) : (
              <div className="ring-items-grid">{radar.anel2.map(renderCardItem)}</div>
            )}
          </section>

          {/* ANEL EXTERNO: Prazo Longo ou Sem Data */}
          <section className="radar-ring ring-externo" aria-labelledby="ring-externo-title">
            <header className="ring-header">
              <span className="ring-indicator ring-indicator-externo" aria-hidden="true" />
              <h2 id="ring-externo-title" className="ring-title">
                Anel Externo — Prazos Mais Longos ({radar.externo.length})
              </h2>
            </header>
            {radar.externo.length === 0 ? (
              <p className="ring-empty-msg">Nenhum item no anel externo.</p>
            ) : (
              <div className="ring-items-grid">{radar.externo.map(renderCardItem)}</div>
            )}
          </section>
        </div>
      )}

      {/* Modais de Conclusão e Histórico */}
      <ModalConcluirItem
        item={itemParaConcluir}
        isOpen={!!itemParaConcluir}
        onClose={() => setItemParaConcluir(null)}
        onSucesso={() => refetch()}
      />

      <ModalHistoricoItem
        item={itemParaHistorico}
        isOpen={!!itemParaHistorico}
        onClose={() => setItemParaHistorico(null)}
      />
    </div>
  );
}
