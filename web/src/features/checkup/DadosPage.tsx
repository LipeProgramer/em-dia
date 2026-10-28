import { useMemo, useState } from 'react';
import { useForm, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import { useQueryClient } from '@tanstack/react-query';
import { mensagemDeErro, problema } from '../../api/client.ts';
import { catalogoApi } from '../../api/endpoints.ts';
import type { CheckupRequest, Modelo } from '../../api/types.ts';
import { Alerta, Button, Field, PageHeader } from '../../components/ui.tsx';
import { hojeIso } from '../../utils/formato.ts';
import { useSession } from '../auth/useSession.tsx';
import { msg } from '../itens/schemas.ts';
import { useCheckup } from './CheckupContext.tsx';

const CATEGORIA_VEICULO = 1;

const rotuloData: Record<Modelo['tipoVencimento'], string> = {
  DataUnica: 'Quando vence?',
  RecorrentePorTempo: 'Quando foi a última vez?',
  UsoOuTempo: 'Data da última vez',
  Calculado: 'Data da compra',
};

type ItemDados = { incluir: boolean; data?: string; kmBase?: string };
type Form = {
  veiculo: { apelido: string; tipo: 'Carro' | 'Moto'; kmAtual: string; mediaKmMes: string };
  itens: Record<string, ItemDados>;
};

const inteiro = (v?: string) => (v === undefined || v === '' ? undefined : Number(v));

/** Schema montado a partir dos modelos escolhidos: campos exigidos pelo tipo de cada modelo (spec §5.5, T04). */
function criarSchema(modelos: Modelo[], precisaVeiculo: boolean) {
  return z.custom<Form>().superRefine((f, ctx) => {
    const erro = (path: (string | number)[], message: string) => ctx.addIssue({ code: 'custom', path, message });
    const v = f.veiculo;
    if (precisaVeiculo) {
      if (!v.apelido?.trim() || v.apelido.trim().length > 50) erro(['veiculo', 'apelido'], 'O apelido deve ter de 1 a 50 caracteres.');
      const kmAtual = inteiro(v.kmAtual);
      if (kmAtual === undefined || Number.isNaN(kmAtual) || kmAtual < 0) erro(['veiculo', 'kmAtual'], 'A km deve ser maior ou igual a 0.');
      const media = inteiro(v.mediaKmMes);
      if (media === undefined || Number.isNaN(media) || media < 1 || media > 20000) erro(['veiculo', 'mediaKmMes'], 'Informe uma média entre 1 e 20.000.');
    }
    for (const m of modelos) {
      const it = f.itens[m.id];
      if (!it?.incluir) continue;
      if (!it.data) erro(['itens', m.id, 'data'], msg.obrigatorio);
      else if (m.tipoVencimento !== 'DataUnica' && it.data > hojeIso()) erro(['itens', m.id, 'data'], msg.dataFutura);
      if (m.tipoVencimento === 'UsoOuTempo') {
        const kmBase = inteiro(it.kmBase);
        if (kmBase === undefined || Number.isNaN(kmBase) || kmBase < 0) erro(['itens', m.id, 'kmBase'], 'A km deve ser maior ou igual a 0.');
        else if (kmBase > (inteiro(v.kmAtual) ?? Infinity)) erro(['itens', m.id, 'kmBase'], 'Não pode ser maior que a km atual do veículo.');
      }
    }
  });
}

/** T04 · Check-up: completar datas, km e veículo dos itens escolhidos (E10 → E11). */
export function DadosPage() {
  const { perguntas, respostas, setResultado, setIndice } = useCheckup();
  const { atualizarUsuario } = useSession();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [erroApi, setErroApi] = useState<string | null>(null);

  const escolhidas = useMemo(() => (perguntas ?? []).filter((p) => respostas[p.codigo]), [perguntas, respostas]);
  const modelos = useMemo(() => escolhidas.flatMap((p) => p.modelos), [escolhidas]);
  const temVeiculo = modelos.some((m) => m.categoriaId === CATEGORIA_VEICULO);

  const { register, handleSubmit, control, setError, formState: { errors, isSubmitting } } = useForm<Form>({
    defaultValues: {
      veiculo: { apelido: '', tipo: 'Carro', kmAtual: '', mediaKmMes: '1000' },
      itens: Object.fromEntries(modelos.map((m) => [m.id, { incluir: true, data: '', kmBase: '' }])),
    },
    resolver: (valores, contexto, opcoes) => {
      const precisa = modelos.some((m) => m.categoriaId === CATEGORIA_VEICULO && valores.itens[m.id]?.incluir);
      return zodResolver(criarSchema(modelos, precisa))(valores, contexto, opcoes);
    },
  });
  const itensValores = useWatch({ control, name: 'itens' });

  if (!perguntas) return <Navigate to="/checkup" replace />;

  const enviar = handleSubmit(async (f) => {
    setErroApi(null);
    const incluidos = modelos.filter((m) => f.itens[m.id]?.incluir);
    const usaVeiculo = incluidos.some((m) => m.categoriaId === CATEGORIA_VEICULO);
    const req: CheckupRequest = {
      respostas: Object.entries(respostas).map(([codigo, resposta]) => ({ codigo, resposta })),
      veiculos: usaVeiculo ? [{
        ref: 'v1', apelido: f.veiculo.apelido.trim(), tipo: f.veiculo.tipo,
        kmAtual: Number(f.veiculo.kmAtual), mediaKmMes: Number(f.veiculo.mediaKmMes),
      }] : [],
      itens: incluidos.map((m) => {
        const it = f.itens[m.id];
        return {
          modeloId: m.id,
          veiculoRef: m.categoriaId === CATEGORIA_VEICULO ? 'v1' : undefined,
          dataVencimento: m.tipoVencimento === 'DataUnica' ? it.data : undefined,
          dataBase: m.tipoVencimento !== 'DataUnica' ? it.data : undefined,
          kmBase: m.tipoVencimento === 'UsoOuTempo' ? Number(it.kmBase) : undefined,
        };
      }),
    };

    try {
      const resultado = await catalogoApi.fazerCheckup(req);
      setResultado(resultado);
      atualizarUsuario({ precisaCheckup: false });
      await Promise.all(['me', 'radar', 'itens', 'veiculos'].map((k) => queryClient.invalidateQueries({ queryKey: [k] })));
      navigate('/checkup/resultado');
    } catch (e) {
      // Erros 400 por campo: "itens[0].dataBase" → campo do modelo correspondente
      const p = problema(e);
      let aplicou = false;
      for (const [campo, mensagens] of Object.entries(p?.errors ?? {})) {
        const m = /^itens\[(\d+)\]\.(\w+)$/.exec(campo);
        const modelo = m ? incluidos[Number(m[1])] : undefined;
        if (modelo) {
          const destino = m![2] === 'kmBase' ? 'kmBase' : 'data';
          setError(`itens.${modelo.id}.${destino}` as const, { message: mensagens[0] }, { shouldFocus: !aplicou });
          aplicou = true;
        }
      }
      if (!aplicou) setErroApi(mensagemDeErro(e));
    }
  });

  return (
    <main className="checkup" id="conteudo">
      <PageHeader titulo="Completar dados" />
      <p className="texto-apoio">Só o necessário para calcular os prazos. Os intervalos são sugestões e você pode mudar depois.</p>

      <form onSubmit={enviar} noValidate className="form">
        {modelos.length === 0 && (
          <p className="texto-apoio">Você não marcou nenhuma área. Tudo bem: dá para adicionar itens a qualquer momento.</p>
        )}

        {temVeiculo && (
          <fieldset className="grupo">
            <legend>Seu veículo</legend>
            <Field label="Apelido" placeholder="Ex.: Gol prata" erro={errors.veiculo?.apelido?.message} {...register('veiculo.apelido')} />
            <div className="field">
              <label htmlFor="veiculo-tipo">Tipo</label>
              <select id="veiculo-tipo" {...register('veiculo.tipo')}>
                <option value="Carro">Carro</option>
                <option value="Moto">Moto</option>
              </select>
            </div>
            <Field label="Km atual" type="number" inputMode="numeric" min={0} erro={errors.veiculo?.kmAtual?.message} {...register('veiculo.kmAtual')} />
            <Field label="Média de km por mês" type="number" inputMode="numeric" min={1} max={20000}
                   ajuda="Um palpite já ajuda: 1.000 km/mês é comum." erro={errors.veiculo?.mediaKmMes?.message}
                   {...register('veiculo.mediaKmMes')} />
          </fieldset>
        )}

        {escolhidas.map((p) => (
          <fieldset className="grupo" key={p.codigo}>
            <legend>{p.texto.replace('?', '')}</legend>
            {p.modelos.map((m) => {
              const incluir = itensValores?.[m.id]?.incluir ?? true;
              const erroItem = errors.itens?.[m.id];
              return (
                <div className="modelo" key={m.id}>
                  <label className="check">
                    <input type="checkbox" {...register(`itens.${m.id}.incluir`)} />
                    <strong>{m.nome}</strong>
                  </label>
                  {incluir && (
                    <div className="modelo-campos">
                      <Field label={rotuloData[m.tipoVencimento]} type="date"
                             max={m.tipoVencimento === 'DataUnica' ? undefined : hojeIso()}
                             erro={erroItem?.data?.message} {...register(`itens.${m.id}.data`)} />
                      {m.tipoVencimento === 'UsoOuTempo' && (
                        <Field label="Km na última vez" type="number" inputMode="numeric" min={0}
                               erro={erroItem?.kmBase?.message} {...register(`itens.${m.id}.kmBase`)} />
                      )}
                    </div>
                  )}
                </div>
              );
            })}
          </fieldset>
        ))}

        {erroApi && <Alerta>{erroApi}</Alerta>}
        <Button type="submit" carregando={isSubmitting}>Ver resultado</Button>
        <Link to="/checkup" className="btn btn-texto" onClick={() => setIndice(0)}>Rever as perguntas</Link>
      </form>
    </main>
  );
}
