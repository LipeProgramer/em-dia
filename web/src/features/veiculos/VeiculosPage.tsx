import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import type { z } from 'zod';
import { useQuery } from '@tanstack/react-query';
import { mensagemDeErro } from '../../api/client.ts';
import { veiculosApi } from '../../api/endpoints.ts';
import type { AtualizarKmResponse, Veiculo } from '../../api/types.ts';
import { Alerta, Button, Dialog, EmptyState, ErrorState, Field, PageHeader, Skeleton, StatusBadge } from '../../components/ui.tsx';
import { Icon } from '../../components/Icon.tsx';
import { aplicarErrosDaApi } from '../../utils/erros.ts';
import { km } from '../../utils/formato.ts';
import { atualizarKmSchema, veiculoSchema } from '../itens/schemas.ts';
import { chaves, useInvalidar } from '../itens/useInvalidar.ts';

type VForm = z.input<typeof veiculoSchema>;
type VValido = z.output<typeof veiculoSchema>;

/** Formulário de veículo (E21/E22): apelido 1 a 50; km ≥ 0; média 1 a 20.000. */
export function VeiculoForm({ veiculo, aoSalvar }: { veiculo?: Veiculo; aoSalvar: () => void }) {
  const invalidar = useInvalidar();
  const [erroApi, setErroApi] = useState<string | null>(null);
  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<VForm, unknown, VValido>({
    resolver: zodResolver(veiculoSchema),
    defaultValues: veiculo
      ? { apelido: veiculo.apelido, tipo: veiculo.tipo, kmAtual: veiculo.kmAtual, mediaKmMes: veiculo.mediaKmMes }
      : { apelido: '', tipo: 'Carro', kmAtual: '', mediaKmMes: 1000 },
  });

  const salvar = handleSubmit(async (v) => {
    setErroApi(null);
    try {
      if (veiculo) await veiculosApi.editar(veiculo.id, { apelido: v.apelido, tipo: v.tipo, mediaKmMes: v.mediaKmMes });
      else await veiculosApi.criar(v);
      await invalidar(chaves.veiculos, chaves.radar, chaves.itens);
      aoSalvar();
    } catch (e) {
      if (!aplicarErrosDaApi(e, setError, ['apelido', 'tipo', 'kmAtual', 'mediaKmMes'])) setErroApi(mensagemDeErro(e));
    }
  });

  return (
    <form onSubmit={salvar} noValidate className="form">
      <Field label="Apelido" placeholder="Ex.: Gol prata" erro={errors.apelido?.message} {...register('apelido')} />
      <div className="field">
        <label htmlFor="tipo-veiculo">Tipo</label>
        <select id="tipo-veiculo" {...register('tipo')}>
          <option value="Carro">Carro</option>
          <option value="Moto">Moto</option>
        </select>
      </div>
      {!veiculo && (
        <Field label="Km atual" type="number" inputMode="numeric" min={0} erro={errors.kmAtual?.message} {...register('kmAtual')} />
      )}
      <Field label="Média de km por mês" type="number" inputMode="numeric" min={1} max={20000}
             erro={errors.mediaKmMes?.message} {...register('mediaKmMes')} />
      {erroApi && <Alerta>{erroApi}</Alerta>}
      <Button type="submit" carregando={isSubmitting}>Salvar</Button>
    </form>
  );
}

/** Atualizar km (E23): número inteiro ≥ km atual; mostra os itens que mudaram de status. */
function AtualizarKmForm({ veiculo }: { veiculo: Veiculo }) {
  const invalidar = useInvalidar();
  const [resultado, setResultado] = useState<AtualizarKmResponse | null>(null);
  const [erroApi, setErroApi] = useState<string | null>(null);
  const schema = atualizarKmSchema(veiculo.kmAtual);
  const { register, handleSubmit, formState: { errors, isSubmitting } } =
    useForm<z.input<typeof schema>, unknown, z.output<typeof schema>>({ resolver: zodResolver(schema), defaultValues: { kmAtual: veiculo.kmAtual } });

  const salvar = handleSubmit(async ({ kmAtual }) => {
    setErroApi(null);
    try {
      setResultado(await veiculosApi.atualizarKm(veiculo.id, kmAtual));
      await invalidar(chaves.veiculos, chaves.radar, chaves.itens);
    } catch (e) {
      setErroApi(mensagemDeErro(e));
    }
  });

  if (resultado) {
    return (
      <div role="status">
        <p>Km atualizada para <strong>{km(resultado.kmAtual)}</strong>.</p>
        {resultado.itensAfetados.length > 0 && (
          <ul className="lista-simples">
            {resultado.itensAfetados.map((i) => <li key={i.id}>{i.nome} <StatusBadge status={i.status} /></li>)}
          </ul>
        )}
      </div>
    );
  }

  return (
    <form onSubmit={salvar} noValidate className="form">
      <Field label="Km atual" type="number" inputMode="numeric" min={veiculo.kmAtual} autoFocus
             ajuda={`Hoje está em ${km(veiculo.kmAtual)}.`} erro={errors.kmAtual?.message} {...register('kmAtual')} />
      {erroApi && <Alerta>{erroApi}</Alerta>}
      <Button type="submit" carregando={isSubmitting}>Atualizar</Button>
    </form>
  );
}

type Janela = { tipo: 'novo' } | { tipo: 'editar' | 'km' | 'excluir'; veiculo: Veiculo } | null;

/** T15 · Veículos: cadastrar, editar, atualizar km e excluir (E20–E24). */
export function VeiculosPage() {
  const invalidar = useInvalidar();
  const [janela, setJanela] = useState<Janela>(null);
  const [erroExclusao, setErroExclusao] = useState<string | null>(null);
  const consulta = useQuery({ queryKey: chaves.veiculos, queryFn: veiculosApi.listar });

  const excluir = async (v: Veiculo) => {
    setErroExclusao(null);
    try {
      await veiculosApi.excluir(v.id);
      await invalidar(chaves.veiculos);
      setJanela(null);
    } catch (e) {
      setErroExclusao(mensagemDeErro(e));
    }
  };

  return (
    <div className="pagina">
      <PageHeader titulo="Veículos" voltar="/perfil">
        <Button className="btn-pequeno" icone="plus" onClick={() => setJanela({ tipo: 'novo' })}>Cadastrar</Button>
      </PageHeader>

      {consulta.isPending && <Skeleton />}
      {consulta.isError && <ErrorState mensagem={mensagemDeErro(consulta.error)} tentarDeNovo={() => consulta.refetch()} />}
      {consulta.data?.length === 0 && <EmptyState titulo="Nenhum veículo cadastrado." texto="Cadastre para acompanhar trocas por quilometragem." />}

      <ul className="cartoes">
        {consulta.data?.map((v) => (
          <li key={v.id} className="cartao-item">
            <div className="cartao-topo">
              <span className="cartao-nome"><Icon name={v.tipo === 'Moto' ? 'bike' : 'car'} size={18} /> {v.apelido}</span>
              <span className="cartao-categoria">{v.quantidadeItens} {v.quantidadeItens === 1 ? 'item' : 'itens'}</span>
            </div>
            <p className="cartao-prazo">{km(v.kmAtual)} · média de {km(v.mediaKmMes)}/mês</p>
            <div className="cartao-acoes">
              <Button variante="secundario" icone="gauge" onClick={() => setJanela({ tipo: 'km', veiculo: v })}>Atualizar km</Button>
              <Button variante="texto" onClick={() => setJanela({ tipo: 'editar', veiculo: v })}>Editar</Button>
              <Button variante="texto" className="link-perigo" onClick={() => setJanela({ tipo: 'excluir', veiculo: v })}>Excluir</Button>
            </div>
          </li>
        ))}
      </ul>

      <Dialog aberto={janela?.tipo === 'novo' || janela?.tipo === 'editar'}
              titulo={janela?.tipo === 'editar' ? 'Editar veículo' : 'Cadastrar veículo'} aoFechar={() => setJanela(null)}>
        {(janela?.tipo === 'novo' || janela?.tipo === 'editar') && (
          <VeiculoForm veiculo={janela.tipo === 'editar' ? janela.veiculo : undefined} aoSalvar={() => setJanela(null)} />
        )}
      </Dialog>

      <Dialog aberto={janela?.tipo === 'km'} titulo="Atualizar km" aoFechar={() => setJanela(null)}>
        {janela?.tipo === 'km' && <AtualizarKmForm veiculo={janela.veiculo} />}
      </Dialog>

      <Dialog aberto={janela?.tipo === 'excluir'} titulo="Excluir veículo?" aoFechar={() => { setJanela(null); setErroExclusao(null); }}>
        {janela?.tipo === 'excluir' && (
          <>
            <p>O veículo <strong>{janela.veiculo.apelido}</strong> será excluído.</p>
            {erroExclusao && <Alerta>{erroExclusao}</Alerta>}
            <Button variante="perigo" onClick={() => excluir(janela.veiculo)}>Excluir</Button>
          </>
        )}
      </Dialog>
    </div>
  );
}
