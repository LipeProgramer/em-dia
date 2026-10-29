import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import type { z } from 'zod';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { mensagemDeErro, problema } from '../../api/client.ts';
import { authApi } from '../../api/endpoints.ts';
import { Alerta, Button, Field } from '../../components/ui.tsx';
import { esqueciSenhaSchema, redefinirSenhaSchema } from '../itens/schemas.ts';
import { AuthLayout } from './AuthLayout.tsx';

/** Extra (DV24) · Esqueci minha senha: pede o link por e-mail. A resposta é a mesma com ou sem conta. */
export function EsqueciSenhaPage() {
  const [enviadoPara, setEnviadoPara] = useState<string | null>(null);
  const [erroApi, setErroApi] = useState<string | null>(null);
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<z.infer<typeof esqueciSenhaSchema>>({
    resolver: zodResolver(esqueciSenhaSchema),
  });

  const enviar = handleSubmit(async ({ email }) => {
    setErroApi(null);
    try {
      await authApi.esqueciSenha(email);
      setEnviadoPara(email);
    } catch (e) {
      setErroApi(mensagemDeErro(e));
    }
  });

  return (
    <AuthLayout titulo="Esqueci minha senha" subtitulo="Enviamos um link para você criar uma nova.">
      {enviadoPara ? (
        <div className="form">
          <Alerta tipo="sucesso">
            Se houver uma conta com {enviadoPara}, o link chega em instantes. Ele vale 30 minutos. Confira também o spam.
          </Alerta>
        </div>
      ) : (
        <form onSubmit={enviar} noValidate className="form">
          <Field label="E-mail da conta" type="email" autoComplete="email" erro={errors.email?.message} {...register('email')} />
          {erroApi && <Alerta>{erroApi}</Alerta>}
          <Button type="submit" carregando={isSubmitting}>Enviar link</Button>
        </form>
      )}
      <p className="auth-troca">Lembrou? <Link to="/entrar">Entrar</Link></p>
    </AuthLayout>
  );
}

/** Extra (DV24) · Nova senha pelo link do e-mail (/redefinir-senha?token=…). */
export function RedefinirSenhaPage() {
  const [params] = useSearchParams();
  const token = params.get('token') ?? '';
  const navigate = useNavigate();
  const [mostrarSenha, setMostrarSenha] = useState(false);
  const [erroApi, setErroApi] = useState<string | null>(null);
  const [linkInvalido, setLinkInvalido] = useState(!token);
  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<z.infer<typeof redefinirSenhaSchema>>({
    resolver: zodResolver(redefinirSenhaSchema),
  });

  const enviar = handleSubmit(async ({ senha }) => {
    setErroApi(null);
    try {
      await authApi.redefinirSenha(token, senha);
      navigate('/entrar', { replace: true, state: { aviso: 'Senha alterada. Entre com a nova senha.' } });
    } catch (e) {
      const p = problema(e);
      const erroSenha = p?.errors?.novaSenha?.[0];
      if (p?.code === 'LINK_INVALIDO') setLinkInvalido(true);
      else if (erroSenha) setError('senha', { type: 'api', message: erroSenha }, { shouldFocus: true });
      else setErroApi(mensagemDeErro(e));
    }
  });

  const tipoSenha = mostrarSenha ? 'text' : 'password';

  return (
    <AuthLayout titulo="Criar nova senha" subtitulo="Depois, entre com a senha nova.">
      {linkInvalido ? (
        <div className="form">
          <Alerta>Este link expirou ou já foi usado. Peça um novo; ele vale 30 minutos.</Alerta>
          <Link className="btn btn-primario" to="/esqueci-senha">Pedir novo link</Link>
        </div>
      ) : (
        <form onSubmit={enviar} noValidate className="form">
          <Field label="Nova senha" type={tipoSenha} autoComplete="new-password" ajuda="Pelo menos 8 caracteres, com letras e números."
                 erro={errors.senha?.message} {...register('senha')} />
          <Field label="Confirmar nova senha" type={tipoSenha} autoComplete="new-password"
                 erro={errors.confirmarSenha?.message} {...register('confirmarSenha')} />
          <label className="check">
            <input type="checkbox" checked={mostrarSenha} onChange={(e) => setMostrarSenha(e.target.checked)} />
            Mostrar senha
          </label>
          {erroApi && <Alerta>{erroApi}</Alerta>}
          <Button type="submit" carregando={isSubmitting}>Salvar nova senha</Button>
        </form>
      )}
      <p className="auth-troca"><Link to="/entrar">Voltar para entrar</Link></p>
    </AuthLayout>
  );
}
