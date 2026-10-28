import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../features/auth/AuthContext.tsx';
import { Icon } from './Icon.tsx';

export function Layout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const primeiroNome = user?.nome?.trim().split(/\s+/)[0] || 'você';
  const iniciais = user?.nome
    ?.trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((parte) => parte[0])
    .join('')
    .toUpperCase() || 'ED';

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <div className="app-container">
      <a className="skip-link" href="#conteudo">Pular para o conteúdo</a>

      {user?.precisaCheckup && (
        <aside className="checkup-banner" aria-label="Configuração inicial pendente">
          <div className="checkup-banner-content">
            <span className="checkup-banner-icon" aria-hidden="true">
              <Icon name="sparkles" size={20} />
            </span>
            <p>
              <strong>Vamos montar seu Radar.</strong>{' '}
              Responda a 5 perguntas rápidas para começar com o que faz parte da sua rotina.
            </p>
            <Link to="/checkup" className="btn-checkup-action">
              Fazer Check-up <Icon name="arrow-right" size={16} />
            </Link>
          </div>
        </aside>
      )}

      <header className="app-header">
        <div className="header-inner">
          <Link to="/" className="brand-logo" aria-label="Em Dia — ir para o Radar">
            <span className="brand-mark"><Icon name="radar" size={23} strokeWidth={2} /></span>
            <span className="brand-wordmark">
              <span className="brand-name">em dia<span className="brand-period">.</span></span>
              <span className="brand-caption">RADAR PESSOAL</span>
            </span>
          </Link>

          <nav className="nav-menu" aria-label="Navegação principal">
            <NavLink to="/" end className={({ isActive }) => `nav-link ${isActive ? 'nav-link-active' : ''}`}>
              <Icon name="radar" size={18} /> <span>Radar</span>
            </NavLink>
            <NavLink to="/itens" className={({ isActive }) => `nav-link ${isActive ? 'nav-link-active' : ''}`}>
              <Icon name="list" size={18} /> <span>Meus itens</span>
            </NavLink>
            <NavLink to="/veiculos" className={({ isActive }) => `nav-link ${isActive ? 'nav-link-active' : ''}`}>
              <Icon name="car" size={18} /> <span>Veículos</span>
            </NavLink>
            <NavLink to="/perfil" className={({ isActive }) => `nav-link ${isActive ? 'nav-link-active' : ''}`}>
              <Icon name="settings" size={18} /> <span>Preferências</span>
            </NavLink>
          </nav>

          <div className="header-user">
            <span className="user-avatar" aria-hidden="true">{iniciais}</span>
            <span className="user-greeting">
              <span className="user-greeting-overline">BEM-VINDO(A)</span>
              <strong>{primeiroNome}</strong>
            </span>
            <span className="header-user-divider" aria-hidden="true" />
            <button type="button" className="btn-logout" onClick={handleLogout} aria-label="Sair da conta" title="Sair da conta">
              <Icon name="logout" size={18} />
              <span>Sair</span>
            </button>
          </div>
        </div>
      </header>

      <main id="conteudo" className="app-main" tabIndex={-1}>
        <Outlet />
      </main>

      <footer className="app-footer">
        <span className="footer-brand"><Icon name="radar" size={15} /> em dia.</span>
        <span>Um passo de cada vez.</span>
        <span>Organização sem culpa · UNICV</span>
      </footer>
    </div>
  );
}
