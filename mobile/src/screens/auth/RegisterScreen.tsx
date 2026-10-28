import React, { useState } from 'react';
import { Pressable, Switch, Text, View } from 'react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '../../navigation/types';
import { colors } from '../../theme';
import { Input } from '../../components/Input';
import { Button } from '../../components/Button';
import { Alerta, Tela, estilos } from '../../components/ui';
import { useAuth } from '../../context/AuthContext';
import { errosPorCampo, mensagemDeErro } from '../../api/client';
import { cadastroSchema, errosDe, type Erros } from '../../utils/validacao';

type Props = NativeStackScreenProps<RootStackParamList, 'Register'>;

/** T02 · Criar conta (E01) e entrar em seguida (E02). */
export function RegisterScreen({ navigation }: Props) {
  const { registrar } = useAuth();
  const [form, setForm] = useState({ nome: '', email: '', senha: '', confirmarSenha: '' });
  const [mostrar, setMostrar] = useState(false);
  const [erros, setErros] = useState<Erros>({});
  const [erroApi, setErroApi] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);
  const campo = (k: keyof typeof form) => (v: string) => setForm((f) => ({ ...f, [k]: v }));

  const enviar = async () => {
    const e = errosDe(cadastroSchema.safeParse(form));
    setErros(e);
    setErroApi(Object.keys(e).length ? 'Confira os campos destacados em vermelho acima.' : null);
    if (Object.keys(e).length) return;
    setEnviando(true);
    try {
      await registrar(form.nome, form.email, form.senha);
    } catch (err) {
      const porCampo = errosPorCampo(err);
      if (Object.keys(porCampo).length) setErros(porCampo);
      else setErroApi(mensagemDeErro(err));
    } finally {
      setEnviando(false);
    }
  };

  return (
    <Tela titulo="Criar conta" voltar>
        <Text style={estilos.textoApoio}>Leva menos de um minuto.</Text>
        <Input label="Como quer ser chamado?" value={form.nome} onChangeText={campo('nome')} error={erros.nome} />
        <Input label="E-mail" value={form.email} onChangeText={campo('email')} keyboardType="email-address"
               autoCapitalize="none" error={erros.email} />
        <Input label="Senha" value={form.senha} onChangeText={campo('senha')} secureTextEntry={!mostrar}
               hint="Pelo menos 8 caracteres, com letras e números." error={erros.senha} />
        <Input label="Confirmar senha" value={form.confirmarSenha} onChangeText={campo('confirmarSenha')}
               secureTextEntry={!mostrar} error={erros.confirmarSenha} />
        <View style={{ flexDirection: 'row', alignItems: 'center', gap: 8, minHeight: 44 }}>
          <Switch value={mostrar} onValueChange={setMostrar} accessibilityLabel="Mostrar senha" />
          <Text style={{ color: colors.textSecondary }}>Mostrar senha</Text>
        </View>
        {erroApi && <Alerta texto={erroApi} />}
        <Button title="Criar conta" onPress={enviar} loading={enviando} fullWidth />
        <Pressable onPress={() => navigation.navigate('Login')} style={{ minHeight: 44, justifyContent: 'center' }}>
          <Text style={{ color: colors.primary, textAlign: 'center' }}>Já tem conta? Entrar</Text>
        </Pressable>
    </Tela>
  );
}
