import React, { useState } from 'react';
import { KeyboardAvoidingView, Platform, Pressable, Text } from 'react-native';
import type { NativeStackScreenProps } from '@react-navigation/native-stack';
import type { RootStackParamList } from '../../navigation/types';
import { colors } from '../../theme';
import { Input } from '../../components/Input';
import { Button } from '../../components/Button';
import { Alerta, Tela, estilos } from '../../components/ui';
import { useAuth } from '../../context/AuthContext';
import { mensagemDeErro } from '../../api/client';
import { errosDe, loginSchema, type Erros } from '../../utils/validacao';

type Props = NativeStackScreenProps<RootStackParamList, 'Login'>;

/** T01 · Entrar (E02). Depois do login, o portão leva ao check-up se ainda não foi feito. */
export function LoginScreen({ navigation }: Props) {
  const { entrar } = useAuth();
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [erros, setErros] = useState<Erros>({});
  const [erroApi, setErroApi] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  const enviar = async () => {
    const e = errosDe(loginSchema.safeParse({ email, senha }));
    setErros(e);
    setErroApi(null);
    if (Object.keys(e).length) return;
    setEnviando(true);
    try {
      await entrar(email, senha);
    } catch (err) {
      setErroApi(mensagemDeErro(err));
    } finally {
      setEnviando(false);
    }
  };

  return (
    <KeyboardAvoidingView style={{ flex: 1 }} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
      <Tela titulo="Entrar" voltar>
        <Text style={estilos.textoApoio}>Seus prazos, sem sustos.</Text>
        <Input label="E-mail" value={email} onChangeText={setEmail} keyboardType="email-address" autoCapitalize="none"
               autoComplete="email" error={erros.email} />
        <Input label="Senha" value={senha} onChangeText={setSenha} secureTextEntry autoComplete="password" error={erros.senha} />
        {erroApi && <Alerta texto={erroApi} />}
        <Button title="Entrar" onPress={enviar} loading={enviando} fullWidth />
        <Pressable onPress={() => navigation.navigate('Register')} style={{ minHeight: 44, justifyContent: 'center' }}>
          <Text style={{ color: colors.primary, textAlign: 'center' }}>Ainda não tem conta? Criar conta</Text>
        </Pressable>
      </Tela>
    </KeyboardAvoidingView>
  );
}
