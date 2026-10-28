import { useState } from 'react';
import { useFieldArray, useForm, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useQuery } from '@tanstack/react-query';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { mensagemDeErro } from '../../api/client.ts';
import { catalogoApi, itensApi, veiculosApi } from '../../api/endpoints.ts';
import type { ItemDetalhe, Modelo, SalvarItemRequest } from '../../api/types.ts';
import { Alerta, Button, ErrorState, Field, PageHeader, Skeleton } from '../../components/ui.tsx';
import { aplicarErrosDaApi } from '../../utils/erros.ts';
import { hojeIso, tiposVencimento } from '../../utils/formato.ts';
import { itemSchema, type ItemForm, type ItemValido } from './schemas.ts';
import { chaves, useInvalidar } from './useInvalidar.ts';

const vazio: ItemForm = {
  nome: '', categoriaId: '', tipoVencimento: 'RecorrentePorTempo', dataBase: '', dataVencimento: '',
  intervaloDias: '', intervaloKm: '', kmBase: '', garantiaFabricanteMeses: '', veiculoId: '',
  antecedenciaDias: 30, observacao: '', passos: [],
};

const deModelo = (m: Modelo): ItemForm => ({
  ...vazio,
  nome: m.nome,
  categoriaId: m.categoriaId,
  tipoVencimento: m.tipoVencimento,
  intervaloDias: m.intervaloDias ?? '',
  intervaloKm: m.intervaloKm ?? '',
  garantiaFabricanteMeses: m.garantiaFabricanteMeses ?? '',
  antecedenciaDias: m.antecedenciaDias,
  passos: m.passos.map((descricao) => ({ descricao })),
});

const deItem = (i: ItemDetalhe): ItemForm => ({
  nome: i.nome,
  categoriaId: i.categoriaId,
  tipoVencimento: i.tipoVencimento,
  dataBase: i.dataBase ?? '',
  dataVencimento: i.dataVencimento ?? '',
  intervaloDias: i.intervaloDias ?? '',
  intervaloKm: i.intervaloKm ?? '',
  kmBase: i.kmBase ?? '',
  garantiaFabricanteMeses: i.garantiaFabricanteMeses ?? '',
  veiculoId: i.veiculo?.id ?? '',
  antecedenciaDias: i.antecedenciaDias,
  observacao: i.observacao ?? '',
  passos: i.passos.map((p) => ({ descricao: p.descricao })),
});

/** Monta o corpo do E15/E16 enviando só os campos do tipo escolhido. */
function paraRequest(v: ItemValido, modeloId?: string): SalvarItemRequest {
  const t = v.tipoVencimento;
  return {
    nome: v.nome,
    categoriaId: v.categoriaId,
    modeloId: modeloId ?? null,
    tipoVencimento: t,
    dataVencimento: t === 'DataUnica' ? v.dataVencimento : null,
    dataBase: t !== 'DataUnica' ? v.dataBase : null,
    intervaloDias: t === 'RecorrentePorTempo' || t === 'UsoOuTempo' ? v.intervaloDias : null,
    intervaloKm: t === 'UsoOuTempo' ? v.intervaloKm : null,
    kmBase: t === 'UsoOuTempo' ? v.kmBase : null,
    veiculoId: t === 'UsoOuTempo' ? v.veiculoId : null,
    garantiaFabricanteMeses: t === 'Calculado' ? v.garantiaFabricanteMeses : null,
    antecedenciaDias: v.antecedenciaDias,
    observacao: v.observacao || null,
    passos: v.passos.map((p) => p.descricao),
  };
}

const camposDoForm = ['nome', 'categoriaId', 'tipoVencimento', 'dataBase', 'dataVencimento', 'intervaloDias',
  'intervaloKm', 'kmBase', 'garantiaFabricanteMeses', 'veiculoId', 'antecedenciaDias', 'observacao', 'passos'];

/** Formulário de item (T09 e T11): campos aparecem e ficam obrigatórios conforme o tipo. */
function ItemFormulario({ inicial, modeloId, enviar, rotuloEnviar }: {
  inicial: ItemForm;
  modeloId?: string;
  enviar: (dados: SalvarItemRequest) => Promise<void>;
  rotuloEnviar: string;
}) {
  const [erroApi, setErroApi] = useState<string | null>(null);
  const categorias = useQuery({ queryKey: ['categorias'], queryFn: catalogoApi.categorias, staleTime: Infinity });
  const veiculos = useQuery({ queryKey: chaves.veiculos, queryFn: veiculosApi.listar });

  // O formulário só é montado com os dados já carregados; trocar de modelo remonta o componente (key).
  const { register, handleSubmit, control, setError, formState: { errors, isSubmitting } } =
    useForm<ItemForm, unknown, ItemValido>({ resolver: zodResolver(itemSchema), defaultValues: inicial });
  const passos = useFieldArray({ control, name: 'passos' });
  const tipo = useWatch({ control, name: 'tipoVencimento' });

  const salvar = handleSubmit(async (v) => {
    setErroApi(null);
    try {
      await enviar(paraRequest(v, modeloId));
    } catch (e) {
      if (!aplicarErrosDaApi(e, setError, camposDoForm)) setErroApi(mensagemDeErro(e));
    }
  });

  return (
    <form onSubmit={salvar} noValidate className="form">
      <Field label="Nome" erro={errors.nome?.message} {...register('nome')} />

      <div className={`field ${errors.categoriaId ? 'field-invalido' : ''}`}>
        <label htmlFor="categoria">Categoria</label>
        <select id="categoria" aria-invalid={!!errors.categoriaId} aria-describedby={errors.categoriaId ? 'categoria-erro' : undefined}
                {...register('categoriaId')}>
          <option value="">Escolha…</option>
          {categorias.data?.map((c) => <option key={c.id} value={c.id}>{c.nome}</option>)}
        </select>
        {errors.categoriaId && <p id="categoria-erro" className="field-erro">{errors.categoriaId.message}</p>}
      </div>

      <div className="field">
        <label htmlFor="tipo">Tipo de vencimento</label>
        <select id="tipo" {...register('tipoVencimento')}>
          {Object.entries(tiposVencimento).map(([valor, rotulo]) => <option key={valor} value={valor}>{rotulo}</option>)}
        </select>
      </div>

      {tipo === 'DataUnica' && (
        <Field label="Data de vencimento" type="date" erro={errors.dataVencimento?.message} {...register('dataVencimento')} />
      )}

      {tipo === 'UsoOuTempo' && (
        <div className={`field ${errors.veiculoId ? 'field-invalido' : ''}`}>
          <label htmlFor="veiculo">Veículo</label>
          <select id="veiculo" aria-invalid={!!errors.veiculoId} aria-describedby={errors.veiculoId ? 'veiculo-erro' : undefined}
                  {...register('veiculoId')}>
            <option value="">Escolha…</option>
            {veiculos.data?.map((v) => <option key={v.id} value={v.id}>{v.apelido} ({v.kmAtual.toLocaleString('pt-BR')} km)</option>)}
          </select>
          {errors.veiculoId && <p id="veiculo-erro" className="field-erro">{errors.veiculoId.message}</p>}
          {veiculos.data?.length === 0 && <p className="field-ajuda">Cadastre um veículo em <Link to="/perfil/veiculos">Perfil › Veículos</Link>.</p>}
        </div>
      )}

      {tipo !== 'DataUnica' && (
        <Field label={tipo === 'Calculado' ? 'Data da compra' : 'Data da última vez'} type="date" max={hojeIso()}
               erro={errors.dataBase?.message} {...register('dataBase')} />
      )}
      {tipo === 'UsoOuTempo' && (
        <>
          <Field label="Km na última vez" type="number" inputMode="numeric" erro={errors.kmBase?.message} {...register('kmBase')} />
          <Field label="Intervalo em km" type="number" inputMode="numeric" erro={errors.intervaloKm?.message} {...register('intervaloKm')} />
        </>
      )}
      {(tipo === 'RecorrentePorTempo' || tipo === 'UsoOuTempo') && (
        <Field label="Intervalo em dias" type="number" inputMode="numeric" erro={errors.intervaloDias?.message} {...register('intervaloDias')} />
      )}
      {tipo === 'Calculado' && (
        <Field label="Garantia do fabricante (meses)" type="number" inputMode="numeric"
               ajuda="Somamos os 90 dias da garantia legal." erro={errors.garantiaFabricanteMeses?.message}
               {...register('garantiaFabricanteMeses')} />
      )}

      <Field label="Avisar com quantos dias de antecedência?" type="number" inputMode="numeric" min={0} max={365}
             erro={errors.antecedenciaDias?.message} {...register('antecedenciaDias')} />

      <fieldset className="grupo">
        <legend>Passos pequenos</legend>
        {passos.fields.map((campo, i) => (
          <div className="passo-editavel" key={campo.id}>
            <Field label={`Passo ${i + 1}`} erro={errors.passos?.[i]?.descricao?.message} {...register(`passos.${i}.descricao`)} />
            <button type="button" className="btn-icone" aria-label={`Remover passo ${i + 1}`} onClick={() => passos.remove(i)}>×</button>
          </div>
        ))}
        {errors.passos?.message && <p className="field-erro">{errors.passos.message}</p>}
        {passos.fields.length < 10 && (
          <Button type="button" variante="texto" icone="plus" onClick={() => passos.append({ descricao: '' })}>Adicionar passo</Button>
        )}
      </fieldset>

      <div className="field">
        <label htmlFor="observacao">Observação (opcional)</label>
        <textarea id="observacao" rows={2} maxLength={500} {...register('observacao')} />
      </div>

      {erroApi && <Alerta>{erroApi}</Alerta>}
      <Button type="submit" carregando={isSubmitting}>{rotuloEnviar}</Button>
    </form>
  );
}

/** T09 · Novo item: escolher um modelo pronto (E08, E09) ou criar do zero (E15). */
export function NovoItemPage() {
  const navigate = useNavigate();
  const invalidar = useInvalidar();
  const [categoria, setCategoria] = useState<number>(1);
  const [modelo, setModelo] = useState<Modelo | null | undefined>(undefined); // undefined = ainda escolhendo
  const categorias = useQuery({ queryKey: ['categorias'], queryFn: catalogoApi.categorias, staleTime: Infinity });
  const modelos = useQuery({ queryKey: ['modelos'], queryFn: () => catalogoApi.modelos(), staleTime: Infinity });

  const criar = async (dados: SalvarItemRequest) => {
    const item = await itensApi.criar(dados);
    await invalidar(chaves.radar, chaves.itens);
    navigate(`/itens/${item.id}`, { replace: true });
  };

  if (modelo === undefined) {
    return (
      <div className="pagina">
        <PageHeader titulo="Novo item" voltar="/itens" />
        <div className="filtros" role="group" aria-label="Categoria">
          {categorias.data?.map((c) => (
            <button type="button" key={c.id} className={`chip ${categoria === c.id ? 'ativo' : ''}`}
                    aria-pressed={categoria === c.id} onClick={() => setCategoria(c.id)}>{c.nome}</button>
          ))}
        </div>
        {modelos.isPending && <Skeleton />}
        {modelos.isError && <ErrorState tentarDeNovo={() => modelos.refetch()} />}
        <ul className="cartoes">
          {modelos.data?.filter((m) => m.categoriaId === categoria).map((m) => (
            <li key={m.id}>
              <button type="button" className="cartao-item cartao-botao" onClick={() => setModelo(m)}>
                <p className="cartao-nome">{m.nome}</p>
                <p className="cartao-prazo">{tiposVencimento[m.tipoVencimento]}</p>
              </button>
            </li>
          ))}
        </ul>
        <Button variante="secundario" icone="plus" onClick={() => setModelo(null)}>Criar do zero</Button>
      </div>
    );
  }

  const inicial = modelo ? deModelo(modelo) : { ...vazio, categoriaId: categoria };
  return (
    <div className="pagina">
      <PageHeader titulo={modelo ? modelo.nome : 'Novo item'} />
      <Button variante="texto" icone="arrow-left" onClick={() => setModelo(undefined)}>Escolher outro modelo</Button>
      <ItemFormulario key={modelo?.id ?? 'zero'} inicial={inicial} modeloId={modelo?.id} enviar={criar} rotuloEnviar="Salvar item" />
    </div>
  );
}

/** T11 · Editar item (E14, E16). */
export function EditarItemPage() {
  const { id = '' } = useParams();
  const navigate = useNavigate();
  const invalidar = useInvalidar();
  const item = useQuery({ queryKey: chaves.item(id), queryFn: () => itensApi.obter(id) });

  const salvar = async (dados: SalvarItemRequest) => {
    await itensApi.editar(id, dados);
    await invalidar(chaves.radar, chaves.itens, chaves.item(id));
    navigate(`/itens/${id}`, { replace: true });
  };

  return (
    <div className="pagina">
      <PageHeader titulo="Editar item" voltar={`/itens/${id}`} />
      {item.isPending && <Skeleton />}
      {item.isError && <ErrorState mensagem={mensagemDeErro(item.error)} tentarDeNovo={() => item.refetch()} />}
      {item.data?.situacao === 'Encerrado' && <Alerta>Um item concluído e encerrado não pode ser editado.</Alerta>}
      {item.data && item.data.situacao !== 'Encerrado' && (
        <ItemFormulario inicial={deItem(item.data)} enviar={salvar} rotuloEnviar="Salvar alterações" />
      )}
    </div>
  );
}
