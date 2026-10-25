import { useEffect, useState } from 'react';
import { Modal } from '../../components/Modal.tsx';
import { itensApi } from '../../api/services.ts';
import type { HistoricoResponse, ItemRadarResponse } from '../../api/types.ts';

interface ModalHistoricoItemProps {
  item: ItemRadarResponse | null;
  isOpen: boolean;
  onClose: () => void;
}

export function ModalHistoricoItem({
  item,
  isOpen,
  onClose,
}: ModalHistoricoItemProps) {
  const [historico, setHistorico] = useState<HistoricoResponse[]>([]);
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen && item) {
      let isMounted = true;
      setCarregando(true);
      setErro(null);

      itensApi
        .listarHistorico(item.id)
        .then((dados) => {
          if (isMounted) setHistorico(dados);
        })
        .catch(() => {
          if (isMounted) setErro('Não foi possível carregar o histórico deste item.');
        })
        .finally(() => {
          if (isMounted) setCarregando(false);
        });

      return () => {
        isMounted = false;
      };
    }
  }, [isOpen, item]);

  if (!item) return null;

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={`Histórico: ${item.nome}`}
    >
      <div className="historico-container">
        {carregando && (
          <div className="loading-state" role="status">
            Carregando histórico...
          </div>
        )}

        {erro && (
          <div className="alert alert-error" role="alert">
            <span aria-hidden="true">⚠️</span> {erro}
          </div>
        )}

        {!carregando && !erro && historico.length === 0 && (
          <div className="empty-state">
            <p>Nenhuma conclusão anterior registrada para este item.</p>
          </div>
        )}

        {!carregando && historico.length > 0 && (
          <div className="timeline-list">
            {historico.map((h) => (
              <div key={h.id} className="timeline-item">
                <div className="timeline-marker" aria-hidden="true">
                  ✓
                </div>
                <div className="timeline-content">
                  <div className="timeline-header">
                    <strong>Concluído em: {h.concluidoEm}</strong>
                    {h.valorGasto !== null && (
                      <span className="badge-valor">
                        R$ {h.valorGasto.toFixed(2)}
                      </span>
                    )}
                  </div>
                  {h.kmNaConclusao !== null && (
                    <p className="timeline-detail">
                      Quilometragem: {h.kmNaConclusao.toLocaleString()} km
                    </p>
                  )}
                  {h.observacao && (
                    <p className="timeline-obs">{h.observacao}</p>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}

        <div className="modal-actions">
          <button type="button" className="btn btn-outline" onClick={onClose}>
            Fechar
          </button>
        </div>
      </div>
    </Modal>
  );
}
