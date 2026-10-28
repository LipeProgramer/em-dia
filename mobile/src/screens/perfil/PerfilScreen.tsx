import React, { useEffect, useState } from 'react';
import { Pressable, Switch, Text, View } from 'react-native';
import { useNavigation } from '@react-navigation/native';
import type { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { Car, ChevronRight, Info, Sparkles } from 'lucide-react-native';
import type { RootStackParamList } from '../../navigation/types';
import type { Preferencias } from '../../api/types';
import { usuariosApi } from '../../api/services';
import { API_URL, errosPorCampo, mensagemDeErro } from '../../api/client';
import { useAuth } from '../../context/AuthContext';
import { Button } from '../../components/Button';
import { Input } from '../../components/Input';
import { Alerta, Carregando, ErroTela, Janela, Tela, estilos, useCarregar } from '../../components/ui';
import { inteiro, type Erros } from '../../utils/validacao';
import { colors, spacing } from '../../theme';

/** Preferências de aviso (E06): reavisos 0 a 5; intervalo 1 a 7 dias. */
function PreferenciasForm({ inicial }: { inicial: Preferencias }) {
  const [p, setP] = useState({ ...inicial, maxReavisos: String(inicial.maxReavisos), intervaloReavisoDias: String(inicial.intervaloReavisoDias) });
  const [erros, setErros] = useState<Erros>({});
  const [aviso, setAviso] = useState<{ texto: string; tipo: 'erro' | 'sucesso' } | null>(null);
  const [salvando, setSalvando] = useState(false);
  useEffect(() => setAviso(null), [p]);

  const salvar = async () => {
    const e: Erros = {};
    const max = inteiro(p.maxReavisos);
    const intervalo = inteiro(p.intervaloReavisoDias);
    if (max === undefined || Number.isNaN(max) || max < 0 || max > 5) e.maxReavisos = 'Escolha entre 0 e 5 reavisos.';
    if (intervalo === undefined || Number.isNaN(intervalo) || intervalo < 1 || intervalo > 7) e.intervaloReavisoDias = 'Escolha entre 1 e 7 dias.';
    setErros(e);
    if (Object.keys(e).length) return;
    setSalvando(true);
    try {
      await usuariosApi.atualizarPreferencias({ receberEmail: p.receberEmail, maxReavisos: max!, intervaloReavisoDias: intervalo!, modoFocoPadrao: p.modoFocoPadrao });
      setAviso({ texto: 'Preferências salvas.', tipo: 'sucesso' });
    } catch (err) {
      const porCampo = errosPorCampo(err);
      if (Object.keys(porCampo).length) setErros(porCampo);
      else setAviso({ texto: mensagemDeErro(err), tipo: 'erro' });
    } finally {
      setSalvando(false);
    }
  };

  return (
    <View style={estilos.cartao}>
      <Text style={estilos.subtitulo}>Avisos</Text>
      <Interruptor rotulo="Receber avisos por e-mail" valor={p.receberEmail} aoMudar={(v) => setP({ ...p, receberEmail: v })} />
      <Input label="Quantos reavisos, no máximo?" keyboardType="number-pad" value={p.maxReavisos} error={erros.maxReavisos}
             onChangeText={(v) => setP({ ...p, maxReavisos: v })} />
      <Input label="Intervalo entre reavisos (dias)" keyboardType="number-pad" value={p.intervaloReavisoDias} error={erros.intervaloReavisoDias}
             onChangeText={(v) => setP({ ...p, intervaloReavisoDias: v })} />
      <Interruptor rotulo="Abrir o Radar já no modo foco" valor={p.modoFocoPadrao} aoMudar={(v) => setP({ ...p, modoFocoPadrao: v })} />
      {aviso && <Alerta texto={aviso.texto} tipo={aviso.tipo} />}
      <Button title="Salvar preferências" variant="outline" onPress={salvar} loading={salvando} />
    </View>
  );
}

function Interruptor({ rotulo, valor, aoMudar }: { rotulo: string; valor: boolean; aoMudar: (v: boolean) => void }) {
  return (
    <View style={{ flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', minHeight: 44 }}>
      <Text style={{ color: colors.textSecondary, flex: 1 }}>{rotulo}</Text>
      <Switch value={valor} onValueChange={aoMudar} accessibilityLabel={rotulo} />
    </View>
  );
}

/** T14 · Perfil: dados, preferências, refazer check-up, veículos, sair e excluir conta (E04–E07). */
export function PerfilScreen() {
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>();
  const { sair, encerrarLocalmente } = useAuth();
  const me = useCarregar(() => usuariosApi.me());
  const [excluindo, setExcluindo] = useState(false);
  const [senha, setSenha] = useState('');
  const [erroExclusao, setErroExclusao] = useState<string | null>(null);

  const excluirConta = async () => {
    if (!senha) {
      setErroExclusao('Digite sua senha para confirmar.');
      return;
    }
    try {
      await usuariosApi.excluirConta(senha);
      await encerrarLocalmente();
    } catch (err) {
      setErroExclusao(mensagemDeErro(err));
    }
  };

  return (
    <Tela titulo="Perfil">
      {me.carregando && !me.dados && <Carregando />}
      {me.erro != null && <ErroTela erro={me.erro} tentarDeNovo={me.recarregar} />}
      {me.dados && (
        <>
          <View style={estilos.cartao}>
            <Text style={estilos.subtitulo}>{me.dados.nome}</Text>
            <Text style={estilos.textoApoio}>{me.dados.email}</Text>
          </View>

          <View style={[estilos.cartao, { gap: 0, paddingVertical: spacing.xs }]}>
            <ItemMenu icone={<Sparkles size={18} color={colors.primary} />} rotulo="Refazer o check-up" aoTocar={() => navigation.navigate('Checkup')} />
            <ItemMenu icone={<Car size={18} color={colors.primary} />} rotulo="Meus veículos" aoTocar={() => navigation.navigate('Veiculos')} />
            <ItemMenu icone={<Info size={18} color={colors.primary} />} rotulo="Sobre o Em Dia" aoTocar={() => navigation.navigate('Sobre')} />
          </View>

          <PreferenciasForm key={me.dados.id} inicial={me.dados.preferencias} />

          <Button title="Sair" variant="outline" onPress={sair} fullWidth />
          <Button title="Excluir conta" variant="ghost" textStyle={{ color: colors.error }} onPress={() => setExcluindo(true)} fullWidth />
          <Text style={[estilos.textoApoio, { textAlign: 'center' }]}>Servidor: {API_URL}</Text>
        </>
      )}

      <Janela aberta={excluindo} titulo="Excluir sua conta?" aoFechar={() => { setExcluindo(false); setErroExclusao(null); }}>
        <Text style={[estilos.textoApoio, { marginBottom: spacing.md }]}>
          Isso apaga todos os seus dados: itens, veículos, histórico e avisos. Não dá para desfazer.
        </Text>
        <Input label="Digite sua senha para confirmar" value={senha} onChangeText={setSenha} secureTextEntry />
        {erroExclusao && <Alerta texto={erroExclusao} />}
        <Button title="Excluir minha conta" variant="danger" onPress={excluirConta} fullWidth style={{ marginTop: spacing.md }} />
      </Janela>
    </Tela>
  );
}

function ItemMenu({ icone, rotulo, aoTocar }: { icone: React.ReactNode; rotulo: string; aoTocar: () => void }) {
  return (
    <Pressable onPress={aoTocar} accessibilityRole="button"
               style={{ flexDirection: 'row', alignItems: 'center', gap: spacing.md, minHeight: 52 }}>
      {icone}
      <Text style={{ color: colors.textPrimary, flex: 1 }}>{rotulo}</Text>
      <ChevronRight size={18} color={colors.textMuted} />
    </Pressable>
  );
}
