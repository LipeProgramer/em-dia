import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import type { z } from 'zod';
import { useNavigate } from 'react-router-dom';
import { mensagemDeErro } from '../../api/client.ts';
import { itensApi } from '../../api/endpoints.ts';
import type { ItemDetalhe } from '../../api/types.ts';
import { Alerta, Button, Dialog, Field } from '../../components/ui.tsx';
import { aplicarErrosDaApi } from '../../utils/erros.ts';
import { hojeIso } from '../../utils/formato.ts';
import { concluirSchema } from './schemas.ts';
import { chaves, useGuardarItem, useInvalidar } from './useInvalidar.ts';

type Schema = ReturnType<typeof concluirSchema>;

/**
 * T12 · Concluir item (E18), em janela sobre o detalhe. Os passos não aparecem no site (DV26), então a conclusão
 * já vai confirmada e a API não pergunta sobre passos pendentes (RN06). No fim, comemora e volta ao Radar.
 */
export function ConcluirDialog({ item, aberto, aoFechar }: { item: ItemDetalhe; aberto: boolean; aoFechar: () => void }) {
  const navigate = useNavigate();
  const invalidar = useInvalidar();
  const guardarItem = useGuardarItem();
  const exigeKm = item.tipoVencimento === 'UsoOuTempo';
  const [mensagem, setMensagem] = useState<string | null>(null);
  const [erroApi, setErroApi] = useState<string | null>(null);

  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } =
    useForm<z.input<Schema>, unknown, z.output<Schema>>({
      resolver: zodResolver(concluirSchema({ exigeKm, kmMinima: item.kmBase })),
      defaultValues: { dataConclusao: hojeIso(), km: '', novaDataVencimento: '' },
    });

  const concluir = handleSubmit(async (v) => {
    setErroApi(null);
    try {
      const r = await itensApi.concluir(item.id, {
        dataConclusao: v.dataConclusao,
        km: exigeKm ? v.km : null,
        valorGasto: null,
        observacao: null,
        novaDataVencimento: item.tipoVencimento === 'DataUnica' ? v.novaDataVencimento ?? null : null,
        confirmarPassosPendentes: true,
      });
      setMensagem(r.mensagem);
      guardarItem(r.item);
      await invalidar(chaves.radar, chaves.itens, chaves.historico, chaves.veiculos);
    } catch (e) {
      if (!aplicarErrosDaApi(e, setError, ['dataConclusao', 'km', 'novaDataVencimento'])) setErroApi(mensagemDeErro(e));
    }
  });

  const fechar = () => {
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
      ) : (
        <form onSubmit={concluir} noValidate className="form">
          <Field label="Quando você concluiu?" type="date" max={hojeIso()} erro={errors.dataConclusao?.message} {...register('dataConclusao')} />
          {exigeKm && (
            <Field label="Km na conclusão" type="number" inputMode="numeric" erro={errors.km?.message} {...register('km')} />
          )}
          {item.tipoVencimento === 'DataUnica' && (
            <Field label="Nova validade (se renovou)" type="date" ajuda="Deixe em branco para encerrar o item."
                   erro={errors.novaDataVencimento?.message} {...register('novaDataVencimento')} />
          )}
          {erroApi && <Alerta>{erroApi}</Alerta>}
          <Button type="submit" carregando={isSubmitting}>Concluir</Button>
        </form>
      )}
    </Dialog>
  );
}
