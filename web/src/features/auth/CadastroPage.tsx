import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import type { z } from 'zod';
import { Link, useNavigate } from 'react-router-dom';
import { mensagemDeErro } from '../../api/client.ts';
import { authApi } from '../../api/endpoints.ts';
import { Alerta, Button, Field } from '../../components/ui.tsx';
import { aplicarErrosDaApi } from '../../utils/erros.ts';
import { cadastroSchema } from '../itens/schemas.ts';
import { useSession } from './useSession.tsx';
import { AuthLayout } from './AuthLayout.tsx';

type Form = z.infer<typeof cadastroSchema>;

/** T02 · Criar conta (E01) e entrar em seguida (E02). */
export function CadastroPage() {
  const { entrar } = useSession();
  const navigate = useNavigate();
  const [mostrarSenha, setMostrarSenha] = useState(false);
  const [erroApi, setErroApi] = useState<string | null>(null);
  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<Form>({
    resolver: zodResolver(cadastroSchema),
  });

  const enviar = handleSubmit(async ({ nome, email, senha }) => {
    setErroApi(null);
    try {
      await authApi.registrar({ nome, email, senha });
      await entrar(email, senha);
      navigate('/checkup', { replace: true });
    } catch (e) {
      if (!aplicarErrosDaApi(e, setError, ['nome', 'email', 'senha'])) setErroApi(mensagemDeErro(e));
    }
  });

  const tipoSenha = mostrarSenha ? 'text' : 'password';

  return (
    <AuthLayout titulo="Criar conta" subtitulo="Leva menos de um minuto.">
      <form onSubmit={enviar} noValidate className="form">
        <Field label="Como quer ser chamado?" autoComplete="given-name" erro={errors.nome?.message} {...register('nome')} />
        <Field label="E-mail" type="email" autoComplete="email" erro={errors.email?.message} {...register('email')} />
        <Field label="Senha" type={tipoSenha} autoComplete="new-password" ajuda="Pelo menos 8 caracteres, com letras e números."
               erro={errors.senha?.message} {...register('senha')} />
        <Field label="Confirmar senha" type={tipoSenha} autoComplete="new-password"
               erro={errors.confirmarSenha?.message} {...register('confirmarSenha')} />
        <label className="check">
          <input type="checkbox" checked={mostrarSenha} onChange={(e) => setMostrarSenha(e.target.checked)} />
          Mostrar senha
        </label>
        {erroApi && <Alerta>{erroApi}</Alerta>}
        <Button type="submit" carregando={isSubmitting}>Criar conta</Button>
      </form>
      <p className="auth-troca">Já tem conta? <Link to="/entrar">Entrar</Link></p>
    </AuthLayout>
  );
}
