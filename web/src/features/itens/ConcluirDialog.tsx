import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import type { z } from 'zod';
import { useNavigate } from 'react-router-dom';
import { mensagemDeErro, problema } from '../../api/client.ts';
import { itensApi } from '../../api/endpoints.ts';
import type { ItemDetalhe } from '../../api/types.ts';
import { Alerta, Button, Dialog, Field } from '../../components/ui.tsx';
import { aplicarErrosDaApi } from '../../utils/erros.ts';
import { hojeIso } from '../../utils/formato.ts';
import { concluirSchema } from './schemas.ts';
import { chaves, useInvalidar } from './useInvalidar.ts';

type Schema = ReturnType<typeof concluirSchema>;

/**
 * T12 · Concluir item (E18), em janela sobre o detalhe. Se a API responder 422 PASSOS_PENDENTES,
 * pergunta "Concluir mesmo assim?" e reenvia com a confirmação (RN06). No fim, comemora e volta ao Radar.
 */
export function ConcluirDialog({ item, aberto, aoFechar }: { item: ItemDetalhe; aberto: boolean; aoFechar: () => void }) {
  const navigate = useNavigate();
  const invalidar = useInvalidar();
  const exigeKm = item.tipoVencimento === 'UsoOuTempo';
  const [pendencia, setPendencia] = useState<string | null>(null);
  const [mensagem, setMensagem] = useState<string | null>(null);
  const [erroApi, setErroApi] = useState<string | null>(null);
  const [dados, setDados] = useState<z.output<Schema> | null>(null);

  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } =
    useForm<z.input<Schema>, unknown, z.output<Schema>>({
      resolver: zodResolver(concluirSchema({ exigeKm, kmMinima: item.kmBase })),
      defaultValues: { dataConclusao: hojeIso(), km: '', valorGasto: '', observacao: '', novaDataVencimento: '' },
    });

  const concluir = async (v: z.output<Schema>, confirmar: boolean) => {
    setErroApi(null);
    try {
      const r = await itensApi.concluir(item.id, {
        dataConclusao: v.dataConclusao,
        km: exigeKm ? v.km : null,
        valorGasto: v.valorGasto ?? null,
        observacao: v.observacao || null,
        novaDataVencimento: item.tipoVencimento === 'DataUnica' ? v.novaDataVencimento ?? null : null,
        confirmarPassosPendentes: confirmar,
      });
      setPendencia(null);
      setMensagem(r.mensagem);
      await invalidar(chaves.radar, chaves.itens, chaves.item(item.id), chaves.historico, chaves.veiculos);
    } catch (e) {
      const p = problema(e);
      if (p?.code === 'PASSOS_PENDENTES') {
        setDados(v);
        setPendencia(p.detail ?? 'Este item ainda tem passos sem marcar. Quer concluir mesmo assim?');
      } else if (!aplicarErrosDaApi(e, setError, ['dataConclusao', 'km', 'valorGasto', 'observacao', 'novaDataVencimento'])) {
        setErroApi(mensagemDeErro(e));
      }
    }
  };

  const fechar = () => {
    setPendencia(null);
    setMensagem(null);
    aoFechar();
  };

  return (
    <Dialog aberto={aberto} titulo={mensagem ? 'Concluído!' : `Concluir: ${item.nome}`} aoFechar={fechar}>
      {mensagem ? (
        <div className="comemoracao" role="status">
          <p className="comemoracao-emoji" aria-hidden="true">🎉</p>
          <p>{mensagem}</p>
          <Button onClick={() => navigate('/')}>Voltar ao Radar</Button>
        </div>
      ) : pendencia ? (
        <div className="confirmacao" role="alertdialog" aria-describedby="pendencia-texto">
          <p id="pendencia-texto">{pendencia}</p>
          <Button onClick={() => dados && concluir(dados, true)} autoFocus>Concluir mesmo assim</Button>
          <Button variante="secundario" onClick={() => setPendencia(null)}>Voltar</Button>
        </div>
      ) : (
        <form onSubmit={handleSubmit((v) => concluir(v, false))} noValidate className="form">
          <Field label="Quando você concluiu?" type="date" max={hojeIso()} erro={errors.dataConclusao?.message} {...register('dataConclusao')} />
          {exigeKm && (
            <Field label="Km na conclusão" type="number" inputMode="numeric" erro={errors.km?.message} {...register('km')} />
          )}
          {item.tipoVencimento === 'DataUnica' && (
            <Field label="Nova validade (se renovou)" type="date" ajuda="Deixe em branco para encerrar o item."
                   erro={errors.novaDataVencimento?.message} {...register('novaDataVencimento')} />
          )}
          <Field label="Valor gasto (opcional)" type="number" inputMode="decimal" step="0.01" min={0}
                 erro={errors.valorGasto?.message} {...register('valorGasto')} />
          <div className="field">
            <label htmlFor="obs-conclusao">Observação (opcional)</label>
            <textarea id="obs-conclusao" rows={2} maxLength={500} {...register('observacao')} />
            {errors.observacao && <p className="field-erro">{errors.observacao.message}</p>}
          </div>
          {erroApi && <Alerta>{erroApi}</Alerta>}
          <Button type="submit" carregando={isSubmitting}>Concluir</Button>
        </form>
      )}
    </Dialog>
  );
}
