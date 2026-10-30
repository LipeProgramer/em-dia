import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import type { z } from 'zod';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { mensagemDeErro } from '../../api/client.ts';
import { Alerta, Button, Field } from '../../components/ui.tsx';
import { VIDEO_DEMONSTRACAO } from '../../utils/links.ts';
import { loginSchema } from '../itens/schemas.ts';
import { useSession } from './useSession.tsx';
import { AuthLayout } from './AuthLayout.tsx';

type Form = z.infer<typeof loginSchema>;

/** T01 · Entrar (E02). Depois do login, vai para o check-up se precisar; senão, volta para onde estava. */
export function LoginPage() {
  const { entrar } = useSession();
  const navigate = useNavigate();
  const location = useLocation();
  const [erroApi, setErroApi] = useState<string | null>(null);
  // Aviso vindo de outra tela (ex.: "Senha alterada" depois de redefinir a senha)
  const aviso = (location.state as { aviso?: string } | null)?.aviso;
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<Form>({
    resolver: zodResolver(loginSchema),
  });

  const enviar = handleSubmit(async ({ email, senha }) => {
    setErroApi(null);
    try {
      const usuario = await entrar(email, senha);
      const destino = (location.state as { de?: string } | null)?.de ?? '/';
      navigate(usuario.precisaCheckup ? '/checkup' : destino, { replace: true });
    } catch (e) {
      setErroApi(mensagemDeErro(e));
    }
  });

  return (
    <AuthLayout titulo="Entrar" subtitulo="Seus prazos, sem sustos.">
      <form onSubmit={enviar} noValidate className="form">
        <Field label="E-mail" type="email" autoComplete="email" erro={errors.email?.message} {...register('email')} />
        <Field label="Senha" type="password" autoComplete="current-password" erro={errors.senha?.message} {...register('senha')} />
        <Link className="auth-esqueci" to="/esqueci-senha">Esqueci minha senha</Link>
        {aviso && !erroApi && <Alerta tipo="sucesso">{aviso}</Alerta>}
        {erroApi && <Alerta>{erroApi}</Alerta>}
        <Button type="submit" carregando={isSubmitting}>Entrar</Button>
      </form>
      <p className="auth-troca">Ainda não tem conta? <Link to="/cadastro">Criar conta</Link></p>
      <p className="auth-troca">
        <a href={VIDEO_DEMONSTRACAO} target="_blank" rel="noopener noreferrer">Ver vídeo da demonstração</a>
      </p>
    </AuthLayout>
  );
}
