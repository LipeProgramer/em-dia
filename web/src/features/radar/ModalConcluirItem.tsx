import { useState, type FormEvent } from 'react';
import { Modal } from '../../components/Modal.tsx';
import { Icon } from '../../components/Icon.tsx';
import { itensApi } from '../../api/services.ts';
import type { ItemRadarResponse } from '../../api/types.ts';

interface ModalConcluirItemProps {
  item: ItemRadarResponse | null;
  isOpen: boolean;
  onClose: () => void;
  onSucesso: () => void;
}

export function ModalConcluirItem({
  item,
  isOpen,
  onClose,
  onSucesso,
}: ModalConcluirItemProps) {
  const [dataConclusao, setDataConclusao] = useState(() =>
    new Date().toISOString().split('T')[0]
  );
  const [kmNaConclusao, setKmNaConclusao] = useState<string>('');
  const [valorGasto, setValorGasto] = useState<string>('');
  const [novaDataVencimento, setNovaDataVencimento] = useState<string>('');
  const [observacao, setObservacao] = useState('');
  const [confirmarPassosPendentes, setConfirmarPassosPendentes] = useState(false);
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  if (!item) return null;

  const passosPendentes = item.passos.filter((p) => !p.concluido).length;
  const temPassosPendentes = passosPendentes > 0;

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErro(null);

    // Validação de passos pendentes sem confirmação
    if (temPassosPendentes && !confirmarPassosPendentes) {
      setErro(
        `Este item ainda tem ${passosPendentes} passo(s) não marcado(s). Marque a opção de confirmação abaixo se deseja concluir mesmo assim.`
      );
      return;
    }

    try {
      setSalvando(true);
      await itensApi.concluir(item.id, {
        dataConclusao,
        kmNaConclusao: kmNaConclusao ? parseInt(kmNaConclusao, 10) : null,
        valorGasto: valorGasto ? parseFloat(valorGasto) : null,
        novaDataVencimento: novaDataVencimento || null,
        observacao: observacao.trim() || null,
        confirmarPassosPendentes,
      });

      onSucesso();
      onClose();
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axErr = err as { response?: { data?: { mensagem?: string } } };
        setErro(axErr.response?.data?.mensagem || 'Erro ao concluir item.');
      } else {
        setErro('Falha de comunicação com o servidor.');
      }
    } finally {
      setSalvando(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={`Concluir: ${item.nome}`}>
      <form onSubmit={handleSubmit} className="concluir-form">
        {erro && (
          <div className="alert alert-error" role="alert">
            <Icon name="alert-triangle" size={17} /> {erro}
          </div>
        )}

        {temPassosPendentes && (
          <div className="alert alert-warning" role="alert">
            <Icon name="lightbulb" size={19} />
            <div>
              <strong>Atenção:</strong> Você ainda tem{' '}
              <strong>{passosPendentes} passo(s) pendente(s)</strong> na lista.
            </div>
          </div>
        )}

        <div className="form-group">
          <label htmlFor="dataConclusao">Data da Conclusão *</label>
          <input
            id="dataConclusao"
            type="date"
            className="form-control"
            value={dataConclusao}
            onChange={(e) => setDataConclusao(e.target.value)}
            required
          />
        </div>

        {item.proximoVencimentoKm !== null && (
          <div className="form-group">
            <label htmlFor="kmConclusao">Quilometragem atual do veículo</label>
            <input
              id="kmConclusao"
              type="number"
              className="form-control"
              placeholder="Ex: 45000"
              value={kmNaConclusao}
              onChange={(e) => setKmNaConclusao(e.target.value)}
            />
            <small className="form-text">
              Usado para recalcular o próximo vencimento por km.
            </small>
          </div>
        )}

        <div className="form-group">
          <label htmlFor="valorGasto">Valor gasto (R$) - Opcional</label>
          <input
            id="valorGasto"
            type="number"
            step="0.01"
            className="form-control"
            placeholder="0,00"
            value={valorGasto}
            onChange={(e) => setValorGasto(e.target.value)}
          />
        </div>

        <div className="form-group">
          <label htmlFor="novaData">
            Nova data de vencimento (se já souber o próximo prazo)
          </label>
          <input
            id="novaData"
            type="date"
            className="form-control"
            value={novaDataVencimento}
            onChange={(e) => setNovaDataVencimento(e.target.value)}
          />
        </div>

        <div className="form-group">
          <label htmlFor="observacao">Observações ou comprovante</label>
          <textarea
            id="observacao"
            className="form-control"
            rows={2}
            placeholder="Ex: Realizado na oficina X, troca de filtro inclusa..."
            value={observacao}
            onChange={(e) => setObservacao(e.target.value)}
          />
        </div>

        {temPassosPendentes && (
          <div className="form-check checkbox-group">
            <input
              id="confirmarPassos"
              type="checkbox"
              className="checkbox-input"
              checked={confirmarPassosPendentes}
              onChange={(e) => setConfirmarPassosPendentes(e.target.checked)}
            />
            <label htmlFor="confirmarPassos" className="checkbox-label">
              Confirmar e concluir mesmo com passos pendentes
            </label>
          </div>
        )}

        <div className="modal-actions">
          <button
            type="button"
            className="btn btn-outline"
            onClick={onClose}
            disabled={salvando}
          >
            Cancelar
          </button>
          <button
            type="submit"
            className="btn btn-primary"
            disabled={salvando}
          >
            {salvando ? 'Salvando...' : 'Confirmar Conclusão'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
