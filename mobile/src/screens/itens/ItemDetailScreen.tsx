import React, { useState } from 'react';
import { Pressable, Text, View } from 'react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import { CheckSquare, Square } from 'lucide-react-native';
import type { RootStackParamList } from '../../navigation/types';
import type { ItemDetalhe } from '../../api/types';
import { itensApi } from '../../api/services';
import { errosPorCampo, mensagemDeErro, problema } from '../../api/client';
import { Button } from '../../components/Button';
import { Input } from '../../components/Input';
import { Alerta, CampoData, Carregando, ErroTela, Janela, Progresso, StatusBadge, Tela, corStatus, estilos, useCarregar } from '../../components/ui';
import { dataBr, dinheiro, hojeIso, km, nomesCategorias } from '../../utils/formato';
import { inteiro, validarData, type Erros } from '../../utils/validacao';
import { colors, spacing, typography } from '../../theme';

type Props = NativeStackScreenProps<RootStackParamList, 'ItemDetail'>;

/**
 * T12 · Concluir item (E18). Se a API responder 422 PASSOS_PENDENTES, pergunta "Concluir mesmo assim?"
 * e reenvia com a confirmação (RN06). Depois comemora e volta ao Radar.
 */
function ConcluirJanela({ item, aberta, aoFechar, aoConcluir }: {
  item: ItemDetalhe; aberta: boolean; aoFechar: () => void; aoConcluir: () => void;
}) {
  const exigeKm = item.tipoVencimento === 'UsoOuTempo';
  const [form, setForm] = useState({ data: dataBr(hojeIso()), km: '', valor: '', observacao: '', novaValidade: '' });
  const [erros, setErros] = useState<Erros>({});
  const [erroApi, setErroApi] = useState<string | null>(null);
  const [pendencia, setPendencia] = useState<string | null>(null);
  const [mensagem, setMensagem] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);
  const campo = (k: keyof typeof form) => (v: string) => setForm((f) => ({ ...f, [k]: v }));

  const concluir = async (confirmar: boolean) => {
    const e: Erros = {};
    const data = validarData(form.data, e, 'dataConclusao', { obrigatoria: 'Informe a data da conclusão.', naoFutura: true });
    const kmNum = inteiro(form.km);
    if (exigeKm && kmNum === undefined) e.km = 'Informe a quilometragem.';
    else if (kmNum !== undefined && (Number.isNaN(kmNum) || (item.kmBase != null && kmNum < item.kmBase)))
      e.km = `A km não pode ser menor que ${(item.kmBase ?? 0).toLocaleString('pt-BR')}.`;
    const valor = form.valor.trim() ? Number(form.valor.replace(',', '.')) : undefined;
    if (valor !== undefined && (Number.isNaN(valor) || valor < 0)) e.valorGasto = 'O valor deve ser maior ou igual a 0.';
    const nova = item.tipoVencimento === 'DataUnica' ? validarData(form.novaValidade, e, 'novaDataVencimento', { futura: true }) : undefined;
    if (form.observacao.length > 500) e.observacao = 'A observação deve ter até 500 caracteres.';
    setErros(e);
    setErroApi(null);
    if (Object.keys(e).length) return;

    setEnviando(true);
    try {
      const r = await itensApi.concluir(item.id, {
        dataConclusao: data!, km: exigeKm ? kmNum : null, valorGasto: valor ?? null,
        observacao: form.observacao || null, novaDataVencimento: nova ?? null, confirmarPassosPendentes: confirmar,
      });
      setPendencia(null);
      setMensagem(r.mensagem);
    } catch (err) {
      const p = problema(err);
      if (p?.code === 'PASSOS_PENDENTES') setPendencia(p.detail ?? 'Este item ainda tem passos sem marcar. Quer concluir mesmo assim?');
      else if (Object.keys(errosPorCampo(err)).length) setErros(errosPorCampo(err));
      else setErroApi(mensagemDeErro(err));
    } finally {
      setEnviando(false);
    }
  };

  return (
    <Janela aberta={aberta} titulo={mensagem ? 'Concluído! 🎉' : `Concluir: ${item.nome}`} aoFechar={aoFechar}>
      {mensagem ? (
        <View style={{ gap: spacing.md }}>
          <Text style={[typography.h3, { color: colors.textPrimary, textAlign: 'center' }]}>{mensagem}</Text>
          <Button title="Voltar ao Radar" onPress={aoConcluir} fullWidth />
        </View>
      ) : pendencia ? (
        <View style={{ gap: spacing.md }} accessibilityRole="alert">
          <Text style={[typography.body, { color: colors.textPrimary, textAlign: 'center' }]}>{pendencia}</Text>
          <Button title="Concluir mesmo assim" onPress={() => concluir(true)} loading={enviando} fullWidth />
          <Button title="Voltar" variant="outline" onPress={() => setPendencia(null)} fullWidth />
        </View>
      ) : (
        <View>
          <CampoData label="Quando você concluiu?" valor={form.data} aoMudar={campo('data')} erro={erros.dataConclusao} />
          {exigeKm && <Input label="Km na conclusão" keyboardType="number-pad" value={form.km} onChangeText={campo('km')} error={erros.km} />}
          {item.tipoVencimento === 'DataUnica' && (
            <CampoData label="Nova validade (se renovou)" valor={form.novaValidade} aoMudar={campo('novaValidade')}
                       dica="Deixe em branco para encerrar o item." erro={erros.novaDataVencimento} />
          )}
          <Input label="Valor gasto (opcional)" keyboardType="decimal-pad" value={form.valor} onChangeText={campo('valor')} error={erros.valorGasto} />
          <Input label="Observação (opcional)" value={form.observacao} onChangeText={campo('observacao')} maxLength={500} error={erros.observacao} />
          {erroApi && <Alerta texto={erroApi} />}
          <Button title="Concluir" onPress={() => concluir(false)} loading={enviando} fullWidth />
        </View>
      )}
    </Janela>
  );
}

/** T10 · Detalhe do item: status, prazo, passos e histórico (E14, E17, E19). */
export function ItemDetailScreen({ route, navigation }: Props) {
  const { itemId } = route.params;
  const item = useCarregar(() => itensApi.obter(itemId), [itemId]);
  const [concluindo, setConcluindo] = useState(false);
  const [excluindo, setExcluindo] = useState(false);
  const [erroAcao, setErroAcao] = useState<string | null>(null);

  if (item.erro != null) return <Tela titulo="Item" voltar><ErroTela erro={item.erro} tentarDeNovo={item.recarregar} /></Tela>;
  if (!item.dados) return <Carregando />;
  const i = item.dados;
  const encerrado = i.situacao === 'Encerrado';
  const feitos = i.passos.filter((p) => p.concluido).length;

  // Atualização otimista: marca na hora e volta atrás se a API falhar (spec §5.6)
  const marcar = async (passoId: string, concluido: boolean) => {
    const anterior = i;
    const passos = i.passos.map((p) => (p.id === passoId ? { ...p, concluido } : p));
    item.setDados({ ...i, passos, progressoPassos: passos.filter((p) => p.concluido).length / passos.length });
    setErroAcao(null);
    try {
      await itensApi.marcarPasso(i.id, passoId, concluido);
    } catch (err) {
      item.setDados(anterior);
      setErroAcao(mensagemDeErro(err));
    }
  };

  const excluir = async () => {
    try {
      await itensApi.excluir(i.id);
      navigation.goBack();
    } catch (err) {
      setExcluindo(false);
      setErroAcao(mensagemDeErro(err));
    }
  };

  return (
    <Tela titulo={i.nome} voltar>
      <View style={[estilos.cartao, { borderLeftWidth: 5, borderLeftColor: corStatus(i.status), flexDirection: 'row', alignItems: 'center', gap: spacing.md }]}>
        <StatusBadge status={i.status} />
        <Text style={estilos.subtitulo}>{i.textoPrazo}</Text>
      </View>

      <View style={estilos.cartao}>
        <Linha rotulo="Área" valor={nomesCategorias[i.categoriaId]} />
        {!encerrado && (
          <Linha rotulo="Próximo vencimento"
                 valor={`${i.proximoVencimentoKm != null ? `${km(i.proximoVencimentoKm)} ou ` : ''}${dataBr(i.proximoVencimentoData)}`} />
        )}
        {i.veiculo && <Linha rotulo="Veículo" valor={i.veiculo.apelido} />}
        <Linha rotulo="Aviso" valor={`${i.antecedenciaDias} dias antes`} />
        {i.observacao && <Linha rotulo="Observação" valor={i.observacao} />}
      </View>

      {i.passos.length > 0 && (
        <View style={estilos.cartao}>
          <Text style={estilos.subtitulo}>Passos  <Text style={estilos.textoApoio}>{feitos} de {i.passos.length}</Text></Text>
          <Progresso valor={i.progressoPassos} />
          {i.passos.map((p) => (
            <Pressable key={p.id} disabled={encerrado} onPress={() => marcar(p.id, !p.concluido)}
                       accessibilityRole="checkbox" accessibilityState={{ checked: p.concluido }}
                       style={{ flexDirection: 'row', alignItems: 'center', gap: spacing.md, minHeight: 44 }}>
              {p.concluido ? <CheckSquare size={22} color={colors.primary} /> : <Square size={22} color={colors.textMuted} />}
              <Text style={{ color: p.concluido ? colors.textMuted : colors.textPrimary, textDecorationLine: p.concluido ? 'line-through' : 'none' }}>
                {p.descricao}
              </Text>
            </Pressable>
          ))}
        </View>
      )}

      {erroAcao && <Alerta texto={erroAcao} />}
      {!encerrado && <Button title="Concluir" onPress={() => setConcluindo(true)} fullWidth size="lg" />}

      <View style={estilos.cartao}>
        <Text style={estilos.subtitulo}>Histórico</Text>
        {i.historico.length === 0 ? <Text style={estilos.textoApoio}>Ainda não foi concluído.</Text>
          : i.historico.map((h) => (
            <Text key={h.id} style={estilos.textoApoio}>
              {dataBr(h.concluidoEm)}{h.km != null ? ` · ${km(h.km)}` : ''}{h.valorGasto != null ? ` · ${dinheiro(h.valorGasto)}` : ''}
            </Text>
          ))}
      </View>

      <View style={{ flexDirection: 'row', gap: spacing.sm }}>
        {!encerrado && <Button title="Editar" variant="outline" fullWidth={false} onPress={() => navigation.navigate('ItemForm', { itemId: i.id })} style={{ flex: 1 }} />}
        <Button title="Excluir" variant="danger" fullWidth={false} onPress={() => setExcluindo(true)} style={{ flex: 1 }} />
      </View>

      <ConcluirJanela key={concluindo ? 'aberta' : 'fechada'} item={i} aberta={concluindo}
                      aoFechar={() => { setConcluindo(false); void item.recarregar(); }}
                      aoConcluir={() => { setConcluindo(false); navigation.navigate('MainTabs', { screen: 'RadarTab' }); }} />

      <Janela aberta={excluindo} titulo="Excluir este item?" aoFechar={() => setExcluindo(false)}>
        <Text style={[estilos.textoApoio, { marginBottom: spacing.md }]}>Os passos e o histórico deste item também serão apagados.</Text>
        <Button title="Excluir" variant="danger" onPress={excluir} fullWidth />
      </Janela>
    </Tela>
  );
}

function Linha({ rotulo, valor }: { rotulo: string; valor: string }) {
  return (
    <View style={{ flexDirection: 'row', justifyContent: 'space-between', gap: spacing.md, paddingVertical: 2 }}>
      <Text style={estilos.textoApoio}>{rotulo}</Text>
      <Text style={[typography.bodyBold, { color: colors.textPrimary, flexShrink: 1, textAlign: 'right' }]}>{valor}</Text>
    </View>
  );
}
