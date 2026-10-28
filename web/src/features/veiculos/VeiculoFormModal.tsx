import { useState, useEffect, type FormEvent } from 'react';
import { Modal } from '../../components/Modal.tsx';
import { veiculosApi } from '../../api/services.ts';
import { TipoVeiculo, type VeiculoResponse } from '../../api/types.ts';
import { Icon } from '../../components/Icon.tsx';

interface VeiculoFormModalProps {
  veiculo: VeiculoResponse | null;
  isOpen: boolean;
  onClose: () => void;
  onSucesso: () => void;
}

export function VeiculoFormModal({
  veiculo,
  isOpen,
  onClose,
  onSucesso,
}: VeiculoFormModalProps) {
  const isEdicao = !!veiculo;

  const [apelido, setApelido] = useState('');
  const [tipoVeiculo, setTipoVeiculo] = useState<TipoVeiculo>(TipoVeiculo.Carro);
  const [placa, setPlaca] = useState('');
  const [kmAtual, setKmAtual] = useState<string>('0');
  const [mediaKmMes, setMediaKmMes] = useState<string>('1000');
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  useEffect(() => {
    if (veiculo) {
      setApelido(veiculo.apelido);
      setTipoVeiculo(veiculo.tipoVeiculo);
      setPlaca(veiculo.placa || '');
      setKmAtual(String(veiculo.kmAtual));
      setMediaKmMes(veiculo.mediaKmMes ? String(veiculo.mediaKmMes) : '');
      setErro(null);
    } else {
      setApelido('');
      setTipoVeiculo(TipoVeiculo.Carro);
      setPlaca('');
      setKmAtual('0');
      setMediaKmMes('1000');
      setErro(null);
    }
  }, [veiculo, isOpen]);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErro(null);

    if (!apelido.trim()) {
      setErro('Informe o apelido do veículo.');
      return;
    }

    const km = parseInt(kmAtual, 10);
    if (isNaN(km) || km < 0) {
      setErro('A quilometragem deve ser um número maior ou igual a zero.');
      return;
    }

    const payload = {
      apelido: apelido.trim(),
      tipoVeiculo,
      placa: placa.trim() || null,
      kmAtual: km,
      mediaKmMes: mediaKmMes ? parseInt(mediaKmMes, 10) : null,
    };

    try {
      setSalvando(true);
      if (isEdicao && veiculo) {
        await veiculosApi.editar(veiculo.id, payload);
      } else {
        await veiculosApi.criar(payload);
      }
      onSucesso();
      onClose();
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axErr = err as { response?: { data?: { mensagem?: string } } };
        setErro(axErr.response?.data?.mensagem || 'Erro ao salvar veículo.');
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
      title={isEdicao ? `Editar: ${veiculo.apelido}` : 'Cadastrar Veículo'}
    >
      <form onSubmit={handleSubmit} className="veiculo-form">
        {erro && (
          <div className="alert alert-error" role="alert">
            <Icon name="alert-triangle" size={17} /> {erro}
          </div>
        )}

        <div className="form-group">
          <label htmlFor="apelidoVeiculo">Apelido do Veículo *</label>
          <input
            id="apelidoVeiculo"
            type="text"
            className="form-control"
            placeholder="Ex: Onix Prata, CG 160 Titan..."
            value={apelido}
            onChange={(e) => setApelido(e.target.value)}
            required
            autoFocus
          />
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="tipoVeiculoSelect">Tipo *</label>
            <select
              id="tipoVeiculoSelect"
              className="form-control"
              value={tipoVeiculo}
              onChange={(e) =>
                setTipoVeiculo(parseInt(e.target.value, 10) as TipoVeiculo)
              }
            >
              <option value={TipoVeiculo.Carro}>Carro</option>
              <option value={TipoVeiculo.Moto}>Moto</option>
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="placaVeiculo">Placa (Opcional)</label>
            <input
              id="placaVeiculo"
              type="text"
              className="form-control"
              placeholder="Ex: ABC1D23"
              value={placa}
              onChange={(e) => setPlaca(e.target.value.toUpperCase())}
            />
          </div>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="kmAtualVeiculo">Km Atual *</label>
            <input
              id="kmAtualVeiculo"
              type="number"
              className="form-control"
              min={0}
              value={kmAtual}
              onChange={(e) => setKmAtual(e.target.value)}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="mediaKmMesInput">Média Estimada (Km/mês)</label>
            <input
              id="mediaKmMesInput"
              type="number"
              className="form-control"
              placeholder="Ex: 1000"
              value={mediaKmMes}
              onChange={(e) => setMediaKmMes(e.target.value)}
            />
            <small className="form-text">Usado para estimar datas de revisões.</small>
          </div>
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
            {salvando ? 'Salvando...' : isEdicao ? 'Salvar Alterações' : 'Cadastrar'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
