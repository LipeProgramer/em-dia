import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import type { z } from 'zod';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Link, useNavigate } from 'react-router-dom';
import { mensagemDeErro } from '../../api/client.ts';
import { usuariosApi } from '../../api/endpoints.ts';
import type { Preferencias } from '../../api/types.ts';
import { Alerta, Button, Dialog, ErrorState, Field, PageHeader, Skeleton } from '../../components/ui.tsx';
import { Icon } from '../../components/Icon.tsx';
import { aplicarErrosDaApi } from '../../utils/erros.ts';
import { useSession } from '../auth/useSession.tsx';
import { excluirContaSchema, preferenciasSchema } from '../itens/schemas.ts';

type PrefForm = z.input<typeof preferenciasSchema>;

/** Preferências de aviso (E06): reavisos 0 a 5; intervalo 1 a 7 dias. */
export function PreferenciasForm({ inicial }: { inicial: Preferencias }) {
  const queryClient = useQueryClient();
  const [salvo, setSalvo] = useState(false);
  const [erroApi, setErroApi] = useState<string | null>(null);
  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } =
    useForm<PrefForm, unknown, z.output<typeof preferenciasSchema>>({ resolver: zodResolver(preferenciasSchema), defaultValues: inicial });

  const salvar = handleSubmit(async (p) => {
    setSalvo(false);
    setErroApi(null);
    try {
      await usuariosApi.atualizarPreferencias(p);
      await queryClient.invalidateQueries({ queryKey: ['me'] });
      await queryClient.invalidateQueries({ queryKey: ['radar'] });
      setSalvo(true);
    } catch (e) {
      if (!aplicarErrosDaApi(e, setError, ['maxReavisos', 'intervaloReavisoDias'])) setErroApi(mensagemDeErro(e));
    }
  });

  return (
    <form onSubmit={salvar} noValidate className="form" aria-labelledby="titulo-preferencias">
      <h2 id="titulo-preferencias">Avisos</h2>
      <label className="check"><input type="checkbox" {...register('receberEmail')} /> Receber avisos por e-mail</label>
      <Field label="Quantos reavisos, no máximo?" type="number" min={0} max={5} erro={errors.maxReavisos?.message} {...register('maxReavisos')} />
      <Field label="Intervalo entre reavisos (dias)" type="number" min={1} max={7} erro={errors.intervaloReavisoDias?.message} {...register('intervaloReavisoDias')} />
      <label className="check"><input type="checkbox" {...register('modoFocoPadrao')} /> Abrir o Radar já no modo foco</label>
      {salvo && <Alerta tipo="sucesso">Preferências salvas.</Alerta>}
      {erroApi && <Alerta>{erroApi}</Alerta>}
      <Button type="submit" variante="secundario" carregando={isSubmitting}>Salvar preferências</Button>
    </form>
  );
}

/** Janela de exclusão da conta (E07): pede a senha e explica que tudo será apagado. */
function ExcluirContaDialog({ aberto, aoFechar }: { aberto: boolean; aoFechar: () => void }) {
  const { encerrarLocalmente } = useSession();
  const navigate = useNavigate();
  const [erroApi, setErroApi] = useState<string | null>(null);
  const { register, handleSubmit, formState: { errors, isSubmitting } } =
    useForm<z.infer<typeof excluirContaSchema>>({ resolver: zodResolver(excluirContaSchema) });

  const excluir = handleSubmit(async ({ senha }) => {
    setErroApi(null);
    try {
      await usuariosApi.excluirConta(senha);
      encerrarLocalmente();
      navigate('/entrar', { replace: true });
    } catch (e) {
      setErroApi(mensagemDeErro(e));
    }
  });

  return (
    <Dialog aberto={aberto} titulo="Excluir sua conta?" aoFechar={aoFechar}>
      <form onSubmit={excluir} noValidate className="form">
        <p>Isso apaga <strong>todos</strong> os seus dados: itens, veículos, histórico e avisos. Não dá para desfazer.</p>
        <Field label="Digite sua senha para confirmar" type="password" autoComplete="current-password"
               erro={errors.senha?.message} {...register('senha')} />
        {erroApi && <Alerta>{erroApi}</Alerta>}
        <Button type="submit" variante="perigo" carregando={isSubmitting}>Excluir minha conta</Button>
        <Button type="button" variante="secundario" onClick={aoFechar}>Cancelar</Button>
      </form>
    </Dialog>
  );
}

/** T14 · Perfil: dados, preferências, refazer check-up, veículos, sair e excluir conta (E04–E07). */
export function PerfilPage() {
  const { sair } = useSession();
  const navigate = useNavigate();
  const [excluindo, setExcluindo] = useState(false);
  const me = useQuery({ queryKey: ['me'], queryFn: usuariosApi.me });

  return (
    <div className="pagina">
      <PageHeader titulo="Perfil" />
      {me.isPending && <Skeleton />}
      {me.isError && <ErrorState mensagem={mensagemDeErro(me.error)} tentarDeNovo={() => me.refetch()} />}
      {me.data && (
        <>
          <section className="perfil-dados">
            <p className="perfil-nome">{me.data.nome}</p>
            <p className="texto-apoio">{me.data.email}</p>
          </section>

          <nav className="menu-perfil" aria-label="Opções do perfil">
            <Link to="/checkup"><Icon name="sparkles" size={18} /> Refazer o check-up</Link>
            <Link to="/perfil/veiculos"><Icon name="car" size={18} /> Meus veículos</Link>
          </nav>

          <PreferenciasForm inicial={me.data.preferencias} />

          <div className="perfil-sair">
            <Button variante="secundario" icone="logout" onClick={async () => { await sair(); navigate('/entrar'); }}>Sair</Button>
            <Button variante="texto" className="link-perigo" onClick={() => setExcluindo(true)}>Excluir conta</Button>
          </div>
          <ExcluirContaDialog aberto={excluindo} aoFechar={() => setExcluindo(false)} />
        </>
      )}
    </div>
  );
}
