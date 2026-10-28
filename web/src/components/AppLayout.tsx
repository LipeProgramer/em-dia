import { NavLink, Outlet } from 'react-router-dom';
import { Icon, type IconName } from './Icon.tsx';
import logo from '../assets/logo.png';

const entradas: { to: string; rotulo: string; icone: IconName; central?: boolean }[] = [
  { to: '/', rotulo: 'Radar', icone: 'radar' },
  { to: '/itens', rotulo: 'Itens', icone: 'list' },
  { to: '/itens/novo', rotulo: 'Novo', icone: 'plus', central: true },
  { to: '/historico', rotulo: 'Histórico', icone: 'history' },
  { to: '/perfil', rotulo: 'Perfil', icone: 'user' },
];

/**
 * Menu inferior no celular (lateral no computador) com 5 entradas:
 * Radar · Itens · + Novo (botão central) · Histórico · Perfil (spec §5.2).
 */
export function BottomNav() {
  return (
    <nav className="bottom-nav" aria-label="Menu principal">
      <div className="nav-marca">
        <img src={logo} alt="" width={32} height={32} />
        <span>Em Dia</span>
      </div>
      <ul>
        {entradas.map((e) => (
          <li key={e.to}>
            <NavLink
              to={e.to}
              end={e.to === '/' || e.to === '/itens'}
              className={({ isActive }) => `nav-item ${e.central ? 'nav-central' : ''} ${isActive ? 'ativo' : ''}`}
            >
              <span className="nav-icone"><Icon name={e.icone} size={e.central ? 24 : 21} /></span>
              <span className="nav-rotulo">{e.rotulo}</span>
            </NavLink>
          </li>
        ))}
      </ul>
    </nav>
  );
}

export function AppLayout() {
  return (
    <div className="app">
      <a className="pular" href="#conteudo">Pular para o conteúdo</a>
      <BottomNav />
      <main id="conteudo" className="conteudo" tabIndex={-1}>
        <Outlet />
      </main>
    </div>
  );
}
