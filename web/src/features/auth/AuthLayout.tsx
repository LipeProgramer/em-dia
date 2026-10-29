import { useEffect, type ReactNode } from 'react';
import logo from '../../assets/logo.png';

const VERDE = '#16564c'; // o mesmo --verde-auth do app.css

/**
 * Enquanto uma tela de entrada está aberta, a página inteira fica verde: no celular, a área atrás do relógio e o
 * espaço que aparece ao arrastar a tela para baixo usam a cor da página (antes ficavam cinza-claro).
 */
function usePaginaVerde() {
  useEffect(() => {
    const html = document.documentElement;
    const meta = document.querySelector<HTMLMetaElement>('meta[name="theme-color"]');
    const corAnterior = meta?.content;
    html.classList.add('pagina-auth');
    meta?.setAttribute('content', VERDE);
    return () => {
      html.classList.remove('pagina-auth');
      if (meta && corAnterior) meta.setAttribute('content', corAnterior);
    };
  }, []);
}

export function AuthLayout({ titulo, subtitulo, children }: { titulo: string; subtitulo: string; children: ReactNode }) {
  usePaginaVerde();
  return (
    <main className="auth" id="conteudo">
      <div className="auth-cartao">
        <div className="auth-marca">
          <img src={logo} alt="" width={48} height={48} />
          <div>
            <p className="auth-nome">Em Dia</p>
            <p className="auth-slogan">Radar de vencimentos</p>
          </div>
        </div>
        <h1>{titulo}</h1>
        <p className="auth-subtitulo">{subtitulo}</p>
        {children}
      </div>
    </main>
  );
}
