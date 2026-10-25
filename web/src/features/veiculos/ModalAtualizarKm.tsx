import { useState, useEffect, type FormEvent } from 'react';
import { Modal } from '../../components/Modal.tsx';
import { veiculosApi } from '../../api/services.ts';
import type { VeiculoResponse } from '../../api/types.ts';

interface ModalAtualizarKmProps {
  veiculo: VeiculoResponse | null;
  isOpen: boolean;
  onClose: () => void;
  onSucesso: () => void;
}

export function ModalAtualizarKm({
  veiculo,
  isOpen,
  onClose,
  onSucesso,
}: ModalAtualizarKmProps) {
  const [km, setKm] = useState<string>('');
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  useEffect(() => {
    if (veiculo) {
      setKm(String(veiculo.kmAtual));
      setErro(null);
    }
  }, [veiculo]);

  if (!veiculo) return null;

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErro(null);

    const novoKm = parseInt(km, 10);
    if (isNaN(novoKm)) {
      setErro('Informe uma quilometragem válida.');
      return;
    }

    if (novoKm < veiculo.kmAtual) {
      setErro(
        `A nova quilometragem (${novoKm} km) não pode ser menor que a atual (${veiculo.kmAtual} km).`
      );
      return;
    }

    try {
      setSalvando(true);
      await veiculosApi.atualizarKm(veiculo.id, novoKm);
      onSucesso();
      onClose();
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axErr = err as { response?: { data?: { mensagem?: string } } };
        setErro(axErr.response?.data?.mensagem || 'Erro ao atualizar km.');
      } else {
        setErro('Falha de conexão com o servidor.');
      }
    } finally {
      setSalvando(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={`Atualizar Odômetro: ${veiculo.apelido}`}
    >
      <form onSubmit={handleSubmit} className="atualizar-km-form">
        {erro && (
          <div className="alert alert-error" role="alert">
            <span aria-hidden="true">⚠️</span> {erro}
          </div>
        )}

        <div className="form-group">
          <label htmlFor="kmAtualInput">
            Quilometragem Atual (anterior: {veiculo.kmAtual.toLocaleString()} km)
          </label>
          <input
            id="kmAtualInput"
            type="number"
            className="form-control"
            min={veiculo.kmAtual}
            value={km}
            onChange={(e) => setKm(e.target.value)}
            required
            autoFocus
          />
          <small className="form-text">
            O radar recalcula automaticamente as revisões e trocas de óleo vinculadas
            a este veículo.
          </small>
        </div>

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
            {salvando ? 'Salvando...' : 'Atualizar Km'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
