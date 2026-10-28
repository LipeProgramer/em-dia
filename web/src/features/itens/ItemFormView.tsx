import { useState, useEffect, type FormEvent } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { itensApi, catalogoApi, veiculosApi } from '../../api/services.ts';
import {
  TipoVencimento,
  type CriarItemRequest,
  type EditarItemRequest,
  type ModeloItemResponse,
} from '../../api/types.ts';
import { Icon } from '../../components/Icon.tsx';

export function ItemFormView() {
  const { id } = useParams<{ id: string }>();
  const isEdicao = !!id;
  const navigate = useNavigate();

  // Dados auxiliares
  const { data: categorias } = useQuery({
    queryKey: ['categorias'],
    queryFn: catalogoApi.listarCategorias,
  });

  const { data: veiculos } = useQuery({
    queryKey: ['veiculos'],
    queryFn: veiculosApi.listar,
  });

  const { data: modelos } = useQuery({
    queryKey: ['modelos'],
    queryFn: () => catalogoApi.listarModelos(),
  });

  // Campos do formulário
  const [nome, setNome] = useState('');
  const [categoriaId, setCategoriaId] = useState<number>(1);
  const [tipoVencimento, setTipoVencimento] = useState<TipoVencimento>(
    TipoVencimento.DataUnica
  );
  const [dataBase, setDataBase] = useState('');
  const [dataVencimento, setDataVencimento] = useState('');
  const [intervaloDias, setIntervaloDias] = useState<string>('365');
  const [intervaloKm, setIntervaloKm] = useState<string>('10000');
  const [kmBase, setKmBase] = useState<string>('0');
  const [garantiaFabricanteMeses, setGarantiaFabricanteMeses] = useState<string>('12');
  const [antecedenciaDias, setAntecedenciaDias] = useState<number>(30);
  const [veiculoId, setVeiculoId] = useState<string>('');
  const [observacao, setObservacao] = useState('');
  const [passos, setPassos] = useState<string[]>([]);
  const [novoPasso, setNovoPasso] = useState('');

  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  // Carregar dados na edição
  useEffect(() => {
    if (isEdicao && id) {
      itensApi
        .obterPorId(id)
        .then((item) => {
          setNome(item.nome);
          setCategoriaId(item.categoriaId);
          setTipoVencimento(item.tipoVencimento);
          setDataBase(item.dataBase || '');
          setDataVencimento(item.dataVencimento || '');
          setIntervaloDias(item.intervaloDias ? String(item.intervaloDias) : '');
          setIntervaloKm(item.intervaloKm ? String(item.intervaloKm) : '');
          setKmBase(item.kmBase !== null ? String(item.kmBase) : '');
          setGarantiaFabricanteMeses(
            item.garantiaFabricanteMeses ? String(item.garantiaFabricanteMeses) : ''
          );
          setAntecedenciaDias(item.antecedenciaDias);
          setVeiculoId(item.veiculoId || '');
          setObservacao(item.observacao || '');
          setPassos(item.passos.map((p) => p.descricao));
        })
        .catch(() => {
          setErro('Não foi possível carregar os dados do item para edição.');
        });
    }
  }, [isEdicao, id]);

  // Aplicar modelo sugerido do catálogo
  const handleSelecionarModelo = (modelo: ModeloItemResponse) => {
    setNome(modelo.nome);
    setCategoriaId(modelo.categoriaId);
    setTipoVencimento(modelo.tipoVencimento);
    if (modelo.intervaloDias) setIntervaloDias(String(modelo.intervaloDias));
    if (modelo.intervaloKm) setIntervaloKm(String(modelo.intervaloKm));
    if (modelo.garantiaFabricanteMeses) {
      setGarantiaFabricanteMeses(String(modelo.garantiaFabricanteMeses));
    }
    setAntecedenciaDias(modelo.antecedenciaDias);
    setPassos(modelo.passos.map((p) => p.descricao));
  };

  const handleAdicionarPasso = () => {
    if (novoPasso.trim()) {
      setPassos((prev) => [...prev, novoPasso.trim()]);
      setNovoPasso('');
    }
  };

  const handleRemoverPasso = (index: number) => {
    setPassos((prev) => prev.filter((_, i) => i !== index));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErro(null);

    if (!nome.trim()) {
      setErro('Informe o nome do item.');
      return;
    }

    const payload: CriarItemRequest | EditarItemRequest = {
      nome: nome.trim(),
      categoriaId,
      tipoVencimento,
      dataBase: dataBase || null,
      dataVencimento: dataVencimento || null,
      intervaloDias: intervaloDias ? parseInt(intervaloDias, 10) : null,
      intervaloKm: intervaloKm ? parseInt(intervaloKm, 10) : null,
      kmBase: kmBase ? parseInt(kmBase, 10) : null,
      garantiaFabricanteMeses: garantiaFabricanteMeses
        ? parseInt(garantiaFabricanteMeses, 10)
        : null,
      antecedenciaDias,
      veiculoId: veiculoId || null,
      observacao: observacao.trim() || null,
      passos: passos.length > 0 ? passos : undefined,
    };

    try {
      setSalvando(true);
      if (isEdicao && id) {
        await itensApi.editar(id, payload);
      } else {
        await itensApi.criar(payload);
      }
      navigate('/itens');
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axErr = err as { response?: { data?: { mensagem?: string } } };
        setErro(axErr.response?.data?.mensagem || 'Erro ao salvar o item.');
      } else {
        setErro('Falha de conexão com o servidor.');
      }
    } finally {
      setSalvando(false);
    }
  };

  return (
    <div className="form-page-container">
      <header className="page-header">
        <div>
          <h1>{isEdicao ? 'Editar Item' : 'Novo Item'}</h1>
          <p>
            Configure prazos, periodicidade e passos para não esquecer nada.
          </p>
        </div>
        <Link to="/itens" className="btn btn-outline">
          Voltar para lista
        </Link>
      </header>

      {/* Modelos de Referência Rápidos (apenas na criação) */}
      {!isEdicao && modelos && modelos.length > 0 && (
        <section className="modelos-sugeridos" aria-label="Modelos pré-configurados">
          <h2>Sugestões rápidas:</h2>
          <div className="modelos-chips">
            {modelos.slice(0, 6).map((m) => (
              <button
                key={m.id}
                type="button"
                className="chip-modelo"
                onClick={() => handleSelecionarModelo(m)}
              >
                + {m.nome}
              </button>
            ))}
          </div>
        </section>
      )}

      {erro && (
        <div className="alert alert-error" role="alert">
          <Icon name="alert-triangle" size={17} /> {erro}
        </div>
      )}

      <form onSubmit={handleSubmit} className="item-form-card">
        <div className="form-group">
          <label htmlFor="nomeItem">Nome do Item *</label>
          <input
            id="nomeItem"
            type="text"
            className="form-control"
            placeholder="Ex: Troca de óleo, CNH, IPVA..."
            value={nome}
            onChange={(e) => setNome(e.target.value)}
            required
            autoFocus
          />
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="categoriaItem">Categoria *</label>
            <select
              id="categoriaItem"
              className="form-control"
              value={categoriaId}
              onChange={(e) => setCategoriaId(parseInt(e.target.value, 10))}
            >
              {categorias?.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.nome}
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="tipoVenc">Tipo de Vencimento *</label>
            <select
              id="tipoVenc"
              className="form-control"
              value={tipoVencimento}
              onChange={(e) =>
                setTipoVencimento(parseInt(e.target.value, 10) as TipoVencimento)
              }
            >
              <option value={TipoVencimento.DataUnica}>Data Única (Ex: CNH, Contrato)</option>
              <option value={TipoVencimento.RecorrentePorTempo}>
                Recorrente por Tempo (Ex: IPVA anual, Filtro semestral)
              </option>
              <option value={TipoVencimento.UsoOuTempo}>
                Uso ou Tempo (Veículo: Óleo por km ou meses)
              </option>
              <option value={TipoVencimento.Calculado}>
                Calculado / Garantia (Data da compra + meses)
              </option>
            </select>
          </div>
        </div>

        {/* Campos condicionais por Tipo de Vencimento */}
        {tipoVencimento === TipoVencimento.DataUnica && (
          <div className="form-group">
            <label htmlFor="dataVenc">Data de Vencimento *</label>
            <input
              id="dataVenc"
              type="date"
              className="form-control"
              value={dataVencimento}
              onChange={(e) => setDataVencimento(e.target.value)}
              required
            />
          </div>
        )}

        {tipoVencimento === TipoVencimento.RecorrentePorTempo && (
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="dataBase">Data Base (Última realização) *</label>
              <input
                id="dataBase"
                type="date"
                className="form-control"
                value={dataBase}
                onChange={(e) => setDataBase(e.target.value)}
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="intervaloDias">Repetir a cada (dias) *</label>
              <input
                id="intervaloDias"
                type="number"
                className="form-control"
                placeholder="Ex: 365 para anual, 180 para semestral"
                value={intervaloDias}
                onChange={(e) => setIntervaloDias(e.target.value)}
                required
              />
            </div>
          </div>
        )}

        {tipoVencimento === TipoVencimento.UsoOuTempo && (
          <>
            <div className="form-group">
              <label htmlFor="veiculoId">Veículo Associado *</label>
              <select
                id="veiculoId"
                className="form-control"
                value={veiculoId}
                onChange={(e) => setVeiculoId(e.target.value)}
                required
              >
                <option value="">Selecione um veículo...</option>
                {veiculos?.map((v) => (
                  <option key={v.id} value={v.id}>
                    {v.apelido} ({v.kmAtual} km)
                  </option>
                ))}
              </select>
            </div>
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="dataBaseUso">Data da última troca/revisão *</label>
                <input
                  id="dataBaseUso"
                  type="date"
                  className="form-control"
                  value={dataBase}
                  onChange={(e) => setDataBase(e.target.value)}
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="kmBase">Km na data da troca *</label>
                <input
                  id="kmBase"
                  type="number"
                  className="form-control"
                  placeholder="Ex: 40000"
                  value={kmBase}
                  onChange={(e) => setKmBase(e.target.value)}
                  required
                />
              </div>
            </div>
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="intervaloKm">Intervalo em Km *</label>
                <input
                  id="intervaloKm"
                  type="number"
                  className="form-control"
                  placeholder="Ex: 10000"
                  value={intervaloKm}
                  onChange={(e) => setIntervaloKm(e.target.value)}
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="intervaloDiasUso">Prazo máximo em dias *</label>
                <input
                  id="intervaloDiasUso"
                  type="number"
                  className="form-control"
                  placeholder="Ex: 365"
                  value={intervaloDias}
                  onChange={(e) => setIntervaloDias(e.target.value)}
                  required
                />
              </div>
            </div>
          </>
        )}

        {tipoVencimento === TipoVencimento.Calculado && (
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="dataCompra">Data da Compra *</label>
              <input
                id="dataCompra"
                type="date"
                className="form-control"
                value={dataBase}
                onChange={(e) => setDataBase(e.target.value)}
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="garantiaMeses">Garantia do Fabricante (Meses) *</label>
              <input
                id="garantiaMeses"
                type="number"
                className="form-control"
                placeholder="Ex: 12"
                value={garantiaFabricanteMeses}
                onChange={(e) => setGarantiaFabricanteMeses(e.target.value)}
                required
              />
              <small className="form-text">
                O sistema adiciona automaticamente os 90 dias de garantia legal.
              </small>
            </div>
          </div>
        )}

        <div className="form-group">
          <label htmlFor="antecedencia">Antecedência do 1º aviso (Dias) *</label>
          <input
            id="antecedencia"
            type="number"
            className="form-control"
            min={1}
            max={365}
            value={antecedenciaDias}
            onChange={(e) => setAntecedenciaDias(parseInt(e.target.value, 10))}
            required
          />
          <small className="form-text">
            Quantos dias antes do vencimento você quer começar a ser avisado.
          </small>
        </div>

        {/* Seção de Passos (Checklist) */}
        <div className="form-group passos-builder">
          <label>Checklist de Passos (Tarefas divididas para facilitar a execução)</label>
          <div className="passo-input-row">
            <input
              type="text"
              className="form-control"
              placeholder="Ex: Agendar vistoria, levar documentos..."
              value={novoPasso}
              onChange={(e) => setNovoPasso(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  e.preventDefault();
                  handleAdicionarPasso();
                }
              }}
            />
            <button
              type="button"
              className="btn btn-secondary"
              onClick={handleAdicionarPasso}
            >
              + Adicionar
            </button>
          </div>

          {passos.length > 0 && (
            <ul className="passos-builder-list" aria-label="Passos adicionados">
              {passos.map((p, idx) => (
                <li key={idx} className="passo-builder-item">
                  <span>
                    {idx + 1}. {p}
                  </span>
                  <button
                    type="button"
                    className="btn-remove-passo"
                    onClick={() => handleRemoverPasso(idx)}
                    aria-label={`Remover passo ${p}`}
                  >
                    <Icon name="close" size={15} strokeWidth={2.4} />
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>

        <div className="form-group">
          <label htmlFor="observacaoItem">Observações adicionais</label>
          <textarea
            id="observacaoItem"
            className="form-control"
            rows={3}
            placeholder="Anotações úteis, locais, telefones..."
            value={observacao}
            onChange={(e) => setObservacao(e.target.value)}
          />
        </div>

        <div className="form-actions">
          <Link to="/itens" className="btn btn-outline">
            Cancelar
          </Link>
          <button
            type="submit"
            className="btn btn-primary btn-lg"
            disabled={salvando}
          >
            {salvando ? 'Salvando...' : isEdicao ? 'Atualizar Item' : 'Criar Item'}
          </button>
        </div>
      </form>
    </div>
  );
}
