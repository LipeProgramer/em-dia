import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { veiculosApi } from '../../api/services.ts';
import { TipoVeiculo, type VeiculoResponse } from '../../api/types.ts';
import { ModalAtualizarKm } from './ModalAtualizarKm.tsx';
import { VeiculoFormModal } from './VeiculoFormModal.tsx';

export function VeiculosView() {
  const queryClient = useQueryClient();

  const {
    data: veiculos,
    isLoading,
    isError,
    refetch,
  } = useQuery({
    queryKey: ['veiculos'],
    queryFn: veiculosApi.listar,
  });

  // Modais
  const [veiculoKm, setVeiculoKm] = useState<VeiculoResponse | null>(null);
  const [veiculoEditar, setVeiculoEditar] = useState<VeiculoResponse | null>(null);
  const [modalNovoAberto, setModalNovoAberto] = useState(false);

  const excluirMutation = useMutation({
    mutationFn: (id: string) => veiculosApi.excluir(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['veiculos'] });
      queryClient.invalidateQueries({ queryKey: ['radar'] });
    },
  });

  const handleExcluir = async (veiculo: VeiculoResponse) => {
    if (
      window.confirm(
        `Tem certeza de que deseja excluir o veículo "${veiculo.apelido}"?`
      )
    ) {
      await excluirMutation.mutateAsync(veiculo.id);
    }
  };

  const atualizarTudo = () => {
    queryClient.invalidateQueries({ queryKey: ['veiculos'] });
    queryClient.invalidateQueries({ queryKey: ['radar'] });
    queryClient.invalidateQueries({ queryKey: ['itens'] });
  };

  return (
    <div className="veiculos-page">
      <header className="page-header">
        <div>
          <h1>Meus Veículos</h1>
          <p>
            Mantenha o odômetro atualizado para recalcular revisões e trocas de
            óleo.
          </p>
        </div>
        <button
          type="button"
          className="btn btn-primary"
          onClick={() => setModalNovoAberto(true)}
        >
          + Cadastrar Veículo
        </button>
      </header>

      {isLoading && (
        <div className="loading-state" role="status">
          <span className="spinner" aria-hidden="true"></span>
          <span>Carregando veículos...</span>
        </div>
      )}

      {isError && (
        <div className="alert alert-error" role="alert">
          <p>Erro ao carregar a lista de veículos.</p>
          <button
            type="button"
            className="btn btn-outline btn-sm"
            onClick={() => refetch()}
          >
            Tentar novamente
          </button>
        </div>
      )}

      {!isLoading && veiculos && veiculos.length === 0 && (
        <div className="empty-state-card">
          <div className="empty-icon" aria-hidden="true">
            🚗
          </div>
          <h2>Nenhum veículo cadastrado</h2>
          <p>
            Cadastre seu carro ou moto para receber alertas de manutenção por
            quilometragem rodada.
          </p>
          <button
            type="button"
            className="btn btn-primary"
            onClick={() => setModalNovoAberto(true)}
          >
            Cadastrar primeiro veículo
          </button>
        </div>
      )}

      {!isLoading && veiculos && veiculos.length > 0 && (
        <div className="veiculos-grid">
          {veiculos.map((v) => (
            <article key={v.id} className="veiculo-card" aria-labelledby={`veiculo-${v.id}`}>
              <div className="veiculo-card-header">
                <span className="veiculo-type-icon" aria-hidden="true">
                  {v.tipoVeiculo === TipoVeiculo.Moto ? '🏍️' : '🚗'}
                </span>
                <div>
                  <h2 id={`veiculo-${v.id}`} className="veiculo-title">
                    {v.apelido}
                  </h2>
                  {v.placa && <span className="veiculo-placa">{v.placa}</span>}
                </div>
              </div>

              <div className="veiculo-odometro-box">
                <span className="odometro-label">Quilometragem Atual</span>
                <span className="odometro-valor">
                  {v.kmAtual.toLocaleString()} <small>km</small>
                </span>
                {v.mediaKmMes && (
                  <span className="odometro-media">
                    Média de ~{v.mediaKmMes.toLocaleString()} km / mês
                  </span>
                )}
              </div>

              <div className="veiculo-actions">
                <button
                  type="button"
                  className="btn btn-primary btn-block"
                  onClick={() => setVeiculoKm(v)}
                  aria-label={`Atualizar km de ${v.apelido}`}
                >
                  ⚡ Atualizar Km
                </button>
                <div className="veiculo-actions-secondary">
                  <button
                    type="button"
                    className="btn btn-outline btn-sm"
                    onClick={() => setVeiculoEditar(v)}
                    aria-label={`Editar ${v.apelido}`}
                  >
                    Editar
                  </button>
                  <button
                    type="button"
                    className="btn btn-danger-outline btn-sm"
                    onClick={() => handleExcluir(v)}
                    aria-label={`Excluir ${v.apelido}`}
                  >
                    Excluir
                  </button>
                </div>
              </div>
            </article>
          ))}
        </div>
      )}

      {/* Modais */}
      <ModalAtualizarKm
        veiculo={veiculoKm}
        isOpen={!!veiculoKm}
        onClose={() => setVeiculoKm(null)}
        onSucesso={atualizarTudo}
      />

      <VeiculoFormModal
        veiculo={veiculoEditar}
        isOpen={!!veiculoEditar}
        onClose={() => setVeiculoEditar(null)}
        onSucesso={atualizarTudo}
      />

      <VeiculoFormModal
        veiculo={null}
        isOpen={modalNovoAberto}
        onClose={() => setModalNovoAberto(false)}
        onSucesso={atualizarTudo}
      />
    </div>
  );
}
