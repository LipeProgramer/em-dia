import React, { useState } from 'react';
import { Text, View } from 'react-native';
import { Bike, Car, Gauge, Plus } from 'lucide-react-native';
import type { AtualizarKmResponse, TipoVeiculo, Veiculo } from '../../api/types';
import { veiculosApi } from '../../api/services';
import { errosPorCampo, mensagemDeErro } from '../../api/client';
import { Button } from '../../components/Button';
import { Input } from '../../components/Input';
import { Alerta, Carregando, ErroTela, Escolha, Janela, StatusBadge, Tela, Vazio, estilos, useCarregar } from '../../components/ui';
import { km } from '../../utils/formato';
import { inteiro, validarVeiculo, type Erros } from '../../utils/validacao';
import { colors, spacing } from '../../theme';

/** Cadastrar ou editar veículo (E21/E22). */
function VeiculoForm({ veiculo, aoSalvar }: { veiculo?: Veiculo; aoSalvar: () => void }) {
  const [form, setForm] = useState({
    apelido: veiculo?.apelido ?? '', tipo: (veiculo?.tipo ?? 'Carro') as TipoVeiculo,
    kmAtual: veiculo ? String(veiculo.kmAtual) : '', mediaKmMes: String(veiculo?.mediaKmMes ?? 1000),
  });
  const [erros, setErros] = useState<Erros>({});
  const [erroApi, setErroApi] = useState<string | null>(null);
  const [salvando, setSalvando] = useState(false);

  const salvar = async () => {
    const e = validarVeiculo(form, !veiculo);
    setErros(e);
    setErroApi(Object.keys(e).length ? 'Confira os campos destacados em vermelho acima.' : null);
    if (Object.keys(e).length) return;
    setSalvando(true);
    try {
      const dados = { apelido: form.apelido.trim(), tipo: form.tipo, mediaKmMes: inteiro(form.mediaKmMes)! };
      if (veiculo) await veiculosApi.editar(veiculo.id, dados);
      else await veiculosApi.criar({ ...dados, kmAtual: inteiro(form.kmAtual)! });
      aoSalvar();
    } catch (err) {
      const porCampo = errosPorCampo(err);
      if (Object.keys(porCampo).length) setErros(porCampo);
      else setErroApi(mensagemDeErro(err));
    } finally {
      setSalvando(false);
    }
  };

  return (
    <View>
      <Input label="Apelido" placeholder="Ex.: Gol prata" value={form.apelido} onChangeText={(v) => setForm({ ...form, apelido: v })} error={erros.apelido} />
      <Escolha label="Tipo" valor={form.tipo} aoEscolher={(v) => setForm({ ...form, tipo: v })}
               opcoes={[{ id: 'Carro', rotulo: 'Carro' }, { id: 'Moto', rotulo: 'Moto' }]} />
      {!veiculo && <Input label="Km atual" keyboardType="number-pad" value={form.kmAtual} onChangeText={(v) => setForm({ ...form, kmAtual: v })} error={erros.kmAtual} />}
      <Input label="Média de km por mês" keyboardType="number-pad" value={form.mediaKmMes} onChangeText={(v) => setForm({ ...form, mediaKmMes: v })} error={erros.mediaKmMes} />
      {erroApi && <Alerta texto={erroApi} />}
      <Button title="Salvar" onPress={salvar} loading={salvando} fullWidth />
    </View>
  );
}

/** Atualizar km (E23 · RN07): número inteiro ≥ km atual; mostra os itens que mudaram de status. */
function AtualizarKmForm({ veiculo, aoAtualizar }: { veiculo: Veiculo; aoAtualizar: () => void }) {
  const [valor, setValor] = useState(String(veiculo.kmAtual));
  const [erro, setErro] = useState<string | null>(null);
  const [resultado, setResultado] = useState<AtualizarKmResponse | null>(null);
  const [salvando, setSalvando] = useState(false);

  const salvar = async () => {
    const n = inteiro(valor);
    if (n === undefined || Number.isNaN(n) || n < veiculo.kmAtual) {
      setErro(`A km não pode ser menor que ${veiculo.kmAtual.toLocaleString('pt-BR')}.`);
      return;
    }
    setSalvando(true);
    try {
      setResultado(await veiculosApi.atualizarKm(veiculo.id, n));
      aoAtualizar();
    } catch (err) {
      setErro(mensagemDeErro(err));
    } finally {
      setSalvando(false);
    }
  };

  if (resultado) {
    return (
      <View style={{ gap: spacing.sm }}>
        <Text style={estilos.subtitulo}>Km atualizada para {km(resultado.kmAtual)}.</Text>
        {resultado.itensAfetados.map((i) => (
          <View key={i.id} style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' }}>
            <Text style={{ color: colors.textPrimary }}>{i.nome}</Text>
            <StatusBadge status={i.status} />
          </View>
        ))}
      </View>
    );
  }

  return (
    <View>
      <Input label="Km atual" keyboardType="number-pad" value={valor} onChangeText={(v) => { setValor(v); setErro(null); }}
             hint={`Hoje está em ${km(veiculo.kmAtual)}.`} error={erro ?? undefined} />
      <Button title="Atualizar" onPress={salvar} loading={salvando} fullWidth />
    </View>
  );
}

type Janelas = { tipo: 'novo' } | { tipo: 'editar' | 'km' | 'excluir'; veiculo: Veiculo } | null;

/** T15 · Veículos: cadastrar, editar, atualizar km e excluir (E20–E24). */
export function VeiculosScreen() {
  const lista = useCarregar(() => veiculosApi.listar());
  const [janela, setJanela] = useState<Janelas>(null);
  const [erroExclusao, setErroExclusao] = useState<string | null>(null);
  const fechar = () => { setJanela(null); setErroExclusao(null); };

  const excluir = async (v: Veiculo) => {
    try {
      await veiculosApi.excluir(v.id);
      fechar();
      void lista.recarregar();
    } catch (err) {
      setErroExclusao(mensagemDeErro(err));
    }
  };

  return (
    <Tela titulo="Veículos" voltar
          acao={<Button title="Cadastrar" size="sm" fullWidth={false} icon={<Plus size={16} color="#0F172A" />} onPress={() => setJanela({ tipo: 'novo' })} />}>
      {lista.carregando && !lista.dados && <Carregando />}
      {lista.erro != null && <ErroTela erro={lista.erro} tentarDeNovo={lista.recarregar} />}
      {lista.dados?.length === 0 && <Vazio titulo="Nenhum veículo cadastrado." texto="Cadastre para acompanhar trocas por quilometragem." />}
      {lista.dados?.map((v) => (
        <View key={v.id} style={estilos.cartao}>
          <View style={{ flexDirection: 'row', alignItems: 'center', gap: spacing.sm }}>
            {v.tipo === 'Moto' ? <Bike size={20} color={colors.primary} /> : <Car size={20} color={colors.primary} />}
            <Text style={[estilos.subtitulo, { flex: 1 }]}>{v.apelido}</Text>
            <Text style={estilos.textoApoio}>{v.quantidadeItens} item(ns)</Text>
          </View>
          <Text style={estilos.textoApoio}>{km(v.kmAtual)} · média de {km(v.mediaKmMes)}/mês</Text>
          <View style={{ flexDirection: 'row', flexWrap: 'wrap', gap: spacing.sm, marginTop: spacing.sm }}>
            <Button title="Atualizar km" size="sm" variant="outline" fullWidth={false} icon={<Gauge size={16} color={colors.primary} />}
                    onPress={() => setJanela({ tipo: 'km', veiculo: v })} />
            <Button title="Editar" size="sm" variant="ghost" fullWidth={false} onPress={() => setJanela({ tipo: 'editar', veiculo: v })} />
            <Button title="Excluir" size="sm" variant="ghost" fullWidth={false} textStyle={{ color: colors.error }} onPress={() => setJanela({ tipo: 'excluir', veiculo: v })} />
          </View>
        </View>
      ))}

      <Janela aberta={janela?.tipo === 'novo' || janela?.tipo === 'editar'} titulo={janela?.tipo === 'editar' ? 'Editar veículo' : 'Cadastrar veículo'} aoFechar={fechar}>
        {(janela?.tipo === 'novo' || janela?.tipo === 'editar') && (
          <VeiculoForm veiculo={janela.tipo === 'editar' ? janela.veiculo : undefined} aoSalvar={() => { fechar(); void lista.recarregar(); }} />
        )}
      </Janela>
      <Janela aberta={janela?.tipo === 'km'} titulo="Atualizar km" aoFechar={fechar}>
        {janela?.tipo === 'km' && <AtualizarKmForm veiculo={janela.veiculo} aoAtualizar={() => void lista.recarregar()} />}
      </Janela>
      <Janela aberta={janela?.tipo === 'excluir'} titulo="Excluir veículo?" aoFechar={fechar}>
        {janela?.tipo === 'excluir' && (
          <View style={{ gap: spacing.md }}>
            <Text style={estilos.textoApoio}>O veículo {janela.veiculo.apelido} será excluído.</Text>
            {erroExclusao && <Alerta texto={erroExclusao} />}
            <Button title="Excluir" variant="danger" onPress={() => excluir(janela.veiculo)} fullWidth />
          </View>
        )}
      </Janela>
    </Tela>
  );
}
