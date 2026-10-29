import { useEffect, useState } from 'react';
import { mensagemDeErro } from '../../api/client.ts';
import { pushApi } from '../../api/endpoints.ts';
import { Alerta, Button } from '../../components/ui.tsx';

type Estado = 'carregando' | 'sem-suporte' | 'instalar-no-iphone' | 'bloqueado' | 'desligado' | 'ligado';

const temSuporte = () => 'serviceWorker' in navigator && 'PushManager' in window && 'Notification' in window;
const ehIphone = () => /iPhone|iPad|iPod/.test(navigator.userAgent);
const estaInstalado = () =>
  window.matchMedia('(display-mode: standalone)').matches || (navigator as Navigator & { standalone?: boolean }).standalone === true;

/** Chave VAPID (base64url) no formato que o navegador pede. */
function base64UrlParaBytes(chave: string) {
  const base64 = (chave + '='.repeat((4 - (chave.length % 4)) % 4)).replace(/-/g, '+').replace(/_/g, '/');
  return Uint8Array.from(atob(base64), (c) => c.charCodeAt(0));
}

/** O service worker pode ainda estar sendo registrado; não espera para sempre. */
function serviceWorkerPronto() {
  return Promise.race([
    navigator.serviceWorker.ready,
    new Promise<never>((_, rejeitar) => setTimeout(() => rejeitar(new Error('O app ainda está carregando. Tente de novo.')), 8000)),
  ]);
}

async function enviarAoServidor(inscricao: PushSubscription) {
  const { keys } = inscricao.toJSON();
  await pushApi.inscrever({ endpoint: inscricao.endpoint, p256dh: keys!.p256dh, auth: keys!.auth });
}

function estadoInicial(): Estado {
  if (!temSuporte()) return ehIphone() && !estaInstalado() ? 'instalar-no-iphone' : 'sem-suporte';
  return Notification.permission === 'denied' ? 'bloqueado' : 'carregando';
}

/** Extra (DV20) · Liga as notificações de vencimento neste aparelho (Web Push). */
export function NotificacoesCard() {
  const [estado, setEstado] = useState<Estado>(estadoInicial);
  const [ocupado, setOcupado] = useState(false);
  const [mensagem, setMensagem] = useState<{ tipo: 'erro' | 'sucesso'; texto: string } | null>(null);

  useEffect(() => {
    if (estadoInicial() !== 'carregando') return;
    serviceWorkerPronto()
      .then((reg) => reg.pushManager.getSubscription())
      .then(async (inscricao) => {
        if (inscricao && Notification.permission === 'granted') {
          await enviarAoServidor(inscricao).catch(() => {}); // mantém o servidor em dia com este aparelho
          setEstado('ligado');
        } else {
          setEstado('desligado');
        }
      })
      .catch(() => setEstado('desligado'));
  }, []);

  const ativar = async () => {
    setMensagem(null);
    setOcupado(true);
    try {
      const permissao = await Notification.requestPermission(); // precisa vir direto do toque no botão
      if (permissao !== 'granted') {
        setEstado(permissao === 'denied' ? 'bloqueado' : 'desligado');
        return;
      }
      const reg = await serviceWorkerPronto();
      const chave = base64UrlParaBytes(await pushApi.chavePublica());
      await (await reg.pushManager.getSubscription())?.unsubscribe(); // inscrição antiga com outra chave
      const inscricao = await reg.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: chave });
      await enviarAoServidor(inscricao);
      setEstado('ligado');
      setMensagem({ tipo: 'sucesso', texto: 'Pronto! Vamos avisar neste aparelho quando um prazo estiver chegando.' });
    } catch (e) {
      setMensagem({ tipo: 'erro', texto: e instanceof Error && !('isAxiosError' in e) ? e.message : mensagemDeErro(e) });
    } finally {
      setOcupado(false);
    }
  };

  const desativar = async () => {
    setMensagem(null);
    setOcupado(true);
    try {
      const inscricao = await (await serviceWorkerPronto()).pushManager.getSubscription();
      if (inscricao) {
        await pushApi.remover(inscricao.endpoint);
        await inscricao.unsubscribe();
      }
      setEstado('desligado');
    } catch (e) {
      setMensagem({ tipo: 'erro', texto: mensagemDeErro(e) });
    } finally {
      setOcupado(false);
    }
  };

  const testar = async () => {
    setMensagem(null);
    setOcupado(true);
    try {
      const enviadas = await pushApi.testar();
      setMensagem(enviadas > 0
        ? { tipo: 'sucesso', texto: 'Notificação de teste enviada. Ela aparece em alguns segundos.' }
        : { tipo: 'erro', texto: 'Nenhum aparelho recebeu. Desative e ative de novo.' });
    } catch (e) {
      setMensagem({ tipo: 'erro', texto: mensagemDeErro(e) });
    } finally {
      setOcupado(false);
    }
  };

  return (
    <section className="form" aria-labelledby="titulo-notificacoes">
      <h2 id="titulo-notificacoes">Notificações no celular</h2>
      {estado === 'carregando' && <p className="texto-apoio">Verificando este aparelho…</p>}
      {estado === 'instalar-no-iphone' && (
        <p className="texto-apoio">
          No iPhone, primeiro adicione o Em Dia à Tela de Início (Safari → Compartilhar → Adicionar à Tela de Início)
          e abra o app por lá. Depois volte aqui para ativar.
        </p>
      )}
      {estado === 'sem-suporte' && <p className="texto-apoio">Este navegador não recebe notificações. Use o e-mail.</p>}
      {estado === 'bloqueado' && (
        <p className="texto-apoio">As notificações estão bloqueadas para o Em Dia. Libere nas configurações do celular ou do navegador.</p>
      )}
      {estado === 'desligado' && (
        <>
          <p className="texto-apoio">Receba um aviso na tela do celular quando um prazo estiver chegando.</p>
          <Button icone="bell" carregando={ocupado} onClick={ativar}>Ativar notificações</Button>
        </>
      )}
      {estado === 'ligado' && (
        <>
          <p className="texto-apoio">Ativadas neste aparelho. Os avisos chegam junto com a rotina diária das 8h.</p>
          <Button variante="secundario" icone="bell" carregando={ocupado} onClick={testar}>Enviar notificação de teste</Button>
          <Button variante="texto" disabled={ocupado} onClick={desativar}>Desativar neste aparelho</Button>
        </>
      )}
      {mensagem && <Alerta tipo={mensagem.tipo}>{mensagem.texto}</Alerta>}
    </section>
  );
}
