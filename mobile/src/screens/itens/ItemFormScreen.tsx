import React, { useEffect, useState } from 'react';
import { Pressable, Text, View } from 'react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import { Plus, Trash2 } from 'lucide-react-native';
import type { RootStackParamList } from '../../navigation/types';
import type { ItemDetalhe, Modelo, SalvarItemRequest, TipoVencimento, Veiculo } from '../../api/types';
import { catalogoApi, itensApi, veiculosApi } from '../../api/services';
import { errosPorCampo, mensagemDeErro } from '../../api/client';
import { Button } from '../../components/Button';
import { Input } from '../../components/Input';
import { FilterPills } from '../../components/FilterPills';
import { Alerta, CampoData, Carregando, ErroTela, Escolha, Tela, estilos } from '../../components/ui';
import { brParaIso, dataBr, nomesCategorias, tiposVencimento } from '../../utils/formato';
import { inteiro, validarItem, type Erros, type ItemTexto } from '../../utils/validacao';
import { colors, spacing } from '../../theme';

type Props = NativeStackScreenProps<RootStackParamList, 'ItemForm'>;

const vazio: ItemTexto = {
  nome: '', categoriaId: null, tipoVencimento: 'RecorrentePorTempo', dataBase: '', dataVencimento: '', intervaloDias: '',
  intervaloKm: '', kmBase: '', garantiaFabricanteMeses: '', veiculoId: null, antecedenciaDias: '30', passos: [],
};

const txt = (n: number | null | undefined) => (n == null ? '' : String(n));

const deModelo = (m: Modelo): ItemTexto => ({
  ...vazio, nome: m.nome, categoriaId: m.categoriaId, tipoVencimento: m.tipoVencimento, intervaloDias: txt(m.intervaloDias),
  intervaloKm: txt(m.intervaloKm), garantiaFabricanteMeses: txt(m.garantiaFabricanteMeses), antecedenciaDias: String(m.antecedenciaDias),
  passos: [...m.passos],
});

const deItem = (i: ItemDetalhe): ItemTexto => ({
  nome: i.nome, categoriaId: i.categoriaId, tipoVencimento: i.tipoVencimento,
  dataBase: i.dataBase ? dataBr(i.dataBase) : '', dataVencimento: i.dataVencimento ? dataBr(i.dataVencimento) : '',
  intervaloDias: txt(i.intervaloDias), intervaloKm: txt(i.intervaloKm), kmBase: txt(i.kmBase),
  garantiaFabricanteMeses: txt(i.garantiaFabricanteMeses), veiculoId: i.veiculo?.id ?? null,
  antecedenciaDias: String(i.antecedenciaDias), passos: i.passos.map((p) => p.descricao),
});

/** Corpo do E15/E16 com só os campos do tipo escolhido. */
function paraRequest(v: ItemTexto, modeloId?: string): SalvarItemRequest {
  const t = v.tipoVencimento;
  return {
    nome: v.nome.trim(), categoriaId: v.categoriaId!, modeloId: modeloId ?? null, tipoVencimento: t,
    dataVencimento: t === 'DataUnica' ? brParaIso(v.dataVencimento) : null,
    dataBase: t !== 'DataUnica' ? brParaIso(v.dataBase) : null,
    intervaloDias: t === 'RecorrentePorTempo' || t === 'UsoOuTempo' ? inteiro(v.intervaloDias) : null,
    intervaloKm: t === 'UsoOuTempo' ? inteiro(v.intervaloKm) : null,
    kmBase: t === 'UsoOuTempo' ? inteiro(v.kmBase) : null,
    veiculoId: t === 'UsoOuTempo' ? v.veiculoId : null,
    garantiaFabricanteMeses: t === 'Calculado' ? inteiro(v.garantiaFabricanteMeses) : null,
    antecedenciaDias: inteiro(v.antecedenciaDias) ?? 0,
    passos: v.passos.map((p) => p.trim()),
  };
}

/** T09 · Novo item (modelo pronto ou do zero) e T11 · Editar item (E08, E09, E14, E15, E16, E20). */
export function ItemFormScreen({ route, navigation }: Props) {
  const itemId = route.params?.itemId;
  const [modelos, setModelos] = useState<Modelo[] | null>(null);
  const [veiculos, setVeiculos] = useState<Veiculo[]>([]);
  const [erroCarga, setErroCarga] = useState<unknown>(null);
  const [categoriaModelos, setCategoriaModelos] = useState(1);
  const [modelo, setModelo] = useState<Modelo | null | undefined>(itemId ? null : undefined); // undefined = escolhendo
  const [form, setForm] = useState<ItemTexto | null>(itemId ? null : vazio);
  const [erros, setErros] = useState<Erros>({});
  const [erroApi, setErroApi] = useState<string | null>(null);
  const [salvando, setSalvando] = useState(false);

  const carregar = () => {
    setErroCarga(null);
    Promise.all([
      catalogoApi.modelos().then(setModelos),
      veiculosApi.listar().then(setVeiculos),
      itemId ? itensApi.obter(itemId).then((i) => setForm(deItem(i))) : Promise.resolve(),
    ]).catch(setErroCarga);
  };
  useEffect(carregar, [itemId]);

  if (erroCarga) return <Tela titulo="Item" voltar><ErroTela erro={erroCarga} tentarDeNovo={carregar} /></Tela>;
  if (!modelos || !form) return <Carregando />;

  // Passo 1 (novo): escolher um modelo pronto ou criar do zero
  if (modelo === undefined) {
    return (
      <Tela titulo="Novo item" voltar>
        <FilterPills selectedId={categoriaModelos} onSelect={setCategoriaModelos}
                     options={Object.entries(nomesCategorias).map(([id, nome]) => ({ id: Number(id), label: nome }))} />
        {modelos.filter((m) => m.categoriaId === categoriaModelos).map((m) => (
          <Pressable key={m.id} onPress={() => { setModelo(m); setForm(deModelo(m)); }} accessibilityRole="button" style={estilos.cartao}>
            <Text style={estilos.subtitulo}>{m.nome}</Text>
            <Text style={estilos.textoApoio}>{tiposVencimento[m.tipoVencimento]}</Text>
          </Pressable>
        ))}
        <Button title="Criar do zero" variant="outline" fullWidth
                onPress={() => { setModelo(null); setForm({ ...vazio, categoriaId: categoriaModelos }); }} />
      </Tela>
    );
  }

  const mudar = <K extends keyof ItemTexto>(k: K, v: ItemTexto[K]) => setForm((f) => ({ ...f!, [k]: v }));
  const t = form.tipoVencimento;

  const salvar = async () => {
    const e = validarItem(form);
    setErros(e);
    setErroApi(Object.keys(e).length ? 'Confira os campos destacados em vermelho acima.' : null);
    if (Object.keys(e).length) return;
    setSalvando(true);
    try {
      if (itemId) {
        await itensApi.editar(itemId, paraRequest(form));
        navigation.goBack();
      } else {
        const criado = await itensApi.criar(paraRequest(form, modelo?.id));
        navigation.replace('ItemDetail', { itemId: criado.id });
      }
    } catch (err) {
      const porCampo = errosPorCampo(err);
      if (Object.keys(porCampo).length) setErros(porCampo);
      else setErroApi(mensagemDeErro(err));
    } finally {
      setSalvando(false);
    }
  };

  return (
    <Tela titulo={itemId ? 'Editar item' : modelo ? modelo.nome : 'Novo item'} voltar>
      {!itemId && <Button title="Escolher outro modelo" variant="ghost" onPress={() => setModelo(undefined)} />}
      <Input label="Nome" value={form.nome} onChangeText={(v) => mudar('nome', v)} error={erros.nome} maxLength={80} />
      <Escolha label="Categoria" valor={form.categoriaId} aoEscolher={(v) => mudar('categoriaId', v)} erro={erros.categoriaId}
               opcoes={Object.entries(nomesCategorias).map(([id, nome]) => ({ id: Number(id), rotulo: nome }))} />
      <Escolha<TipoVencimento> label="Tipo de vencimento" valor={t} aoEscolher={(v) => mudar('tipoVencimento', v)}
               opcoes={Object.entries(tiposVencimento).map(([id, rotulo]) => ({ id: id as TipoVencimento, rotulo }))} />

      {t === 'DataUnica' && <CampoData label="Data de vencimento" valor={form.dataVencimento} aoMudar={(v) => mudar('dataVencimento', v)} erro={erros.dataVencimento} />}
      {t === 'UsoOuTempo' && (
        veiculos.length === 0
          ? <Alerta texto="Cadastre um veículo em Perfil › Meus veículos para usar este tipo." />
          : <Escolha label="Veículo" valor={form.veiculoId} aoEscolher={(v) => mudar('veiculoId', v)} erro={erros.veiculoId}
                     opcoes={veiculos.map((v) => ({ id: v.id, rotulo: v.apelido }))} />
      )}
      {t !== 'DataUnica' && (
        <CampoData label={t === 'Calculado' ? 'Data da compra' : 'Data da última vez'} valor={form.dataBase}
                   aoMudar={(v) => mudar('dataBase', v)} erro={erros.dataBase} />
      )}
      {t === 'UsoOuTempo' && (
        <>
          <Input label="Km na última vez" keyboardType="number-pad" value={form.kmBase} onChangeText={(v) => mudar('kmBase', v)} error={erros.kmBase} />
          <Input label="Intervalo em km" keyboardType="number-pad" value={form.intervaloKm} onChangeText={(v) => mudar('intervaloKm', v)} error={erros.intervaloKm} />
        </>
      )}
      {(t === 'RecorrentePorTempo' || t === 'UsoOuTempo') && (
        <Input label="Intervalo em dias" keyboardType="number-pad" value={form.intervaloDias} onChangeText={(v) => mudar('intervaloDias', v)} error={erros.intervaloDias} />
      )}
      {t === 'Calculado' && (
        <Input label="Garantia do fabricante (meses)" keyboardType="number-pad" value={form.garantiaFabricanteMeses}
               hint="Somamos os 90 dias da garantia legal." onChangeText={(v) => mudar('garantiaFabricanteMeses', v)} error={erros.garantiaFabricanteMeses} />
      )}
      <Input label="Avisar com quantos dias de antecedência?" keyboardType="number-pad" value={form.antecedenciaDias}
             onChangeText={(v) => mudar('antecedenciaDias', v)} error={erros.antecedenciaDias} />

      <View style={estilos.cartao}>
        <Text style={estilos.subtitulo}>Passos pequenos</Text>
        {form.passos.map((p, i) => (
          <View key={i} style={{ flexDirection: 'row', alignItems: 'center', gap: spacing.sm }}>
            <View style={{ flex: 1 }}>
              <Input label={`Passo ${i + 1}`} value={p} maxLength={120}
                     onChangeText={(v) => mudar('passos', form.passos.map((x, j) => (j === i ? v : x)))} />
            </View>
            <Pressable onPress={() => mudar('passos', form.passos.filter((_, j) => j !== i))} style={estilos.botaoIcone}
                       accessibilityRole="button" accessibilityLabel={`Remover passo ${i + 1}`}>
              <Trash2 size={20} color={colors.error} />
            </Pressable>
          </View>
        ))}
        {erros.passos && <Text style={estilos.erro}>{erros.passos}</Text>}
        {form.passos.length < 10 && (
          <Button title="Adicionar passo" variant="ghost" icon={<Plus size={18} color={colors.primary} />}
                  onPress={() => mudar('passos', [...form.passos, ''])} />
        )}
      </View>

      {erroApi && <Alerta texto={erroApi} />}
      <Button title={itemId ? 'Salvar alterações' : 'Salvar item'} onPress={salvar} loading={salvando} fullWidth />
    </Tela>
  );
}
