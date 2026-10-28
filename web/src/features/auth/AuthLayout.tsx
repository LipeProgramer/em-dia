import type { ReactNode } from 'react';
import logo from '../../assets/logo.png';

export function AuthLayout({ titulo, subtitulo, children }: { titulo: string; subtitulo: string; children: ReactNode }) {
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
